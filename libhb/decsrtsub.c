/* 
   This file is part of the HandBrake source code.
   Homepage: <http://handbrake.fr/>.
   It may be used under the terms of the GNU General Public License. */

#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <iconv.h>
#include <errno.h>
#include "hb.h"

struct start_and_end {
    unsigned long start, end;
};

enum
{
    k_state_inEntry,
    k_state_inEntry_or_new,
    k_state_potential_new_entry,
    k_state_timecode,
};

typedef struct srt_entry_s {
    long offset, duration;
    long start, stop;
    char text[1024];
    int  pos;
} srt_entry_t;

/*
 * Store all context in the work private struct,
 */
struct hb_work_private_s
{
    hb_job_t * job;
    FILE     * file;
    char       buf[1024];
    int        pos;
    int        end;
    char       utf8_buf[2048];
    int        utf8_pos;
    int        utf8_end;
    int        utf8_bom_skipped;
    unsigned long current_time;
    unsigned long number_of_entries;
    unsigned long last_entry_number;
    unsigned long current_state;
    srt_entry_t current_entry;
    iconv_t *iconv_context;
    hb_subtitle_t *subtitle;
    uint64_t start_time;              // In HB time
    uint64_t stop_time;               // In HB time
};

static int 
read_time_from_string( const char* timeString, struct start_and_end *result )
{
    // for ex. 00:00:15,248 --> 00:00:16,545
    
    long houres1, minutes1, seconds1, milliseconds1,
         houres2, minutes2, seconds2, milliseconds2;
    int scanned;
    
    scanned = sscanf(timeString, "%ld:%ld:%ld,%ld --> %ld:%ld:%ld,%ld\n",
                    &houres1, &minutes1, &seconds1, &milliseconds1,
                    &houres2, &minutes2, &seconds2, &milliseconds2);
    if (scanned != 8)
    {
        return 0;
    }
    result->start =
        milliseconds1 + seconds1*1000 + minutes1*60*1000 + houres1*60*60*1000;
    result->end =
        milliseconds2 + seconds2*1000 + minutes2*60*1000 + houres2*60*60*1000;
    return 1;
}

static int utf8_fill( hb_work_private_t * pv )
{
    int bytes, conversion = 0;
    size_t out_size;

    /* Align utf8 data to beginning of the buffer so that we can
     * fill the buffer to its maximum */
    memmove( pv->utf8_buf, pv->utf8_buf + pv->utf8_pos, pv->utf8_end - pv->utf8_pos );
    pv->utf8_end -= pv->utf8_pos;
    pv->utf8_pos = 0;
    out_size = 2048 - pv->utf8_end;
    while( out_size )
    {
        char *p, *q;
        size_t in_size, retval;

        if( pv->end == pv->pos )
        {
            bytes = fread( pv->buf, 1, 1024, pv->file );
            pv->pos = 0;
            pv->end = bytes;
            if( bytes == 0 )
            {
                if( conversion )
                    return 1;
                else
                    return 0;
            }
        }

        p = pv->buf + pv->pos;
        q = pv->utf8_buf + pv->utf8_end;
        in_size = pv->end - pv->pos;

        retval = iconv( pv->iconv_context, &p, &in_size, &q, &out_size);
        if( q != pv->utf8_buf + pv->utf8_pos )
            conversion = 1;

        pv->utf8_end = q - pv->utf8_buf;
        pv->pos = p - pv->buf;

        if ( !pv->utf8_bom_skipped )
        {
            uint8_t *buf = (uint8_t*)pv->utf8_buf;
            if (buf[0] == 0xef && buf[1] == 0xbb && buf[2] == 0xbf)
            {
                pv->utf8_pos = 3;
            }
            pv->utf8_bom_skipped = 1;
        }

        if( ( retval == -1 ) && ( errno == EINVAL ) )
        {
            /* Incomplete multibyte sequence, read more data */
            memmove( pv->buf, p, pv->end - pv->pos );
            pv->end -= pv->pos;
            pv->pos = 0;
            bytes = fread( pv->buf + pv->end, 1, 1024 - pv->end, pv->file );
            if( bytes == 0 )
            {
                if( !conversion )
                    return 0;
                else
                    return 1;
            }
            pv->end += bytes;
        } else if ( ( retval == -1 ) && ( errno == EILSEQ ) )
        {
            hb_error( "Invalid byte for codeset in input, discard byte" );
            /* Try the next byte of the input */
            pv->pos++;
        } else if ( ( retval == -1 ) && ( errno == E2BIG ) )
        {
            /* buffer full */
            return conversion;
        }
    }
    return 1;
}

static int get_line( hb_work_private_t * pv, char *buf, int size )
{
    int i;
    char c;

    /* Find newline in converted UTF-8 buffer */
    for( i = 0; i < size - 1; i++ )
    {
        if( pv->utf8_pos >= pv->utf8_end )
        {
            if( !utf8_fill( pv ) )
            {
                if( i )
                    return 1;
                else
                    return 0;
            }
        }
        c = pv->utf8_buf[pv->utf8_pos++];
        if( c == '\n' )
        {
            buf[i] = '\n';
            buf[i+1] = '\0';
            return 1;
        }
        buf[i] = c;
    }
    buf[0] = '\0';
    return 1;
}

/*
 * Read the SRT file and put the entries into the subtitle fifo for all to read
 */
static hb_buffer_t *srt_read( hb_work_private_t *pv )
{
    char line_buffer[1024];
    int reprocess = 0, resync = 0;

    if( !pv->file )
    {
        return NULL;
    }
    
    while( reprocess || get_line( pv, line_buffer, sizeof( line_buffer ) ) ) 
    {
        reprocess = 0;
        switch (pv->current_state)
        {
        case k_state_timecode:
        {
            struct start_and_end timing;
            int result;

            result = read_time_from_string( line_buffer, &timing );
            if (!result)
            {
                resync = 1;
                pv->current_state = k_state_potential_new_entry;
                continue;
            }
            pv->current_entry.duration = timing.end - timing.start;
            pv->current_entry.offset = timing.start - pv->current_time;
            
            pv->current_time = timing.end;

            pv->current_entry.start = timing.start;
            pv->current_entry.stop = timing.end;

            pv->current_state = k_state_inEntry;
            continue;
        }

        case k_state_inEntry_or_new:
        {
            char *endpoint;
            long entry_number;
            /*
             * Is this really new next entry begin?
             */
            entry_number = strtol(line_buffer, &endpoint, 10);
            if (endpoint == line_buffer ||
                (endpoint && *endpoint != '\n' && *endpoint != '\r'))
            {
                /*
                 * Doesn't resemble an entry number
                 * must still be in an entry
                 */
                if (!resync)
                {
                    reprocess = 1;
                    pv->current_state = k_state_inEntry;
                }
                continue;
            }
            reprocess = 1;
            pv->current_state = k_state_potential_new_entry;
            break;
        }

        case k_state_inEntry:
        {
            char *q;
            int  size, len;

            // If the current line is empty, we assume this is the
            //	seperation betwene two entries. In case we are wrong,
            //	the mistake is corrected in the next state.
            if (strcmp(line_buffer, "\n") == 0 || strcmp(line_buffer, "\r\n") == 0) {
                pv->current_state = k_state_potential_new_entry;
                continue;
            }
            
            q = pv->current_entry.text + pv->current_entry.pos;
            len = strlen( line_buffer );
            size = MIN(1024 - pv->current_entry.pos - 1, len );
            memcpy(q, line_buffer, size);
            pv->current_entry.pos += size;
            pv->current_entry.text[pv->current_entry.pos] = '\0';
            break;
        }

        case k_state_potential_new_entry:
        {
            char *endpoint;
            long entry_number;
            hb_buffer_t *buffer = NULL;
            /*
             * Is this really new next entry begin?
             */
            entry_number = strtol(line_buffer, &endpoint, 10);
            if (!resync && (*line_buffer == '\n' || *line_buffer == '\r'))
            {
                /*
                 * Well.. looks like we are in the wrong mode.. lets add the
                 * newline we misinterpreted...
                 */
                strncat(pv->current_entry.text, " ", 1024);
                pv->current_state = k_state_inEntry_or_new;
                continue;
            }
            if (endpoint == line_buffer ||
                (endpoint && *endpoint != '\n' && *endpoint != '\r'))
            {
                /*
                 * Well.. looks like we are in the wrong mode.. lets add the
                 * line we misinterpreted...
                 */
                if (!resync)
                {
                    reprocess = 1;
                    pv->current_state = k_state_inEntry;
                }
                continue;
            }
            /*
             * We found the next entry - or a really rare error condition
             */
            pv->last_entry_number = entry_number;
            resync = 0;
            if( *pv->current_entry.text )
            {
                long length;
                char *p, *q;
                int  line = 1;
                uint64_t start_time = ( pv->current_entry.start + 
                                        pv->subtitle->config.offset ) * 90;
                uint64_t stop_time = ( pv->current_entry.stop + 
                                       pv->subtitle->config.offset ) * 90;

                if( !( start_time > pv->start_time && stop_time < pv->stop_time ) )
                {
                    hb_deep_log( 3, "Discarding SRT at time start %"PRId64", stop %"PRId64, start_time, stop_time);
                    memset( &pv->current_entry, 0, sizeof( srt_entry_t ) );
                    ++(pv->number_of_entries);
                    pv->current_state = k_state_timecode;
                    continue;
                }

                length = strlen( pv->current_entry.text );

                for( q = p = pv->current_entry.text; *p; p++)
                {
                    if( *p == '\n' )
                    {
                        if ( line == 1 )
                        {
                            *q = *p;
                            line = 2;
                        }
                        else
                        {
                            *q = ' ';
                        }
                        q++;
                    }
                    else if( *p != '\r' )
                    {
                        *q = *p;
                        q++;
                    }
                    else
                    {
                        length--;
                    }
                }
                *q = '\0';

                buffer = hb_buffer_init( length + 1 );

                if( buffer )
                {
                    buffer->start = start_time - pv->start_time;
                    buffer->stop = stop_time - pv->start_time;

                    memcpy( buffer->data, pv->current_entry.text, length + 1 );
                }
            }
            memset( &pv->current_entry, 0, sizeof( srt_entry_t ) );
            ++(pv->number_of_entries);
            pv->current_state = k_state_timecode;
            if( buffer )
            {
                return buffer;
            }
            continue;
        } 
        }
    }

    hb_buffer_t *buffer = NULL;
    if( *pv->current_entry.text )
    {
        long length;
        char *p, *q;
        int  line = 1;
        uint64_t start_time = ( pv->current_entry.start + 
                                pv->subtitle->config.offset ) * 90;
        uint64_t stop_time = ( pv->current_entry.stop + 
                               pv->subtitle->config.offset ) * 90;

        if( !( start_time > pv->start_time && stop_time < pv->stop_time ) )
        {
            hb_deep_log( 3, "Discarding SRT at time start %"PRId64", stop %"PRId64, start_time, stop_time);
            memset( &pv->current_entry, 0, sizeof( srt_entry_t ) );
            return NULL;
        }

        length = strlen( pv->current_entry.text );

        for( q = p = pv->current_entry.text; *p; p++)
        {
            if( *p == '\n' )
            {
                if ( line == 1 )
                {
                    *q = *p;
                    line = 2;
                }
                else
                {
                    *q = ' ';
                }
                q++;
            }
            else if( *p != '\r' )
            {
                *q = *p;
                q++;
            }
            else
            {
                length--;
            }
        }
        *q = '\0';

        buffer = hb_buffer_init( length + 1 );

        if( buffer )
        {
            buffer->start = start_time - pv->start_time;
            buffer->stop = stop_time - pv->start_time;

            memcpy( buffer->data, pv->current_entry.text, length + 1 );
        }
    }
    memset( &pv->current_entry, 0, sizeof( srt_entry_t ) );
    if( buffer )
    {
        return buffer;
    }
    
    return NULL;
}

static int decsrtInit( hb_work_object_t * w, hb_job_t * job )
{
    int retval = 1;
    hb_work_private_t * pv;
    hb_buffer_t *buffer;
    int i;
    hb_chapter_t * chapter;
    hb_title_t *title = job->title;

    pv = calloc( 1, sizeof( hb_work_private_t ) );
    if( pv )
    {
        w->private_data = pv;

        pv->job = job;

        buffer = hb_buffer_init( 0 );
        hb_fifo_push( w->fifo_in, buffer);
        
        pv->current_state = k_state_potential_new_entry;
        pv->number_of_entries = 0;
        pv->last_entry_number = 0;
        pv->current_time = 0;
        pv->subtitle = w->subtitle;

        /*
         * Figure out the start and stop times from teh chapters being
         * encoded - drop subtitle not in this range.
         */
        pv->start_time = 0;
        for( i = 1; i < job->chapter_start; ++i )
        {
            chapter = hb_list_item( title->list_chapter, i - 1 );
            if( chapter )
            {
                pv->start_time += chapter->duration;
            } else {
                hb_error( "Could not locate chapter %d for SRT start time", i );
                retval = 0;
            }
        }
        pv->stop_time = pv->start_time;
        for( i = job->chapter_start; i <= job->chapter_end; ++i )
        {
            chapter = hb_list_item( title->list_chapter, i - 1 );
            if( chapter )
            {
                pv->stop_time += chapter->duration;
            } else {
                hb_error( "Could not locate chapter %d for SRT start time", i );
                retval = 0;
            }
        }

        hb_deep_log( 3, "SRT Start time %"PRId64", stop time %"PRId64, pv->start_time, pv->stop_time);

        pv->iconv_context = iconv_open( "utf-8", pv->subtitle->config.src_codeset );


        if( pv->iconv_context == (iconv_t) -1 )
        {
            hb_error("Could not open the iconv library with those file formats\n");

        } else {
            memset( &pv->current_entry, 0, sizeof( srt_entry_t ) );
            
            pv->file = fopen( w->subtitle->config.src_filename, "r" );
            
            if( !pv->file )
            {
                hb_error("Could not open the SRT subtitle file '%s'\n", 
                         w->subtitle->config.src_filename);
            } else {
                retval = 0;
            }
        }
    } 

    return retval;
}

static int decsrtWork( hb_work_object_t * w, hb_buffer_t ** buf_in,
                       hb_buffer_t ** buf_out )
{
    hb_work_private_t * pv = w->private_data;
    hb_buffer_t * in = *buf_in;
    hb_buffer_t * out = NULL;

    out = srt_read( pv );

    if( out )
    {
        /*
         * Keep a buffer in our input fifo so that we get run.
         */
        hb_fifo_push( w->fifo_in, in);
        *buf_in = NULL;
        *buf_out = out;
    } else {
        *buf_out = NULL;
        return HB_WORK_OK;
    }

    return HB_WORK_OK;  
}

static void decsrtClose( hb_work_object_t * w )
{
    hb_work_private_t * pv = w->private_data;
    fclose( pv->file );
    iconv_close(pv->iconv_context);
    free( w->private_data );
}

hb_work_object_t hb_decsrtsub =
{
    WORK_DECSRTSUB,
    "SRT Subtitle Decoder",
    decsrtInit,
    decsrtWork,
    decsrtClose
};

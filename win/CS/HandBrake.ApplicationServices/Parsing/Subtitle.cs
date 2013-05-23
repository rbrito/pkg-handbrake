// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Subtitle.cs" company="HandBrake Project (http://handbrake.fr)">
//   This file is part of the HandBrake source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   An object that represents a subtitle associated with a Title, in a DVD
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandBrake.ApplicationServices.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    using HandBrake.ApplicationServices.Model.Encoding;
    using HandBrake.ApplicationServices.Utilities;

    /// <summary>
    /// An object that represents a subtitle associated with a Title, in a DVD
    /// </summary>
    public class Subtitle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Subtitle"/> class.
        /// </summary>
        public Subtitle()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Subtitle"/> class.
        /// </summary>
        /// <param name="trackNumber">
        /// The track number.
        /// </param>
        /// <param name="language">
        /// The language.
        /// </param>
        /// <param name="languageCode">
        /// The language code.
        /// </param>
        /// <param name="subtitleType">
        /// The subtitle type.
        /// </param>
        public Subtitle(int trackNumber, string language, string languageCode, SubtitleType subtitleType)
        {
            this.TrackNumber = trackNumber;
            this.Language = language;
            this.LanguageCode = languageCode;
            this.SubtitleType = subtitleType;
        }


        /// <summary>
        /// Gets or sets the track number of this Subtitle
        /// </summary>
        public int TrackNumber { get; set; }

        /// <summary>
        /// Gets or sets the The language (if detected) of this Subtitle
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the Langauage Code
        /// </summary>
        public string LanguageCode { get; set; }

        /// <summary>
        /// Gets or sets the Subtitle Type
        /// </summary>
        public SubtitleType SubtitleType { get; set; }

        /// <summary>
        /// Gets Subtitle Type
        /// </summary>
        public string TypeString
        {
            get
            {
                return EnumHelper<Enum>.GetDescription(this.SubtitleType);
            }
        }

        /// <summary>
        /// Parse the input strings related to subtitles
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        /// <returns>
        /// A Subitle object
        /// </returns>
        public static Subtitle Parse(StringReader output)
        {
            string curLine = output.ReadLine();

            // + 1, English (iso639-2: eng) (Text)(SSA)
            // + 1, English (iso639-2: eng) (Text)(UTF-8)
            Match m = Regex.Match(curLine, @"^    \+ ([0-9]*), ([A-Za-z, ]*) \((.*)\) \(([a-zA-Z]*)\)\(([a-zA-Z0-9\-]*)\)");

            if (m.Success && !curLine.Contains("HandBrake has exited."))
            {
                var thisSubtitle = new Subtitle
                                       {
                                           TrackNumber = int.Parse(m.Groups[1].Value.Trim()),
                                           Language = m.Groups[2].Value,
                                           LanguageCode = m.Groups[3].Value,
                                       };

                switch (m.Groups[5].Value)
                {
                    case "VOBSUB":
                        thisSubtitle.SubtitleType = SubtitleType.VobSub;
                        break;
                    case "SRT":
                        thisSubtitle.SubtitleType = SubtitleType.SRT;
                        break;
                    case "CC":
                        thisSubtitle.SubtitleType = SubtitleType.CC;
                        break;
                    case "UTF-8":
                        thisSubtitle.SubtitleType = SubtitleType.UTF8Sub;
                        break;
                    case "TX3G":
                        thisSubtitle.SubtitleType = SubtitleType.TX3G;
                        break;
                    case "SSA":
                        thisSubtitle.SubtitleType = SubtitleType.SSA;
                        break;
                    case "PGS":
                        thisSubtitle.SubtitleType = SubtitleType.PGS;
                        break;
                    default:
                        thisSubtitle.SubtitleType = SubtitleType.Unknown;
                        break;
                }

                return thisSubtitle;
            }
            return null;
        }

        /// <summary>
        /// Parse a list of Subtitle tracks from an input string.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        /// <returns>
        /// An array of Subtitle objects
        /// </returns>
        public static IEnumerable<Subtitle> ParseList(StringReader output)
        {
            var subtitles = new List<Subtitle>();
            while ((char)output.Peek() != '+')
            {
                Subtitle thisSubtitle = Parse(output);

                if (thisSubtitle != null)
                    subtitles.Add(thisSubtitle);
                else
                    break;
            }
            return subtitles.ToArray();
        }

        /// <summary>
        /// Override of the ToString method to make this object easier to use in the UI
        /// </summary>
        /// <returns>A string formatted as: {track #} {language}</returns>
        public override string ToString()
        {
            return this.SubtitleType == SubtitleType.ForeignAudioSearch ? "Foreign Audio Scan" : string.Format("{0} {1} ({2})", this.TrackNumber, this.Language, this.TypeString);
        }

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="other">
        /// The other.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public bool Equals(Subtitle other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return other.TrackNumber == this.TrackNumber && object.Equals(other.Language, this.Language) && object.Equals(other.LanguageCode, this.LanguageCode) && object.Equals(other.SubtitleType, this.SubtitleType);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != typeof(Subtitle))
            {
                return false;
            }
            return Equals((Subtitle)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = this.TrackNumber;
                result = (result * 397) ^ (this.Language != null ? this.Language.GetHashCode() : 0);
                result = (result * 397) ^ (this.LanguageCode != null ? this.LanguageCode.GetHashCode() : 0);
                result = (result * 397) ^ this.SubtitleType.GetHashCode();
                return result;
            }
        }
    }
}
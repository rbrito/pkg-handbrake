﻿/*  EncodeTask.cs $
    This file is part of the HandBrake source code.
    Homepage: <http://handbrake.fr>.
    It may be used under the terms of the GNU General Public License. */

namespace HandBrake.ApplicationServices.Model
{
    using System.Collections.ObjectModel;
    using System.Linq;

    using HandBrake.ApplicationServices.Model.Encoding;
    using HandBrake.Interop.Model;
    using HandBrake.Interop.Model.Encoding;
    using HandBrake.Interop.Model.Encoding.x264;

    using OutputFormat = HandBrake.ApplicationServices.Model.Encoding.OutputFormat;

    /// <summary>
    /// An Encode Task
    /// </summary>
    public class EncodeTask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EncodeTask"/> class.
        /// </summary>
        public EncodeTask()
        {
            this.Cropping = new Cropping();
            this.AudioTracks = new ObservableCollection<AudioTrack>();
            this.SubtitleTracks = new ObservableCollection<SubtitleTrack>();
            this.ChapterNames = new ObservableCollection<ChapterMarker>();
            this.AllowedPassthruOptions = new AllowedPassthru();
        }

        #region Source
        /// <summary>
        /// Gets or sets Source.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets Title.
        /// </summary>
        public int Title { get; set; }

        /// <summary>
        /// Gets or sets the Angle
        /// </summary>
        public int Angle { get; set; }

        /// <summary>
        /// Gets or sets PointToPointMode.
        /// </summary>
        public PointToPointMode PointToPointMode { get; set; }

        /// <summary>
        /// Gets or sets StartPoint.
        /// </summary>
        public int StartPoint { get; set; }

        /// <summary>
        /// Gets or sets EndPoint.
        /// </summary>
        public int EndPoint { get; set; }

        #endregion

        #region Destination

        /// <summary>
        /// Gets or sets Destination.
        /// </summary>
        public string Destination { get; set; }

        #endregion

        #region Output Settings
        /// <summary>
        /// Gets or sets OutputFormat.
        /// </summary>
        public OutputFormat OutputFormat { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether LargeFile.
        /// </summary>
        public bool LargeFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Optimize.
        /// </summary>
        public bool OptimizeMP4 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IPod5GSupport.
        /// </summary>
        public bool IPod5GSupport { get; set; }
        #endregion

        #region Picture

        /// <summary>
        /// Gets or sets Width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets Height.
        /// </summary>
        public int? Height { get; set; }

        /// <summary>
        /// Gets or sets MaxWidth.
        /// </summary>
        public int? MaxWidth { get; set; }

        /// <summary>
        /// Gets or sets MaxHeight.
        /// </summary>
        public int? MaxHeight { get; set; }

        /// <summary>
        /// Gets or sets Cropping.
        /// </summary>
        public Cropping Cropping { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether HasCropping.
        /// </summary>
        public bool HasCropping { get; set; }

        /// <summary>
        /// Gets or sets Anamorphic.
        /// </summary>
        public Anamorphic Anamorphic { get; set; }

        /// <summary>
        /// Gets or sets DisplayWidth.
        /// </summary>
        public double? DisplayWidth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether KeepDisplayAspect.
        /// </summary>
        public bool KeepDisplayAspect { get; set; }

        /// <summary>
        /// Gets or sets PixelAspectX.
        /// </summary>
        public int PixelAspectX { get; set; }

        /// <summary>
        /// Gets or sets PixelAspectY.
        /// </summary>
        public int PixelAspectY { get; set; }

        /// <summary>
        /// Gets or sets Modulus.
        /// </summary>
        public int? Modulus { get; set; }
        #endregion

        #region Filters

        /// <summary>
        /// Gets or sets Deinterlace.
        /// </summary>
        public Deinterlace Deinterlace { get; set; }

        /// <summary>
        /// Gets or sets CustomDeinterlace.
        /// </summary>
        public string CustomDeinterlace { get; set; }

        /// <summary>
        /// Gets or sets Decomb.
        /// </summary>
        public Decomb Decomb { get; set; }

        /// <summary>
        /// Gets or sets CustomDecomb.
        /// </summary>
        public string CustomDecomb { get; set; }

        /// <summary>
        /// Gets or sets Detelecine.
        /// </summary>
        public Detelecine Detelecine { get; set; }

        /// <summary>
        /// Gets or sets CustomDetelecine.
        /// </summary>
        public string CustomDetelecine { get; set; }

        /// <summary>
        /// Gets or sets Denoise.
        /// </summary>
        public Denoise Denoise { get; set; }

        /// <summary>
        /// Gets or sets CustomDenoise.
        /// </summary>
        public string CustomDenoise { get; set; }

        /// <summary>
        /// Gets or sets Deblock.
        /// </summary>
        public int Deblock { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Grayscale.
        /// </summary>
        public bool Grayscale { get; set; }
        #endregion

        #region Video

        /// <summary>
        /// Gets or sets VideoEncodeRateType.
        /// </summary>
        public VideoEncodeRateType VideoEncodeRateType { get; set; }

        /// <summary>
        /// Gets or sets the VideoEncoder
        /// </summary>
        public VideoEncoder VideoEncoder { get; set; }

        /// <summary>
        /// Gets or sets the Video Encode Mode
        /// </summary>
        public FramerateMode FramerateMode { get; set; }

        /// <summary>
        /// Gets or sets Quality.
        /// </summary>
        public double? Quality { get; set; }

        /// <summary>
        /// Gets or sets VideoBitrate.
        /// </summary>
        public int? VideoBitrate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether TwoPass.
        /// </summary>
        public bool TwoPass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether TurboFirstPass.
        /// </summary>
        public bool TurboFirstPass { get; set; }

        /// <summary>
        /// Gets or sets Framerate.
        /// Null = Same as Source
        /// </summary>
        public double? Framerate { get; set; }

        #endregion

        #region Audio

        /// <summary>
        /// Gets or sets AudioEncodings.
        /// </summary>
        public ObservableCollection<AudioTrack> AudioTracks { get; set; }

        /// <summary>
        /// Gets or sets AllowedPassthruOptions.
        /// </summary>
        public AllowedPassthru AllowedPassthruOptions { get; set; }
        #endregion

        #region Subtitles

        /// <summary>
        /// Gets or sets SubtitleTracks.
        /// </summary>
        public ObservableCollection<SubtitleTrack> SubtitleTracks { get; set; }
        #endregion

        #region Chapters

        /// <summary>
        /// Gets or sets a value indicating whether IncludeChapterMarkers.
        /// </summary>
        public bool IncludeChapterMarkers { get; set; }

        /// <summary>
        /// Gets or sets ChapterMarkersFilePath.
        /// </summary>
        public string ChapterMarkersFilePath { get; set; }

        /// <summary>
        /// Gets or sets ChapterNames.
        /// </summary>
        public ObservableCollection<ChapterMarker> ChapterNames { get; set; }

        #endregion

        #region Advanced

        /// <summary>
        /// Gets or sets AdvancedEncoderOptions.
        /// </summary>
        public string AdvancedEncoderOptions { get; set; }

        /// <summary>
        /// Gets or sets x264Preset.
        /// </summary>
        public x264Preset x264Preset { get; set; }

        /// <summary>
        /// Gets or sets x264Profile.
        /// </summary>
        public x264Profile x264Profile { get; set; }

        /// <summary>
        /// Gets or sets X264Tune.
        /// </summary>
        public x264Tune X264Tune { get; set; }

        /// <summary>
        /// Gets or sets Verbosity.
        /// </summary>
        public int Verbosity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether disableLibDvdNav.
        /// </summary>
        public bool DisableLibDvdNav { get; set; }

        #endregion

        #region Preset Information (TODO This should probably be dropped)

        /// <summary>
        /// Gets or sets PresetBuildNumber.
        /// </summary>
        public int PresetBuildNumber { get; set; }

        /// <summary>
        /// Gets or sets PresetDescription.
        /// </summary>
        public string PresetDescription { get; set; }

        /// <summary>
        /// Gets or sets PresetName.
        /// </summary>
        public string PresetName { get; set; }

        /// <summary>
        /// Gets or sets Type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether UsesMaxPictureSettings.
        /// </summary>
        public bool UsesMaxPictureSettings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether UsesPictureFilters.
        /// </summary>
        public bool UsesPictureFilters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether UsesPictureSettings.
        /// </summary>
        public bool UsesPictureSettings { get; set; }
        #endregion

        #region Helpers

        /// <summary>
        /// Gets a value indicating whether M4v extension is required.
        /// </summary>
        public bool RequiresM4v
        {
            get
            {
                if (this.OutputFormat == OutputFormat.M4V || this.OutputFormat == OutputFormat.Mp4)
                {
                    bool audio = this.AudioTracks.Any(item => item.Encoder == AudioEncoder.Ac3Passthrough || 
                        item.Encoder == AudioEncoder.Ac3 || item.Encoder == AudioEncoder.DtsPassthrough || item.Encoder == AudioEncoder.Passthrough);

                    bool subtitles = this.SubtitleTracks.Any(track => track.SubtitleType != SubtitleType.VobSub);

                    return audio || subtitles;
                }

                return false;
            }
        }
        #endregion
    }
}

﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PictureSettingsViewModel.cs" company="HandBrake Project (http://handbrake.fr)">
//   This file is part of the HandBrake source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The Picture Settings View Model
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandBrakeWPF.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Drawing;

    using Caliburn.Micro;

    using HandBrake.ApplicationServices.Model;
    using HandBrake.ApplicationServices.Parsing;
    using HandBrake.ApplicationServices.Services.Interfaces;
    using HandBrake.Interop.Model.Encoding;

    using HandBrakeWPF.ViewModels.Interfaces;

    /// <summary>
    /// The Picture Settings View Model
    /// </summary>
    [Export(typeof(IPictureSettingsViewModel))]
    public class PictureSettingsViewModel : ViewModelBase, IPictureSettingsViewModel
    {
        /*
         * TODO:
         * Handle Presets when a new title is set
         * Handle changes in cropping affecting the resolution calcuation.
         * 
         */

        #region Backing Fields

        /// <summary>
        /// The crop bottom.
        /// </summary>
        private int cropBottom;

        /// <summary>
        /// The crop left.
        /// </summary>
        private int cropLeft;

        /// <summary>
        /// The crop right.
        /// </summary>
        private int cropRight;

        /// <summary>
        /// The crop top.
        /// </summary>
        private int cropTop;

        /// <summary>
        /// The display size.
        /// </summary>
        private string displaySize;

        /// <summary>
        /// The display width.
        /// </summary>
        private int displayWidth;

        /// <summary>
        /// The height.
        /// </summary>
        private int height;

        /// <summary>
        /// The is custom crop.
        /// </summary>
        private bool isCustomCrop;

        /// <summary>
        /// The maintain aspect ratio.
        /// </summary>
        private bool maintainAspectRatio;

        /// <summary>
        /// The par height.
        /// </summary>
        private int parHeight;

        /// <summary>
        /// The par width.
        /// </summary>
        private int parWidth;

        /// <summary>
        /// The selected anamorphic mode.
        /// </summary>
        private Anamorphic selectedAnamorphicMode;

        /// <summary>
        /// The selected modulus
        /// </summary>
        private int selectedModulus;

        /// <summary>
        /// The source info.
        /// </summary>
        private string sourceInfo;

        /// <summary>
        /// The width.
        /// </summary>
        private int width;

        /// <summary>
        ///  Backing field for show custom anamorphic controls
        /// </summary>
        private bool showCustomAnamorphicControls;

        /// <summary>
        ///  Backing field for for height control enabled
        /// </summary>
        private bool heightControlEnabled = true;

        /// <summary>
        /// Backing field for width control enabled.
        /// </summary>
        private bool widthControlEnabled = true;

        #endregion

        #region Source Information

        /// <summary>
        /// Source Resolution
        /// </summary>
        private Size sourceResolution;

        /// <summary>
        /// Source Aspect Ratio
        /// </summary>
        private double sourceAspectRatio;

        /// <summary>
        /// Source Par Values
        /// </summary>
        private Size sourceParValues;
        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HandBrakeWPF.ViewModels.PictureSettingsViewModel"/> class.
        /// </summary>
        /// <param name="windowManager">
        /// The window manager.
        /// </param>
        /// <param name="userSettingService">
        /// The user Setting Service.
        /// </param>
        public PictureSettingsViewModel(IWindowManager windowManager, IUserSettingService userSettingService)
        {
            this.SelectedModulus = 16;
            this.MaintainAspectRatio = true;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets AnamorphicModes.
        /// </summary>
        public IEnumerable<Anamorphic> AnamorphicModes
        {
            get
            {
                return new List<Anamorphic> { Anamorphic.None, Anamorphic.Strict, Anamorphic.Loose, Anamorphic.Custom };
            }
        }

        /// <summary>
        /// Gets or sets CropBottom.
        /// </summary>
        public int CropBottom
        {
            get
            {
                return this.cropBottom;
            }
            set
            {
                this.cropBottom = this.CorrectForModulus(this.cropBottom, value);
                this.NotifyOfPropertyChange(() => this.CropBottom);
            }
        }

        /// <summary>
        /// Gets or sets CropLeft.
        /// </summary>
        public int CropLeft
        {
            get
            {
                return this.cropLeft;
            }
            set
            {
                this.cropLeft = this.CorrectForModulus(this.cropLeft, value);
                this.NotifyOfPropertyChange(() => this.CropLeft);
            }
        }

        /// <summary>
        /// Gets or sets CropRight.
        /// </summary>
        public int CropRight
        {
            get
            {
                return this.cropRight;
            }
            set
            {
                this.cropRight = this.CorrectForModulus(this.cropRight, value);
                this.NotifyOfPropertyChange(() => this.CropRight);
            }
        }

        /// <summary>
        /// Gets or sets CropTop.
        /// </summary>
        public int CropTop
        {
            get
            {
                return this.cropTop;
            }
            set
            {
                this.cropTop = this.CorrectForModulus(this.cropTop, value);
                this.NotifyOfPropertyChange(() => this.CropTop);
            }
        }

        /// <summary>
        /// Gets or sets DisplaySize.
        /// </summary>
        public string DisplaySize
        {
            get
            {
                return this.displaySize;
            }
            set
            {
                this.displaySize = value;
                this.NotifyOfPropertyChange(() => this.DisplaySize);
            }
        }

        /// <summary>
        /// Gets or sets DisplayWidth.
        /// </summary>
        public int DisplayWidth
        {
            get
            {
                return this.displayWidth;
            }
            set
            {
                this.displayWidth = value;
                this.CustomAnamorphicAdjust();
                this.NotifyOfPropertyChange(() => this.DisplayWidth);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether IsCustomCrop.
        /// </summary>
        public bool IsCustomCrop
        {
            get
            {
                return this.isCustomCrop;
            }
            set
            {
                this.isCustomCrop = value;
                this.NotifyOfPropertyChange(() => this.IsCustomCrop);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether MaintainAspectRatio.
        /// </summary>
        public bool MaintainAspectRatio
        {
            get
            {
                return this.maintainAspectRatio;
            }
            set
            {
                this.maintainAspectRatio = value;
                this.WidthAdjust();
                this.NotifyOfPropertyChange(() => this.MaintainAspectRatio);
            }
        }

        /// <summary>
        /// Gets ModulusValues.
        /// </summary>
        public IEnumerable<int> ModulusValues
        {
            get
            {
                return new List<int> { 16, 8, 4, 2 };
            }
        }

        /// <summary>
        /// Gets or sets ParHeight.
        /// </summary>
        public int ParHeight
        {
            get
            {
                return this.parHeight;
            }
            set
            {
                this.parHeight = value;
                this.CustomAnamorphicAdjust();
                this.NotifyOfPropertyChange(() => this.ParHeight);
            }
        }

        /// <summary>
        /// Gets or sets ParWidth.
        /// </summary>
        public int ParWidth
        {
            get
            {
                return this.parWidth;
            }
            set
            {
                this.parWidth = value;
                this.CustomAnamorphicAdjust();
                this.NotifyOfPropertyChange(() => this.ParWidth);
            }
        }

        /// <summary>
        /// Gets or sets SelectedAnamorphicMode.
        /// </summary>
        public Anamorphic SelectedAnamorphicMode
        {
            get
            {
                return this.selectedAnamorphicMode;
            }
            set
            {
                this.selectedAnamorphicMode = value;
                this.AnamorphicAdjust();
                this.NotifyOfPropertyChange(() => this.SelectedAnamorphicMode);
            }
        }

        /// <summary>
        /// Gets or sets SelectedModulus.
        /// </summary>
        public int SelectedModulus
        {
            get
            {
                return this.selectedModulus;
            }
            set
            {
                this.selectedModulus = value;
                this.ModulusAdjust();
                this.NotifyOfPropertyChange(() => this.SelectedModulus);
            }
        }

        /// <summary>
        /// Gets or sets SourceInfo.
        /// </summary>
        public string SourceInfo
        {
            get
            {
                return this.sourceInfo;
            }
            set
            {
                this.sourceInfo = value;
                this.NotifyOfPropertyChange(() => this.SourceInfo);
            }
        }

        /// <summary>
        /// Gets or sets Width.
        /// </summary>
        public int Width
        {
            get
            {
                return this.width;
            }
            set
            {
                this.width = value;
                this.WidthAdjust();
                this.NotifyOfPropertyChange(() => this.Width);
            }
        }

        /// <summary>
        /// Gets or sets Height.
        /// </summary>
        public int Height
        {
            get
            {
                return this.height;
            }
            set
            {
                this.height = value;
                this.HeightAdjust();
                this.NotifyOfPropertyChange(() => this.Height);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether ShowCustomAnamorphicControls.
        /// </summary>
        public bool ShowCustomAnamorphicControls
        {
            get
            {
                return this.showCustomAnamorphicControls;
            }
            set
            {
                this.showCustomAnamorphicControls = value;
                this.NotifyOfPropertyChange(() => ShowCustomAnamorphicControls);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether HeightControlEnabled.
        /// </summary>
        public bool HeightControlEnabled
        {
            get
            {
                return this.heightControlEnabled;
            }
            set
            {
                this.heightControlEnabled = value;
                this.NotifyOfPropertyChange(() => HeightControlEnabled);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether WidthControlEnabled.
        /// </summary>
        public bool WidthControlEnabled
        {
            get
            {
                return this.widthControlEnabled;
            }
            set
            {
                this.widthControlEnabled = value;
                this.NotifyOfPropertyChange(() => WidthControlEnabled);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Setup this window for a new source
        /// </summary>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="preset">
        /// The preset.
        /// </param>
        /// <param name="task">
        /// The task.
        /// </param>
        public void SetSource(Title title, Preset preset, EncodeTask task)
        {
            if (title != null)
            {
                // Set cached info
                this.sourceAspectRatio = title.AspectRatio;
                this.sourceParValues = title.ParVal;
                this.sourceResolution = title.Resolution;

                // Set Screen Controls
                this.SourceInfo = string.Format("{0}x{1}, Aspect Ratio: {2:0.00}", title.Resolution.Width, title.Resolution.Height, title.AspectRatio);
                this.CropTop = title.AutoCropDimensions.Top;
                this.CropBottom = title.AutoCropDimensions.Bottom;
                this.CropLeft = title.AutoCropDimensions.Left;
                this.CropRight = title.AutoCropDimensions.Right;

                // TODO handle preset max width / height
                this.Width = title.Resolution.Width;
                this.Height = title.Resolution.Height;
                this.MaintainAspectRatio = true;
            }
        }
        #endregion

        /// <summary>
        /// Adjust other values after the user has altered the width
        /// </summary>
        private void WidthAdjust()
        {
            if (this.width > this.sourceResolution.Width)
            {
                this.width = this.sourceResolution.Width;
            }

            switch (SelectedAnamorphicMode)
            {
                case Anamorphic.None:
                    if (this.MaintainAspectRatio)
                    {
                        double crop_width = this.sourceResolution.Width - this.CropLeft - this.CropRight;
                        double crop_height = this.sourceResolution.Height - this.CropTop - this.CropBottom;

                        if (SourceAspect.Width == 0 && SourceAspect.Height == 0)
                            break;

                        double newHeight = ((double)this.Width * this.sourceResolution.Width * SourceAspect.Height * crop_height) /
                                           (this.sourceResolution.Height * SourceAspect.Width * crop_width);

                        this.height = (int)Math.Round(GetModulusValue(newHeight), 0);
                        this.NotifyOfPropertyChange("Height");
                    }
                    break;
                case Anamorphic.Strict:
                    this.width = 0;
                    this.height = 0;

                    this.NotifyOfPropertyChange("Width");
                    this.NotifyOfPropertyChange("Height");
                    this.SetDisplaySize();
                    break;
                case Anamorphic.Loose:
                    this.height = 0;
                    this.NotifyOfPropertyChange("Width");
                    this.NotifyOfPropertyChange("Height");
                    this.SetDisplaySize();
                    break;
                case Anamorphic.Custom:
                    this.SetDisplaySize();
                    break;
            }
        }

        /// <summary>
        /// Adjust other values after the user has altered the height
        /// </summary>
        private void HeightAdjust()
        {
            if (this.height > this.sourceResolution.Height)
            {
                this.height = this.sourceResolution.Height;
            }

            switch (SelectedAnamorphicMode)
            {
                case Anamorphic.None:
                    if (this.MaintainAspectRatio)
                    {
                        double crop_width = this.sourceResolution.Width - this.CropLeft - this.CropRight;
                        double crop_height = this.sourceResolution.Height - this.CropTop - this.CropBottom;

                        double new_width = ((double)this.Height * this.sourceResolution.Height * SourceAspect.Width * crop_width) /
                                           (this.sourceResolution.Width * SourceAspect.Height * crop_height);

                        this.Width = (int)Math.Round(GetModulusValue(new_width), 0);
                        this.NotifyOfPropertyChange("Width");
                    }
                    break;
                case Anamorphic.Custom:
                    this.SetDisplaySize();
                    break;
            }
        }

        /// <summary>
        /// Adjust other values after the user has altered one of the custom anamorphic settings
        /// </summary>
        private void CustomAnamorphicAdjust()
        {
            this.SetDisplaySize();
        }

        /// <summary>
        /// Adjust other values after the user has altered the modulus
        /// </summary>
        private void ModulusAdjust()
        {
            this.WidthAdjust();
        }

        /// <summary>
        /// Adjust other values after the user has altered the anamorphic.
        /// </summary>
        private void AnamorphicAdjust()
        {
            this.DisplaySize = this.sourceResolution.IsEmpty
                                   ? "No Title Selected"
                                   : string.Format("{0}x{1}",
                                       this.CalculateAnamorphicSizes().Width,
                                       this.CalculateAnamorphicSizes().Height);

            switch (SelectedAnamorphicMode)
            {
                case Anamorphic.None:
                    this.WidthControlEnabled = true;
                    this.HeightControlEnabled = true;
                    this.ShowCustomAnamorphicControls = false;
                    this.Width = sourceResolution.Width;
                    this.SetDisplaySize();
                    break;
                case Anamorphic.Strict:
                    this.WidthControlEnabled = false;
                    this.HeightControlEnabled = false;
                    this.ShowCustomAnamorphicControls = false;

                    this.width = 0;
                    this.height = 0;
                    this.NotifyOfPropertyChange(() => Width);
                    this.NotifyOfPropertyChange(() => Height);
                    this.SetDisplaySize();
                    break;

                case Anamorphic.Loose:
                    this.WidthControlEnabled = true;
                    this.HeightControlEnabled = false;
                    this.ShowCustomAnamorphicControls = false;

                    this.width = this.sourceResolution.Width;
                    this.height = 0;
                    this.NotifyOfPropertyChange(() => Width);
                    this.NotifyOfPropertyChange(() => Height);
                    this.SetDisplaySize();
                    break;

                case Anamorphic.Custom:
                    this.WidthControlEnabled = true;
                    this.HeightControlEnabled = true;
                    this.ShowCustomAnamorphicControls = true;

                    this.width = this.sourceResolution.Width;
                    this.height = 0;
                    this.NotifyOfPropertyChange(() => Width);
                    this.NotifyOfPropertyChange(() => Height);

                    this.displayWidth = this.CalculateAnamorphicSizes().Width;
                    this.parWidth = this.sourceParValues.Width;
                    this.parHeight = this.sourceParValues.Height;
                    this.NotifyOfPropertyChange(() => ParHeight);
                    this.NotifyOfPropertyChange(() => ParWidth);
                    this.NotifyOfPropertyChange(() => DisplayWidth);

                    this.SetDisplaySize();
                    break;
            }
        }

        /// <summary>
        /// Gets SourceAspect.
        /// </summary>
        private Size SourceAspect
        {
            get
            {
                // display aspect = (width * par_width) / (height * par_height)
                return new Size((this.sourceParValues.Width * this.sourceResolution.Width),
                                (this.sourceParValues.Height * this.sourceResolution.Height));
            }
        }

        /// <summary>
        /// Set the display size text
        /// </summary>
        private void SetDisplaySize()
        {
            this.DisplaySize = this.sourceResolution.IsEmpty
                                ? "No Title Selected"
                                : string.Format("{0}x{1}",
                                    this.CalculateAnamorphicSizes().Width,
                                    this.CalculateAnamorphicSizes().Height);
        }

        /// <summary>
        /// Calculate the Anamorphic Resolution for the selected title.
        /// </summary>
        /// <returns>
        /// A Size With Width/Height for this title.
        /// </returns>
        private Size CalculateAnamorphicSizes()
        {
            if (this.sourceResolution.IsEmpty)
            {
                return new Size(0, 0);
            }

            /* Set up some variables to make the math easier to follow. */
            int croppedWidth = this.sourceResolution.Width - this.CropLeft - this.CropRight;
            int croppedHeight = this.sourceResolution.Height - this.CropTop - this.CropBottom;
            double storageAspect = (double)croppedWidth / croppedHeight;

            /* Figure out what width the source would display at. */
            double sourceDisplayWidth = (double)croppedWidth * this.sourceParValues.Width / this.sourceParValues.Height;

            /*
                 3 different ways of deciding output dimensions:
                  - 1: Strict anamorphic, preserve source dimensions
                  - 2: Loose anamorphic, round to mod16 and preserve storage aspect ratio
                  - 3: Power user anamorphic, specify everything
              */
            double calcWidth, calcHeight;
            switch (this.SelectedAnamorphicMode)
            {
                default:
                case Anamorphic.Strict:
                    /* Strict anamorphic */
                    double dispWidth = ((double)croppedWidth * this.sourceParValues.Width / this.sourceParValues.Height);
                    dispWidth = Math.Round(dispWidth, 0);
                    Size output = new Size((int)dispWidth, croppedHeight);
                    return output;

                case Anamorphic.Loose:
                    /* "Loose" anamorphic.
                        - Uses mod16-compliant dimensions,
                        - Allows users to set the width
                    */
                    calcWidth = GetModulusValue(this.Width); /* Time to get picture width that divide cleanly.*/
                    calcHeight = (calcWidth / storageAspect) + 0.5;
                    calcHeight = GetModulusValue(calcHeight); /* Time to get picture height that divide cleanly.*/

                    /* The film AR is the source's display width / cropped source height.
                       The output display width is the output height * film AR.
                       The output PAR is the output display width / output storage width. */
                    double pixelAspectWidth = calcHeight * sourceDisplayWidth / croppedHeight;
                    double pixelAspectHeight = calcWidth;

                    double disWidthLoose = (calcWidth * pixelAspectWidth / pixelAspectHeight);
                    if (double.IsNaN(disWidthLoose))
                        disWidthLoose = 0;

                    return new Size((int)disWidthLoose, (int)calcHeight);

                case Anamorphic.Custom:
                    // Get the User Interface Values
                    double UIdisplayWidth;
                    double.TryParse(this.DisplayWidth.ToString(), out UIdisplayWidth);

                    /* Anamorphic 3: Power User Jamboree - Set everything based on specified values */
                    calcHeight = GetModulusValue(this.Height);

                    if (this.MaintainAspectRatio)
                        return new Size((int)Math.Truncate(UIdisplayWidth), (int)calcHeight);

                    return new Size((int)Math.Truncate(UIdisplayWidth), (int)calcHeight);
            }
        }

        /// <summary>
        /// For a given value, correct so that it matches the users currently selected modulus value
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// Value corrected so that value % selected modulus == 0
        /// </returns>
        private double GetModulusValue(double value)
        {
            double remainder = value % this.SelectedModulus;

            if (remainder == 0)
                return value;

            return remainder >= ((double)this.SelectedModulus / 2) ? value + (this.SelectedModulus - remainder) : value - remainder;
        }

        /// <summary>
        /// Correct the new value so that the result of the modulus of that value is 0
        /// </summary>
        /// <param name="oldValue">
        /// The old value.
        /// </param>
        /// <param name="newValue">
        /// The new value.
        /// </param>
        /// <returns>
        /// The Value corrected so that for a given modulus the result is 0
        /// </returns>
        private int CorrectForModulus(int oldValue, int newValue)
        {
            int remainder = newValue % 2;
            if (remainder == 0)
            {
                return newValue;
            }

            return newValue > oldValue ? newValue + remainder : newValue - remainder;
        }
    }
}
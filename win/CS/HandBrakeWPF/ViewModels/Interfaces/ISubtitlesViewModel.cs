﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISubtitlesViewModel.cs" company="HandBrake Project (http://handbrake.fr)">
//   This file is part of the HandBrake source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Defines the ISubtitlesViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandBrakeWPF.ViewModels.Interfaces
{
    using HandBrake.ApplicationServices.Model;
    using HandBrake.ApplicationServices.Parsing;

    /// <summary>
    /// The Subtiles View Model Interface
    /// </summary>
    public interface ISubtitlesViewModel
    {
        /// <summary>
        /// Set the selected preset
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
        void SetSource(Title title, Preset preset, EncodeTask task);
    }
}

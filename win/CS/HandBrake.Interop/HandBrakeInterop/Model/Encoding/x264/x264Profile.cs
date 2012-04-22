﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="x264Profile.cs" company="HandBrake Project (http://handbrake.fr)">
//   This file is part of the HandBrake source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The X264 Profile
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandBrake.Interop.Model.Encoding.x264
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The X264 Profile
    /// </summary>
    public enum x264Profile
    {
        [Display(Name = "None")]
        None = 0,

        [Display(Name = "Baseline")]
        Baseline,

        [Display(Name = "Main")]
        Main,

        [Display(Name = "High")]
        High,

        [Display(Name = "High 10")]
        High10,

        [Display(Name = "High 422")]
        High422,

        [Display(Name = "High 444")]
        High444,
    }
}

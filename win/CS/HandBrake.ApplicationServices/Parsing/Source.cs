// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Source.cs" company="HandBrake Project (http://handbrake.fr)">
//   This file is part of the HandBrake source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   An object representing a scanned DVD
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandBrake.ApplicationServices.Parsing
{
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;

    using HandBrake.ApplicationServices.Services.Interfaces;

    /// <summary>
    /// An object representing a scanned DVD
    /// </summary>
    [DataContract]
    public class Source
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Source"/> class. 
        /// Default constructor for this object
        /// </summary>
        public Source()
        {
            Titles = new List<Title>();
        }

        /// <summary>
        /// Gets or sets ScanPath.
        /// The Path used by the Scan Service.
        /// </summary>
        [DataMember]
        public string ScanPath { get; set; }

        /// <summary>
        /// Gets or sets Titles. A list of titles from the source
        /// </summary>
        [DataMember]
        public List<Title> Titles { get; set; }

        /// <summary>
        /// Parse the StreamReader output into a List of Titles
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        /// <param name="isDvdNavDisabled">
        /// The is Dvd Nav Disabled.
        /// </param>
        /// <returns>
        /// A DVD object which contains a list of title inforamtion
        /// </returns>
        public static Source Parse(StreamReader output, bool isDvdNavDisabled)
        {
            var thisDVD = new Source();

            while (!output.EndOfStream)
            {
                if ((char) output.Peek() == '+')
                    thisDVD.Titles.AddRange(Title.ParseList(output.ReadToEnd(), isDvdNavDisabled));
                else
                    output.ReadLine();
            }

            return thisDVD;
        }

        /// <summary>
        /// Copy this Source to another Source Model
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        public void CopyTo(Source source)
        {
            source.Titles = this.Titles;
            source.ScanPath = this.ScanPath;
        }
    }
}
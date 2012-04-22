/*  Common.cs $
 	
 	   This file is part of the HandBrake source code.
 	   Homepage: <http://handbrake.fr>.
 	   It may be used under the terms of the GNU General Public License. */

using System;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Handbrake.Functions
{
    class Main
    {
        /// <summary>
        /// Calculate the duration of the selected title and chapters
        /// </summary>
        public TimeSpan calculateDuration(string chapter_start, string chapter_end, Parsing.Title selectedTitle)
        {
            TimeSpan Duration = TimeSpan.FromSeconds(0.0);

            // Get the durations between the 2 chapter points and add them together.
            if (chapter_start != "Auto" && chapter_end != "Auto")
            {
                int start_chapter, end_chapter = 0;
                int.TryParse(chapter_start, out start_chapter);
                int.TryParse(chapter_end, out end_chapter);

                int position = start_chapter - 1;

                if (start_chapter <= end_chapter)
                {
                    if (end_chapter > selectedTitle.Chapters.Count)
                        end_chapter = selectedTitle.Chapters.Count;

                    while (position != end_chapter)
                    {
                        TimeSpan dur = selectedTitle.Chapters[position].Duration;
                        Duration = Duration + dur;
                        position++;
                    }
                }
            }
            return Duration;
        }

        /// <summary>
        /// Calculate the non-anamorphic resoltuion of the source
        /// </summary>
        /// <param name="width"></param>
        /// <returns></returns>
        public int cacluateNonAnamorphicHeight(int width, decimal top, decimal bottom, decimal left, decimal right, Parsing.Title selectedTitle)
        {
            float aspect = selectedTitle.AspectRatio;
            int aw;
            int ah;
            if (aspect.ToString() == "1.78")
            {
                aw = 16;
                ah = 9;
            }
            else
            {
                aw = 4;
                ah = 3;
            }

            double a = width * selectedTitle.Resolution.Width * ah * (selectedTitle.Resolution.Height - (double)top - (double)bottom);
            double b = selectedTitle.Resolution.Height * aw * (selectedTitle.Resolution.Width - (double)left - (double)right);

            double y = a / b;

            // If it's not Mod 16, make it mod 16
            if ((y % 16) != 0)
            {
                double mod16 = y % 16;
                if (mod16 >= 8)
                {
                    mod16 = 16 - mod16;
                    y = y + mod16;
                }
                else
                {
                    y = y - mod16;
                }
            }

            //16 * (421 / 16)
            //double z = ( 16 * (( y + 8 ) / 16 ) );
            int x = int.Parse(y.ToString());
            return x;
        }

        /// <summary>
        /// Select the longest title in the DVD title dropdown menu on frmMain
        /// </summary>
        public Handbrake.Parsing.Title selectLongestTitle(ComboBox drp_dvdtitle)
        {
            int current_largest = 0;
            Handbrake.Parsing.Title title2Select;

            // Check if there are titles in the DVD title dropdown menu and make sure, it's not just "Automatic"
            if (drp_dvdtitle.Items[0].ToString() != "Automatic")
                title2Select = (Handbrake.Parsing.Title)drp_dvdtitle.Items[0];
            else
                title2Select = null;

            // So, If there are titles in the DVD Title dropdown menu, lets select the longest.
            if (title2Select != null)
            {
                foreach (Handbrake.Parsing.Title x in drp_dvdtitle.Items)
                {
                    string title = x.ToString();
                    if (title != "Automatic")
                    {
                        string[] y = title.Split(' ');
                        string time = y[1].Replace("(", "").Replace(")", "");
                        string[] z = time.Split(':');

                        int hours = int.Parse(z[0]) * 60 * 60;
                        int minutes = int.Parse(z[1]) * 60;
                        int seconds = int.Parse(z[2]);
                        int total_sec = hours + minutes + seconds;

                        if (current_largest == 0)
                        {
                            current_largest = hours + minutes + seconds;
                            title2Select = x;
                        }
                        else
                        {
                            if (total_sec > current_largest)
                            {
                                current_largest = total_sec;
                                title2Select = x;
                            }
                        }
                    }
                }
            }
            return title2Select;
        }

        /// <summary>
        /// Set's up the DataGridView on the Chapters tab (frmMain)
        /// </summary>
        /// <param name="mainWindow"></param>
        public DataGridView chapterNaming(DataGridView data_chpt, string chapter_start, string chapter_end)
        {
            try
            {
                int i = 0;
                int rowCount = 0;
                int start = 0;
                int finish = 0;
                if (chapter_end != "Auto")
                    finish = int.Parse(chapter_end);

                if (chapter_start != "Auto")
                    start = int.Parse(chapter_start);

                rowCount = finish - (start - 1);

                while (i < rowCount)
                {
                    DataGridViewRow row = new DataGridViewRow();

                    data_chpt.Rows.Insert(i, row);
                    data_chpt.Rows[i].Cells[0].Value = (i + 1);
                    data_chpt.Rows[i].Cells[1].Value = "Chapter " + (i + 1);
                    i++;
                }
                return data_chpt;
            }
            catch (Exception exc)
            {
                MessageBox.Show("chapterNaming() Error has occured: \n" + exc.ToString());
                return null;
            }
        }

        /// <summary>
        /// Function which generates the filename and path automatically based on 
        /// the Source Name, DVD title and DVD Chapters
        /// </summary>
        /// <param name="mainWindow"></param>
        public string autoName(ComboBox drp_dvdtitle, string chapter_start, string chatper_end, string source, string dest, int format)
        {

            string AutoNamePath = string.Empty;

            if (drp_dvdtitle.Text != "Automatic")
            {
                // Todo: This code is a tad messy. Clean it up at some point.
                // Get the Source Name
                string[] sourceName = source.Split('\\');
                source = sourceName[sourceName.Length - 1].Replace(".iso", "").Replace(".mpg", "").Replace(".ts", "").Replace(".ps", "");

                // Get the Selected Title Number
                string title = drp_dvdtitle.Text;
                string[] titlesplit = title.Split(' ');
                title = titlesplit[0];

                // Get the Chapter Start and Chapter End Numbers
                string cs = chapter_start;
                string cf = chatper_end;

                // Just incase the above are set to their default Automatic values, set the varible to ""
                if (title == "Automatic")
                    title = "";
                if (cs == "Auto")
                    cs = "";
                if (cf == "Auto")
                    cf = "";

                // If both CS and CF are populated, set the dash varible
                string dash = "";
                if (cf != "Auto")
                    dash = "-";

                // Get the destination filename.
                string destination_filename = "";
                if (Properties.Settings.Default.autoNameFormat != "")
                {
                    destination_filename = Properties.Settings.Default.autoNameFormat;
                    destination_filename = destination_filename.Replace("{source}", source).Replace("{title}", title).Replace("{chapters}", cs + dash + cf);
                }
                else
                    destination_filename = source + "_T" + title + "_C" + cs + dash + cf;

                // If the text box is blank
                if (!dest.Contains("\\"))
                {
                    string filePath = "";
                    if (Properties.Settings.Default.autoNamePath.Trim() != "")
                    {
                        if (Properties.Settings.Default.autoNamePath.Trim() != "Click 'Browse' to set the default location")
                            filePath = Properties.Settings.Default.autoNamePath + "\\";
                    }

                    if (format == 0)
                        AutoNamePath = filePath + destination_filename + ".mp4";
                    else if (format == 1)
                        AutoNamePath = filePath + destination_filename + ".m4v";
                    else if (format == 2)
                        AutoNamePath = filePath + destination_filename + ".mkv";
                    else if (format == 3)
                        AutoNamePath = filePath + destination_filename + ".avi";
                    else if (format == 4)
                        AutoNamePath = filePath + destination_filename + ".ogm";
                }
                else // If the text box already has a path and file
                {
                    string destination = AutoNamePath;
                    string[] destName = dest.Split('\\');
                    string[] extension = dest.Split('.');
                    string ext = extension[extension.Length - 1];

                    destName[destName.Length - 1] = destination_filename + "." + ext;

                    string fullDest = "";
                    foreach (string part in destName)
                    {
                        if (fullDest != "")
                            fullDest = fullDest + "\\" + part;
                        else
                            fullDest = fullDest + part;
                    }
                    return fullDest;
                }
            }
            return AutoNamePath;
        }

        /// <summary>
        /// Checks for updates and returns true if an update is available.
        /// </summary>
        /// <param name="debug">Turns on debug mode. Don't use on program startup</param>
        /// <returns>Boolean True = Update available</returns>
        public Boolean updateCheck(Boolean debug)
        {
            try
            {
                Functions.AppcastReader rssRead = new Functions.AppcastReader();
                rssRead.getInfo(); // Initializes the class.
                string build = rssRead.build();

                int latest = int.Parse(build);
                int current = Properties.Settings.Default.hb_build;
                int skip = Properties.Settings.Default.skipversion;

                if (latest == skip)
                    return false;
                else
                {
                    Boolean update = (latest > current);
                    return update;
                }
            }
            catch (Exception exc)
            {
                if (debug == true)
                    MessageBox.Show("Unable to check for updates, Please try again later. \n" + exc.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Get's HandBrakes version data from the CLI.
        /// </summary>
        /// <returns>Arraylist of Version Data. 0 = hb_version 1 = hb_build</returns>
        public ArrayList getCliVersionData()
        {
            ArrayList cliVersionData = new ArrayList();
            // 0 = SVN Build / Version
            // 1 = Build Date

            Process cliProcess = new Process();
            ProcessStartInfo handBrakeCLI = new ProcessStartInfo("HandBrakeCLI.exe", " -u");
            handBrakeCLI.UseShellExecute = false;
            handBrakeCLI.RedirectStandardError = true;
            handBrakeCLI.RedirectStandardOutput = true;
            handBrakeCLI.CreateNoWindow = true;
            cliProcess.StartInfo = handBrakeCLI;
            cliProcess.Start();

            // Retrieve standard output and report back to parent thread until the process is complete
            String line;
            TextReader stdOutput = cliProcess.StandardError;

            while (!cliProcess.HasExited)
            {
                line = stdOutput.ReadLine();
                if (line == null) line = "";
                Match m = Regex.Match(line, @"HandBrake ([0-9\.]*)*(svn[0-9]*[M]*)* \([0-9]*\)");

                if (m.Success != false)
                {
                    string data = line.Replace("(", "").Replace(")", "").Replace("HandBrake ", "");
                    string[] arr = data.Split(' ');
                    cliVersionData.Add(arr[0]);
                    cliVersionData.Add(arr[1]);
                    return cliVersionData;
                }
            }
            return null;
        }

        /// <summary>
        /// Check if the queue recovery file contains records.
        /// If it does, it means the last queue did not complete before HandBrake closed.
        /// So, return a boolean if true. 
        /// </summary>
        public Boolean check_queue_recovery()
        {
            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), "hb_queue_recovery.dat");
                using (StreamReader reader = new StreamReader(tempPath))
                {
                    string queue_item = reader.ReadLine();
                    if (queue_item == null)
                    {
                        reader.Close();
                        reader.Dispose();
                        return false;
                    }
                    else // There exists an item in the recovery queue file, so try and recovr it.
                    {
                        reader.Close();
                        reader.Dispose();
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // Keep quiet about the error.
                return false;
            }
        }

    }
}
/*  frmOptions.cs $
 	
 	   This file is part of the HandBrake source code.
 	   Homepage: <http://handbrake.fr>.
 	   It may be used under the terms of the GNU General Public License. */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Handbrake
{
    public partial class frmOptions : Form
    {
        private frmMain mainWindow;
        /// <summary>
        /// When the form loads, Initialise all the setting components with their correct values
        /// </summary>
        public frmOptions(frmMain window)
        {
            InitializeComponent();
            mainWindow = window;

            // #############################
            // General
            // #############################

            // Enable Tooltips.
            if (Properties.Settings.Default.tooltipEnable == "Checked")
            {
                check_tooltip.CheckState = CheckState.Checked;
                ToolTip.Active = true;
            }

            // Setup Widgets to match settings.
            if (Properties.Settings.Default.updateStatus == "Checked")
                check_updateCheck.CheckState = CheckState.Checked;

            // enable loading of default user settings.
            if (Properties.Settings.Default.defaultSettings == "Checked")
                check_userDefaultSettings.CheckState = CheckState.Checked;

            // On Encode Completeion Action
            drp_completeOption.Text = Properties.Settings.Default.CompletionOption;
            
            // Enable auto naming feature.
            if (Properties.Settings.Default.autoNaming == "Checked")
                check_autoNaming.CheckState = CheckState.Checked;

            // Store the auto name path
            text_an_path.Text = Properties.Settings.Default.autoNamePath;
            if (text_an_path.Text == string.Empty)
                text_an_path.Text = "Click 'Browse' to set the default location";

            // Store auto name format
            txt_autoNameFormat.Text = Properties.Settings.Default.autoNameFormat;

            // #############################
            // Picture Tab
            // #############################

            // Store decomb value string
            txt_decomb.Text = Properties.Settings.Default.decomb;

            // #############################
            // CLI
            // #############################

            // Enable Start CLI minimized
            if (Properties.Settings.Default.cli_minimized == "Checked")
                check_cli_minimized.CheckState = CheckState.Checked;

            // Number of processor cores
            drp_processors.Text = Properties.Settings.Default.Processors;

            // Priority level for encodes
            drp_Priority.Text = Properties.Settings.Default.processPriority;

            // Save individual log files
            if (Properties.Settings.Default.saveLog == "Checked")
                check_keepLogs.CheckState = CheckState.Checked;

            // Save logs in the same directory as encoded files
            if (Properties.Settings.Default.saveLogWithVideo == "Checked")
                check_saveLogWithVideo.CheckState = CheckState.Checked;

            // The saved log path
            text_logPath.Text = Properties.Settings.Default.saveLogPath;


            // #############################
            // Advanced
            // #############################

            // Unstable Snapshot checking should only be visible for stable builds.
            if (Properties.Settings.Default.hb_build.ToString().EndsWith("1"))
            {
                lbl_appcastUnstable.Visible = false;
                check_snapshot.Visible = false;
            }

            // Enable snapshot updating
            if (Properties.Settings.Default.checkSnapshot == "Checked")
                check_snapshot.CheckState = CheckState.Checked;

            // Enable GUI DVD Drive detection code
            if (Properties.Settings.Default.drive_detection == "Checked")
                btn_drive_detect.CheckState = CheckState.Checked;

        }

        #region General
        private void check_updateCheck_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.updateStatus = check_updateCheck.CheckState.ToString();
        }

        private void check_userDefaultSettings_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.defaultSettings = check_userDefaultSettings.CheckState.ToString();
        }

        private void check_tooltip_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.tooltipEnable = check_tooltip.CheckState.ToString();
            if (check_tooltip.Checked)
            {
                ToolTip.Active = true;
                mainWindow.ToolTip.Active = true;
            }
            else
            {
                ToolTip.Active = false;
                mainWindow.ToolTip.Active = false;
            }
        }
        private void drp_completeOption_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.CompletionOption = drp_completeOption.Text;
        }

        private void check_autoNaming_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.autoNaming = check_autoNaming.CheckState.ToString();
        }

        private void txt_autoNameFormat_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.autoNameFormat = txt_autoNameFormat.Text;
        }

        private void btn_browse_Click(object sender, EventArgs e)
        {
            pathFinder.ShowDialog();
            text_an_path.Text = pathFinder.SelectedPath;
        }

        private void text_an_path_TextChanged(object sender, EventArgs e)
        {
            if (text_an_path.Text == string.Empty)
            {
                Properties.Settings.Default.autoNamePath = "";
                text_an_path.Text = "Click 'Browse' to set the default location";
            }
            else
                Properties.Settings.Default.autoNamePath = text_an_path.Text;
        }
        #endregion

        #region Picture
        private void txt_decomb_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.decomb = txt_decomb.Text;
        }
        #endregion

        #region CLI
        private void check_cli_minimized_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.cli_minimized = check_cli_minimized.CheckState.ToString();
        }

        private void drp_processors_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Processors = drp_processors.Text;
        }

        private void drp_Priority_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.processPriority = drp_Priority.Text;
        }

        private void check_keepLogs_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.saveLog = check_keepLogs.CheckState.ToString();
        }

        private void check_saveLogWithVideo_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.saveLogWithVideo = check_saveLogWithVideo.CheckState.ToString();
            if (check_saveLogWithVideo.Checked == true)
                text_logPath.Text = "";
        }

        private void btn_saveLog_Click(object sender, EventArgs e)
        {
            pathFinder.SelectedPath = String.Empty;
            pathFinder.ShowDialog();
            if (pathFinder.SelectedPath != string.Empty)
                text_logPath.Text = pathFinder.SelectedPath;
        }

        private void text_logPath_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.saveLogPath = text_logPath.Text;
            if (text_logPath.Text != String.Empty)
                check_saveLogWithVideo.Checked = false;
        }

        #endregion

        #region Advanced
        private void btn_drive_detect_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.drive_detection = btn_drive_detect.CheckState.ToString();
        }

        private void check_snapshot_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.checkSnapshot = check_snapshot.CheckState.ToString();
        }
        #endregion

        private void btn_close_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save(); // Small hack for Vista. Seems to work fine on XP without this
            this.Close();
        }
    }
}
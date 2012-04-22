/*  frmReadDVD.cs $
 	
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
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections;


namespace Handbrake
{
    public partial class frmReadDVD : Form
    {
        private string inputFile;
        private frmMain mainWindow;
        private Parsing.DVD thisDvd;
        private delegate void UpdateUIHandler();
        Process hbproc;
        Functions.Main hb_common_func = new Functions.Main();
        Functions.Encode process = new Functions.Encode();

        public frmReadDVD(string inputFile, frmMain parent)
        {
            InitializeComponent();
            this.inputFile = inputFile;
            this.mainWindow = parent;
            startScan();
        }

        private void startScan()
        {
            try
            {
                lbl_status.Visible = true;
                ThreadPool.QueueUserWorkItem(startProc);
            }
            catch (Exception exc)
            {
                MessageBox.Show("frmReadDVD.cs - startScan " + exc.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void startProc(object state)
        {
            try
            {
                string handbrakeCLIPath = Path.Combine(Application.StartupPath, "HandBrakeCLI.exe");
                string dvdInfoPath = Path.Combine(Path.GetTempPath(), "dvdinfo.dat");

                // Make we don't pick up a stale hb_encode_log.dat (and that we have rights to the file)
                if (File.Exists(dvdInfoPath))
                    File.Delete(dvdInfoPath);

                string strCmdLine = String.Format(@"cmd /c """"{0}"" -i ""{1}"" -t0 -v >""{2}"" 2>&1""", handbrakeCLIPath, inputFile, dvdInfoPath);

                ProcessStartInfo hbParseDvd = new ProcessStartInfo("CMD.exe", strCmdLine);
                hbParseDvd.WindowStyle = ProcessWindowStyle.Hidden;

                using (hbproc = Process.Start(hbParseDvd))
                {
                    hbproc.WaitForExit();
                }

                if (!File.Exists(dvdInfoPath))
                {
                    throw new Exception("Unable to retrieve the DVD Info. dvdinfo.dat is missing. \nExpected location of dvdinfo.dat: \n" + dvdInfoPath);
                }

                using (StreamReader sr = new StreamReader(dvdInfoPath))
                {
                    thisDvd = Parsing.DVD.Parse(sr);
                    sr.Close();
                    sr.Dispose();
                }

                updateUIElements();
            }
            catch (Exception exc)
            {
                MessageBox.Show("frmReadDVD.cs - startProc() " + exc.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                closeWindowAfterError();
            }
        }

        private void updateUIElements()
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new UpdateUIHandler(updateUIElements));
                    return;
                }
                // Now pass this streamreader to frmMain so that it can be used there.
                mainWindow.setStreamReader(thisDvd);

                mainWindow.drp_dvdtitle.Items.Clear();
                if (thisDvd.Titles.Count != 0)
                    mainWindow.drp_dvdtitle.Items.AddRange(thisDvd.Titles.ToArray());
                mainWindow.drp_dvdtitle.Text = "Automatic";
                mainWindow.drop_chapterFinish.Text = "Auto";
                mainWindow.drop_chapterStart.Text = "Auto";

                // Now select the longest title
                if (thisDvd.Titles.Count != 0)
                    mainWindow.drp_dvdtitle.SelectedItem = hb_common_func.selectLongestTitle(mainWindow.drp_dvdtitle);

                this.Close();
            }
            catch (Exception exc)
            {
                MessageBox.Show("frmReadDVD.cs - updateUIElements " + exc.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void closeWindowAfterError()
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new UpdateUIHandler(closeWindowAfterError));
                    return;
                }
                this.Close();
            }
            catch (Exception exc)
            {
                MessageBox.Show("frmReadDVD.cs - closeWindowAfterError - Unable to recover from a serious error. \nYou may need to restart HandBrake. \n\n Error Information: \n " + exc.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            // This may seem like a long way of killing HandBrakeCLI, but for whatever reason,
            // hbproc.kill/close just won't do the trick.
            try
            {
                string AppName = "HandBrakeCLI";

                AppName = AppName.ToUpper();

                System.Diagnostics.Process[] prs = System.Diagnostics.Process.GetProcesses();
                foreach (System.Diagnostics.Process proces in prs)
                {
                    if (proces.ProcessName.ToUpper() == AppName)
                    {
                        proces.Refresh();
                        if (!proces.HasExited)
                            proces.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to kill HandBrakeCLI.exe \nYou may need to manually kill HandBrakeCLI.exe using the Windows Task Manager if it does not close automatically within the next few minutes. \n\nError Information: \n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
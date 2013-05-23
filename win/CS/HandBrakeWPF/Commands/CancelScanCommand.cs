﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CancelScanCommand.cs" company="HandBrake Project (http://handbrake.fr)">
//   This file is part of the HandBrake source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Command to cancel a scan that is in progress
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandBrakeWPF.Commands
{
    using System;
    using System.Windows.Input;

    using HandBrake.ApplicationServices.Services.Interfaces;

    /// <summary>
    /// Command to cancel a scan that is in progress
    /// </summary>
    public class CancelScanCommand : ICommand
    {
        /// <summary>
        /// The scan service wrapper.
        /// </summary>
        private readonly IScanServiceWrapper scanServiceWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="CancelScanCommand"/> class.
        /// </summary>
        /// <param name="ssw">
        /// The scan service wrapper.
        /// </param>
        public CancelScanCommand(IScanServiceWrapper ssw)
        {
            this.scanServiceWrapper = ssw;
            this.scanServiceWrapper.ScanStared += this.ScanServiceWrapperScanStared;
            this.scanServiceWrapper.ScanCompleted += this.ScanServiceWrapperScanCompleted;
        }

        /// <summary>
        /// The scan service Scan Completed Event Handler.
        /// Fires CanExecuteChanged
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The ScanCompletedEventArgs.
        /// </param>
        private void ScanServiceWrapperScanCompleted(object sender, HandBrake.ApplicationServices.EventArgs.ScanCompletedEventArgs e)
        {
            Caliburn.Micro.Execute.OnUIThread(() => this.CanExecuteChanged(sender, EventArgs.Empty));    
        }

        /// <summary>
        /// The scan service scan started event handler.
        /// Fires CanExecuteChanged
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The EventArgs.
        /// </param>
        private void ScanServiceWrapperScanStared(object sender, EventArgs e)
        {
            Caliburn.Micro.Execute.OnUIThread(() => this.CanExecuteChanged(sender, EventArgs.Empty));    
        }

        #region Implementation of ICommand

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter)
        {
            this.scanServiceWrapper.Stop();
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public bool CanExecute(object parameter)
        {
            if (this.scanServiceWrapper != null)
            {
                return this.scanServiceWrapper.IsScanning;
            }

            return false;
        }

        /// <summary>
        /// The can execute changed.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        #endregion
    }
}

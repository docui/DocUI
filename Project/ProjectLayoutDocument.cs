using Org.Filedrops.FileSystem;
using Org.Filedrops.FileSystem.UI;
using System;
using System.Windows;
using Xceed.Wpf.AvalonDock.Layout;

namespace Org.DocUI.Project
{
    public class ProjectLayoutDocument : LayoutDocument
    {
        private readonly ProjectInfo _pi;
        private readonly FiledropsFile _fi;

        public ProjectLayoutDocument(ProjectInfo pi, FiledropsFile fi, GetIcon iconfunction)
        {
            this._pi = pi;
            this._fi = fi;
            this.Title = fi.NameWithoutExtension;
            if (iconfunction != null)
                this.IconSource = iconfunction(fi, null, 32);
            if (this.IconSource == null)
                this.IconSource = fi.Icon32x32;
            this.Closed += CloseDoc;
            this.Closing += new EventHandler<System.ComponentModel.CancelEventArgs>(TryClose);
        }

        public void CloseDoc(object sender, EventArgs e)
        {
            _pi.FileClosed(_fi.FullName);
        }

        /// <summary>
        /// Tries to close the document.
        /// Note: Close() is necessary to close the doc when this event is fired manually
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TryClose(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //TODO: fix this strange way of closing
            e.Cancel = true;
            if (this.Title.EndsWith(@"*"))
            {
                // Configure the message box to be displayed 
                string messageBoxText = "Your file isn't saved yet. Would you like to save it before closing?";
                string caption = "Closing file";
                MessageBoxButton button = MessageBoxButton.YesNoCancel;
                MessageBoxImage icon = MessageBoxImage.Warning;
                // Display message box
                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);

                // Process message box results 
                // Yes = save and close, No = close without saving, Cancel = keep the form open
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        _pi.Manager.saveForm();
                        Close();
                        break;
                    case MessageBoxResult.No:
                        Close();
                        break;
                    case MessageBoxResult.Cancel:
                        //do nothing
                        break;
                }
            }
            else
            {
                Close();
            }
        }
    }
}

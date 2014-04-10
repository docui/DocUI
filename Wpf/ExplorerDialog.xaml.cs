using Org.Filedrops.FileSystem;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Org.DocUI.Wpf
{

    /// <summary>
    /// Interaction logic for ExplorerDialog.xaml
    /// </summary>
    public partial class ExplorerDialog : Window
    {
        public FiledropsFileSystemEntry SelectedEntry;

        private FiledropsFileSystem fs;

        private bool foldersonly;

        public ExplorerDialog(bool foldersonly, FiledropsDirectory root, string title, string filter = null)
        {
            InitializeComponent();
            this.foldersonly = foldersonly;
            if (foldersonly)
            {
                ExplorerControl.ShowFoldersList = true;
                ExplorerControl.ShowFiles = false;
            }
            else
            {
                ExplorerControl.ShowFoldersList = true;
                ExplorerControl.ShowFilesTree = false;
                ExplorerControl.ShowFilesList = true;
                ExplorerControl.ExtensionFilter = filter;
                ExplorerControl.ListDisplay.MouseDoubleClick += Check_File;
            }
            this.Title = title;
            this.ExplorerControl.AddRoot(root);
            this.ExplorerControl.Confirm.Click += Confirm_Click;
            this.ExplorerControl.FileSelectBox.KeyDown += CheckName;
            this.fs = root.FileSystem;
        }

        //TODO: make this more efficient
        public void CheckName(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Enter)
            {
                if (foldersonly)
                {
                    FiledropsDirectory dir = this.fs.ConstructDirectory(Path.Combine(
                        this.ExplorerControl.ListDisplay.RootDirectory.FullName, this.ExplorerControl.FileSelectBox.Text));
                    if (dir.Exists())
                    {
                        this.SelectedEntry = dir;
                        this.DialogResult = true;
                        this.Close();
                    }
                }
                else
                {
                    FiledropsFile file = this.fs.ConstructFile(Path.Combine(
                       this.ExplorerControl.ListDisplay.RootDirectory.FullName, this.ExplorerControl.FileSelectBox.Text));
                    if (file.Exists())
                    {
                        this.SelectedEntry = file;
                        this.DialogResult = true;
                        this.Close();
                    }
                }
            }
        }

        public void Confirm_Click(object sender, EventArgs args)
        {
            FiledropsFileSystemEntry entry;
            if (foldersonly)
            {
                entry = fs.ConstructDirectory(this.ExplorerControl.getInput());
                if (entry.Exists() && entry.EntryType == FileSystemEntryType.Folder)
                {
                    this.SelectedEntry = entry;
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    string messageBoxText = "You have to enter a Valid directory";
                    string caption = "Project Manager";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Warning;
                    MessageBox.Show(messageBoxText, caption, button, icon);
                }
            }
            else
            {
                entry = fs.ConstructFile(this.ExplorerControl.getInput());
                if (entry != null && entry.EntryType == FileSystemEntryType.File)
                {
                    this.SelectedEntry = entry as FiledropsFile;
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    string messageBoxText = "You have to select a file in the list";
                    string caption = "Project Manager";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Warning;
                    MessageBox.Show(messageBoxText, caption, button, icon);
                }
            }
        }


        public void Check_File(object sender, EventArgs args)
        {
            FiledropsFileSystemEntry entry = fs.ConstructFile(this.ExplorerControl.getInput());
            if (entry.Exists() && entry.EntryType == FileSystemEntryType.File)
            {
                this.SelectedEntry = entry;
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}

using Org.Filedrops.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace Org.DocUI.Project
{
    /// <summary>
    /// class that contains all info about a certain project
    /// also contains common operations
    /// </summary>
    public class ProjectInfo
    {
        //private FiledropsDirectory _projectroot;
        public ProjectManager Manager { get; set; }
        private readonly Dictionary<string, ProjectLayoutDocument> _openfiles;
        public FiledropsFileSystem FileSystem { get; set; }
        public FiledropsDirectory Root { get; set; }
        public FiledropsFile ProjectFile { get; set; }

        public ProjectInfo(FiledropsFile projectfile, ProjectManager manager, FiledropsFileSystem fs)
        {
            fs.GetFileIcon = this.GetFileIcon;
            fs.GetClosedDirIcon = this.GetClosedDirIcon;
            fs.GetOpenDirIcon = this.GetOpenDirIcon;
            fs.WorkingDirectory = projectfile.Parent;

            this.FileSystem = fs;
            projectfile.FileSystem = this.FileSystem;
            projectfile.Parent.FileSystem = this.FileSystem;

            this.ProjectFile = projectfile;
            this.Root = projectfile.Parent;
            _openfiles = new Dictionary<string, ProjectLayoutDocument>();
            this.Manager = manager;
        }

        public ProjectLayoutDocument GetDocument(string path)
        {
            ProjectLayoutDocument doc;
            if (!_openfiles.TryGetValue(path, out doc))
            {
                //logger.Logger.log("Could not find the document corresponding with this file", 2);
            }
            return doc;
        }

        public Boolean IsOpen(string path)
        {
            return this._openfiles.ContainsKey(path);
        }

        public void FileClosed(string path)
        {
            if (IsOpen(path))
            {
                _openfiles.Remove(path);
            }
        }

        public void fileOpened(string path, ProjectLayoutDocument doc)
        {
            if (!IsOpen(path))
            {
                _openfiles.Add(path, doc);
            }
        }

        public void SetFocus(string path)
        {
            ProjectLayoutDocument doc;
            if (_openfiles.TryGetValue(path, out doc))
            {
                doc.IsSelected = true;
            }
        }

        public void SetPendingChanges(string path)
        {
            ProjectLayoutDocument doc;
            if (_openfiles.TryGetValue(path, out doc) && !doc.Title.EndsWith("*"))
            {
                doc.Title += "*";
            }
        }

        public void SetSaved(string path)
        {
            ProjectLayoutDocument doc;
            if (_openfiles.TryGetValue(path, out doc) && doc.Title.EndsWith("*"))
            {
                doc.Title = doc.Title.Substring(0, doc.Title.Length - 1);
            }
        }

        public bool CloseProject()
        {
            bool allclosed = true;
            //copy needed to remove documents safely from dictionary
            List<string> list = new List<string>();
            foreach (string doc in this._openfiles.Keys)
            {
                list.Add(doc);
            }
            foreach (string doc in list)
            {
                allclosed = allclosed && closeTab(doc);
            }
            return allclosed;
        }

        /// <summary>
        /// Tries to close a tab
        /// TODO: do closing instead of close, somehow (no method provided...)
        /// </summary>
        /// <param name="path"></param>
        /// <returns>true if successfully closed</returns>
        public bool closeTab(string path)
        {
            ProjectLayoutDocument doc;
            if (_openfiles.TryGetValue(path, out doc))
            {
                doc.TryClose(this, new System.ComponentModel.CancelEventArgs());
                if (_openfiles.TryGetValue(path, out doc))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public void Open(FiledropsFile f)
        {
            // if tab is not Open yet, otherwise just focus.
            if (!IsOpen(f.FullName))
            {
                this.Manager.openDocument(f, this);
            }
            else
            {
                SetFocus(f.FullName);
            }
        }

        public void Rename(string oldpath, string newpath, string newtitle)
        {
            ProjectLayoutDocument doc;
            if (_openfiles.TryGetValue(oldpath, out doc))
            {
                doc.Title = newtitle;
                _openfiles.Add(newpath, doc);
                _openfiles.Remove(oldpath);
            }
        }

        public void RenameProject(string newfolder, string newname)
        {
            string oldname = this.ProjectFile.FullName;
            this.ProjectFile.FullName = Path.Combine(newfolder, this.ProjectFile.Name);
            try
            {
                this.ProjectFile.Rename(newname);
                this.Manager.RenameProject(oldname, newname, this);
            }
            catch
            {
                this.ProjectFile.FullName = oldname;
            }
        }

        public BitmapImage GetFileIcon(FiledropsFile f, int size)
        {
            BitmapImage img = null;
            if (f.Extension != null)
            {
                ProjectManager.FILE_ICONS.TryGetValue(f.Extension.Substring(1), out img);
            }
            return img;
        }

        public BitmapImage GetOpenDirIcon(FiledropsDirectory d, int size)
        {
            BitmapImage img = null;
            string folder = GetFolderKey(d);
            if (folder != null)
            {
                int valueindex = Array.IndexOf(ProjectManager.ACCEPTED, folder);
                if (valueindex >= 0)
                {
                    ProjectManager.OPENFOLDER_ICONS.TryGetValue(ProjectManager.ACCEPTEDXTS[valueindex], out img);
                }
            }
            else
            {
                if (ProjectManager.projOpenFolderIcon != null)
                {
                    img = ProjectManager.projOpenFolderIcon;
                }
            }
            return img;
        }

        public BitmapImage GetClosedDirIcon(FiledropsDirectory d, int size)
        {
            BitmapImage img = null;
            string folder = GetFolderKey(d);
            if (folder != null)
            {
                int valueindex = Array.IndexOf(ProjectManager.ACCEPTED, folder);
                if (valueindex >= 0)
                {
                    ProjectManager.CLOSEDFOLDER_ICONS.TryGetValue(ProjectManager.ACCEPTEDXTS[valueindex], out img);
                }
            }
            else
            {
                if (ProjectManager.projFolderIcon != null)
                {
                    img = ProjectManager.projFolderIcon;
                }
            }
            return img;
        }

        private string GetFolderKey(FiledropsDirectory d)
        {
            if (d.FullName.StartsWith(this.Root.FullName) && d.FullName != this.Root.FullName)
            {
                string topfolder = d.FullName.Substring(this.Root.FullName.Length + 1);
                int index = topfolder.IndexOf('\\');
                if (index < 0)
                {
                    index = topfolder.IndexOf('/');
                }
                if (index >= 0)
                {
                    topfolder = topfolder.Substring(0, index);
                }
                return topfolder;
            }
            return null;
        }

        public ProjectLayoutDocument GetActive()
        {
            foreach (ProjectLayoutDocument doc in this._openfiles.Values)
            {
                if (doc.IsActive)
                {
                    return doc;
                }
            }
            return null;
        }

        //todo: implement this function which creates a file in the default folder
        public void CreateFileInDefault(string ext)
        {

        }
    }
}

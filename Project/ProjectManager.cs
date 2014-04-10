using Org.DocUI.FormBuilder;
using Org.DocUI.ProjectTreeView;
using Org.DocUI.Tools;
using Org.DocUI.Wpf;
using Org.Filedrops.FileSystem;
using Org.Filedrops.FileSystem.UI;
using Org.Filedrops.FileSystem.UI.FileTreeView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Xceed.Wpf.AvalonDock.Layout;

namespace Org.DocUI.Project
{
    /// <summary>
    /// Interaction logic for ProjectManager.xaml
    /// </summary>
    /// <summary>
    /// Interaction logic for ProjectManager.xaml
    /// </summary>
    public class ProjectManager : UserControl
    {
        public GetIcon getIcon { get; set; }
        public Assembly Assembly { get; set; }

        /// <summary>
        /// Default config for docui
        /// Default values are used if not changed
        /// </summary>
        public static string[] ACCEPTED;
        public static string[] ACCEPTEDXTS;
        public static string[] TYPES;
        public static List<String> NoSchema { get; set; }
        public static string PXT = "proj";

        /// <summary>
        /// Project icons.
        /// Note: size is currently NOT taken into account
        /// </summary>
        public static Dictionary<string, BitmapImage> OPENFOLDER_ICONS = new Dictionary<string, BitmapImage>();
        public static Dictionary<string, BitmapImage> CLOSEDFOLDER_ICONS = new Dictionary<string, BitmapImage>();
        public static Dictionary<string, BitmapImage> FILE_ICONS = new Dictionary<string, BitmapImage>();
        public static Dictionary<string, BitmapImage> SCHEMA_ICONS = new Dictionary<string, BitmapImage>();

        public static string preferencesfile = null;
        public static BitmapImage projFolderIcon;
        public static BitmapImage projOpenFolderIcon;
        public static BitmapImage projFileIcon;

        /// <summary>
        /// List of all Projects momentarily opened
        /// TODO: list of all documents opened (possibility to close all documents when project is closed)
        /// </summary>
        protected Dictionary<string, ProjectInfo> projects;
        public DynamicProjectFormFactory FormFactory { get; set; }
        public ProjectFileTree projectTree { get; set; }
        public LayoutDocumentPane DocPane { get; set; }

        //TODO: fix this dependency?
        public FiledropsFileSystem FileSystem { get; set; }
        protected Grid treeGrid;

        /// <summary>
        /// Creates new ProjectManager (can act as model)
        /// </summary>
        public ProjectManager(FiledropsFileSystem fs, DynamicProjectFormFactory dff, Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = Assembly.GetExecutingAssembly();
            }
            this.Assembly = assembly;
            this.FileSystem = fs;
            this.FormFactory = dff;
            projects = new Dictionary<string, ProjectInfo>();
        }

        public virtual void initTree()
        {
            projectTree = new ProjectFileTree(ACCEPTED, ACCEPTEDXTS, this, preferencesfile != null);
            treeGrid = new Grid();
            projectTree.ShowRoot = true;
            projectTree.ShowFileExtensions = false;
            projectTree.ShowFiles = true;
            projectTree.ShowMenu = true;
            projectTree.Filters = ACCEPTEDXTS;
            treeGrid.Children.Add(projectTree);
            this.AddChild(treeGrid);
        }

        /// <summary>
        /// Opens the selected document in a new tab
        /// the newly created tab contains a form representing the info in the selected file
        /// </summary>
        /// <param name="f">the selected file</param>
        public virtual void openDocument(FiledropsFile f, ProjectInfo pi)
        {
            if (!pi.IsOpen(f.FullName))
            {
                // create new tab and add it to the documentContainer
                ProjectLayoutDocument layout = new ProjectLayoutDocument(pi, f, GetIcon);

                bool ok;
                if (IsFileEmpty(f))
                {
                    ok = XmlValidator.Validate(f, XmlSchemaUtilities.getXmlSchemaFromXml(f.FullName));
                }
                else
                {
                    ok = true;
                }
                if (ok)
                {
                    ScrollViewer scroller = new ScrollViewer();
                    scroller.Content = FormFactory.GetNewForm(f, pi);
                    layout.Content = scroller;
                }
                else
                {
                    string messageBoxText = "File invalid, see log files for more info.";
                    string caption = "Filedrops";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Warning;
                    MessageBox.Show(messageBoxText, caption, button, icon);
                }

                if (layout.Content != null)
                {
                    DocPane.Children.Add(layout);									 // add to documentContainer
                    layout.IsActive = true;                                          // focus on tab
                    pi.fileOpened(f.FullName, layout);
                }
            }
            else
            {
                pi.SetFocus(f.FullName);
            }
        }

        public bool IsFileEmpty(FiledropsFile file)
        {
            file.Read();
            return file.BytesContent.Length == 0;
        }

        /// <summary>
        /// This method opens the project represented by the fdproj file.
        /// </summary>
        /// <param name="f">fdproj file</param>
        public void OpenProject()
        {
            //filedrops file dialog disabled because of necessary functionality improvements
            ExplorerDialog dialog = new ExplorerDialog(false, FileSystem.WorkingDirectory, "Choose a projectfile", "." + PXT);
            bool? result = dialog.ShowDialog();
            if (result == true)

            //System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            //if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //filedrops file dialog disabled because of necessary functionality improvements
                FiledropsFile f = dialog.SelectedEntry as FiledropsFile;

                //FiledropsFile f = FileSystem.ConstructFile(dialog.FileName);

                if (f.Extension == "." + PXT && !projects.ContainsKey(f.Parent.FullName))
                {
                    //check preferences validity. if no xmlschema available, this is ignored
                    if (preferencesfile == null || XmlValidator.Validate(f, XmlSchemaUtilities.getXmlSchemaFromXml(f.FullName)))
                    {
                        initProject(f);
                    }
                    else
                    {
                        string messageBoxText = "The projectfile is invalid.";
                        string caption = "Filedrops";
                        MessageBoxButton button = MessageBoxButton.OK;
                        MessageBoxImage icon = MessageBoxImage.Error;
                        MessageBox.Show(messageBoxText, caption, button, icon);
                    }
                }
                else if (projects.ContainsKey(f.FullName))
                {
                    string messageBoxText = "This project is opened already.";
                    string caption = "Filedrops";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Warning;
                    MessageBox.Show(messageBoxText, caption, button, icon);
                }

            }
        }

        public virtual void initProject(ProjectRootNode prn)
        {
            if (prn != null && prn.Project != null)
            {
                initProject(prn.Project.ProjectFile);

                //initProject(prn);
            }
        }

        public void initProject(FiledropsFile f)
        {
            // add project
            FiledropsFileSystem pfs = this.FileSystem.Clone<FiledropsFileSystem>();
            pfs.WorkingDirectory = f.Parent;
            ProjectInfo pi = new ProjectInfo(f, this, pfs);
            f.Parent.FileSystem = pfs;
            f.FileSystem = pfs;
            projectTree.AddRoot(f.Parent, pi);
            projects.Add(f.FullName, pi);
        }

        protected virtual Dictionary<string, ProjectInfo> GetAllProjects()
        {
            /*
            Dictionary<string, ProjectInfo> dict = new Dictionary<string, ProjectInfo>();
            foreach (ProjectInfo pi in projects.Values)
            {
                dict.Add(pi.Root.FullName, pi);
            }
            return dict;
            */
            return projects;
        }

        public bool hasActiveForm()
        {
            return getActiveForm() != null;
        }

        public DynamicProjectForm getActiveForm()
        {
            /*
                        DynamicProjectForm dfm = null;
                        foreach (ProjectInfo pi in GetAllProjects().Values)
                        {
                            ProjectLayoutDocument doc = pi.getActive();
                            if (doc != null && doc.Content is ScrollViewer)
                            {
                                dfm = (doc.Content as ScrollViewer).Content as DynamicProjectForm;
                            }
                        }
                        return dfm;
             */
            DynamicProjectForm dfm = null;
            if (DocPane.SelectedContent != null)
            {
                dfm = (DocPane.SelectedContent.Content as ScrollViewer).Content as DynamicProjectForm;
            }
            return dfm;
        }

        public virtual void saveForm()
        {
            DynamicProjectForm dfm = getActiveForm();
            if (dfm != null)
            {
                dfm.saveFile(this, null);
            }
        }

        public ProjectInfo GetProjectFromSystemEntry(FiledropsFileSystemEntry e, Dictionary<string, ProjectInfo> dict)
        {
            /*
                        FiledropsFileSystemEntry current = e;

                        while (current != null
                            && current.FullName != null
                            && !(dict.ContainsKey(current.FullName)
                                    && current.EntryType.Equals(FileSystemEntryType.Folder)))
                        {
                            string tmp = current.FullName;
                            current = current.Parent;
                            if (current.FullName.Equals(tmp)) break;
                        }

                        if (current != null
                            && dict.ContainsKey(current.FullName)
                            && current.EntryType.Equals(FileSystemEntryType.Folder))
                        {
                            return dict[current.FullName];
                        }
             */

            foreach (ProjectInfo projectInfo in dict.Values)
            {
                if (e.FullName.StartsWith(projectInfo.Root.FullName))
                {
                    return projectInfo;
                }
            }
            return null;
        }

        public virtual ProjectInfo GetProjectFromSystemEntry(FiledropsFileSystemEntry e)
        {
            return GetProjectFromSystemEntry(e, this.projects);
        }

        public void closeForm()
        {
            DynamicProjectForm dfm = getActiveForm();
            if (dfm != null)
            {
                string path = dfm.FI.FullName;
                FiledropsFileSystemEntry entry = FileSystem.ConstructFile(path);
                ProjectInfo pi = GetProjectFromSystemEntry(entry);
                if (pi != null)
                {
                    pi.closeTab(path);
                }
            }
        }

        /// <summary>
        /// Returns a bitmapimage for a certain file.
        /// Note: size is currently NOT taken into account.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="foldertype"></param>
        /// <param name="size"></param>
        /// <param name="expanded"></param>
        /// <returns></returns>
        public virtual BitmapImage GetIcon(FiledropsFileSystemEntry entry, string foldertype = null, int size = 32, bool expanded = false)
        {
            BitmapImage image = null;

            if (entry is FiledropsFile)
            {
                FiledropsFile fi = entry as FiledropsFile;
                if (fi.Extension != null)
                {
                    if (fi.Extension == ".xsd")
                    {
                        SCHEMA_ICONS.TryGetValue(fi.NameWithoutExtension, out image);
                    }
                    else
                    {
                        FILE_ICONS.TryGetValue(fi.Extension.Substring(1), out image);
                    }
                }
            }
            else if (entry is FiledropsDirectory)
            {
                if (foldertype != null)
                {
                    FiledropsDirectory di = entry as FiledropsDirectory;
                    if (expanded)
                    {
                        OPENFOLDER_ICONS.TryGetValue(foldertype, out image);
                    }
                    else
                    {
                        CLOSEDFOLDER_ICONS.TryGetValue(foldertype, out image);
                    }
                }
            }
            return image;
        }

        /// <summary>
        /// Closes the project.
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="args">arguments</param>
        public virtual void closeProject(ProjectInfo pi)
        {
            FiledropsFile f = pi.ProjectFile;
            if (projects.ContainsKey(f.FullName) && pi.CloseProject())
            {
                projects.Remove(f.FullName);
                foreach (TreeViewItem item in projectTree.Items)
                {
                    if ((item as ProjectRootNode).Entry == f.Parent)
                    {
                        projectTree.Items.Remove(item);
                        if (item.Parent != null)
                        {
                            (item.Parent as TreeViewItem).Items.Remove(item);
                        }
                        break;
                    }
                }
            }
        }

        public virtual void closeProject(ProjectRootNode prn)
        {
            if (prn != null) closeProject(prn.Project);
        }

        public virtual void CloseProjectFormMain()
        {
            IProjectNode item = this.projectTree.SelectedItem as IProjectNode;
            if (item != null)
            {
                this.closeProject(item.GetProject());
            }
        }

        public virtual void deleteProject()
        {
            FolderTreeNode item = this.projectTree.SelectedItem as FolderTreeNode;
            if (item != null && item is IProjectNode)
            {
                if (item.removeNode())
                {
                    this.projectTree.removeNode(item);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public FiledropsFile NewFile(string ext, FiledropsDirectory dir)
        {

            FiledropsDirectory schemadir = dir.FileSystem
                .ConstructDirectory(Environment.CurrentDirectory + @"\Resources\DocUI\" + ACCEPTED[Array.IndexOf(ACCEPTEDXTS, ext)]);
            if (schemadir.Exists())
            {
                if (schemadir.GetFiles("xsd").Count > 1)
                {
                    FiledropsFileSystem fs = this.FileSystem.Clone<FiledropsFileSystem>();
                    fs.WorkingDirectory = schemadir;
                    schemadir.FileSystem = fs;
                    fs.GetFileIcon = GetSchemaImage;
                    NewFilePanel dialog = new NewFilePanel(dir, schemadir, ext, GetIcon);
                    bool? result = dialog.ShowDialog();
                    if (result == true)
                    {
                        FiledropsFile file = dir.FileSystem.ConstructFileRecursive(dir.FullName + @"\", dir.Name, ext);
                        XmlSchemaUtilities.generateCleanXml(file.FullName, dialog.NewType.FullName, ACCEPTED[Array.IndexOf(ACCEPTEDXTS, ext)]);
                        return dir.FileSystem.ConstructFile(file.FullName);
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (schemadir.GetFiles("xsd").Count == 1)
                {
                    FiledropsFile xsd = schemadir.GetFiles("xsd")[0];
                    FiledropsFile file = dir.FileSystem.ConstructFileRecursive(dir.FullName + @"\", dir.Name, ext);
                    XmlSchemaUtilities.generateCleanXml(file.FullName, xsd.FullName, ACCEPTED[Array.IndexOf(ACCEPTEDXTS, ext)]);
                    return file;
                }
                else
                {
                    return null;
                }
            }
            else if (NoSchema.Contains<string>(ext))
            {
                FiledropsFile file = dir.FileSystem.ConstructFileRecursive(dir.FullName + @"\", dir.Name, ext);
                return file;
            }

            return null;
        }

        /// <summary>
        /// TODO: fix this dependency
        /// </summary>
        public virtual void createNewProject()
        {
            FiledropsFileSystem pfs = this.FileSystem.Clone<FiledropsFileSystem>();

            //filedrops folder dialog disabled because of necessary functionality improvements
            //ExplorerDialog dialog = new ExplorerDialog(true, FileSystem.WorkingDirectory, "Choose a destination folder");
            //bool? result = dialog.ShowDialog();
            //if (result == true)

            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //filedrops folder dialog disabled because of necessary functionality improvements
                //FiledropsDirectory dir = dialog.SelectedEntry as FiledropsDirectory;
                //FiledropsDirectory newdir = pfs.ConstructDirectoryRecursive(FileTools.Combine(dir.FullName, dir.Name));

                FiledropsDirectory newdir = pfs.ConstructDirectoryRecursive(Path.Combine(dialog.SelectedPath, "Project"));
                initTopDirs(newdir);

                FiledropsFile projectfile = pfs.ConstructFile(Path.Combine(newdir.FullName, newdir.Name + "." + ProjectManager.PXT));
                projectfile.Create();

                pfs.WorkingDirectory = projectfile.Parent;

                ProjectInfo pi = new ProjectInfo(projectfile, this, pfs);
                projects.Add(projectfile.FullName, pi);

                projectfile.Parent.FileSystem = pfs;
                projectfile.FileSystem = pfs;

                projectTree.AddRootAndRename(projectfile.Parent, pi);

                if (preferencesfile != null)
                    XmlSchemaUtilities.generateCleanXml(projectfile.FullName, Environment.CurrentDirectory + @"\Resources\DocUI\preferences.xsd", ".");
            }
        }

        protected void initTopDirs(FiledropsDirectory dir)
        {
            foreach (String s in ACCEPTED)
            {
                try
                {
                    FiledropsDirectory newdir = this.FileSystem.ConstructDirectory(dir.FullName + @"\" + s);
                    newdir.Create();
                }
                catch { }
            }
        }

        public void RenameProject(string oldname, string newname, ProjectInfo pi)
        {
            this.projects.Remove(oldname);
            this.projects.Add(newname, pi);
        }

        public BitmapImage GetSchemaImage(FiledropsFile f, int size)
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            string imagesresource = "Com.Xploreplus.Filedrops.Client.Resources.Images.schema.filetype_";
            return EmbeddedResourceTools.GetImage(imagesresource + f.NameWithoutExtension.ToLower() + ".png", size);
        }

        public void ShowPreferences(ProjectInfo pi)
        {
            if (!pi.IsOpen(pi.ProjectFile.FullName))
            {
                // create new tab and add it to the documentContainer
                ProjectLayoutDocument layout = new ProjectLayoutDocument(pi, pi.ProjectFile, null);

                bool ok;
                if (IsFileEmpty(pi.ProjectFile))
                {
                    ok = XmlValidator.Validate(pi.ProjectFile, XmlSchemaUtilities.getXmlSchemaFromXml(pi.ProjectFile.FullName));
                }
                else
                {
                    ok = true;
                }
                if (ok)
                {
                    ScrollViewer scroller = new ScrollViewer();
                    scroller.Content = FormFactory.GetNewForm(pi.ProjectFile, pi);
                    layout.Content = scroller;
                }
                else
                {
                    string messageBoxText = "File invalid, see log files for more info.";
                    string caption = "Filedrops";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Warning;
                    MessageBox.Show(messageBoxText, caption, button, icon);
                }

                if (layout.Content != null)
                {
                    layout.IsActive = true;															 // focus on tab
                    DocPane.Children.Add(layout);									 // add to documentContainer
                    pi.fileOpened(pi.ProjectFile.FullName, layout);
                }
            }
            else
            {
                pi.SetFocus(pi.ProjectFile.FullName);
            }
        }

    }

}

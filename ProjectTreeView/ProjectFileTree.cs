using Com.Xploreplus.Common;
using Org.DocUI.Project;
using Org.Filedrops.FileSystem;
using Org.Filedrops.FileSystem.UI.FileTreeView;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Org.DocUI.ProjectTreeView
{
    public class ProjectFileTree : FileTree
    {
        public string[] AcceptedDirs { get; set; } //filter for rootdirectories
        public string[] Filters { get; set; } //filter for each accepted

        private Boolean hasPref; //true if project has preferences
        public ProjectManager Manager { get; set; }

        /// <summary>
        /// This constructor can be used by Components
        /// </summary>
        public ProjectFileTree()
        {
        }

        /// <summary>
        /// This constructor is used by the projectmanager
        /// </summary>
        /// <param name="accepteddirs"></param>
        /// <param name="filters"></param>
        /// <param name="manager"></param>
        /// <param name="hasPref"></param>
        public ProjectFileTree(string[] accepteddirs, string[] filters, ProjectManager manager, bool hasPref)
        {
            this.AcceptedDirs = accepteddirs;
            this.Filters = filters;
            this.Manager = manager;
            this.hasPref = hasPref;

            //Background = Brushes.LightSteelBlue;
            InitOtherMenus();
        }

        /// <summary>
        /// Can only be executed after construction
        /// </summary>
        public void InitOtherMenus()
        {
            ContextMenu projectcm = new ContextMenu();

            // MenuItem for the preferences of the project
            if (hasPref)
            {
                MenuItem Pref = new MenuItem();
                Pref.Click += PrefProject_Click;
                Pref.Header = "Edit Preferences";
                projectcm.Items.Add(Pref);

                // Adds a nice seperator between Preferences and Expand
                projectcm.Items.Add(new Separator());
            }

            // The menu item for expanding all the folders and files in the project
            MenuItem ExpandAllP = new MenuItem();
            ExpandAllP.Click += ExpandAll_Click;
            ExpandAllP.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.ResizeIcons.Expand, 16);
            ExpandAllP.Name = "ExpandProject";
            ExpandAllP.Header = "Expand Project";
            projectcm.Items.Add(ExpandAllP);

            // The menu item for collapsing all the folders and files in the project
            MenuItem CollapseAllP = new MenuItem();
            CollapseAllP.Click += CollapseAll_Click;
            CollapseAllP.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.ResizeIcons.Collapse, 16);
            CollapseAllP.Name = "CollapseProject";
            CollapseAllP.Header = "Collapse Project";
            projectcm.Items.Add(CollapseAllP);

            // Adds a nice seperator between Collapse and Rename
            projectcm.Items.Add(new Separator());

            // The menu item for renaming the project
            MenuItem RenameProject = new MenuItem();
            RenameProject.Click += RenameNode_Click;
            RenameProject.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Rename, 16);
            RenameProject.Name = "RenameProject";
            RenameProject.Header = "Rename Project";
            projectcm.Items.Add(RenameProject);

            // The menu item for closing the project
            MenuItem CloseProject = new MenuItem();
            CloseProject.Click += CloseProject_Click;
            CloseProject.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Close, 16);
            CloseProject.Name = "closeProject";
            CloseProject.Header = "Close Project";
            projectcm.Items.Add(CloseProject);

            // The menu item for removing the project
            MenuItem RemoveProject = new MenuItem();
            RemoveProject.Click += RemoveFolder_Click;
            RemoveProject.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Delete, 16);
            RemoveProject.Header = "Remove Project";
            projectcm.Items.Add(RemoveProject);

            ContextMenu foldercm = new ContextMenu();

            // The menu item for adding a new file to the topfolder
            MenuItem AddFile = new MenuItem();
            AddFile.Name = "AddFile";
            AddFile.Header = "Add File";
            AddFile.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Add, 16);
            AddFile.Click += AddFileToFolder_Click;

            // The menu item for adding a new folder to the topfolder
            MenuItem AddFolder = new MenuItem();
            AddFolder.Name = "AddFolder";
            AddFolder.Header = "Add Folder";
            AddFolder.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FolderIcons.Add, 16);
            AddFolder.Click += AddFolderToFolder_Click;

            // The menu item for expanding all the folders and files in the topfolder
            MenuItem ExpandAll = new MenuItem();
            ExpandAll.Name = "ExpandAll";
            ExpandAll.Header = "Expand All";
            ExpandAll.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.ResizeIcons.Expand, 16);
            ExpandAll.Click += ExpandAll_Click;

            // The menu item for collapsing all the folders and files in the topfolder
            MenuItem CollapseAll = new MenuItem();
            CollapseAll.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.ResizeIcons.Collapse, 16);
            CollapseAll.Name = "CollapseAll";
            CollapseAll.Header = "Collapse All";
            CollapseAll.Click += CollapseAll_Click;

            // Adding all the menu items to the context menu
            foldercm.Items.Add(AddFile);
            foldercm.Items.Add(AddFolder);

            // Adds a nice seperator between Add and Expand
            foldercm.Items.Add(new Separator());

            foldercm.Items.Add(ExpandAll);
            foldercm.Items.Add(CollapseAll);

            Menus.Add("TopFolder", foldercm);

            Menus.Add("Project", projectcm);
        }



        public FolderTreeNode AddRoot(FiledropsDirectory dir, ProjectInfo pi)
        {
            FolderTreeNode node = new ProjectRootNode(Manager, pi, dir, showExtBinding);
            this.RootDirectories.Add(node.Entry);
            this.buildFolderRoot(node);
            if (ShowRoot)
            {
                this.Items.Add(node);
            }
            return node;
        }

        public void AddRootAndRename(FiledropsDirectory dir, ProjectInfo pi)
        {
            FolderTreeNode node = new ProjectRootNode(Manager, pi, dir, showExtBinding);
            this.RootDirectories.Add(node.Entry);
            this.buildFolderRoot(node);
            this.Items.Add(node);
            node.RenameNode();
        }

        public override void buildFolderRoot(FolderTreeNode node)
        {
            List<FiledropsFileSystemEntry> entries = (node.Entry as FiledropsDirectory).GetEntries();
            int i = 0;
            foreach (string accepted in AcceptedDirs)//this way top folders will be sorted like the accepted list
            {
                foreach (FiledropsFileSystemEntry entry in entries)
                {
                    if (entry.Name == accepted && entry.EntryType == FileSystemEntryType.Folder)
                    {

                        FolderTreeNodeFilter filter = new FolderTreeNodeFilter(new string[] { Filters[i] }, new string[] { });
                        FolderTreeNode item = node.createFolderNode(entry as FiledropsDirectory, filter);

                        item.Tag = "TopFolder";
                        item.FontWeight = FontWeights.Normal;
                        if (ShowRoot)
                            node.Items.Add(item);
                        else
                            this.Items.Add(item);
                    }
                }
                i++;
            }
        }

        /// <summary>
        /// Called when clicked on close project menuitem
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void CloseProject_Click(object sender, EventArgs args)
        {
            this.Manager.closeProject((this.SelectedItem as ProjectRootNode).Project);
        }

        /// <summary>
        /// Called when clicked on edit preferences menuitem
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void PrefProject_Click(object sender, EventArgs args)
        {
            ProjectRootNode node = this.SelectedItem as ProjectRootNode;
            node.ProjectManager.ShowPreferences(node.Project);
        }
    }
}

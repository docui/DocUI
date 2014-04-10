using Com.Xploreplus.Common;
using Org.DocUI.ProjectTreeView;
using Org.DocUI.Solution;
using Org.Filedrops.FileSystem;
using Org.Filedrops.FileSystem.UI.FileTreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;

namespace Org.DocUI.SolutionTreeView
{
    public class SolutionFileTree : FileTree
    {
        public SolutionManager Manager { get; set; }

        /// <summary>
        /// Dictionary with the accepted directories as keys, and accepted extensions as values
        /// </summary>
        protected Dictionary<string, string> accepteddirs;

        private Boolean hasPref; //true if project has preferences

        public SolutionFileTree()
        {
            accepteddirs = new Dictionary<string, string>();
            hasPref = false;
        }

        public SolutionFileTree(Dictionary<string, string> accepteddirs, SolutionManager manager, bool hasPref)
        {
            this.accepteddirs = accepteddirs;
            this.Manager = manager;
            this.hasPref = hasPref;
            initOtherMenus();
        }

        public void initOtherMenus()
        {
            ContextMenu solutioncm = new ContextMenu();

            // The menu item for adding a new project in the solution
            MenuItem AddProject = new MenuItem();
            AddProject.Click += AddProject_Click;
            AddProject.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FolderIcons.Add, 16);
            AddProject.Header = "Add Project";
            solutioncm.Items.Add(AddProject);

            // Adds a nice seperator between Add and Expand
            solutioncm.Items.Add(new Separator());

            // The menu item for expanding all the folders and files in the solution
            MenuItem ExpandAllS = new MenuItem();
            ExpandAllS.Click += ExpandAll_Click;
            ExpandAllS.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.ResizeIcons.Expand, 16);
            ExpandAllS.Header = "Expand Solution";
            solutioncm.Items.Add(ExpandAllS);

            // The menu item for collapsing all the folders and files in the solution
            MenuItem CollapseAllS = new MenuItem();
            CollapseAllS.Click += CollapseAll_Click;
            CollapseAllS.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.ResizeIcons.Collapse, 16);
            CollapseAllS.Header = "Collapse Solution";
            solutioncm.Items.Add(CollapseAllS);

            // Adds a nice seperator between Collapse and Rename
            solutioncm.Items.Add(new Separator());

            // The menu item for renaming the solution
            MenuItem RenameSolution = new MenuItem();
            RenameSolution.Click += RenameNode_Click;
            RenameSolution.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Rename, 16);
            RenameSolution.Header = "Rename Solution";
            solutioncm.Items.Add(RenameSolution);

            // The menu item for closing the solution
            MenuItem CloseSolution = new MenuItem();
            CloseSolution.Click += CloseSolution_Click;
            CloseSolution.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Close, 16);
            CloseSolution.Header = "Close Solution";
            solutioncm.Items.Add(CloseSolution);

            // The menu item for removing the solution
            MenuItem RemoveS = new MenuItem();
            RemoveS.Click += RemoveFolder_Click;
            RemoveS.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Delete, 16);
            RemoveS.Header = "Remove Solution";
            solutioncm.Items.Add(RemoveS);

            ContextMenu projectcm = new ContextMenu();

            if (hasPref)
            {
                MenuItem Pref = new MenuItem();
                Pref.Click += PrefProject_Click;
                Pref.Header = "Edit preferences";
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
            MenuItem RenameP = new MenuItem();
            RenameP.Click += RenameNode_Click;
            RenameP.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Rename, 16);
            RenameP.Header = "Rename Project";
            projectcm.Items.Add(RenameP);

            // The menu item for removing the project
            MenuItem RemoveP = new MenuItem();
            RemoveP.Click += RemoveFolder_Click;
            RemoveP.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Delete, 16);
            RemoveP.Header = "Remove Project";
            projectcm.Items.Add(RemoveP);

            ContextMenu foldercm = new ContextMenu();

            // The menu item for adding a file to the topfolder
            MenuItem AddFile = new MenuItem();
            AddFile.Name = "AddFile";
            AddFile.Header = "Add File";
            AddFile.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Add, 16);
            AddFile.Click += AddFileToFolder_Click;

            // The menu item for adding a folder to the topfolder
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

            // adding the menu items to the folder context menu
            foldercm.Items.Add(AddFile);
            foldercm.Items.Add(AddFolder);

            // Adds a nice seperator between Add and Expand
            foldercm.Items.Add(new Separator());

            foldercm.Items.Add(ExpandAll);
            foldercm.Items.Add(CollapseAll);

            Menus.Add("TopFolder", foldercm);

            Menus.Add("Project", projectcm);

            Menus.Add("Solution", solutioncm);
        }

        public FolderTreeNode addRoot(FiledropsDirectory dir, SolutionInfo si)
        {
            FolderTreeNode node = new SolutionRootNode(Manager, si, dir, showExtBinding);
            this.RootDirectories.Add(node.Entry);
            this.buildFolderRoot(node);
            if (ShowRoot)
            {
                this.Items.Add(node);
            }
            return node;
        }

        public void addRootAndRename(FiledropsDirectory dir, SolutionInfo si)
        {
            FolderTreeNode node = new SolutionRootNode(Manager, si, dir, showExtBinding);
            this.RootDirectories.Add(node.Entry);
            this.buildFolderRoot(node);
            this.Items.Add(node);
            node.RenameNode();
        }

        public void addProjectToSolutionAndRename(FiledropsFile projectfile, SolutionInfo si)
        {
            SolutionRootNode solutionsRoot = null;
            foreach (ISolutionNode n in this.Items)
            {
                if (n.getSolution().Solutionfile.FullName.Equals(si.Solutionfile.FullName))
                {
                    solutionsRoot = n as SolutionRootNode;
                }
            }

            if (solutionsRoot != null)
            {
                FolderTreeNodeFilter filter = new FolderTreeNodeFilter(new string[] { }, accepteddirs.Keys.ToArray());
                FolderTreeNode item = solutionsRoot.createFolderNode(projectfile.Parent, filter, false);
                //item.Tag = "TopFolder";
                item.FontWeight = FontWeights.Normal;
                buildProjectFolder(item);
                solutionsRoot.Items.Add(item);
                item.RenameNode();
            }
        }

        public void AddProject_Click(object sender, EventArgs args)
        {
            this.Manager.createNewProject();
        }

        public void CloseSolution_Click(object sender, EventArgs args)
        {
            this.Manager.closeSolution((this.SelectedItem as SolutionRootNode).Solution);
        }

        public void buildProjectFolder(FolderTreeNode node)
        {
            List<FiledropsFileSystemEntry> entries = node.filter.getAllowedDirectoryContents(node.Entry as FiledropsDirectory, FileSystemEntryType.Folder);

            // double loop: for sorting the folders as in the list of accepted directories.
            foreach (string accepted in accepteddirs.Keys)
            {
                foreach (FiledropsFileSystemEntry entry in entries)
                {
                    if (entry.Name == accepted && entry.EntryType == FileSystemEntryType.Folder)
                    {
                        // Filter: show only files with an extension in Filters[i]; show all subdirectories
                        FolderTreeNodeFilter filter = new FolderTreeNodeFilter(new string[] { accepteddirs[entry.Name] });
                        FolderTreeNode item = node.createFolderNode(entry as FiledropsDirectory, filter);
                        item.Tag = "TopFolder";
                        item.FontWeight = FontWeights.Normal;
                        node.Items.Add(item);
                    }
                }
            }
        }

        public override void buildFolderRoot(FolderTreeNode node)
        {
            List<FiledropsFileSystemEntry> entries = (node.Entry as FiledropsDirectory).GetEntries();
            SolutionInfo solution = Manager.GetSolutionWithPath((node.Entry as FiledropsDirectory).FullName);
            if (solution == null) return;

            foreach (string projectdir in solution.GetProjectNames())
            {
                foreach (FiledropsFileSystemEntry entry in entries)
                {
                    if (entry.Name == projectdir && entry.EntryType == FileSystemEntryType.Folder)
                    {
                        if (solution.projects[entry.Name] != null)
                        {
                            entry.FileSystem = solution.projects[entry.Name].FileSystem;
                        }
                        // Filter: don't show any files; show only subdirectories in AcceptedDirs
                        FolderTreeNodeFilter filter = new FolderTreeNodeFilter(new string[] { }, accepteddirs.Keys.ToArray());
                        FolderTreeNode item = node.createFolderNode(entry as FiledropsDirectory, filter, false);
                        //item.Tag = "TopFolder";
                        item.FontWeight = FontWeights.Normal;
                        buildProjectFolder(item);
                        node.Items.Add(item);
                    }
                }
            }
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

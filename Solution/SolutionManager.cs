using Org.DocUI.FormBuilder;
using Org.DocUI.Project;
using Org.DocUI.ProjectTreeView;
using Org.DocUI.SolutionTreeView;
using Org.DocUI.Tools;
using Org.Filedrops.FileSystem;
using Org.Filedrops.FileSystem.UI.FileTreeView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Org.DocUI.Solution
{
    public class SolutionManager : ProjectManager
    {
        protected Dictionary<string, SolutionInfo> solutions;

        public static string SXT = "sln";

        public SolutionFileTree solutionTree { get; set; }

        public static BitmapImage solFolderIcon;
        public static BitmapImage solOpenFolderIcon;

        public SolutionManager(FiledropsFileSystem fs, DynamicProjectFormFactory dff, Assembly assembly = null)
            : base(fs, dff, assembly)
        {
            this.FileSystem = fs;
            solutions = new Dictionary<string, SolutionInfo>();
        }

        protected override Dictionary<string, ProjectInfo> GetAllProjects()
        {
            Dictionary<string, ProjectInfo> dict = new Dictionary<string, ProjectInfo>();
            foreach (SolutionInfo si in solutions.Values)
            {
                foreach (SolutionProjectInfo spi in si.projects.Values)
                {
                    dict.Add(spi.ProjectFile.FullName, spi);
                }
            }
            return dict;
        }

        public FolderTreeNode GetTreeNodeWithSolution(SolutionInfo si)
        {
            foreach (object n in this.solutionTree.Items)
            {
                if (n != null
                    && n is ISolutionNode
                    && (n as ISolutionNode).getSolution().Solutionfile.FullName.Equals(si.Solutionfile.FullName))
                {
                    return n as FolderTreeNode;
                }
            }
            return null;
        }

        public FolderTreeNode GetTreeNodeWithProject(SolutionProjectInfo spi)
        {
            foreach (object n in this.solutionTree.Items)
            {
                if (n != null
                    && n is IProjectNode
                    && (n as IProjectNode).GetProject().ProjectFile.FullName.Equals(spi.ProjectFile.FullName))
                {
                    return n as FolderTreeNode;
                }
            }
            return null;
        }

        public SolutionInfo GetSolutionWithPath(string path)
        {
            foreach (SolutionInfo solution in solutions.Values)
            {
                if (solution.Root.FullName == path)
                {
                    return solution;
                }
            }
            return null;
        }

        public override void initTree()
        {
            Dictionary<string, string> filter = new Dictionary<string, string>();
            for (int i = 0; i < ACCEPTED.Length; i++)
            {
                filter.Add(ACCEPTED[i], ACCEPTEDXTS[i]);
            }
            solutionTree = new SolutionFileTree(filter, this, preferencesfile != null);
            treeGrid = new Grid();
            solutionTree.ShowRoot = true;
            solutionTree.ShowFileExtensions = false;
            solutionTree.ShowFiles = true;
            solutionTree.ShowMenu = true;
            treeGrid.Children.Add(solutionTree);
            this.AddChild(treeGrid);
        }

        public void openSolution()
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //filedrops file dialog disabled because of necessary functionality improvements
                //FiledropsFile f = dialog.SelectedEntry as FiledropsFile;

                FiledropsDirectory dir = FileSystem.ConstructDirectory(dialog.SelectedPath);
                List<FiledropsFile> files = dir.GetFiles(".+[.]" + SXT, System.IO.SearchOption.TopDirectoryOnly);
                if (files == null || files.Count != 1)
                {
                    string messageBoxText = "This folder does not contain a valid solution file.";
                    string caption = "Filedrops";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Error;
                    MessageBox.Show(messageBoxText, caption, button, icon);
                }
                else
                {
                    FiledropsFile f = files[0];

                    if (f.Extension == "." + SXT && !solutions.ContainsKey(f.FullName))
                    {
                        initSolution(f);
                    }
                    else if (solutions.ContainsKey(f.FullName))
                    {
                        string messageBoxText = "This solution is opened already.";
                        string caption = "Filedrops";
                        MessageBoxButton button = MessageBoxButton.OK;
                        MessageBoxImage icon = MessageBoxImage.Warning;
                        MessageBox.Show(messageBoxText, caption, button, icon);
                    }
                }
            }
        }

        public FiledropsFileSystem GetFileSystemClone()
        {
            return this.FileSystem.Clone<FiledropsFileSystem>();
        }

        public void initSolution(FiledropsFile f)
        {
            FiledropsFileSystem sfs = this.FileSystem.Clone<FiledropsFileSystem>();
            sfs.WorkingDirectory = f.Parent;
            f.Parent.FileSystem = sfs;
            f.FileSystem = sfs;
            SolutionInfo si = new SolutionInfo(f, this, sfs);
            solutions.Add(f.FullName, si);
            solutionTree.addRoot(f.Parent, si);
        }

        public override void initProject(ProjectRootNode prn)
        {
            if (prn == null
                || prn.Project == null
                || !(prn.Project is SolutionProjectInfo))
            {
                return;
            }

            SolutionProjectInfo spi = (prn.Project as SolutionProjectInfo);
            spi.solution.addProject(spi);
            prn.Items.Clear();
            this.solutionTree.buildProjectFolder(prn);
        }

        public override void closeProject(ProjectInfo pi)
        {
            if (!(pi is SolutionProjectInfo)) return;
            SolutionProjectInfo solutionProject = pi as SolutionProjectInfo;
            solutionProject.solution.closeProject(solutionProject);
            this.solutionTree.Items.Remove(GetTreeNodeWithProject(solutionProject));
        }

        public void closeSolution(SolutionInfo si)
        {
            if (si != null)
            {
                FiledropsFile f = si.Solutionfile;

                if (solutions.ContainsKey(f.FullName) && si.closeSolution())
                {
                    solutions.Remove(f.FullName);
                    foreach (TreeViewItem item in solutionTree.Items)
                    {
                        if ((item as SolutionRootNode).Entry == f.Parent)
                        {
                            solutionTree.Items.Remove(item);
                            break;
                        }
                    }
                }
            }
        }

        public void closeSolutionFromTree()
        {
            closeSolution(GetCurrentSolution());
        }

        public void createNewSolution()
        {
            FiledropsFileSystem sfs = this.FileSystem.Clone<FiledropsFileSystem>();

            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FiledropsDirectory newdir = sfs.ConstructDirectoryRecursive(Path.Combine(dialog.SelectedPath, "Solution"));

                FiledropsFile solutionfile = sfs.ConstructFile(Path.Combine(newdir.FullName, newdir.Name + "." + SolutionManager.SXT));
                solutionfile.Create();

                sfs.WorkingDirectory = solutionfile.Parent;

                SolutionInfo si = new SolutionInfo(solutionfile, this, sfs);
                solutions.Add(solutionfile.FullName, si);

                solutionfile.Parent.FileSystem = sfs;
                solutionfile.FileSystem = sfs;

                solutionTree.addRootAndRename(solutionfile.Parent, si);
            }
        }

        public void renameSolution(string oldname, string newname, SolutionInfo si)
        {
            this.solutions.Remove(oldname);
            this.solutions.Add(newname, si);
        }

        public SolutionProjectInfo GetCurrentProject()
        {
            FileSystemEntryNode current = this.solutionTree.SelectedItem as FileSystemEntryNode;
            SolutionProjectInfo project = null;
            if (current != null)
            {
                project = GetProjectFromSystemEntry(current.Entry) as SolutionProjectInfo;
            }
            if (project == null)
            {
                project = GetLastOpenProject();
            }
            return project;
        }

        public SolutionInfo GetCurrentSolution()
        {
            FileSystemEntryNode current = this.solutionTree.SelectedItem as FileSystemEntryNode;
            SolutionInfo solution = null;
            if (current != null)
            {
                solution = GetSolutionFromSystemEntry(current.Entry);
            }
            if (solution == null)
            {
                solution = GetLastOpenSolution();
            }
            return solution;
        }

        public override ProjectInfo GetProjectFromSystemEntry(FiledropsFileSystemEntry e)
        {
            return GetProjectFromSystemEntry(e, GetAllProjects());
        }

        private SolutionInfo GetSolutionFromSystemEntry(FiledropsFileSystemEntry e)
        {
            FiledropsFileSystemEntry current = e;

            while (current != null
                && current.FullName != null
                && !solutions.ContainsKey(Path.Combine(current.FullName, current.Name + "." + SXT)))
            {
                current = current.Parent;
            }

            if (current != null && solutions.ContainsKey(Path.Combine(current.FullName, current.Name + "." + SXT)))
            {
                return solutions[Path.Combine(current.FullName, current.Name + "." + SXT)];
            }
            return null;
        }

        private SolutionProjectInfo GetLastOpenProject()
        {
            ProjectInfo project = null;
            foreach (FileSystemEntryNode n in this.solutionTree.Items)
            {
                if (n != null
                    && n is IProjectNode
                    && (n as IProjectNode).GetProject() != null)
                {
                    project = (n as IProjectNode).GetProject();
                }
            }
            return project as SolutionProjectInfo;
        }

        private SolutionInfo GetLastOpenSolution()
        {
            SolutionInfo solution = null;
            foreach (FileSystemEntryNode n in this.solutionTree.Items)
            {
                if (n != null
                    && n is ISolutionNode
                    && (n as ISolutionNode).getSolution() != null)
                {
                    solution = (n as ISolutionNode).getSolution();
                }
            }
            return solution;
        }

        public override void createNewProject()
        {
            SolutionInfo currentSolution = GetCurrentSolution();
            if (currentSolution != null)
            {
                FiledropsFileSystem pfs = this.FileSystem.Clone<FiledropsFileSystem>();

                FiledropsDirectory newdir = pfs.ConstructDirectoryRecursive(Path.Combine(currentSolution.Root.FullName, "Project"));
                initTopDirs(newdir);

                FiledropsFile projectfile = pfs.ConstructFile(Path.Combine(newdir.FullName, newdir.Name + "." + ProjectManager.PXT));
                projectfile.Create();

                pfs.WorkingDirectory = projectfile.Parent;
                projectfile.Parent.FileSystem = pfs;
                projectfile.FileSystem = pfs;

                currentSolution.addProject(projectfile);

                FolderTreeNode i = GetTreeNodeWithSolution(currentSolution);
                if (i != null)
                {
                    i.IsExpanded = true;
                    i.RebuildExpandedTreeNode(i);
                }

                solutionTree.addProjectToSolutionAndRename(projectfile, currentSolution);

                if (preferencesfile != null)
                    XmlSchemaUtilities.generateCleanXml(projectfile.FullName, Environment.CurrentDirectory + @"\Resources\DocUI\preferences.xsd", ".");
            }
        }

        public override void deleteProject()
        {
            /*
                        FileSystemEntryNode item = this.solutionTree.SelectedItem as FileSystemEntryNode;
                        GetProjectFromSystemEntry(item.Entry).closeProject();
                        if (item.removeNode())
                        {
                            this.solutionTree.removeNode(item);
                        }
            */
            FolderTreeNode item = this.solutionTree.SelectedItem as FolderTreeNode;
            if (item != null && item is ISolutionProjectNode)
            {
                SolutionProjectInfo spi = (item as ISolutionProjectNode).getSolutionProject();
                if (spi != null)
                {
                    spi.solution.closeProject(spi);
                }
                if (item.removeNode())
                {
                    this.solutionTree.removeNode(item);
                }
            }
        }
    }
}

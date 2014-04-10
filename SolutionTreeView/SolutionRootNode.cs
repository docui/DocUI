using Org.DocUI.Solution;
using Org.Filedrops.FileSystem;
using Org.Filedrops.FileSystem.UI.FileTreeView;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;


namespace Org.DocUI.SolutionTreeView
{
    public class SolutionRootNode : RootNode, ISolutionNode
    {
        private SolutionManager manager;
        public SolutionManager SolutionManager { get { return manager; } }

        public SolutionInfo Solution { get; set; }

        public SolutionRootNode(SolutionManager manager, SolutionInfo si, FiledropsDirectory di, Binding showext, FolderTreeNodeFilter filter = null)
            : base(di, showext, filter)
        {
            this.Solution = si;
            this.manager = manager;
            this.Tag = "Solution";
        }

        public SolutionInfo getSolution()
        {
            return this.Solution;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public override void doEdit(object sender, PropertyChangedEventArgs args)
        {
            SolutionManager manager = this.Solution.Manager;
            manager.closeSolution(this.Solution);
            //rename folder
            string oldname = this.Entry.FullName;
            try
            {
                this.Entry.Rename(this.TextBlock.Text);
                this.Solution.Solutionfile.FullName = Path.Combine(this.Entry.FullName, Solution.Solutionfile.Name);

                try
                {
                    this.Solution.Solutionfile.Rename(this.Entry.Name + "." + SolutionManager.SXT);
                    manager.initSolution(this.Solution.Solutionfile);
                }
                catch
                {
                    this.Entry.Rename(oldname);
                }
            }
            catch
            {
                manager.initSolution(this.Solution.Solutionfile);
            }
        }

        public override FolderTreeNode createFolderNode(FiledropsDirectory dir, FolderTreeNodeFilter filter, bool recursive = true)
        {
            if (filter == null)
                return new SolutionProjectRootNode(manager, this.Solution.GetSolutionProjectWithDirectory(dir), dir, this.ShowExtBinding, this.filter);
            else
                return new SolutionProjectRootNode(manager, this.Solution.GetSolutionProjectWithDirectory(dir), dir, this.ShowExtBinding, filter);
        }
    }
}

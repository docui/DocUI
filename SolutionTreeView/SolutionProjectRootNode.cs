using Org.DocUI.Project;
using Org.DocUI.ProjectTreeView;
using Org.DocUI.Solution;
using Org.Filedrops.FileSystem;
using Org.Filedrops.FileSystem.UI.FileTreeView;
using System.Windows.Data;

namespace Org.DocUI.SolutionTreeView
{
    public class SolutionProjectRootNode : ProjectRootNode, ISolutionProjectNode
    {
        private new SolutionManager manager;
        public new SolutionManager ProjectManager { get { return manager; } }

        private new SolutionProjectInfo pi;

        public new SolutionProjectInfo Project
        {
            get { return pi; }
            set { pi = value; }
        }

        public SolutionProjectRootNode(SolutionManager manager, SolutionProjectInfo pi, FiledropsDirectory di, Binding showext, FolderTreeNodeFilter filter)
            : base(manager, pi, di, showext)
        {
            this.filter = filter;
            this.pi = pi;
            this.manager = manager;
        }

        public override ProjectInfo GetProject()
        {
            return this.Project;
        }

        public SolutionProjectInfo getSolutionProject()
        {
            return this.Project;
        }

        /// <summary>
        /// Removes the folder the node represents
        /// </summary>
        public override bool removeNode()
        {
            bool success = base.removeNode();
            if (success)
            {
                this.Project.solution.closeProject(this.Project);
            }
            return success;
        }
    }
}

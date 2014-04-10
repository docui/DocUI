using Org.DocUI.Project;
using Org.Filedrops.FileSystem;
using Org.Filedrops.FileSystem.UI.FileTreeView;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;

namespace Org.DocUI.ProjectTreeView
{
    public class ProjectRootNode : RootNode, IProjectNode
    {
        private readonly ProjectManager manager;
        public ProjectManager ProjectManager { get { return manager; } }

        private ProjectInfo pi;

        public ProjectInfo Project
        {
            get { return pi; }
            set { pi = value; }
        }

        public ProjectRootNode(ProjectManager manager, ProjectInfo pi, FiledropsDirectory di, Binding showext)
            : base(di, showext)
        {
            this.pi = pi;
            this.manager = manager;
            Tag = "Project";
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public override void doEdit(object sender, PropertyChangedEventArgs args)
        {
            ProjectManager manager = this.pi.Manager;

            //close project
            manager.closeProject(this.pi);
            //Rename folder
            string oldname = this.Entry.FullName;
            try
            {
                this.Entry.Rename(this.TextBlock.Text);
                this.pi.ProjectFile.FullName = Path.Combine(this.Entry.FullName, pi.ProjectFile.Name);

                try
                {
                    this.pi.ProjectFile.Rename(this.Entry.Name + "." + ProjectManager.PXT);
                    manager.initProject(this);
                }
                catch
                {
                    this.Entry.Rename(oldname);
                }
            }
            catch
            {
                manager.initProject(this);
            }
        }

        public override FileTreeNode CreateFileNode(FiledropsFile file)
        {
            return new ProjectFileNode(this.Project, file, this.ShowExtBinding);
        }

        public override FolderTreeNode createFolderNode(FiledropsDirectory dir, FolderTreeNodeFilter filter, bool recursive = true)
        {
            if (filter == null)
                return new ProjectFolderNode(pi, dir, this.ShowExtBinding, this.filter);
            else
                return new ProjectFolderNode(pi, dir, this.ShowExtBinding, filter);
        }

        public virtual ProjectInfo GetProject()
        {
            return this.Project;
        }
    }
}

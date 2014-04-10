using Org.DocUI.Project;
using Org.Filedrops.FileSystem;
using Org.Filedrops.FileSystem.UI.FileTreeView;
using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace Org.DocUI.ProjectTreeView
{
    public class ProjectFolderNode : FolderTreeNode, IProjectNode
    {
        private ProjectInfo pi;

        public ProjectInfo Project
        {
            get { return pi; }
            set { pi = value; }
        }

        public ProjectFolderNode(ProjectInfo pi, FiledropsDirectory di, Binding showext, FolderTreeNodeFilter filter = null)
            : base(di, showext, filter)
        {
            this.pi = pi;
            //this.ContextMenuOpening
            //pi.Manager
        }

        public override FileTreeNode CreateFileNode(FiledropsFile file)
        {
            return new ProjectFileNode(this.Project, file, this.ShowExtBinding);
        }

        public override FolderTreeNode createFolderNode(FiledropsDirectory dir, FolderTreeNodeFilter filter = null, bool recursive = true)
        {
            if (filter == null)
                return new ProjectFolderNode(pi, dir, this.ShowExtBinding, this.filter);
            else
                return new ProjectFolderNode(pi, dir, this.ShowExtBinding, filter);
        }

        public override void RenameNode()
        {
            foreach (TreeViewItem item in this.Items)
            {
                if (item is ProjectFileNode)
                {
                    (item as ProjectFileNode).CloseFile();
                }
            }
            if (!(this.Tag.ToString().Contains("TopFolder")))
                base.RenameNode();
        }

        public override void CreateFile()
        {
            this.IsExpanded = true;
            FiledropsFile file = this.pi.Manager.NewFile(filter.getFirstExtension(), this.Entry as FiledropsDirectory);
            if (file != null)
            {
                FileTreeNode node = this.CreateFileNode(file);
                this.Items.Add(node);
                node.RenameNode();
                Project.Manager.openDocument(node.Entry as FiledropsFile, Project);
            }
        }

        public ProjectInfo GetProject()
        {
            return this.Project;
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            if (this.IsSelected)
            {
                //var things = this.ContextMenu.Items.select(x => x != Separator()).asenumerable();

                foreach (var item in this.ContextMenu.Items)
                {
                    if (item.GetType().Equals(typeof(MenuItem)))
                    {
                        MenuItem mi = (MenuItem)item;

                        if ((String)mi.Name == "AddFile")
                        {
                            mi.Header = "Add " + ProjectManager.TYPES[
                                Array.IndexOf(ProjectManager.ACCEPTEDXTS, filter.getFirstExtension())];
                            ((Image)mi.Icon).Source = ProjectManager.FILE_ICONS[filter.getFirstExtension()];
                            break;
                        }
                    }
                }
            }
        }
    }
}

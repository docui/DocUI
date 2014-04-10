using Org.DocUI.Project;
using Org.Filedrops.FileSystem;
using Org.Filedrops.FileSystem.UI.FileTreeView;
using System.Windows.Data;

namespace Org.DocUI.ProjectTreeView
{
    public class ProjectFileNode : FileTreeNode, IProjectNode
    {
        private ProjectInfo _pi;
        private string _oldpath;

        public ProjectInfo Project
        {
            get { return _pi; }
            set { _pi = value; }
        }

        public ProjectFileNode(ProjectInfo pi, FiledropsFile fi, Binding showext)
            : base(fi, showext)
        {
            this._pi = pi;
            this.MouseDoubleClick += (sender, args) => { pi.Open(fi); };
        }

        public override void RenameNode()
        {
            _oldpath = this.Entry.FullName;
            base.RenameNode();
        }

        public override void doEdit(object sender, System.ComponentModel.PropertyChangedEventArgs args)
        {
            base.doEdit(sender, args);
            if (this.Entry.FullName != _oldpath)
            {
                Project.Rename(_oldpath, this.Entry.FullName, this.Entry.NameWithoutExtension);
            }
        }

        public ProjectInfo GetProject()
        {
            return this.Project;
        }

        public override bool removeNode()
        {
            bool success = base.removeNode();
            if (success)
            {
                this.CloseFile();
            }
            return success;
        }

        public void CloseFile()
        {
            this.Project.closeTab(this.Entry.FullName);
        }
    }
}

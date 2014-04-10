using Org.Filedrops.FileSystem;
using System.Windows;

namespace Org.DocUI.FormBuilder.Templates
{
    /// <summary>
    /// This option displays a tree of files. Only files of a specific extension are displayed.
    /// You can select a file with this.
    /// </summary>
    public partial class Selector : Window
    {
        private string _ext;

        /// <summary>
        /// This creates a new instance of the Selector.
        /// To display it, you still need to call "showDialog()".
        /// </summary>
        /// <param name="f">The project file (fdproj). This file will determine the root directory of this component.</param>
        /// <param name="ext">The extension to display. (Extensions should be named with a dot at the start)</param>
        public Selector(FiledropsDirectory dir, string ext, string folder)
        {
            InitializeComponent();

            this._ext = ext;
            view.ShowFiles = true;
            view.ShowRoot = false;
            view.Filters = new string[] { ext.Substring(1) };
            view.AcceptedDirs = new string[] { folder };
            FillTree(dir);
        }

        private void FillTree(FiledropsDirectory dir)
        {
            this.view.addRoot(dir);
        }
    }
}

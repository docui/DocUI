using Org.Filedrops.FileSystem;
using System;
using System.Windows.Controls;

namespace Org.DocUI.FormBuilder.Templates
{
    /// <summary>
    /// A function that returns a file.
    /// </summary>
    /// <returns>the assigned file.</returns>
    public delegate string GetFile();

    /// <summary>
    /// Edit EdButton opens the selected file in a new tab.
    /// </summary>
    public partial class EditButton : UserControl
    {
        /// <summary>
        /// Function which will get the file that needs to be opened.
        /// </summary>
        public GetFile Getter { get; set; }
        private readonly DynamicProjectForm _parent;
        private readonly string _ext;

        /// <summary>
        /// Creates a new instance of the EditButton.
        /// </summary>
        /// <param name="pi">Files that will be opened, will always be of this project.</param>
        /// <param name="ext">Files that will be opened, will always have this extendsion.</param>
        public EditButton(string ext, DynamicProjectForm parent)
        {
            this._parent = parent;
            this._ext = ext;
            InitializeComponent();
        }

        /// <summary>
        /// When edit EdButton gets clicked, a file should be opened.
        /// The file is fetched from the Getter property.
        /// </summary>
        /// <param name="sender">isn't used</param>
        /// <param name="e">isn't used</param>
        private void editButton_Click(object sender, EventArgs e)
        {
            string path = Getter();
            if (path != "")
            {
                FiledropsFile f = _parent.ProjectSystem.ConstructFile(Getter());
                // There should be a file           
                if (f != null && f.Extension == _ext)
                {
                    _parent.Project.Open(f);
                }
            }
        }
    }
}

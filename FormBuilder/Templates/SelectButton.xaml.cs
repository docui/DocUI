using System;
using System.IO;
using System.Windows.Controls;

namespace Org.DocUI.FormBuilder.Templates
{
    public delegate void ValueChanged(string file);

    /// <summary>
    /// This option displays a EdButton. Clicking the EdButton will Open a Selector.
    /// </summary>
    public partial class SelectButton : UserControl
    {
        private readonly string _ext;
        private readonly string _folder;

        //filedrops file dialog disabled because of necessary functionality improvements
        //private Selector _sel;

        private System.Windows.Forms.OpenFileDialog _sel;
        public ValueChanged Event;
        private readonly DynamicProjectForm _parentForm;

        /// <summary>
        /// Gets or sets the file that has been selected by the selector.
        /// </summary>
        private string _selectedFile;
        public string SelectedFile
        {
            get
            {
                return _selectedFile;
            }

            set
            {
                _selectedFile = value;
                Event(_selectedFile);
            }
        }

        /// <summary>
        /// Creates a new instance of the Selectbutton.
        /// </summary>
        /// <param name="ext">The extension that should be displayed by the Selector.</param>
        /// <param name="parentForm">The form of which this EdButton is a part.</param>
        public SelectButton(string ext, string folder, DynamicProjectForm parentForm)
        {
            InitializeComponent();
            this._ext = ext;
            this._folder = folder;
            this._parentForm = parentForm;
        }

        private void selectbutton_Click(object sender, EventArgs args)
        {
            //filedrops file dialog disabled because of necessary functionality improvements            
            //_sel = new Selector(_parentForm.ProjectSystem.WorkingDirectory, _ext, _folder);
            //_sel.Select.Click += response;
            //_sel.View.MouseDoubleClick += response;
            //_sel.Cancel.Click += (object other_sender, RoutedEventArgs e) => { _sel.Close(); };

            _sel = new System.Windows.Forms.OpenFileDialog();
            if (_folder != "")
            {
                _sel.InitialDirectory = Path.Combine(_parentForm.ProjectSystem.WorkingDirectory.FullName, _folder);
            }
            if (_ext != "")
            {
                _sel.DefaultExt = _ext;
            }

            if (_sel.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SelectedFile = _sel.FileName;
            }
        }

        //filedrops file dialog disabled because of necessary functionality improvements            
        /*
		private void response(object sender, RoutedEventArgs e)
		{
            if (_sel != null && _sel.View.SelectedValue != null)
			{
				SelectedFile = ((_sel.View.SelectedValue as FileSystemEntryNode).Entry).FullName;
				_sel.Close();
			}
		}
        */
    }
}

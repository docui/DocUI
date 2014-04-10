using Com.Xploreplus.Common;
using Microsoft.Win32;
using Org.DocUI.Tools;
using Org.DocUI.Wpf;
using Org.Filedrops.FileSystem;
using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Schema;

namespace Org.DocUI.FormBuilder.Components
{
    public class DocUIFile : AbstractDocUIComponent
    {
        private readonly ExtendedTextBox _tb;
        /// <summary>
        /// EdButton that opens the dialog.
        /// </summary>
        private readonly string _ext;
        private readonly string _folder;
        private readonly Boolean _relative;
        private Boolean _required;
        private readonly string _projectpath;

        private DynamicProjectForm parentForm;
        private ContextMenu menu;

        /// <summary>
        /// Creates a new instance of the FileOption
        /// </summary>
        /// <param name="xmlNode">The xmlNode containing the data.</param>
        /// <param name="xsdNode">The corresponding xsdNode</param>
        /// <param name="panel">The panel on which this option should be placed.</param>
        /// <param name="parentForm">The form of which this option is a part.</param>
        /// <param name="isMetaName">Whether metadata data is allowed and meta name or value should be chosen.</param>
        public DocUIFile(XmlNode xmlNode, XmlSchemaAnnotated xsdNode, Panel contentpanel, Panel overlaypanel, DynamicProjectForm parentForm)
            : base(xmlNode, xsdNode, contentpanel, overlaypanel, parentForm)
        {
            //init variables
            this.parentForm = parentForm;

            _projectpath = parentForm.ProjectSystem.WorkingDirectory.FullName;
            _ext = XmlSchemaUtilities.tryGetUnhandledAttribute(xsdNode, "_ext");
            _folder = XmlSchemaUtilities.tryGetUnhandledAttribute(xsdNode, "_folder");
            string tmprel = XmlSchemaUtilities.tryGetUnhandledAttribute(xsdNode, "_relative");
            _relative = tmprel == "true" ? true : false;
            string tmpreq = XmlSchemaUtilities.tryGetUnhandledAttribute(xsdNode, "_required");
            _required = tmpreq == "true" ? true : false;

            DockPanel panel = new DockPanel();
            StackPanel buttons = new StackPanel();
            //panel.Orientation = Orientation.Horizontal;
            panel.HorizontalAlignment = HorizontalAlignment.Stretch;
            buttons.Orientation = Orientation.Horizontal;
            buttons.HorizontalAlignment = HorizontalAlignment.Right;

            // init the Components
            menu = new ContextMenu();
            MenuItem OpenMenu = new MenuItem();
            OpenMenu.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Open, 16);
            OpenMenu.Click += OpenMenu_Click;
            OpenMenu.Header = "Open";
            menu.Items.Add(OpenMenu);

            if (_folder != "")
            {
                MenuItem EditMenu = new MenuItem();
                EditMenu.Click += EditMenu_Click;
                EditMenu.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Edit, 16);
                EditMenu.Header = "Edit";
                menu.Items.Add(EditMenu);
            }

            MenuItem RemoveMenu = new MenuItem();
            RemoveMenu.Click += RemoveMenu_Click;
            RemoveMenu.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Close, 16);
            RemoveMenu.Header = "Remove";
            menu.Items.Add(RemoveMenu);

            Image DropMenuImage = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Shortcut, 16);
            DropMenuImage.ContextMenu = menu;
            DropMenuImage.MouseEnter += DropMenuImage_MouseEnter;

            _tb = new ExtendedTextBox("", "", "", _required, parentForm);
            _tb.TextBlock.TextChanged += (s, e) => { hasPendingChanges(); };

            //positioning
            DockPanel.SetDock(_tb, Dock.Left);
            DockPanel.SetDock(DropMenuImage, Dock.Right);
            panel.LastChildFill = true;
            _tb.HorizontalAlignment = HorizontalAlignment.Stretch;

            panel.Children.Add(DropMenuImage);
            panel.Children.Add(_tb);

            //init drag en drop extras
            _tb.TextBlock.PreviewDragEnter += DropTextBox_Drag;
            _tb.TextBlock.PreviewDrop += DropTextBox_Drop;
            _tb.TextBlock.PreviewDragOver += DropTextBox_Drag;
            _tb.AllowDrop = true;

            Control = panel;
            setup();
        }

        void DropMenuImage_MouseEnter(object sender, MouseEventArgs e)
        {
            menu.IsOpen = true;
        }

        /// <summary>
        /// Adds an item to the list. 
        /// Is executed by the SeButton.
        /// </summary>
        /// <param name="path">the file that should be added</param>
        public virtual void OpenMenu_Click(object sender, EventArgs args)
        {
            System.Windows.Forms.OpenFileDialog sel = new System.Windows.Forms.OpenFileDialog();
            if (_folder != "")
            {
                sel.InitialDirectory = Path.Combine(parentForm.ProjectSystem.WorkingDirectory.FullName, _folder);
            }
            if (_ext != "")
            {
                sel.Filter = _ext + " file|*." + _ext;
                sel.DefaultExt = _ext;
            }

            if (sel.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                AddNewFile(sel.FileName);
            }
        }

        private void AddNewFile(string path)
        {
            if (path != "" && _relative)
            {
                Value = WebUtility.UrlDecode((new Uri(parentForm.FI.FullName)).MakeRelativeUri(new Uri(path)).ToString());
            }
            else
            {
                Value = path;
            }
        }

        /// <summary>
        /// Removes selected items from the View
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void RemoveMenu_Click(object sender, EventArgs args)
        {
            _tb.TextBlock.Text = "";
        }

        private void EditMenu_Click(object sender, EventArgs args)
        {
            if (_tb.TextBlock.Text != "")
            {
                string path = _tb.TextBlock.Text;
                if (_relative)
                {
                    path = Path.GetFullPath(Path.Combine(parentForm.FI.Parent.FullName, path));
                }
                FiledropsFile f = parentForm.ProjectSystem.ConstructFile(path);
                // There should be a file
                if (f != null && f.Extension == "." + _ext)
                {
                    parentForm.Project.Open(f);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the textbox. (the textbox should always contain a fileName)
        /// </summary>
        public override object Value
        {
            get { return _tb.TextBlock.Text; }
            set { _tb.TextBlock.Text = value.ToString(); }
        }

        /// <summary>
        /// Function that opens a file dialog. Once a file is selected, it's path will be filled in the textbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void fileChooser(object sender, EventArgs args)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                if (dlg.CheckFileExists && dlg.CheckPathExists)
                {
                    _tb.TextBlock.Text = dlg.FileName;
                }
            }
        }

        private void DropTextBox_Drag(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                FileInfo fi = new FileInfo(files[0]);
                e.Effects = DragDropEffects.None;

                if (fi.Extension.Equals("." + this._ext))
                {
                    if (_folder != "")
                    {
                        if (fi.FullName.StartsWith(Path.Combine(_projectpath, _folder)))
                        {
                            e.Effects = DragDropEffects.Move;
                        }
                    }
                    else
                    {
                        e.Effects = DragDropEffects.Move;
                    }
                }
                e.Handled = true;
            }
        }

        private void DropTextBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                FileInfo fi = new FileInfo(files[0]);

                AddNewFile(fi.FullName);
            }
        }


        public override bool CheckValid()
        {
            this._tb.CheckRegex(this, null);
            return this._tb.Valid();
        }
    }
}

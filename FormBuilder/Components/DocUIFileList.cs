using Com.Xploreplus.Common;
using Org.DocUI.Tools;
using Org.Filedrops.FileSystem;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Schema;
using WPF.JoshSmith.ServiceProviders.UI;

namespace Org.DocUI.FormBuilder.Components
{
    public class DocUIFileList : AbstractDocUIComponent
    {
        /// <summary>
        /// Collection of all the items in the list
        /// </summary>
        protected ObservableCollection<string> ViewCollection = new ObservableCollection<string>();
        /// <summary>
        /// the list that contains all the items
        /// </summary>
        protected ListView View;
        private readonly string ext;
        private readonly string folder;
        private readonly Boolean relative = false;
        private readonly string projectpath;
        private readonly DynamicProjectForm parentForm;
        private readonly ContextMenu menu;
        protected bool ordered = false;

        /// <summary>
        /// gets or sets the value of the list. 
        /// This means it will fill in the list with the right data, or return a list of items.
        /// </summary>
        public override object Value
        {
            get { return ViewCollection; }
            set
            {
                foreach (XmlNode n in xmlNode.ChildNodes)
                    ViewCollection.Add(n.InnerText);
            }
        }

        /// <summary>
        /// Creates a new instance of the ListSelectOption.
        /// </summary>
        /// <param name="xmlNode">The xmlNode that contains the data for the list</param>
        /// <param name="xsdNode">The corresponding xsdNode</param>
        /// <param name="p">The panel on which the option should be placed</param>
        /// <param name="ordered">Whether the list should be ordered or not.</param>
        public DocUIFileList(XmlNode xmlNode, XmlSchemaAnnotated xsdNode, Panel contentpanel, Panel overlaypanel, DynamicProjectForm parentForm)
            : base(xmlNode, xsdNode, contentpanel, overlaypanel, parentForm)
        {
            //init variables
            this.parentForm = parentForm;
            this.projectpath = parentForm.ProjectSystem.WorkingDirectory.FullName;
            ext = XmlSchemaUtilities.tryGetUnhandledAttribute(xsdNode, "ext");
            folder = XmlSchemaUtilities.tryGetUnhandledAttribute(xsdNode, "folder");
            string tmprel = XmlSchemaUtilities.tryGetUnhandledAttribute(xsdNode, "relative");
            relative = tmprel == "true" ? true : false;

            DockPanel dock = new DockPanel();

            // init the grid
            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition());
            g.ColumnDefinitions.Add(new ColumnDefinition());
            g.RowDefinitions.Add(new RowDefinition());
            g.RowDefinitions.Add(new RowDefinition());

            // init the Components
            menu = new ContextMenu();
            MenuItem OpenMenu = new MenuItem();
            OpenMenu.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Open, 16);
            OpenMenu.Click += OpenMenu_Click;
            OpenMenu.Header = "Add";
            menu.Items.Add(OpenMenu);

            if (folder != "")
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

            StackPanel stack = new StackPanel();
            stack.Children.Add(DropMenuImage);
            DockPanel.SetDock(stack, Dock.Right);
            dock.Children.Add(stack);

            View = new ListView() { ItemsSource = ViewCollection };
            View.Height = 50;
            ((INotifyCollectionChanged)ViewCollection).CollectionChanged += (s, e) => { hasPendingChanges(); };
            dock.Children.Add(View);

            //init drag en drop extras
            this.View.DragEnter += DropList_Drag;
            this.View.Drop += DropList_Drop;
            this.View.DragOver += DropList_Drag;
            this.View.AllowDrop = true;

            // set the Control
            this.Control = dock;

            // make the list ordered
            if (ordered)
                new ListViewDragDropManager<string>(View);

            setup();
        }

        void DropMenuImage_MouseEnter(object sender, MouseEventArgs e)
        {
            menu.IsOpen = true;
        }

        public override void placeOption()
        {
            // make the list ordered
            if (ordered)
                new ListViewDragDropManager<string>(View);
            base.placeOption();
        }

        /// <summary>
        /// Adds an item to the list. 
        /// Is executed by the SeButton.
        /// </summary>
        /// <param name="path">the file that should be added</param>
        public virtual void OpenMenu_Click(object sender, EventArgs args)
        {
            System.Windows.Forms.OpenFileDialog sel = new System.Windows.Forms.OpenFileDialog();
            if (folder != "")
            {
                sel.InitialDirectory = Path.Combine(parentForm.ProjectSystem.WorkingDirectory.FullName, folder);
            }
            if (ext != "")
            {
                sel.Filter = ext + " file|*." + ext;
                sel.DefaultExt = ext;
            }

            if (sel.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                AddNewFile(sel.FileName);
            }

        }

        private void AddNewFile(string path)
        {
            if (path != "" && relative)
            {
                ViewCollection.Add(WebUtility.UrlDecode((new Uri(parentForm.FI.FullName)).MakeRelativeUri(new Uri(path)).ToString()));
            }
            else
            {
                ViewCollection.Add(path);
            }
        }

        /// <summary>
        /// Removes selected items from the View
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void RemoveMenu_Click(object sender, EventArgs args)
        {
            for (int i = View.SelectedItems.Count - 1; i > -1; --i)
                ViewCollection.Remove(View.SelectedItems[i].ToString());
        }

        private void EditMenu_Click(object sender, EventArgs args)
        {
            if (View.SelectedItems.Count > 0)
            {
                string path = View.SelectedValue.ToString();
                if (relative)
                {
                    path = Path.GetFullPath(Path.Combine(parentForm.FI.Parent.FullName, path));
                }
                FiledropsFile f = parentForm.ProjectSystem.ConstructFile(path);
                // There should be a file
                if (f != null && f.Extension == "." + ext)
                {
                    parentForm.Project.Open(f);
                }
            }
        }

        /// <summary>
        /// Updates the xmlNode to the current content of the list.
        /// </summary>
        public override void update()
        {
            xmlNode.RemoveAll();
            XmlSchemaElement schemaEl = xsdNode as XmlSchemaElement;
            if (schemaEl != null)
            {
                XmlSchemaSequence seq = XmlSchemaUtilities.tryGetSequence(schemaEl.ElementSchemaType);
                if (seq != null && seq.Items.Count > 0)
                {
                    XmlNode node = xmlNode.OwnerDocument.CreateElement((seq.Items[0] as XmlSchemaElement).QualifiedName.Name);
                    foreach (string file in Value as ObservableCollection<string>)
                    {
                        XmlNode newNode = node.Clone();
                        newNode.InnerText = file;
                        xmlNode.AppendChild(newNode);
                    }
                }
                else if ((Value as ObservableCollection<string>).Count == 1)
                {
                    xmlNode.InnerText = (Value as ObservableCollection<string>)[0];
                }
                else
                {
                    Log.Error("[ListSelectDocUIComponent] Could not find the nodes to save the list in");
                }
            }
        }

        private void DropList_Drag(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                FileInfo fi = new FileInfo(files[0]);
                e.Effects = DragDropEffects.None;

                if (fi.Extension.Equals("." + this.ext))
                {
                    if (folder != "")
                    {
                        if (fi.FullName.StartsWith(Path.Combine(projectpath, folder)))
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

        private void DropList_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                FileInfo fi = new FileInfo(files[0]);

                AddNewFile(fi.FullName);
            }
        }
    }

}

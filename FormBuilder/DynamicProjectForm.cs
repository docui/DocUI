using Org.DocUI.Project;
using Org.Filedrops.FileSystem;
using System;
using System.Windows;
using System.Windows.Input;
using System.Xml;

namespace Org.DocUI.FormBuilder
{
    public class DynamicProjectForm : DynamicForm
    {
        /// <summary>
        /// The project the form is for
        /// </summary>
        public ProjectInfo Project;

        /// <summary>
        /// The xml file that is the back bone of this form.
        /// </summary>
        public FiledropsFile FI { get; set; }

        /// <summary>
        /// The FileSystem of the project
        /// </summary>
        public FiledropsFileSystem ProjectSystem { get; private set; }

        /// <summary>
        /// Saves the xml file.
        /// </summary>
        public static DependencyProperty SaveCommandProperty
            = DependencyProperty.Register(
                "SaveCommand",
                typeof(ICommand),
                typeof(DynamicForm));

        public ICommand SaveCommand
        {
            get
            {
                return (ICommand)GetValue(SaveCommandProperty);
            }

            set
            {
                SetValue(SaveCommandProperty, value);
            }
        }

        public override bool PendingChanges
        {
            get
            {
                return _pendingChanges;
            }
            set
            {
                if (value != _pendingChanges && Project.IsOpen(FI.FullName))
                {
                    _pendingChanges = value;
                    if (value)
                        this.Project.SetPendingChanges(this.FI.FullName);
                    else
                        this.Project.SetSaved(this.FI.FullName);
                }
            }
        }

        public DynamicProjectForm(FiledropsFile fi, ProjectInfo pi)
        {
            this.FI = fi;
            this.Project = pi;
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, saveFile));
            this.ProjectSystem = pi.FileSystem;

            XmlDocument doc = new XmlDocument();
            doc.Load(fi.FullName);

            Setup(doc);
        }

        public void openDocument(FiledropsFile f)
        {
            Project.Open(f);
        }

        /// <summary>
        /// Saves all pending changes to the appropriate xmlnode.
        /// Than the file is saved to the harddrive.
        /// </summary>
        /// <param name="sender">isn't used.</param>
        /// <param name="args">isn't used.</param>
        public void saveFile(object sender, EventArgs args)
        {
            bool valid = CheckValid();

            if (valid)
            {
                Xml.Save(FI.FullName);
                PendingChanges = false;
            }
            else
            {
                string messageBoxText = "The form is invalid, are you sure you want to save your changes?";
                string caption = "Filedrops";
                MessageBoxButton button = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Warning;
                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Xml.Save(FI.FullName);
                        PendingChanges = false;
                        break;
                }
            }
        }
    }
}

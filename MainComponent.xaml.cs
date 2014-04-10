using Com.Xploreplus.Common;
using Org.DocUI.FormBuilder;
using Org.DocUI.Project;
using Org.DocUI.Solution;
using Org.DocUI.Tools;
using Org.Filedrops.FileSystem.UI.FileTreeView;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml;

namespace Org.DocUI
{
    /// <summary>
    // The main structure of the docui
    /// </summary>
    public partial class MainComponent : UserControl
    {
        public static RoutedCommand OpenSolution = new RoutedCommand("OpenSolution", typeof(MainComponent));
        public static RoutedCommand NewSolution = new RoutedCommand("NewSolution", typeof(MainComponent));
        public static RoutedCommand CloseSolution = new RoutedCommand("CloseSolution", typeof(MainComponent));

        public static RoutedCommand OpenProject = new RoutedCommand("OpenProject", typeof(MainComponent));
        public static RoutedCommand NewProject = new RoutedCommand("NewProject", typeof(MainComponent));
        public static RoutedCommand CloseProject = new RoutedCommand("closeProject", typeof(MainComponent));
        public static RoutedCommand DeleteProject = new RoutedCommand("DeleteProject", typeof(MainComponent));

        public static RoutedCommand SaveForm = new RoutedCommand("SaveForm", typeof(MainComponent));
        public static RoutedCommand CloseForm = new RoutedCommand("CloseForm", typeof(MainComponent));

        private ProjectManager manager;

        public MainComponent(ProjectManager manager, string docuiconfig)
        {
            InitializeComponent();

            this.initManager(manager, docuiconfig);

            XmlDocument root = new XmlDocument();
            root.Load(manager.Assembly.GetManifestResourceStream(docuiconfig));
            if (root.DocumentElement.SelectSingleNode("Solution") != null)
            {
                this.SolutionMenu.Visibility = System.Windows.Visibility.Visible;
                this.OpenProjectMenu.Visibility = System.Windows.Visibility.Collapsed;
                this.CloseProjectMenu.Visibility = System.Windows.Visibility.Collapsed;
            }

            this.manager = manager;

            managerpanel.Content = manager;
            manager.DocPane = this.DocumentContainer;

            //this.dockingManager.Theme = new AeroTheme();
            //this.ApplyTheme("BureauBlue");
        }

        private void NewProjectMenu_Click(object sender, ExecutedRoutedEventArgs e)
        {
            manager.createNewProject();
        }

        private void OpenProjectMenu_Click(object sender, RoutedEventArgs e)
        {
            manager.OpenProject();
        }

        private void CloseProjectMenu_Click(object sender, ExecutedRoutedEventArgs e)
        {
            if (manager is ProjectManager)
            {
                manager.CloseProjectFormMain();
            }
        }

        private void CloseProjectMenu_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (manager == null)
            {
                e.CanExecute = false;
            }
            else if (manager is SolutionManager)
            {
                SolutionManager m = manager as SolutionManager;
                e.CanExecute = m.solutionTree != null
                                && m.solutionTree.SelectedItem != null
                                && m.GetProjectFromSystemEntry((m.solutionTree.SelectedItem as FileSystemEntryNode).Entry) != null;
            }
            else
            {
                e.CanExecute = manager.projectTree != null
                                && manager.projectTree.SelectedItem != null
                                && manager.GetProjectFromSystemEntry((manager.projectTree.SelectedItem as FileSystemEntryNode).Entry) != null;
            }
        }

        private void DeleteProjectMenu_Click(object sender, ExecutedRoutedEventArgs e)
        {
            manager.deleteProject();
        }

        private void DeleteProjectMenu_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            CloseProjectMenu_CanExecute(sender, e);
        }

        private void OpenSolutionMenu_Click(object sender, ExecutedRoutedEventArgs e)
        {
            if (manager is SolutionManager)
            {
                (manager as SolutionManager).openSolution();
            }
        }

        private void NewSolutionMenu_Click(object sender, ExecutedRoutedEventArgs e)
        {
            if (manager is SolutionManager)
            {
                (manager as SolutionManager).createNewSolution();
            }
        }

        private void CloseSolutionMenu_Click(object sender, ExecutedRoutedEventArgs e)
        {
            if (manager is SolutionManager)
            {
                (manager as SolutionManager).closeSolutionFromTree();
            }
        }

        private void CloseSolutionMenu_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (manager != null && manager is SolutionManager)
            {
                SolutionManager m = manager as SolutionManager;
                e.CanExecute = m.solutionTree != null
                                && m.solutionTree.SelectedItem != null
                                && m.GetCurrentSolution() != null;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void SaveFormMenu_Click(object sender, ExecutedRoutedEventArgs e)
        {
            manager.saveForm();
        }

        private void SaveFormMenu_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //e.CanExecute = manager.hasActiveForm();
            e.CanExecute = false;
            if (manager.hasActiveForm())
            {
                e.CanExecute = manager.getActiveForm().PendingChanges;
            }
        }

        private void CloseFormMenu_Click(object sender, ExecutedRoutedEventArgs e)
        {
            manager.closeForm();
        }

        private void CloseFormMenu_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = manager.hasActiveForm();
        }

        private void initManager(ProjectManager manager, String config)
        {
            InitMenuIcons();

            //var Assembly = Assembly.GetEntryAssembly();
            var assembly = manager.Assembly;

            //load docui config
            XmlDocument doc = new XmlDocument(); //EmbeddedResourceTools.GetXmlDocument(config, assembly);
            doc.Load(assembly.GetManifestResourceStream(config));

            XmlNode root = doc.DocumentElement;

            XmlNode componentsnode = root.SelectSingleNode("Components");
            if (componentsnode != null && componentsnode.InnerText != "")
            {
                string components = componentsnode.InnerText;
                XmlDocument fdcompdoc = new XmlDocument();
                string componentsstring = Environment.CurrentDirectory + components;
                fdcompdoc.Load(componentsstring);
                DynamicForm.InitComponents(fdcompdoc);
            }

            XmlNode currentNode = root;

            XmlNode solution = root.SelectSingleNode("Solution");
            if (solution != null && solution.InnerText != "")
            {
                SolutionManager.solFolderIcon = this.fetchIconFromXml("FolderIcon", solution, 16, assembly);
                SolutionManager.solOpenFolderIcon = this.fetchIconFromXml("OpenFolderIcon", solution, 16, assembly);

                currentNode = solution;
            }

            XmlNode project = currentNode.SelectSingleNode("Project");
            if (project != null && project.InnerText != "")
            {
                ProjectManager.projFolderIcon = this.fetchIconFromXml("FolderIcon", project, 16, assembly);
                ProjectManager.projOpenFolderIcon = this.fetchIconFromXml("OpenFolderIcon", project, 16, assembly);

                XmlNode prefnode = project.SelectSingleNode("Preferences");
                if (prefnode != null)
                {
                    ProjectManager.projFileIcon = this.fetchIconFromXml("FileIcon", prefnode, 16, assembly);
                    ProjectManager.preferencesfile = prefnode.SelectSingleNode("SchemaName").InnerText;
                }

                XmlNode dirsnode = project.SelectSingleNode("Directories");
                if (dirsnode != null)
                {
                    XmlNodeList dirs = dirsnode.SelectNodes("Directory");
                    int size = dirs.Count;
                    string[] accepted = new string[size];
                    string[] acceptedxts = new string[size];
                    string[] types = new string[size];
                    Dictionary<string, BitmapImage> openfoldericons = new Dictionary<string, BitmapImage>();
                    Dictionary<string, BitmapImage> closedfoldericons = new Dictionary<string, BitmapImage>();
                    Dictionary<string, BitmapImage> fileicons = new Dictionary<string, BitmapImage>();
                    List<string> noSchema = new List<string>();
                    int i = 0;
                    foreach (XmlNode node in dirs)
                    {
                        try
                        {
                            accepted[i] = node.SelectSingleNode("Name").InnerText;
                            acceptedxts[i] = node.SelectSingleNode("FileExtension").InnerText;
                            types[i] = node.SelectSingleNode("Type").InnerText;
                            XmlAttribute att = node.Attributes["NoSchema"];
                            if (att != null && att.InnerText == "true")
                            {
                                noSchema.Add(acceptedxts[i]);
                            }

                            BitmapImage img = fetchIconFromXml("OpenFolderIcon", node, 16, assembly);
                            if (img != null)
                            {
                                openfoldericons.Add(acceptedxts[i], img);
                            }
                            img = fetchIconFromXml("FolderIcon", node, 16, assembly);
                            if (img != null)
                            {
                                closedfoldericons.Add(acceptedxts[i], img);
                            }
                            img = fetchIconFromXml("FileIcon", node, 16, assembly);
                            if (img != null)
                            {
                                fileicons.Add(acceptedxts[i], img);
                            }
                            i++;
                        }
                        catch (NullReferenceException e)
                        {
                            //todo: solve this misconfig
                        }
                    }
                    ProjectManager.ACCEPTED = accepted;
                    ProjectManager.ACCEPTEDXTS = acceptedxts;
                    ProjectManager.TYPES = types;
                    ProjectManager.NoSchema = noSchema;

                    XmlNode projext = project.SelectSingleNode("FileExtension");
                    if (projext != null)
                    {
                        ProjectManager.PXT = projext.InnerText;
                    }

                    if (solution != null)
                    {
                        XmlNode solext = solution.SelectSingleNode("FileExtension");
                        if (solext != null)
                        {
                            SolutionManager.SXT = solext.InnerText;
                        }
                    }

                    ProjectManager.CLOSEDFOLDER_ICONS = closedfoldericons;
                    ProjectManager.OPENFOLDER_ICONS = openfoldericons;
                    ProjectManager.FILE_ICONS = fileicons;

                }
            }
            //safely initialize the tree
            manager.initTree();
        }

        private void InitMenuIcons()
        {
            /*
            SaveMenu.LargeImageSource = loadImage("Com.Xploreplus.DocUI.Resources.Images.menu.file-save", 16, Assembly.GetExecutingAssembly());
            OpenMenu.SmallImageSource = loadImage("Com.Xploreplus.DocUI.Resources.Images.menu.file-Open", 16, Assembly.GetExecutingAssembly());
            NewMenu.SmallImageSource = loadImage("Com.Xploreplus.DocUI.Resources.Images.menu.file-add", 16, Assembly.GetExecutingAssembly());
            CloseMenu.SmallImageSource = loadImage("Com.Xploreplus.DocUI.Resources.Images.menu.file-close", 16, Assembly.GetExecutingAssembly());
            */

            OpenProjectMenu.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Open, 16);
            NewProjectMenu.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Add, 16);
            CloseProjectMenu.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Close, 16);
            DeleteProjectMenu.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Delete, 16);

            SaveFormMenu.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Save, 16);
            CloseFormMenu.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Close, 16);

            OpenSolutionMenu.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Open, 16);
            NewSolutionMenu.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Add, 16);
            CloseSolutionMenu.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Close, 16);

            UndoMenu.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.TextIcons.Undo, 16);
            RedoMenu.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.TextIcons.Redo, 16);

            CutMenu.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.TextIcons.Cut, 16); ;
            CopyMenu.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.TextIcons.Copy, 16);
            PasteMenu.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.TextIcons.Paste, 16);
            DeleteMenu.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.TextIcons.Erase, 16);

            SelectAllMenu.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.TextIcons.Select, 16);
        }

        private BitmapImage fetchIconFromXml(string nodename, XmlNode node, int size = -1, Assembly assembly = null)
        {
            XmlNode iconnode = node.SelectSingleNode(nodename);
            if (iconnode != null)
            {
                BitmapImage img = loadImage(iconnode.InnerText, size, assembly);
                if (img != null)
                {
                    return img;
                }
            }
            return null;
        }

        private BitmapImage loadImage(string path, int size = -1, Assembly assembly = null)
        {
            return EmbeddedResourceTools.GetImage(path + ".png", size, assembly);
        }
    }
}

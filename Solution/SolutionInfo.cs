using Org.DocUI.Project;
using Org.Filedrops.FileSystem;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Org.DocUI.Solution
{
    public class SolutionInfo
    {
        public FiledropsDirectory Root { get; set; }
        public SolutionManager Manager { get; set; }
        public FiledropsFileSystem FileSystem { get; set; }
        public FiledropsFile Solutionfile { get; set; }
        public Dictionary<string, SolutionProjectInfo> projects { get; protected set; }

        public SolutionInfo(FiledropsFile solutionfile, SolutionManager manager, FiledropsFileSystem fs)
        {
            this.FileSystem = fs;
            this.Manager = manager;
            this.Solutionfile = solutionfile;
            this.Root = solutionfile.Parent;
            projects = new Dictionary<string, SolutionProjectInfo>();

            initSolutionFile();
            initSolutionProjects();
        }

        public SolutionProjectInfo GetSolutionProjectWithDirectory(FiledropsDirectory dir)
        {
            SolutionProjectInfo spi = projects[dir.Name];
            if (spi != null && spi.ProjectFile.Parent.FullName.Equals(dir.FullName))
            {
                return spi;
            }
            return null;
        }

        public string[] GetProjectNames()
        {
            return projects.Keys.ToArray();
        }

        private void initSolutionFile()
        {
            if (!Solutionfile.Exists())
            {
                Solutionfile.Create();
            }

            Solutionfile.Read();
            if (Solutionfile.BytesContent.Length == 0)
            {
                XmlDocument doc = new XmlDocument();
                XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                doc.AppendChild(docNode);
                XmlNode root = doc.CreateElement("Solution");
                doc.AppendChild(root);
                Solutionfile.Create(doc.OuterXml);
            }
        }

        private void initSolutionProjects()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(Solutionfile.FullName);
                XmlNode root = doc.DocumentElement;

                foreach (XmlElement el in root.SelectNodes("Project"))
                {
                    string location = el.SelectSingleNode("Location").InnerText;
                    FiledropsFileSystem fs = Manager.GetFileSystemClone();
                    FiledropsFile f = fs.ConstructFile(location);
                    f.Parent.FileSystem = fs;
                    f.FileSystem = fs;

                    initSolutionProject(f, fs);
                }
            }
            catch (XmlException e) { }
        }

        public void initSolutionProject(SolutionProjectInfo spi)
        {
            projects.Add(spi.ProjectFile.Parent.Name, spi);
        }

        public void initSolutionProject(FiledropsFile f, FiledropsFileSystem fs)
        {
            SolutionProjectInfo spi = new SolutionProjectInfo(this, f, Manager, fs);
            initSolutionProject(spi);
        }

        public bool closeSolution()
        {
            bool b = true;
            foreach (ProjectInfo pi in projects.Values)
            {
                b &= pi.CloseProject();
            }
            return b;
        }

        public void closeProject(SolutionProjectInfo pi)
        {
            FiledropsFile f = pi.ProjectFile;
            if (projects.ContainsKey(f.NameWithoutExtension) && pi.CloseProject())
            {
                projects.Remove(f.NameWithoutExtension);

                // Remove this project from the .fdsln file
                XmlDocument doc = new XmlDocument();
                doc.Load(Solutionfile.FullName);
                XmlNode root = doc.DocumentElement;

                foreach (XmlNode node in root.SelectNodes("Project"))
                {
                    if (node.SelectSingleNode("Location").InnerText.Equals(pi.ProjectFile.FullName))
                    {
                        root.RemoveChild(node);
                        break;
                    }
                }

                Solutionfile.Create(doc.OuterXml);
            }
        }

        public void addProject(SolutionProjectInfo spi)
        {
            // Initialize this project
            initSolutionProject(spi);

            AddProjectToSolutionFile(spi.ProjectFile);
        }


        public void addProject(FiledropsFile f)
        {
            // Initialize this project
            initSolutionProject(f, f.FileSystem);

            AddProjectToSolutionFile(f);
        }

        private void AddProjectToSolutionFile(FiledropsFile f)
        {
            // Add this project to the .fdsln xmlfile
            XmlDocument doc = new XmlDocument();
            doc.Load(Solutionfile.FullName);
            XmlNode root = doc.DocumentElement;

            XmlElement newproject = doc.CreateElement("Project");
            XmlElement location = doc.CreateElement("Location");
            location.InnerText = f.FullName;
            newproject.AppendChild(location);
            root.AppendChild(newproject);

            Solutionfile.Create(doc.OuterXml);
        }
    }
}

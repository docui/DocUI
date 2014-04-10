using Org.DocUI.Project;
using Org.Filedrops.FileSystem;
using System;

namespace Org.DocUI.Solution
{
    [Serializable]
    public class SolutionProjectInfo : ProjectInfo
    {
        public SolutionInfo solution { get; set; }


        public SolutionProjectInfo(SolutionInfo solution, FiledropsFile projectfile, ProjectManager manager, FiledropsFileSystem fs)
            : base(projectfile, manager, fs)
        {
            this.solution = solution;
        }
    }
}

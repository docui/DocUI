using Org.DocUI.Project;
using Org.Filedrops.FileSystem;

namespace Org.DocUI.FormBuilder
{
    /// <summary>
    /// Returns a DynamicProjectForm (can be extended to return an extension of DynamicProjectForm)
    /// </summary>
    public class DynamicProjectFormFactory
    {
        public virtual DynamicProjectForm GetNewForm(FiledropsFile fi, ProjectInfo pi)
        {
            DynamicProjectForm form = new DynamicProjectForm(fi, pi);
            return form;
        }
    }
}

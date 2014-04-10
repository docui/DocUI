using Org.DocUI.ProjectTreeView;
using System.Collections.Generic;

namespace Org.DocUI.FormBuilder.Templates
{
    public class SelectorTree : ProjectFileTree
    {
        public override void InitMenus()
        {
            Menus = new Dictionary<string, System.Windows.Controls.ContextMenu>();
            //do nothing
        }
    }
}

using System.Windows.Controls;
using System.Xml;
using System.Xml.Schema;

namespace Org.DocUI.FormBuilder.Components
{
    /// <summary>
    /// Represents a checkbox
    /// </summary>
    public class DocUIBoolean : AbstractDocUIComponent
    {
        public override object Value
        {
            get { return (Control as CheckBox).IsChecked == true ? "true" : "false"; }
            set
            {
                string s = value.ToString().ToLower();
                (Control as CheckBox).IsChecked = s == "true" ? true : false;
            }
        }

        public DocUIBoolean(XmlNode xmlNode, XmlSchemaAnnotated xsdNode, Panel contentpanel, Panel overlaypanel, DynamicForm parentForm)
            : base(xmlNode, xsdNode, contentpanel, overlaypanel, parentForm)
        {
            Control = new CheckBox();

            (Control as CheckBox).Click += (s, e) => { hasPendingChanges(); };

            setup();
        }
    }
}

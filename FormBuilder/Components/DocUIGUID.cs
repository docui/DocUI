using System;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Schema;

namespace Org.DocUI.FormBuilder.Components
{
    public class DocUIGUID : AbstractDocUIComponent
    {
        /// <summary>
        /// set or get the value of the textbox.
        /// </summary>
        public override object Value
        {
            get { return (Control as Label).Content; }
            set
            {
                string s = value.ToString();
                (Control as Label).Content = s;
            }
        }

        public DocUIGUID(XmlNode xmlNode, XmlSchemaAnnotated xsdNode, Panel contentpanel, Panel overlaypanel, DynamicForm parentForm) :
            base(xmlNode, xsdNode, contentpanel, overlaypanel, parentForm)
        {
            Label l = new Label();
            this.Control = l;
            //check if node contains default value
            if (xmlNode.InnerText == "")
            {
                xmlNode.InnerText = Guid.NewGuid().ToString();
                //save the key
                //parentForm.saveFile(this, null);
            }
            l.Content = xmlNode.InnerText;
        }
    }
}

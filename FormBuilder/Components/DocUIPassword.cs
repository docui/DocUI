using Org.DocUI.Tools;
using Org.DocUI.Wpf;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Schema;

namespace Org.DocUI.FormBuilder.Components
{
    public class DocUIPassword : AbstractDocUIComponent
    {
        private bool required;

        /// <summary>
        /// set or get the value of the current control
        /// </summary>
        public override object Value
        {
            get { return (Control as PasswordBox).Password; }
            set { (Control as PasswordBox).Password = value.ToString(); }
        }

        /// <summary>
        /// Creates a new instance of the passwordOption
        /// </summary>
        /// <param name="xmlNode">The xmlNode that contains the data.</param>
        /// <param name="xsdNode">The corresponding xsdNode.</param>
        /// <param name="panel">The panel on which this option should be placed.</param>
        /// <param name="parentForm">The form of which this option is a part.</param>
        public DocUIPassword(XmlNode xmlNode, XmlSchemaAnnotated xsdNode, Panel contentpanel, Panel overlaypanel, DynamicForm parentForm) :
            base(xmlNode, xsdNode, contentpanel, overlaypanel, parentForm)
        {
            PasswordBox pw = new PasswordBox() { PasswordChar = '*' };
            string req = XmlSchemaUtilities.tryGetUnhandledAttribute(xsdNode, "required");
            required = req == "true" ? true : false;

            pw.PasswordChanged += (s, e) =>
            {
                pw.Background = required && pw.Password == "" ? ExtendedTextBox.IncorrectColor : ExtendedTextBox.CorrectColor;
                hasPendingChanges();
            };
            Control = pw;

            setup();
        }

    }
}

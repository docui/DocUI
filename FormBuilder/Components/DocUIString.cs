using Org.DocUI.Tools;
using Org.DocUI.Wpf;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Schema;

namespace Org.DocUI.FormBuilder.Components
{
    public class DocUIString : AbstractDocUIComponent
    {
        /// <summary>
        /// set or get the value of the textbox.
        /// </summary>
        public override object Value
        {
            get { return (Control as ExtendedTextBox).TextBlock.Text; }
            set
            {
                string s = value.ToString();
                TextBox t = (Control as ExtendedTextBox).TextBlock;
                t.Text = s;
            }
        }

        /// <summary>
        /// Creates a new incstance of the StringOption.
        /// </summary>
        /// <param name="xmlNode">The xmlnode containing the data.</param>
        /// <param name="xsdNode">The corresponding xsdNode.</param>
        /// <param name="panel">The panel on which this option should be placed.</param>
        /// <param name="parentForm">The form of which this option is a part.</param>
        /// <param name="metaname">Whether this option can be filled with metadata. And if so, whether it will get the name or the value of the metadata.</param>
        public DocUIString(XmlNode xmlNode, XmlSchemaAnnotated xsdNode, Panel contentpanel, Panel overlaypanel, DynamicForm parentForm) :
            base(xmlNode, xsdNode, contentpanel, overlaypanel, parentForm)
        {
            string regex = XmlSchemaUtilities.tryGetPatternRestriction(xsdNode);

            if (regex == null) { regex = ""; }

            string watermark = XmlSchemaUtilities.tryGetUnhandledAttribute(xsdNode, "watermark");
            string req = XmlSchemaUtilities.tryGetUnhandledAttribute(xsdNode, "required");
            bool required = req == "true" ? true : false;

            Control = new ExtendedTextBox("", watermark, regex, required, parentForm);
            (Control as ExtendedTextBox).TextBlock.TextChanged += (s, e) => { hasPendingChanges(); };
            Setup();
        }


        public override bool CheckValid()
        {
            ExtendedTextBox etb = Control as ExtendedTextBox;
            etb.CheckRegex(this, null);
            return etb.Valid();
        }

        /// <summary>
        /// Called and executed after component construction
        /// </summary>
        protected virtual void Setup()
        {
            ExtendedTextBox etb = Control as ExtendedTextBox;
            etb.TextBlock.Text = xmlNode.InnerText;
            Control.Margin = new System.Windows.Thickness(5);
            this.Control.GotFocus += onGotFocus;
        }
    }
}

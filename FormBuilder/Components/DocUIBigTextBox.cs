using Org.DocUI.Tools;
using Org.DocUI.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Schema;

namespace Org.DocUI.FormBuilder.Components
{
    public class DocUIBigTextBox : AbstractDocUIComponent
    {
        private ExtendedTextBox box;

        /// <summary>
        /// Gets or Sets the current text of the textbox.
        /// </summary>
        public override object Value
        {
            get { return box.TextBlock.Text; }
            set { box.TextBlock.Text = value.ToString(); }
        }

        /// <summary>
        /// Creates a new instance of the BigTextOption
        /// </summary>
        /// <param name="xmlNode">The node containing the data for the textbox</param>
        /// <param name="xsdNode">The corresponding xsdNode</param>
        /// <param name="panel">the panel on which this option should be placed</param>
        /// <param name="parentForm">the form of which this option is a part</param>
        public DocUIBigTextBox(XmlNode xmlNode, XmlSchemaAnnotated xsdNode, Panel contentpanel, Panel overlaypanel, DynamicForm parentForm)
            : base(xmlNode, xsdNode, contentpanel, overlaypanel, parentForm)
        {
            ScrollViewer scroll = new ScrollViewer();

            string req = XmlSchemaUtilities.tryGetUnhandledAttribute(xsdNode, "required");
            bool required = req == "true" ? true : false;

            box = new ExtendedTextBox("", "", "", required, parentForm);
            box.TextBlock.AcceptsReturn = true;
            box.TextBlock.TextWrapping = TextWrapping.Wrap;
            box.Height = 200;

            scroll.Content = box;
            Control = scroll;

            setup();
        }

        /// <summary>
        /// validates the input
        /// </summary>
        /// <returns>true of correct input</returns>
        public override bool CheckValid()
        {
            this.box.CheckRegex(this, null);
            return box.Valid();
        }

    }
}

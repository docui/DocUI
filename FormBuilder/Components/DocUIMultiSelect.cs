using Org.DocUI.Tools;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Schema;

namespace Org.DocUI.FormBuilder.Components
{
    public class DocUIMultiSelect : AbstractDocUIComponent
    {
        private readonly Dictionary<string, CheckBox> _checkBoxes = new Dictionary<string, CheckBox>();
        private readonly WrapPanel _wrapPanel = new WrapPanel();

        /// <summary>
        /// Creates a new instance of MultiSelectOption
        /// </summary>
        /// <param name="xmlNode">The xmlNode that contains the data.</param>
        /// <param name="xsdNode">The corresponding xsdNode.</param>
        /// <param name="panel">The panel on which the option should be placed.</param>
        /// <param name="parentForm">The form of which this option is a part.</param>
        public DocUIMultiSelect(XmlNode xmlNode, XmlSchemaAnnotated xsdNode, Panel contentpanel, Panel overlaypanel, DynamicForm parentForm) :
            base(xmlNode, xsdNode, contentpanel, overlaypanel, parentForm)
        {
            XmlSchemaElement schemaEl = xsdNode as XmlSchemaElement;
            if (schemaEl != null)
            {
                XmlSchemaSequence seq = XmlSchemaUtilities.tryGetSequence(schemaEl.ElementSchemaType);
                if (seq != null && seq.Items.Count > 0)
                {
                    XmlSchemaElement el = seq.Items[0] as XmlSchemaElement;

                    IEnumerable<XmlSchemaEnumerationFacet> restrictions = XmlSchemaUtilities.tryGetEnumRestrictions(el.ElementSchemaType);
                    foreach (XmlSchemaEnumerationFacet e in restrictions)
                    {

                        AddOption(e.Value, FontWeights.Normal);
                    }
                }

                CheckBox all = AddOption("All", FontWeights.Bold);
                all.Checked += (s, e) => { SelectAll(); };
                all.Unchecked += (s, e) => { UnselectAll(); };

                _wrapPanel.Orientation = Orientation.Horizontal;
                Control = _wrapPanel;

                setup();
            }
        }

        /// <summary>
        /// Set the right values for the checkboxes + 
        /// Sets a margin for the control.
        /// </summary>
        protected override void setup()
        {
            foreach (XmlNode node in xmlNode)
                _checkBoxes[node.InnerText].IsChecked = true;

            bool allSelected = true;
            foreach (KeyValuePair<string, CheckBox> entry in _checkBoxes)
            {
                if (entry.Key != "All" && !entry.Value.IsChecked.Value)
                {
                    allSelected = false;
                }
            }

            if (allSelected)
            {
                _checkBoxes["All"].IsChecked = true;
            }

            Control.Margin = new Thickness(5);
        }

        /// <summary>
        /// Updates the xmlNode to the current value of the checkboxes.
        /// </summary>
        public override void update()
        {
            xmlNode.RemoveAll();

            XmlSchemaSequence seq = XmlSchemaUtilities.tryGetSequence((xsdNode as XmlSchemaElement).ElementSchemaType);
            if (seq != null && seq.Items.Count > 0)
            {
                XmlNode node = xmlNode.OwnerDocument.CreateElement((seq.Items[0] as XmlSchemaElement).QualifiedName.Name);
                foreach (CheckBox c in _checkBoxes.Values)
                {
                    if (c.IsChecked == true && c.Tag.ToString() != "All")
                    {
                        XmlNode option = node.Clone();
                        option.InnerText = c.Tag.ToString();
                        xmlNode.AppendChild(option);
                    }
                }
            }
        }

        /// <summary>
        /// Add a checkbox to the list.
        /// </summary>
        /// <param name="o">the text to display along with the checkbox.</param>
        private CheckBox AddOption(string o, FontWeight weight)
        {
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;

            CheckBox c = new CheckBox() { Tag = o, Margin = new Thickness(5), FontWeight = weight };
            c.Checked += (s, e) => { hasPendingChanges(); };
            c.Unchecked += (s, e) => { hasPendingChanges(); };

            TextBlock tb = new TextBlock() { Text = o, Margin = new Thickness(5), FontWeight = weight };

            stack.Children.Add(c);
            stack.Children.Add(tb);
            _wrapPanel.Children.Add(stack);

            _checkBoxes.Add(o, c);
            return c;
        }

        private void SelectAll()
        {
            foreach (CheckBox c in _checkBoxes.Values)
            {
                c.IsChecked = true;
            }
        }

        private void UnselectAll()
        {
            foreach (CheckBox c in _checkBoxes.Values)
            {
                c.IsChecked = false;
            }
        }

        /// <summary>
        /// option is to complex, so Value will not be used
        /// </summary>
        public override object Value
        {
            get { Log.Info("MultiSelect option does not contain a definition for Value. Please don't use it."); return null; }
            set { Log.Info("MultiSelect option does not contain a definition for Value. Please don't use it."); }
        }
    }
}

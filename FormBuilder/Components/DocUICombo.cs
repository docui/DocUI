using Org.DocUI.Tools;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Schema;

namespace Org.DocUI.FormBuilder.Components
{
    /// <summary>
    /// This option displays a combobox. The possibilities in the combobox are defined by the enumeration restriction.
    /// NOTE: an enumeration restriction is necessary (otherwise the combobox will be empty).
    /// </summary>
    public class DocUICombo : AbstractDocUIComponent
    {
        private ComboBox cb;

        /// <summary>
        /// Gets or sets the selected value of the combobox
        /// </summary>
        public override object Value
        {
            get { return cb.SelectedValue; }
            set { cb.SelectedValue = value.ToString(); }
        }

        /// <summary>
        /// Creates a new instance of the comboOption
        /// </summary>
        /// <param name="xmlNode">The xmlNode containing the data (selected value) of the comboOption</param>
        /// <param name="xsdNode">The corresponding xsdNode</param>
        /// <param name="panel">The panel on which the option should be placed</param>
        /// <param name="parentForm">The form of which this option is a part.</param>
        public DocUICombo(XmlNode xmlNode, XmlSchemaAnnotated xsdNode, Panel contentpanel, Panel overlaypanel, DynamicForm parentForm)
            : base(xmlNode, xsdNode, contentpanel, overlaypanel, parentForm)
        {
            XmlSchemaElement schemaEl = xsdNode as XmlSchemaElement;
            if (schemaEl != null)
            {
                cb = new ComboBox() { Margin = new Thickness(5) };
                cb.SelectionChanged += (s, e) => { hasPendingChanges(); };
                cb.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;

                // get enumeration
                IEnumerable<XmlSchemaEnumerationFacet> enumFacets = XmlSchemaUtilities.tryGetEnumRestrictions(schemaEl.ElementSchemaType);

                if (enumFacets != null)
                {
                    foreach (var facet in enumFacets)
                    {
                        // fill the combobox
                        cb.Items.Add(facet.Value);
                    }
                    cb.SelectedIndex = 0;
                }
                else
                {
                    Log.Info("This combobox has no enumeration restriction. The Combobox will be empty.");
                }

                Control = cb;
                cb.Padding = new Thickness(10, 2, 10, 2);

                setup();
            }
        }

        /// <summary>
        /// Places this option on the panel.
        /// </summary>
        public override void placeOption()
        {
            // put combobox in stackpanel so that it is not stretched over full width.
            StackPanel stack = new StackPanel() { Orientation = Orientation.Horizontal };
            stack.Children.Add(Control);

            if (Contentpanel is Grid)
            {
                Grid g = Contentpanel as Grid;
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
                int nr = g.RowDefinitions.Count;
                Grid.SetRow(Label, nr - 1);
                Grid.SetRow(stack, nr - 1);

                Grid.SetColumn(Label, 0);
                Grid.SetColumn(stack, 1);
            }

            Contentpanel.Children.Add(Label);
            Contentpanel.Children.Add(stack);
        }

    }
}

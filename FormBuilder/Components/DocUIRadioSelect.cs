using Org.DocUI.Tools;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Schema;

namespace Org.DocUI.FormBuilder.Components
{
    /// <summary>
    /// This option will display a series of radiobuttons.
    /// Each radiobutton corresponds with the subsection of options next to it.
    /// Only the selected radiobutton's subsection will be enabled.
    /// The selected radio EdButton is saved in the Xml Attribute "selected" as an int.
    /// </summary>
    public class DocUIRadioSelect : AbstractDocUIComponent
    {
        private Grid g = new Grid();
        private List<AbstractDocUIComponent> options = new List<AbstractDocUIComponent>();
        private Dictionary<string, RadioButton> radioList = new Dictionary<string, RadioButton>();

        /// <summary>
        /// Creates a new instance of the RadioSelector
        /// </summary>
        /// <param name="xmlNode">The xmlNode containing the data.</param>
        /// <param name="xsdNode">The corresponding xsdNode.</param>
        /// <param name="panel">The panel on which this option should be placed.</param>
        /// <param name="parentForm">The form of which this option is a part.</param>
        public DocUIRadioSelect(XmlNode xmlNode, XmlSchemaAnnotated xsdNode, Panel contentpanel, Panel overlaypanel, DynamicForm parentForm) :
            base(xmlNode, xsdNode, contentpanel, overlaypanel, parentForm)
        {
            XmlSchemaElement schemaEl = xsdNode as XmlSchemaElement;
            if (schemaEl != null)
            {
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                g.HorizontalAlignment = HorizontalAlignment.Stretch;

                Control = g;

                XmlSchemaSequence seq = XmlSchemaUtilities.tryGetSequence(schemaEl.ElementSchemaType);
                if (seq != null)
                    foreach (XmlSchemaElement el in seq.Items)
                        Utilities.recursive(el, xmlNode.SelectSingleNode(el.Name), contentpanel, overlaypanel, addOption, parentForm);
                else
                    Log.Info("this comboselector does not contain any options.");

                setup();
            }
        }

        /// <summary>
        /// Sets the right radio EdButton selected.
        /// </summary>
        protected override void setup()
        {
            Value = xmlNode.Attributes["selected"] != null ? xmlNode.Attributes["selected"].Value : "";
            foreach (RadioButton radio in radioList.Values)
                radioButton_Changed(radio, EventArgs.Empty);
        }

        /// <summary>
        /// Updates the xmlnode to the current value of the control.
        /// </summary>
        public override void update()
        {
            xmlNode.Attributes["selected"].Value = Value.ToString();

            foreach (AbstractDocUIComponent o in options)
                o.update();
        }

        /// <summary>
        /// Adds another option with a radioButton.
        /// </summary>
        /// <param name="o">The option that corresponds with the option.</param>
        private void addOption(AbstractDocUIComponent o)
        {
            RadioButton radio = new RadioButton() { GroupName = xmlNode.GetHashCode() + xsdNode.GetHashCode() + "selection", Margin = new Thickness(5) };
            radio.Checked += radioButton_Changed;
            radio.Checked += (s, e) => { hasPendingChanges(); };
            radio.Unchecked += radioButton_Changed;
            radio.Unchecked += (s, e) => { hasPendingChanges(); };
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            Grid.SetColumn(radio, 0);
            Grid.SetColumn(o.Label, 1);
            Grid.SetColumn(o.Control, 2);
            Grid.SetRow(radio, options.Count);
            Grid.SetRow(o.Label, options.Count);
            Grid.SetRow(o.Control, options.Count);
            g.Children.Add(radio);
            g.Children.Add(o.Control);

            radio.Tag = o;
            options.Add(o);
            radioList.Add(o.xmlNode.Name, radio);
            //radioList.Add(
        }

        /// <summary>
        /// Sets or Gets the selected radiobutton.
        /// This is done with an index.
        /// </summary>
        public override object Value
        {
            get
            {
                foreach (KeyValuePair<string, RadioButton> pair in radioList)
                {
                    if (pair.Value.IsChecked == true)
                    {
                        return pair.Key;
                    }
                }
                return "";
            }
            set
            {
                radioList[value.ToString()].IsChecked = true;
            }
        }

        /// <summary>
        /// The selected radiobutton changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void radioButton_Changed(object sender, EventArgs args)
        {
            RadioButton radio = sender as RadioButton;
            AbstractDocUIComponent o = radio.Tag as AbstractDocUIComponent;
            o.Control.IsEnabled = radio.IsChecked == true;
        }

    }
}

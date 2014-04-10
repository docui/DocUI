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
    /// This option contains other options. 
    /// If the main xmlNode contains an attribute named "checked", than the subsection can be disabled.
    /// </summary>
    public class DocUISubSection : AbstractDocUIComponent
    {
        private CheckBox check;
        private List<AbstractDocUIComponent> optionlist;

        /// <summary>
        /// set or get the value of the current control.
        /// Only applicable if the subsection can be checked.
        /// </summary>
        public override object Value
        {
            get
            {
                if (check == null) { return null; }
                return check.IsChecked == true ? "true" : "false";
            }
            set
            {
                if (check == null) { return; }
                check.IsChecked = value.ToString().ToLower() == "true" ? true : false;
            }
        }

        public override DynamicForm ParentForm
        {
            get { return base.ParentForm; }
            set
            {
                base.ParentForm = value;
                if (optionlist != null)
                    foreach (AbstractDocUIComponent a in optionlist)
                        a.ParentForm = value;
            }
        }

        protected GroupBox box;

        /// <summary>
        /// Creates a new instance of the SubSectionOption
        /// </summary>
        /// <param name="xmlNode">The xmlnode containing the data.</param>
        /// <param name="xsdNode">The corresponding xsdNode.</param>
        /// <param name="panel">The panel on which this option should be placed.</param>
        /// <param name="parentForm">The form of which this option is a part.</param>
        public DocUISubSection(XmlNode xmlNode, XmlSchemaAnnotated xsdNode, Panel contentpanel, Panel overlaypanel, DynamicForm parentForm) :
            base(xmlNode, xsdNode, contentpanel, overlaypanel, parentForm)
        {
            XmlSchemaElement schemaEl = xsdNode as XmlSchemaElement;
            if (schemaEl != null)
            {
                optionlist = new List<AbstractDocUIComponent>();
                box = new GroupBox();
                DockPanel panel = new DockPanel();

                Grid g = new Grid() { Margin = new Thickness(5, 0, 0, 0) };
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                XmlSchemaSequence seq = XmlSchemaUtilities.tryGetSequence(schemaEl.ElementSchemaType);
                if (seq != null)
                {
                    foreach (XmlSchemaElement el in seq.Items)
                        Utilities.recursive(el, xmlNode.SelectSingleNode(el.Name), g, overlaypanel, (comp) =>
                        {
                            comp.placeOption();
                            optionlist.Add(comp);
                        }, parentForm);
                }

                if (xmlNode.Attributes["checked"] != null)
                {
                    check = new CheckBox() { Margin = new Thickness(5, 5, 0, 5) };
                    check.Checked += check_changed;
                    check.Checked += (s, e) => { hasPendingChanges(); };
                    check.Unchecked += check_changed;
                    check.Unchecked += (s, e) => { hasPendingChanges(); };
                    panel.Children.Add(check);
                }

                // if there is no label, there should be no groupbox.
                if (Label.Text != "")
                {
                    panel.Children.Add(Label);
                    box.Header = panel;
                    box.Content = g;
                    Control = box;
                }
                else
                {
                    panel.Children.Add(g);
                    //Control = g;
                    Control = panel;
                }

                setup();
            }
        }


        /// <summary>
        /// Assignes the right value to the checkbox (if available).
        /// </summary>
        protected override void setup()
        {
            if (check != null)
            {
                Value = xmlNode.Attributes["checked"].Value;
                check_changed(check, EventArgs.Empty);
                Control.Margin = new Thickness(5);
            }
        }

        /// <summary>
        /// this will Update the xml node to the current value of the control
        /// </summary>
        /// Default implementations are given here.
        public override void update()
        {
            if (check != null)
                xmlNode.Attributes["checked"].Value = Value.ToString();

            // Update subOptions
            foreach (AbstractDocUIComponent o in optionlist)
                o.update();
        }

        /// <summary>
        /// this will position the option in the assigned panel.
        /// </summary>
        /// Default implementations are given here.
        public override void placeOption()
        {
            if (Contentpanel is Grid)
            {
                Grid g = Contentpanel as Grid;
                g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                int nr = g.RowDefinitions.Count;
                Grid.SetRow(Control, nr - 1);

                Grid.SetColumn(Control, 0);
                Grid.SetColumnSpan(Control, 2);
            }

            Contentpanel.Children.Add(Control);

        }

        /// <summary>
        /// If the checkbox has been (un)checked. The subsection options should be (en/dis)abled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void check_changed(object sender, EventArgs args)
        {
            bool value = check.IsChecked.Value;

            foreach (AbstractDocUIComponent o in optionlist)
                o.Control.IsEnabled = value;
        }


        public override bool CheckValid()
        {
            bool valid = true;
            foreach (AbstractDocUIComponent comp in this.optionlist)
            {
                valid = valid && comp.CheckValid();
            }
            return valid;
        }
    }
}

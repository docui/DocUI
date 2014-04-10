using Org.DocUI.Tools;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Schema;

namespace Org.DocUI.FormBuilder.Components
{
    /// <summary>
    /// This option represents a combobox. Corresponding with each item in the combobox, there is a panel.
    /// The panel of the selected item will be visible.
    /// </summary>
    public class DocUIComboSubSection : AbstractDocUIComponent
    {
        private readonly ComboBox _cb;
        private readonly Dictionary<string, DocUISubSection> _subsections = new Dictionary<string, DocUISubSection>();
        private readonly StackPanel _currentSubsection;

        /// <summary>
        /// Creates a new instance of a comboSelector.
        /// </summary>
        /// <param name="xmlNode">The xmlNode that contains the data of the comboSelector</param>
        /// <param name="xsdNode">The corresponding xsdNode</param>
        /// <param name="contentpanel">the panel on which the option should be placed.</param>
        /// <param name="overlaypanel"></param>
        /// <param name="parentForm"></param>
        public DocUIComboSubSection(XmlNode xmlNode, XmlSchemaAnnotated xsdNode, Panel contentpanel, Panel overlaypanel, DynamicForm parentForm)
            : base(xmlNode, xsdNode, contentpanel, overlaypanel, parentForm)
        {
            XmlSchemaElement schemaEl = xsdNode as XmlSchemaElement;
            if (schemaEl != null)
            {
                _cb = new ComboBox();
                _cb.Margin = new Thickness(5);

                XmlSchemaSequence seq = XmlSchemaUtilities.tryGetSequence(schemaEl.ElementSchemaType);
                if (seq != null && schemaEl.ElementSchemaType is XmlSchemaComplexType)
                {
                    XmlSchemaAttribute att = XmlSchemaUtilities.tryGetAttribute(schemaEl.ElementSchemaType as XmlSchemaComplexType, "selected");
                    if (att != null)
                    {
                        IEnumerable<XmlSchemaEnumerationFacet> enumFacets = XmlSchemaUtilities.tryGetEnumRestrictions(att.AttributeSchemaType);

                        if (enumFacets != null)
                        {
                            foreach (var facet in enumFacets)
                            {
                                // fill the combobox
                                _cb.Items.Add(facet.Value);
                            }
                        }

                        int i = 0;
                        foreach (XmlSchemaElement el in seq.Items)
                        {
                            DocUISubSection comp = new DocUISubSection(xmlNode.SelectSingleNode(el.Name), el, contentpanel, overlaypanel, parentForm);
                            object e = _cb.Items[i++];
                            string s = e.ToString();
                            _subsections.Add(s, comp);
                        }
                        if (_cb.Items.Count > 0)
                        {
                            _cb.SelectionChanged += combo_Selectionchanged;
                            _cb.SelectionChanged += (s, e) => { hasPendingChanges(); };
                        }

                        DockPanel dock = new DockPanel();

                        DockPanel.SetDock(_cb, Dock.Top);
                        dock.Children.Add(_cb);


                        _currentSubsection = new StackPanel();
                        dock.Children.Add(_currentSubsection);

                        Control = dock;


                        if (Label.Text != "")
                        {
                            GroupBox gb = new GroupBox();
                            gb.Header = Label.Text;
                            gb.Content = dock;
                            gb.Margin = new Thickness(5);
                            this.Control = gb;
                        }

                        setup();
                    }
                    else
                    {
                        Log.Info("this comboselector does not contain any options.");
                    }
                }
                else
                {
                    Log.Info("the xsdnode does not have the right structure for a combosubsection.");
                }
            }
        }

        /// <summary>
        /// Set the Control to the right value.
        /// </summary>
        protected override void setup()
        {
            Value = xmlNode.Attributes["selected"].Value;
            Control.Margin = new Thickness(5);
        }

        /// <summary>
        /// Update the xmlNode to the current value of the Control
        /// </summary>
        public override void update()
        {
            xmlNode.Attributes["selected"].Value = Value.ToString();
            foreach (AbstractDocUIComponent comp in _subsections.Values)
                comp.update();
        }

        /// <summary>
        /// Combobox selection changed. Update the visible subsection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void combo_Selectionchanged(object sender, SelectionChangedEventArgs args)
        {
            _currentSubsection.Children.Clear();
            _currentSubsection.Children.Add(_subsections[_cb.SelectedItem.ToString()].Control);
        }

        /// <summary>
        /// get or set the current value of the Control.
        /// </summary>
        public override object Value
        {
            get { return _cb.SelectedValue; }
            set { _cb.SelectedValue = value.ToString(); }
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
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
                int nr = g.RowDefinitions.Count;
                Grid.SetRow(Control, nr - 1);

                Grid.SetColumn(Control, 0);
                Grid.SetColumnSpan(Control, 2);
            }

            Contentpanel.Children.Add(Control);
        }

        /// <summary>
        /// Checks if the content of each subpanel is Valid
        /// </summary>
        /// <returns>true if Valid</returns>
        public override bool CheckValid()
        {
            bool valid = true;
            foreach (DocUISubSection comp in this._subsections.Values)
            {
                valid = comp.CheckValid() && valid;
            }
            return valid;
        }
    }
}

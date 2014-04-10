using Org.DocUI.Tools;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Schema;

namespace Org.DocUI.FormBuilder.Components
{
    /// <summary>
    /// Represents a panel with one or more tabs
    /// </summary>
    public class DocUITabbed : AbstractDocUIComponent
    {
        private readonly TabControl _tabControl;
        private readonly List<AbstractDocUIComponent> _optlist;
        protected bool Sideways;

        /// <summary>
        /// get or set the current value of the Control.
        /// </summary>
        public override object Value
        {
            get { return _tabControl.SelectedContent; }
            set { }
        }

        public DocUITabbed(XmlNode xmlNode, XmlSchemaAnnotated xsdNode, Panel contentpanel, Panel overlaypanel, DynamicForm parentForm)
            : base(xmlNode, xsdNode, contentpanel, overlaypanel, parentForm)
        {
            this.Sideways = true;
            _tabControl = new TabControl();
            this.Control = _tabControl;
            _optlist = new List<AbstractDocUIComponent>();
            XmlSchemaElement schemaEl = xsdNode as XmlSchemaElement;
            if (schemaEl != null)
            {
                XmlSchemaSequence seq = XmlSchemaUtilities.tryGetSequence(schemaEl.ElementSchemaType);
                if (seq != null)
                {
                    foreach (XmlSchemaElement el in seq.Items)
                    {
                        TabItem ti = new TabItem();
                        ti.Header = XmlSchemaUtilities.tryGetDocumentation(el); ;
                        Grid newpanel = new Grid();
                        ColumnDefinition cdnew1 = new ColumnDefinition();
                        cdnew1.Width = new GridLength(1, GridUnitType.Auto);
                        ColumnDefinition cdnew2 = new ColumnDefinition();
                        newpanel.ColumnDefinitions.Add(cdnew1);
                        newpanel.ColumnDefinitions.Add(cdnew2);
                        Utilities.recursive(el, xmlNode.SelectSingleNode(el.Name), newpanel, overlaypanel, (comp) =>
                        {
                            _optlist.Add(comp);
                            comp.placeOption();
                        }, parentForm);
                        ti.Content = newpanel;
                        this._tabControl.Items.Add(ti);
                    }
                }
            }
        }


        public override void update()
        {
            foreach (AbstractDocUIComponent comp in this._optlist)
            {
                comp.update();
            }
        }

        public override void placeOption()
        {
            if (Sideways)
            {
                this._tabControl.TabStripPlacement = Dock.Left;
            }
            if (Contentpanel == null) { return; }

            if (Contentpanel is Grid)
            {
                Grid g = Contentpanel as Grid;

                Grid.SetColumn(Label, 0);
                Grid.SetColumn(Control, 1);
            }

            Contentpanel.Children.Add(Label);
            Contentpanel.Children.Add(Control);
        }
    }
}

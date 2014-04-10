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
    /// Represents a splitted panel (Horizontal or vertical)
    /// </summary>
    public class DocUISplitPanel : AbstractDocUIComponent
    {
        private Grid _grid = new Grid();
        private List<AbstractDocUIComponent> _optlist;
        protected bool Horizontal = true;

        public override object Value
        {
            get { return _grid; }
            set { _grid = value as Grid; }
        }

        public override void update()
        {
            foreach (AbstractDocUIComponent io in _optlist)
                io.update();
        }

        public DocUISplitPanel(XmlNode xmlNode, XmlSchemaAnnotated xsdNode, Panel contentpanel, Panel overlaypanel, DynamicForm parentForm)
            : base(xmlNode, xsdNode, contentpanel, overlaypanel, parentForm)
        {

        }

        public override void placeOption()
        {
            XmlSchemaElement schemaEl = xsdNode as XmlSchemaElement;
            if (schemaEl != null)
            {
                _optlist = new List<AbstractDocUIComponent>();

                Grid grid1 = new Grid();
                Grid grid2 = new Grid();

                StackPanel gridpanel1 = new StackPanel();
                StackPanel gridpanel2 = new StackPanel();
                gridpanel1.Children.Add(grid1);
                gridpanel2.Children.Add(grid2);

                Grid overlay1 = new Grid();
                Grid overlay2 = new Grid();
                overlay1.Children.Add(gridpanel1);
                overlay2.Children.Add(gridpanel2);

                //set orientation
                # region setting orientation
                if (Horizontal)
                {
                    _grid.ColumnDefinitions.Add(new ColumnDefinition());
                    _grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
                    _grid.ColumnDefinitions.Add(new ColumnDefinition());
                    _grid.VerticalAlignment = VerticalAlignment.Stretch;

                    GridSplitter splitter = new GridSplitter()
                    {
                        VerticalAlignment = VerticalAlignment.Stretch,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        ResizeBehavior = GridResizeBehavior.PreviousAndNext,
                        Width = 5
                    };
                    Grid.SetColumn(splitter, 1);
                    _grid.Children.Add(splitter);

                    Grid.SetColumn(overlay1, 0);
                    Grid.SetColumn(overlay2, 2);
                }
                else
                {
                    _grid.RowDefinitions.Add(new RowDefinition());
                    _grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
                    _grid.RowDefinitions.Add(new RowDefinition());
                    _grid.HorizontalAlignment = HorizontalAlignment.Stretch;

                    GridSplitter splitter = new GridSplitter()
                    {
                        VerticalAlignment = VerticalAlignment.Bottom,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        ResizeBehavior = GridResizeBehavior.PreviousAndNext,
                        Height = 5
                    };
                    Grid.SetRow(splitter, 1);
                    _grid.Children.Add(splitter);

                    Grid.SetRow(overlay1, 0);
                    Grid.SetRow(overlay2, 2);
                }
                #endregion

                //determine children and use recursive
                XmlSchemaSequence seq = XmlSchemaUtilities.tryGetSequence(schemaEl.ElementSchemaType);
                if (xmlNode.ChildNodes.Count > 1 && seq != null && seq.Items.Count > 1)
                {
                    XmlNode node1 = xmlNode.ChildNodes[0];
                    XmlNode node2 = xmlNode.ChildNodes[1];

                    recursive(grid1, node1, seq.Items[0] as XmlSchemaElement, overlay1);
                    recursive(grid2, node2, seq.Items[1] as XmlSchemaElement, overlay2);

                    # region ui specific
                    string size1 = XmlSchemaUtilities.tryGetUnhandledAttribute(seq.Items[0] as XmlSchemaElement, "size");
                    string size2 = XmlSchemaUtilities.tryGetUnhandledAttribute(seq.Items[1] as XmlSchemaElement, "size");

                    if (size1 != "")
                        if (Horizontal)
                            _grid.ColumnDefinitions[0].Width = new GridLength(Int32.Parse(size1));
                        else
                            _grid.RowDefinitions[0].Height = new GridLength(Int32.Parse(size1));
                    if (size2 != "")
                        if (Horizontal)
                            _grid.ColumnDefinitions[2].Width = new GridLength(Int32.Parse(size2));
                        else
                            _grid.RowDefinitions[2].Height = new GridLength(Int32.Parse(size2));
                    #endregion uispecific

                    this.Control = _grid;
                    _grid.Children.Add(overlay1);
                    _grid.Children.Add(overlay2);
                    _grid.VerticalAlignment = VerticalAlignment.Stretch;
                }
            }

            if (Contentpanel is Grid)
            {
                Grid g = Contentpanel as Grid;
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                int nr = g.RowDefinitions.Count;
                Grid.SetRow(Label, nr - 1);
                Grid.SetRow(Control, nr - 1);

                Grid.SetColumn(Label, 0);
                Grid.SetColumn(Control, 1);
            }

            Contentpanel.Children.Add(Label);
            Contentpanel.Children.Add(Control);
        }

        public void recursive(Grid newpanel, XmlNode childxmlnode, XmlSchemaElement childxsdnode, Panel overlay)
        {
            ColumnDefinition cdnew1 = new ColumnDefinition();
            cdnew1.Width = new GridLength(1, GridUnitType.Auto);
            ColumnDefinition cdnew2 = new ColumnDefinition();
            newpanel.ColumnDefinitions.Add(cdnew1);
            newpanel.ColumnDefinitions.Add(cdnew2);

            Utilities.recursive(childxsdnode, childxmlnode, newpanel, overlay, (comp) =>
            {
                _optlist.Add(comp);
                comp.placeOption();
            }, ParentForm);
        }
    }
}

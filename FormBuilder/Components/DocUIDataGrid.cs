using Org.DocUI.Tools;
using Org.DocUI.Wpf;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Schema;

namespace Org.DocUI.FormBuilder.Components
{
    /// <summary>
    /// Represents a datagrid
    /// </summary>
    public class DocUIDataGrid : AbstractDocUIComponent
    {
        private readonly MyDataGrid _dgrid;
        private bool _isEditing = false;
        private readonly XmlSchemaElement _el;
        public delegate DataGridColumn GetColumn(Binding b);
        public static Dictionary<string, GetColumn> ColDict;

        static DocUIDataGrid()
        {
            ColDict = new Dictionary<string, GetColumn>();
            ColDict.Add("string", (Binding b) =>
            {
                DataGridTextColumn col = new DataGridTextColumn();
                col.Binding = b;
                col.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                return col;
            });
            ColDict.Add("combobox", (Binding b) =>
            {
                DataTemplate dt = new DataTemplate();

                FrameworkElementFactory templatecheckbox = new FrameworkElementFactory(typeof(ComboBox));

                templatecheckbox.SetBinding(ComboBox.TextProperty, b);

                dt.VisualTree = templatecheckbox;

                DataGridTemplateColumn col = new DataGridTemplateColumn();
                col.CanUserResize = false;
                col.CellTemplate = dt;
                return col;
            });
        }

        private DataGridCellInfo _lastFocused;

        public override object Value
        {
            get
            {
                string value = null;
                if (_lastFocused != null)
                {
                    DataGridCellInfo item = _lastFocused;
                    _dgrid.CurrentItem = item;
                    if (item.Item is XmlNode)
                    {
                        XmlNode node = item.Item as XmlNode;
                        value = node.ChildNodes[item.Column.DisplayIndex].InnerText;
                    }
                }
                return value;
            }
            set
            {
                if (_lastFocused != null)
                {
                    DataGridCellInfo item = _lastFocused;
                    _dgrid.CurrentItem = item;
                    if (item.Item is XmlNode)
                    {
                        XmlNode node = item.Item as XmlNode;
                        node.ChildNodes[item.Column.DisplayIndex].InnerText = value.ToString();
                    }
                }
            }
        }


        public DocUIDataGrid(XmlNode xmlNode, XmlSchemaAnnotated xsdNode, Panel contentpanel, Panel overlaypanel, DynamicForm parentForm)
            : base(xmlNode, xsdNode, contentpanel, overlaypanel, parentForm)
        {
            XmlSchemaElement schemaEl = xsdNode as XmlSchemaElement;
            if (schemaEl != null)
            {
                _dgrid = new MyDataGrid() { Margin = new Thickness(5), CanUserReorderColumns = false };
                _dgrid.CellEditEnding += (s, e) => { hasPendingChanges(); };
                _dgrid.SelectionUnit = DataGridSelectionUnit.Cell;

                this.xmlNode = xmlNode;
                XmlSchemaSequence seq1 = XmlSchemaUtilities.tryGetSequence(schemaEl.ElementSchemaType);

                XmlDataProvider provider = new XmlDataProvider();
                provider.Document = xmlNode.OwnerDocument;

                Binding myNewBindDef = new Binding();
                myNewBindDef.Source = provider;
                myNewBindDef.XPath = "//" + xmlNode.Name + "/*";
                _dgrid.SetBinding(DataGrid.ItemsSourceProperty, myNewBindDef);
                _dgrid.AutoGenerateColumns = false;
                if (seq1 != null && seq1.Items.Count > 0)
                {
                    _el = seq1.Items[0] as XmlSchemaElement;
                    XmlSchemaSequence seq2 = XmlSchemaUtilities.tryGetSequence((seq1.Items[0] as XmlSchemaElement).ElementSchemaType);
                    if (seq2 != null)
                    {
                        foreach (XmlSchemaElement child in seq2.Items)
                        {
                            XmlSchemaType type = child.ElementSchemaType;
                            string result1;
                            GetColumn result2;
                            DataGridColumn col;
                            if (DynamicForm.Components.TryGetValue(type.QualifiedName.Name, out result1)
                                && ColDict.TryGetValue(result1, out result2))
                            {

                                var b = new Binding
                                   {
                                       XPath = child.Name
                                   };
                                col = result2(b);

                                if (result1 == "combobox")
                                {
                                    DataGridTemplateColumn col2 = col as DataGridTemplateColumn;
                                    FrameworkElementFactory fact = col2.CellTemplate.VisualTree;
                                    List<string> list = new List<string>();
                                    IEnumerable<XmlSchemaEnumerationFacet> enumFacets = XmlSchemaUtilities.tryGetEnumRestrictions(child.ElementSchemaType);
                                    foreach (var facet in enumFacets)
                                    {
                                        list.Add(facet.Value);
                                    }
                                    fact.SetValue(ComboBox.ItemsSourceProperty, list);
                                    string widthstr = XmlSchemaUtilities.tryGetUnhandledAttribute(child, "width");
                                    int width = 0;
                                    if (widthstr != "")
                                        width = Int32.Parse(widthstr);
                                    else
                                        width = 50;
                                    col2.Width = width;
                                }
                            }
                            else
                            {
                                col = new DataGridTextColumn();
                                DataGridTextColumn textcol = col as DataGridTextColumn;
                                var b = new Binding
                                {
                                    XPath = child.Name
                                };
                                textcol.Binding = b;
                                textcol.Width = new DataGridLength(1, DataGridLengthUnitType.Star);

                            }
                            col.Header = child.Name;
                            _dgrid.Columns.Add(col);
                        }

                        DataTemplate dt = new DataTemplate();

                        FrameworkElementFactory templatebutton = new FrameworkElementFactory(typeof(Button));
                        FrameworkElementFactory img = new FrameworkElementFactory(typeof(Image));


                        Binding bind = new Binding
                        {
                            XPath = ".",
                            Mode = BindingMode.TwoWay
                        };

                        BitmapImage bi3 = EmbeddedResourceTools.GetImage("Com.Xploreplus.DocUI.Resources.Images.component.delete.png");

                        img.SetValue(Image.SourceProperty, bi3);

                        templatebutton.AddHandler(Button.ClickEvent, new RoutedEventHandler(click_Delete));
                        templatebutton.AppendChild(img);
                        templatebutton.SetBinding(Button.TagProperty, bind);
                        dt.VisualTree = templatebutton;

                        DataGridTemplateColumn buttoncol = new DataGridTemplateColumn();
                        buttoncol.CanUserResize = false;
                        buttoncol.CellTemplate = dt;
                        _dgrid.Columns.Add(buttoncol);
                        _dgrid.RowHeight = 25;
                        _dgrid.CanUserResizeRows = false;

                        _dgrid.GotFocus += gotFocus;
                        _dgrid.PreparingCellForEdit += checkEmptyRow;

                        _dgrid.PreparingCellForEdit += (s, e) => { this._isEditing = true; };
                        _dgrid.CellEditEnding += (s, e) => { this._isEditing = false; };
                    }
                }
            }
        }



        public override void placeOption()
        {
            Grid.SetColumn(_dgrid, 1);
            this.Contentpanel.Children.Add(_dgrid);
        }

        public override void update()
        {
            //commit edit
            this._dgrid.CurrentCell = new DataGridCellInfo(this._dgrid.Items[_dgrid.Items.Count - 1], this._dgrid.Columns[_dgrid.Columns.Count - 1]);
        }

        public void gotFocus(object sender, EventArgs args)
        {
            this.ParentForm.LastFocusedElement = this;
            this._lastFocused = _dgrid.CurrentCell;
        }


        public void checkEmptyRow(object sender, DataGridPreparingCellForEditEventArgs args)
        {
            if (args.Row.GetIndex() == (_dgrid.Items.Count - 1))
            {
                XmlElement node = this.xmlNode.OwnerDocument.CreateElement(_el.Name);
                XmlDocument doc = XmlSchemaUtilities.generateCleanXmlToString(xsdNode.SourceUri);
                XmlNode samplenode = doc.DocumentElement.SelectSingleNode("//" + _el.Name);
                node.InnerXml = samplenode.InnerXml;
                xmlNode.AppendChild(node);
                hasPendingChanges();
            }
        }

        /// <summary>
        /// Deletes the corresponding row
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void click_Delete(Object sender, RoutedEventArgs args)
        {
            if (sender is Button)
            {
                Button b = sender as Button;
                object tag = b.Tag;
                if (tag is XmlNode)
                {
                    XmlNode node = tag as XmlNode;
                    XmlNode parent = node.ParentNode;
                    parent.RemoveChild(node);
                    if (_dgrid.Items.Count == 0)
                    {
                        XmlElement newnode = this.xmlNode.OwnerDocument.CreateElement(_el.Name);
                        XmlDocument doc = XmlSchemaUtilities.generateCleanXmlToString(xsdNode.SourceUri);
                        XmlNode samplenode = doc.DocumentElement.SelectSingleNode("//" + _el.Name);
                        newnode.InnerXml = samplenode.InnerXml;
                        parent.AppendChild(newnode);
                    }
                }
            }
            hasPendingChanges();
        }


    }
}

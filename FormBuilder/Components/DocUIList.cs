using Org.DocUI.Tools;
using Org.DocUI.Wpf;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Schema;

namespace Org.DocUI.FormBuilder.Components
{
    public class DocUIList : AbstractDocUIComponent
    {
        private readonly StackPanel _listpanel;
        private readonly List<ListItemComponent> _optlist;
        private readonly XmlSchemaElement _el;
        private readonly XmlNode _parent;

        public override object Value
        {
            get { return null; }
            set { }
        }

        public DocUIList(XmlNode xmlNode, XmlSchemaAnnotated xsdNode, Panel contentpanel, Panel overlaypanel, DynamicForm parentForm, bool horizontal = true)
            : base(xmlNode, xsdNode, contentpanel, overlaypanel, parentForm)
        {
            _optlist = new List<ListItemComponent>();
            _parent = xmlNode;
            _listpanel = new StackPanel();
            _listpanel.Orientation = horizontal ? Orientation.Horizontal : Orientation.Vertical;
            XmlSchemaElement schemaEl = xsdNode as XmlSchemaElement;
            if (schemaEl != null)
            {
                XmlSchemaSequence seq = XmlSchemaUtilities.tryGetSequence(schemaEl.ElementSchemaType);
                if (seq != null && seq.Items.Count == 1 && seq.Items[0] is XmlSchemaElement)
                {
                    _el = seq.Items[0] as XmlSchemaElement;
                    //get all elements from current node
                    foreach (XmlNode node in xmlNode.ChildNodes)
                    {
                        ListItemComponent lio = new ListItemComponent(node, _el, _listpanel, overlaypanel, _optlist, parentForm);
                        _optlist.Add(lio);
                    }
                }

                Button add = new Button();
                add.Content = "Add item";
                add.Click += AddItem;
                _listpanel.Children.Add(add);
                contentpanel.Children.Add(_listpanel);
            }
        }

        public override void update()
        {
            foreach (ListItemComponent opt in _optlist)
                opt.Update();
        }

        /// <summary>
        /// Adds an item to the list and appends a child to the xmlnode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void AddItem(object sender, EventArgs args)
        {
            if (_el != null)
            {
                XmlNode node = _parent.OwnerDocument.CreateElement("");
                _parent.AppendChild(node);
                ListItemComponent lio = new ListItemComponent(node, _el, _listpanel, Overlaypanel, _optlist, ParentForm);
                _optlist.Add(lio);
            }
        }

    }
}

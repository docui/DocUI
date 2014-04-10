using Org.DocUI.FormBuilder;
using Org.DocUI.FormBuilder.Components;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Schema;

namespace Org.DocUI.Wpf
{
    class ListItemComponent
    {
        private StackPanel _listpanel;
        private StackPanel _itempanel;

        private List<AbstractDocUIComponent> _optionlist;
        private List<ListItemComponent> _liolist;
        private XmlNode _node;

        public ListItemComponent(XmlNode xmlNode, XmlSchemaElement xsdNode, StackPanel contentpanel, Panel overlaypanel, List<ListItemComponent> liolist, DynamicForm parentForm)
        {
            _optionlist = new List<AbstractDocUIComponent>();
            _itempanel = new StackPanel() { Orientation = Orientation.Horizontal };
            this._liolist = liolist;
            this._listpanel = contentpanel;
            this._node = xmlNode;
            Utilities.recursive(xsdNode, xmlNode, contentpanel, overlaypanel, (comp) =>
            {
                comp.placeOption();
                _optionlist.Add(comp);
            }, parentForm);

            Button delete = new Button();
            delete.Content = "Remove item";
            delete.Click += RemoveItem;
            delete.Click += (s, e) => { parentForm.PendingChanges = true; };
            _itempanel.Children.Add(delete);
            contentpanel.Children.Add(_itempanel);
        }

        public void Update()
        {
            foreach (AbstractDocUIComponent io in _optionlist)
                io.update();
        }

        public void RemoveItem(object sender, EventArgs args)
        {
            _listpanel.Children.Remove(_itempanel);
            _liolist.Remove(this);
            this._node.ParentNode.RemoveChild(this._node);
        }

    }
}

using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Schema;
using Xceed.Wpf.Toolkit;

namespace Org.DocUI.FormBuilder.Components
{
    /// <summary>
    /// This option allows the user to enter timedata.
    /// </summary>
    public class DocUITime : AbstractDocUIComponent
    {
        /// <summary>
        /// Creates a new instance of the TimeOption
        /// </summary>
        /// <param name="xmlNode">The xmlnode containing the data.</param>
        /// <param name="xsdNode">The corresponding xsdnode.</param>
        /// <param name="panel">The panel on which this option should be placed.</param>
        /// <param name="parentForm">The form of which this option is a part.</param>
        public DocUITime(XmlNode xmlNode, XmlSchemaAnnotated xsdNode, Panel contentpanel, Panel overlaypanel, DynamicForm parentForm) :
            base(xmlNode, xsdNode, contentpanel, overlaypanel, parentForm)
        {
            Control = new TimePicker() { Format = TimeFormat.Custom, FormatString = "HH:mm:ss" };
            (Control as TimePicker).ValueChanged += (s, e) => { hasPendingChanges(); };

            setup();
        }

        /// <summary>
        /// Positions this option on the panel.
        /// </summary>
        public override void placeOption()
        {
            // placed on stack panel so that, timeoption does not get stretched.
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

        /// <summary>
        /// Gets or sets the value of the Control (time spinner).
        /// </summary>
        public override object Value
        {
            get
            {
                TimePicker t = Control as TimePicker;
                if (t.Value != null)
                {
                    string temp = t.Value.Value.ToLongTimeString();
                    // add aditional 0 (dirty fix)
                    if (temp.ToString().IndexOf(':') < 2)
                        temp = "0" + temp;
                    return temp.Substring(0, 8);
                }
                else
                {
                    return null;
                }
            }
            set
            {
                // parse parts of time
                /*
                int firstColon = value.ToString().IndexOf(':');
                int lastColon = value.ToString().LastIndexOf(':');
                int hrs = int.Parse(value.ToString().Substring(0, firstColon));
                int min = int.Parse(value.ToString().Substring(firstColon + 1, lastColon - (firstColon + 1)));
                int sec = int.Parse(value.ToString().Substring(lastColon + 1));

                (Control as TimePicker).Value = new DateTime(1, 1, 1, hrs, min, sec);
                 */
                (Control as TimePicker).Value = XmlConvert.ToDateTime(value.ToString());
            }
        }

        public override void update()
        {
            if (this.Value == null)
            {
                (this.Control as TimePicker).Value = new System.DateTime(1900, 1, 1, 1, 0, 0);
            }
            else
            {
                (this.Control as TimePicker).Value = XmlConvert.ToDateTime(this.Value.ToString());
            }
            base.update();
        }

    }
}

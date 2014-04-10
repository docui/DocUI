using System;
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
    public class DocUIDateTime : AbstractDocUIComponent
    {
        private StackPanel stack;
        private DatePicker DateControl;
        private TimePicker TimeControl;

        /// <summary>
        /// Creates a new instance of the TimeOption
        /// </summary>
        /// <param name="xmlNode">The xmlnode containing the data.</param>
        /// <param name="xsdNode">The corresponding xsdnode.</param>
        /// <param name="contentpanel">The panel on which this option should be placed.</param>
        /// <param name="parentForm">The form of which this option is a part.</param>
        public DocUIDateTime(XmlNode xmlNode, XmlSchemaAnnotated xsdNode, Panel contentpanel, Panel overlaypanel, DynamicForm parentForm) :
            base(xmlNode, xsdNode, contentpanel, overlaypanel, parentForm)
        {
            //Control = new DatePicker(); //{ Form Format = TimeFormat.Custom, FormatString = "HH:mm:ss" };
            //(Control as DatePicker).ValueChanged += (s, e) => { hasPendingChanges(); };

            stack = new StackPanel() { Orientation = Orientation.Horizontal };
            Control = stack;
            DateControl = new DatePicker();
            DateControl.SelectedDateChanged += (s, e) => { hasPendingChanges(); };
            TimeControl = new TimePicker() { Format = TimeFormat.Custom, FormatString = "HH:mm:ss" };
            TimeControl.ValueChanged += (s, e) => { hasPendingChanges(); };

            setup();
        }

        /// <summary>
        /// Positions this option on the panel.
        /// </summary>
        public override void placeOption()
        {
            // placed on stack panel so that, timeoption does not get stretched.
            stack.Children.Add(DateControl);
            stack.Children.Add(TimeControl);

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
                string returnValue = "";
                DatePicker d = DateControl;
                d.Margin = new Thickness(0, 0, 5, 0);
                TimePicker t = TimeControl;
                if (d.SelectedDate != null)
                {
                    //string temp = t.Value.Value.ToLongTimeString();
                    // add aditional 0 (dirty fix)
                    //if (temp.ToString().IndexOf(':') < 2)
                    //    temp = "0" + temp;
                    //return temp.Substring(0, 8);
                    //return t.Text;
                    returnValue += ((DateTime)d.SelectedDate).ToString("yyyy-MM-dd");
                    if (t.Value != null)
                    {
                        string temp = t.Value.Value.ToLongTimeString();
                        // add aditional 0 (dirty fix)
                        if (temp.ToString().IndexOf(':') < 2)
                            temp = "0" + temp;
                        returnValue += "T" + temp.Substring(0, 8);
                    }
                }
                if (returnValue != "")
                {
                    return returnValue;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                // parse parts of time
                DateControl.SelectedDate = XmlConvert.ToDateTime(value.ToString());
                DateControl.DisplayDate = XmlConvert.ToDateTime(value.ToString());
                TimeControl.Value = XmlConvert.ToDateTime(value.ToString());
            }
        }

        public override void update()
        {
            if (this.Value == null)
            {
                DateControl.DisplayDate = DateTime.Now;
                DateControl.SelectedDate = DateTime.Now;
                TimeControl.Value = DateTime.Now;
            }
            else
            {
                try
                {
                    DateControl.SelectedDate = XmlConvert.ToDateTime(this.Value.ToString());
                    DateControl.DisplayDate = XmlConvert.ToDateTime(this.Value.ToString());
                    TimeControl.Value = XmlConvert.ToDateTime(this.Value.ToString());
                }
                catch
                {
                    //TODO: log 
                }
            }
            base.update();
        }

    }
}

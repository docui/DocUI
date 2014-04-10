using Org.DocUI.Tools;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Schema;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.Primitives;

namespace Org.DocUI.FormBuilder.Components
{
    public class DocUIInteger : AbstractDocUIComponent
    {
        private readonly int _defaultValue;

        /// <summary>
        /// Creates a new instance of the IntegerOption
        /// </summary>
        /// <param name="xmlNode">The node that contains the data for the integerOption</param>
        /// <param name="xsdNode">The corresponding xsdNode.</param>
        /// <param name="panel">The panel in which this option should be placed.</param>
        /// <param name="parentForm">The form of which this option is a part.</param>
        public DocUIInteger(XmlNode xmlNode, XmlSchemaAnnotated xsdNode, Panel contentpanel, Panel overlaypanel, DynamicForm parentForm) :
            base(xmlNode, xsdNode, contentpanel, overlaypanel, parentForm)
        {
            int maxIncl = XmlSchemaUtilities.tryGetMaxIncl(xsdNode);
            int minIncl = XmlSchemaUtilities.tryGetMinIncl(xsdNode);
            _defaultValue = minIncl;

            Control = new DoubleUpDown()
            {
                ShowButtonSpinner = true,
                AllowSpin = true,
                MouseWheelActiveTrigger = MouseWheelActiveTrigger.MouseOver,
                Increment = 1,
                ClipValueToMinMax = true,
                Minimum = minIncl,
                Maximum = maxIncl
            };
            (Control as DoubleUpDown).ValueChanged += (s, e) => { hasPendingChanges(); };

            setup();
        }

        /// <summary>
        /// Gets or sets the current value of the spinner
        /// </summary>
        public override object Value
        {
            get
            {
                return (Control as DoubleUpDown).Value;
            }
            set
            {
                (Control as DoubleUpDown).Value = int.Parse(value.ToString());
            }
        }

        /// <summary>
        /// Places this option on the panel.
        /// </summary>
        public override void placeOption()
        {
            // put combobox in stackpanel so that it is not stretched over full width.
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

        public override void update()
        {
            (this.Control as DoubleUpDown).CommitInput();
            if (this.Value == null)
            {
                (this.Control as DoubleUpDown).Text = "" + _defaultValue;
            }
            base.update();
        }

    }
}

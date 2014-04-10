using Common.Logging;
using Org.DocUI.Tools;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Schema;

namespace Org.DocUI.FormBuilder.Components
{
    public abstract class AbstractDocUIComponent
    {
        protected ILog Log;

        /// <summary>
        /// This is the description of this option.
        /// Will be used as display label.
        /// </summary>
        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                Label.Text = value;
            }
        }

        private bool _visible;
        public bool Visible
        {
            get { return _visible; }
            protected set
            {
                _visible = value;
                /*
                                if (value)
                                {
                                    Control.Visibility = Visibility.Visible;
                                } else
                                {
                                    Control.Visibility = Visibility.Collapsed;
                                }
                */
            }
        }

        /// <summary>
        /// This is the label that will be displayed.
        /// </summary>
        public TextBlock Label { get; protected set; }
        /// <summary>
        /// This is the control that is linked to the xml node
        /// </summary>
        public FrameworkElement Control { get; protected set; }
        /// <summary>
        /// set or get the value of the current control
        /// </summary>
        public abstract object Value { get; set; }
        /// <summary>
        /// Is the parent form of this option
        /// </summary>
        public virtual DynamicForm ParentForm { get; set; }
        /// <summary>
        /// The panel on which the component should be placed
        /// </summary>
        public virtual Panel Contentpanel { get; set; }
        /// <summary>
        /// The panel which represents the 'rootpanel' (isn't necessarily the dynamicform itself)
        /// </summary>
        public virtual Panel Overlaypanel { get; set; }

        /// <summary>
        /// The current xmlnode reached in the xml document
        /// </summary>
        public XmlNode xmlNode;

        /// <summary>
        /// The current xsdnode reached in the csd document
        /// </summary>
        public XmlSchemaAnnotated xsdNode;

        /// <summary>
        /// Creates a new AbstractOption. This, of course, is impossible, due to the fact that AbstractOption is an abstract class.
        /// </summary>
        /// <param name="xmlNode">The xml node containing the data that will be manipulated.</param>
        /// <param name="xsdNode">The corresponding xsdnode of this xmlnode.</param>
        /// <param name="contentpanel">The panel on which this option should be placed.</param>
        /// <param name="parentForm">The form of which this option is a part.</param>
        /// <param name="ismdname">Whether this option allows metadata and if so, whether it needs the name or the value of the metadata.</param>
        public AbstractDocUIComponent(XmlNode xmlNode, XmlSchemaAnnotated xsdNode, Panel contentpanel = null, Panel overlaypanel = null, DynamicForm parentForm = null)
        {
            Log = LogManager.GetCurrentClassLogger();

            Label = new TextBlock() { Margin = new Thickness(5) };
            this.xmlNode = xmlNode;
            this.xsdNode = xsdNode;
            this.Contentpanel = contentpanel;
            this.ParentForm = parentForm;
            this.Overlaypanel = overlaypanel;
            Description = XmlSchemaUtilities.tryGetDocumentation(xsdNode);
            Visible = XmlSchemaUtilities.tryGetUnhandledAttribute(xsdNode, "visible") == "false" ? false : true;
            if (Description == null)
            {
                Description = xmlNode.LocalName;
            }
        }

        /// <summary>
        /// Let the form know that this option has been modified.
        /// </summary>
        public void hasPendingChanges()
        {
            if (ParentForm != null)
                ParentForm.PendingChanges = true;
        }

        /// <summary>
        /// this will Update the xml node to the current value of the control
        /// </summary>
        /// Default implementations are given here.
        public virtual void update()
        {
            if (Value != null)
            {
                xmlNode.InnerText = Value.ToString();
            }
            else
            {
                xmlNode.InnerText = "";
            }
        }

        /// <summary>
        /// Check whether or not the form is Valid
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckValid()
        {
            return true;
        }

        /// <summary>
        /// this will position the option in the assigned panel.
        /// </summary>
        /// Default implementations are given here.
        public virtual void placeOption()
        {
            if (Contentpanel == null) { return; }

            if (Contentpanel is Grid)
            {
                Grid g = Contentpanel as Grid;
                GridLength height;
                if (Visible)
                {
                    height = new GridLength(1, GridUnitType.Auto);
                }
                else
                {
                    height = new GridLength(0);
                }
                g.RowDefinitions.Add(new RowDefinition() { Height = height });
                int nr = g.RowDefinitions.Count;
                Grid.SetRow(Label, nr - 1);
                Grid.SetRow(Control, nr - 1);

                Grid.SetColumn(Label, 0);
                Grid.SetColumn(Control, 1);
            }

            Contentpanel.Children.Add(Label);
            Contentpanel.Children.Add(Control);
        }

        /// <summary>
        /// Called and executed after component construction
        /// </summary>
        protected virtual void setup()
        {
            Value = xmlNode.InnerText;
            Control.Margin = new Thickness(5);
            //Visible = vis == "true" ? true : false;
            //this.Control.GotFocus += onGotFocus;
        }

        /// <summary>
        /// Updates the "LastFocusedElement" of the parentForm.
        /// </summary>
        /// <param name="sender">isn't used</param>
        /// <param name="e">isn't used</param>
        public void onGotFocus(object sender, RoutedEventArgs e)
        {
            if (ParentForm != null)
                ParentForm.LastFocusedElement = this;
        }

    }
}

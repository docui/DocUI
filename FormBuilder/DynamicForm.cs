using Common.Logging;
using Org.DocUI.FormBuilder.Components;
using Org.DocUI.Tools;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using MessageBox = System.Windows.MessageBox;
using Panel = System.Windows.Controls.Panel;
using UserControl = System.Windows.Controls.UserControl;

namespace Org.DocUI.FormBuilder
{
    public delegate AbstractDocUIComponent getProduct(XmlSchemaAnnotated xsdnode, XmlNode xmlnode, Panel content, Panel overlay, DynamicForm parentForm);

    public class DynamicForm : UserControl
    {
        private static readonly ILog Log;

        public DynamicFormTemplate Form { get; private set; }

        /// <summary>
        /// The last option that has been focussed.
        /// </summary>
        public AbstractDocUIComponent LastFocusedElement;

        /// <summary>
        /// Whether there are still options that have unsaved changes.
        /// </summary>
        protected bool _pendingChanges;
        public virtual bool PendingChanges
        {
            get
            {
                return _pendingChanges;
            }
            set
            {
                _pendingChanges = value;
            }
        }

        /// <summary>
        /// The dictionary that maps _xml type names on component names.
        /// </summary>
        public static Dictionary<string, string> Components;

        /// <summary>
        /// The top level Components
        /// </summary>
        protected List<AbstractDocUIComponent> Optionlist;

        /// <summary>
        /// The document the file represents
        /// </summary>
        private XmlDocument _xml;
        public XmlDocument Xml
        {
            get
            {
                foreach (AbstractDocUIComponent io in Optionlist)
                {
                    io.update();
                }
                return _xml;
            }
            set
            {
                _xml = value;
            }
        }

        /// <summary>
        /// The XMLschema the document refers to
        /// </summary>
        protected XmlSchema FormSchema;

        /// <summary>
        /// The dictionary that maps component names on Optionfactories.
        /// </summary>
        /// public Dictionary<string, getProduct> Factory { get; protected set; }
        /// <summary>
        /// Initiates the dictionaries.
        /// </summary>
        static DynamicForm()
        {
            Log = LogManager.GetCurrentClassLogger();

            Components = new Dictionary<string, string>();
            // initiate Components dictionary
            XmlDocument componentsDoc = new XmlDocument(); //EmbeddedResourceTools.GetXmlDocument("Org.DocUI.Resources.DocUI.Components.xml", Assembly.GetExecutingAssembly());
            componentsDoc.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream("Org.DocUI.Resources.DocUI.Components.xml"));
            InitComponents(componentsDoc);
        }

        public static void InitComponents(XmlDocument compdoc)
        {
            foreach (XmlNode node in compdoc.DocumentElement.SelectNodes("Component"))
            {
                string name = node.Attributes["schematype"].Value;
                string value = node.Attributes["type"].Value;
                if (Components.ContainsKey(name))
                {
                    Components.Remove(name);
                }
                Components.Add(name, value);
            }
        }

        public DynamicForm()
        {

        }

        public DynamicForm(string schema)
        {
            XmlDocument doc = XmlSchemaUtilities.generateCleanXmlToString(schema);
            Setup(doc);
        }

        public DynamicForm(XmlDocument doc)
        {
            Setup(doc);
        }

        protected void Setup(XmlDocument doc)
        {
            Form = new DynamicFormTemplate();
            this.AddChild(Form);

            this.Optionlist = new List<AbstractDocUIComponent>();

            this.FormSchema = XmlSchemaUtilities.getXmlSchemaFromXml(doc);

            if (!XmlValidator.Validate(doc, FormSchema))
            {
                MessageBox.Show("Your file does not match the schema. It is invalid, please create a new file or check the log for more information.");
                return;
            }
            this.Xml = doc;

            LoadComponents();
        }

        /// <summary>
        /// This method can be called after object construction (so that all fields are initialized)
        /// </summary>
        protected void LoadComponents()
        {
            if (Xml != null)
            {
                XmlSchemaElement el = XmlSchemaUtilities.tryGetElement(FormSchema);
                if (el != null)
                {
                    // load all Components
                    Utilities.recursive((XmlSchemaElement)el, Xml.DocumentElement, this.Form.formgrid, this.Form.formgrid, (comp) =>
                    {
                        comp.placeOption();
                        Optionlist.Add(comp);
                    }, this);
                }
                else
                {
                    Log.Info("No SchemaElement was found.");
                }
            }
        }

        public bool CheckValid()
        {
            bool valid = true;
            foreach (AbstractDocUIComponent io in Optionlist)
                valid = io.CheckValid() && valid;
            return valid;
        }
    }
}

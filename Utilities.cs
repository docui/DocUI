using Org.DocUI.FormBuilder;
using Org.DocUI.FormBuilder.Components;
using Org.DocUI.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using System.Xml.Schema;

namespace Org.DocUI
{
    public delegate void processDocUIComponent(AbstractDocUIComponent comp);

    /// <summary>
    /// This class contains several static methods that can be used when parsing xmlschema file.
    /// </summary>
    public abstract class Utilities
    {
        /// <summary>
        /// This method parses through an xml schema file. Whenever it stumbles upon a xml Type, it takes the following actions:
        /// 1: Check whether this a component in the component.xml file. 
        ///    If so, there is a corresponding Option class which can handle the loading of this xml node.
        ///    The parsing will continue with the next type.
        /// 2: If this is not a component, the parser will check if the type is a complex type.
        ///    The methode will start recursive calls to get to the inside of this complex type.
        ///    The parsing restarts from step 1.
        /// 3: If the type is not complex, it is ignored. All simple types should be defined in the Components.xml file.
        /// </summary>
        /// <param name="xsdnode">The schema node that is parsed.</param>
        /// <param name="xmlnode">The corresponding xmlnode in the data file.</param>
        /// <param name="p">The panel on which loaded Components will be placed.</param>								// TODO: add delegate to handle creation of new option
        /// <param name="optionlist">The list of options to which the newly created option will be added.</param>		//		 this will remove the last 3 parameters
        /// <param name="form">The form to which this option will be added.</param>										//
        public static void recursive(XmlSchemaAnnotated xsdnode, XmlNode xmlnode, Panel content, Panel overlay, processDocUIComponent comp, DynamicForm manager)
        {
            string result = "";
            XmlSchemaType type = null;
            if (xsdnode is XmlSchemaElement)
            {
                type = ((XmlSchemaElement)xsdnode).ElementSchemaType;
            }

            if (xsdnode is XmlSchemaAttribute)
            {
                type = ((XmlSchemaAttribute)xsdnode).AttributeSchemaType;
            }

            if (DynamicForm.Components.TryGetValue(type.QualifiedName.Name, out result))
            {
                //getProduct gp;
                AbstractDocUIComponent abstractComp = ComponentUtilities.TryGetComponent(result, new object[] { xmlnode, xsdnode, content, overlay, manager });
                /*if (!Manager.Factory.TryGetValue(result, out gp))
                    gp = (_xsdNode, _xmlNode, _content, _overlay, _form) => { return new DocUIString(_xmlNode, _xsdNode as XmlSchemaElement, _content, _overlay, _form); };

                AbstractDocUIComponent abstractComp = gp(xsdnode, xmlnode,content,overlay,Manager);*/
                if (abstractComp == null)
                {
                    abstractComp = new DocUIString(xmlnode, xsdnode, content, overlay, manager);
                }
                comp(abstractComp);
            }
            else
            {
                XmlSchemaSequence seq = XmlSchemaUtilities.tryGetSequence(type);
                if (seq != null)
                {
                    foreach (XmlSchemaElement el in seq.Items)
                    {
                        XmlNamespaceManager nsm = new XmlNamespaceManager(xmlnode.OwnerDocument.NameTable);
                        nsm.AddNamespace("ns", el.QualifiedName.Namespace);
                        recursive(el, xmlnode.SelectSingleNode("ns:" + el.QualifiedName.Name, nsm), content, overlay, comp, manager);
                    }
                }
                else //try get attr
                {
                    XmlSchemaObjectCollection attributes = XmlSchemaUtilities.tryGetAttributes(type);
                    if (attributes != null)
                    {
                        foreach (XmlSchemaAttribute att in attributes)
                        {
                            XmlNamespaceManager nsm = new XmlNamespaceManager(xmlnode.OwnerDocument.NameTable);
                            nsm.AddNamespace("ns", att.QualifiedName.Namespace);
                            recursive(att, xmlnode.SelectSingleNode("@ns:" + att.QualifiedName.Name, nsm), content, overlay, comp, manager);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This function will search for a component that is visually a parent of the child componennt.
        /// The use of this function is deprecated. Please use the provided Components.
        /// </summary>
        /// <param name="child">The child component from which the</param>
        /// <param name="type">The type of component to search in the parent tree.</param>
        /// <returns>The parent of the type "type"</returns>
        [Obsolete("Improper to use. Please avoid", true)]
        public static T getRoot<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) { return default(T); }
            return parentObject is T ? (T)parentObject : getRoot<T>(parentObject);
        }

        /// <summary>
        /// This will replace metadata that in a string with it's actual value.
        /// Metadata is recognised by the surrounding of '[' and ']'. Every time "[metadata_name]" is found in the string,
        /// the method will search for a node 'metadata_name' in the metadata Xmlnode. If it is found,
        /// It will replace the '[metadata_name]' with '"metadata_value"' (Note the double quotation mark).
        /// Here "metadata_value" is the inner text of the node "metadata_name.
        /// NOTE: metadata in metadata is possible, example [metadata_[name]] will be replaced with
        /// "metadata_value_name_value".
        /// </summary>
        /// <param name="input">The input string containing metadata</param>
        /// <param name="metadata">The tree containing the metadata</param>
        /// <returns>The input string with replaced metadata</returns>
        public static string ReplaceMetadata(string input, XmlNode metadata)
        {
            string newString = input;

            Stack<int> brackets = new Stack<int>();
            int stack_size = 0;
            for (int index = 0; index < newString.Length; ++index)
            {
                switch (newString[index])
                {
                    case '[':
                        brackets.Push(index);
                        ++stack_size;
                        break;
                    case ']':
                        int prev = brackets.Pop();
                        string metaField = newString.Substring(prev + 1, index - (prev + 1));
                        string metaFieldWithBrackets = newString.Substring(prev, index - prev + 1);

                        // replace metaField
                        string xpath = "//" + metaField;
                        XmlNode node = (from XmlNode m in metadata.ChildNodes where m.Name == metaField select m).First<XmlNode>();
                        string replacement = node != null ? node.InnerText : "";

                        if (stack_size == 0)
                            replacement = "\"" + replacement + "\"";

                        newString = newString.Replace(metaFieldWithBrackets, replacement);

                        // fix index
                        index = prev + replacement.Length;
                        break;
                }
            }

            return newString;
        }

        public static string getDiffPath(string sourceString, string removeString)
        {
            int index = sourceString.IndexOf(removeString);
            string cleanPath = (index < 0)
                ? sourceString
                : sourceString.Remove(index, removeString.Length);
            return cleanPath;
        }

        public static string removeExtension(string source)
        {
            int index = source.LastIndexOf(".");
            if (index >= 0)
            {
                source = source.Substring(0, index);
            }
            return source;
        }

        public static string removeFirstFolder(string source)
        {
            source = source.TrimStart(new[] { '/', '\\' });
            string sepchar = source.Contains('/') ? @"/" : @"\";
            int index = source.IndexOf(sepchar);

            if (index > 0)
            {
                source = getDiffPath(source, source.Substring(0, index + 1));
            }
            return source;
        }
    }
}

using Common.Logging;
using Microsoft.Xml.XMLGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace Org.DocUI.Tools
{
    public static class XmlSchemaUtilities
    {
        private static ILog _log;

        /// <summary>
        /// Generates a new xml document based on a xml schema.
        /// Default values are important, because these will be filled in the innertext of a node.
        /// Nodes that have a minOccurance of 0 will not be created.
        /// If the xml file at the given location allready exists, it will be overwritten.
        /// 
        /// The root node of the new xmlfile will contain a "noNamespaceSchemaLocation" attribute.
        /// The value will be the given xml schema.
        /// </summary>
        /// <param name="xml">The location of the new xml file.</param>
        /// <param name="xsd">The xml schema.</param>
        /// <returns>The xml document in which the newly created xml file is loaded.</returns>
        /// 

        private const string xsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";

        static XmlSchemaUtilities()
        {
            _log = LogManager.GetCurrentClassLogger();
        }

        public static XmlDocument generateCleanXml(string xml, string xsd, string schemarelativepath)
        {
            using (XmlTextWriter textWriter = new XmlTextWriter(xml, null))
            {
                textWriter.Formatting = Formatting.Indented;
                XmlSampleGenerator generator = new XmlSampleGenerator(xsd, null);
                generator.MaxThreshold = 0;
                generator.WriteXml(textWriter);
            }

            // adjust xml directly
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(xml);
                XmlAttribute schemaReference = doc.CreateAttribute("noNamespaceSchemaLocation", xsiNamespace);
                string fileName = schemarelativepath + @"\" + xsd.Substring(xsd.LastIndexOf('\\') + 1);
                schemaReference.Value = fileName;
                doc.DocumentElement.Attributes.Append(schemaReference);
                doc.Save(xml);
            }
            catch { _log.Error("Sample document could not be loaded. Path: " + xml); }

            return doc;
        }

        /// <summary>
        /// Generates a new xml document and writes it to a string.
        /// This is usefull when only needing a part of the new xml file.
        /// </summary>
        /// <param name="source">The schema file.</param>
        /// <returns>the Xml document representing the newly created xml.</returns>
        public static XmlDocument generateCleanXmlToString(string source)
        {
            XmlDocument doc = new XmlDocument();

            using (var sw = new StringWriter())
            {
                using (var xw = XmlWriter.Create(sw))
                {
                    XmlSampleGenerator generator = new XmlSampleGenerator(source, null);
                    generator.WriteXml(xw);
                    doc.LoadXml(sw.ToString());
                    XmlAttribute schemaReference = doc.CreateAttribute("noNamespaceSchemaLocation", xsiNamespace);
                    schemaReference.Value = (new Uri(Assembly.GetEntryAssembly().Location)).MakeRelativeUri(new Uri(source)).ToString().Replace(@"Resources/DocUI/", "");
                    doc.DocumentElement.Attributes.Append(schemaReference);
                }

            }

            return doc;
        }

        /// <summary>
        /// This method gets a sequence from the the given xmlschematype.
        /// If no sequence is available, null will be returned.
        /// </summary>
        /// <param name="type">The schematype in which a sequence will be searched.</param>
        /// <returns>The xml sequence form the schematype or null if none exists.</returns>
        public static XmlSchemaSequence tryGetSequence(XmlSchemaType type)
        {
            if (type is XmlSchemaComplexType)
            {
                XmlSchemaComplexType ctype = type as XmlSchemaComplexType;
                if (ctype.Particle is XmlSchemaSequence)
                {
                    return ctype.Particle as XmlSchemaSequence;
                }
            }

            _log.Info("no sequence was found");
            return null;
        }

        public static XmlSchemaAttribute tryGetAttribute(XmlSchemaComplexType ctype, string name)
        {
            foreach (XmlSchemaAttribute att in ctype.Attributes)
            {
                if (att.Name == name)
                {
                    return att;
                }
            }
            return null;
        }

        public static XmlSchemaObjectCollection tryGetAttributes(XmlSchemaType type)
        {
            if (type is XmlSchemaComplexType)
            {
                XmlSchemaComplexType ctype = type as XmlSchemaComplexType;
                return ctype.Attributes;
            }
            return null;
        }

        /// <summary>
        /// This function will fetch an enumeration restriction from a schematype.
        /// If none is available, null will be returned.
        /// </summary>
        /// <param name="type">the schema type.</param>
        /// <returns>the available enumeration restriction or null.</returns>
        public static IEnumerable<XmlSchemaEnumerationFacet> tryGetEnumRestrictions(XmlSchemaType type)
        {
            if (type is XmlSchemaSimpleType)
            {
                XmlSchemaSimpleType simpleType = type as XmlSchemaSimpleType;
                if (simpleType.Content is XmlSchemaSimpleTypeRestriction)
                {
                    XmlSchemaSimpleTypeRestriction restriction = simpleType.Content as XmlSchemaSimpleTypeRestriction;
                    var enumFacets = restriction.Facets.OfType<XmlSchemaEnumerationFacet>();

                    if (enumFacets.Any())
                    {
                        return enumFacets;
                    }
                }
            }

            _log.Warn("no restrictions could be found");
            return null;
        }

        /// <summary>
        /// Searches for a maxIncl restriction in the given schematype.
        /// If none was found, 0 will be returned.
        /// </summary>
        /// <param name="type">The schema type</param>
        /// <returns>The max inclusive bound.</returns>
        public static int tryGetMaxIncl(XmlSchemaAnnotated node)
        {
            XmlSchemaType type = getType(node);
            if (type is XmlSchemaSimpleType)
            {
                XmlSchemaSimpleType simpleType = type as XmlSchemaSimpleType;
                if (simpleType.Content is XmlSchemaSimpleTypeRestriction)
                {
                    XmlSchemaSimpleTypeRestriction restriction = simpleType.Content as XmlSchemaSimpleTypeRestriction;
                    var enumFacets = restriction.Facets.OfType<XmlSchemaMaxInclusiveFacet>();

                    if (enumFacets.Any())
                    {
                        XmlSchemaMaxInclusiveFacet bound = enumFacets.First<XmlSchemaMaxInclusiveFacet>();
                        if (bound != null)
                        {
                            try
                            {
                                return int.Parse(bound.Value);
                            }
                            catch (FormatException e)
                            {
                                _log.Info("could not parse this bound restriction: " + e.Message);
                                return 0;
                            }
                        }
                    }
                }
            }

            _log.Warn("no restriction could be found");
            return 0;
        }

        /// <summary>
        /// Searches for a minIncl restriction in the given schematype.
        /// If none was found, 0 will be returned.
        /// </summary>
        /// <param name="type">the schema type</param>
        /// <returns>the min incl bound.</returns>
        public static int tryGetMinIncl(XmlSchemaAnnotated node)
        {
            XmlSchemaType type = getType(node);

            if (type is XmlSchemaSimpleType)
            {
                XmlSchemaSimpleType simpleType = type as XmlSchemaSimpleType;
                if (simpleType.Content is XmlSchemaSimpleTypeRestriction)
                {
                    XmlSchemaSimpleTypeRestriction restriction = simpleType.Content as XmlSchemaSimpleTypeRestriction;
                    var enumFacets = restriction.Facets.OfType<XmlSchemaMinInclusiveFacet>();

                    if (enumFacets.Any())
                    {
                        XmlSchemaMinInclusiveFacet bound = enumFacets.First<XmlSchemaMinInclusiveFacet>();
                        if (bound != null)
                        {
                            try
                            {
                                return int.Parse(bound.Value);
                            }
                            catch (FormatException e)
                            {
                                _log.Info("could not parse this bound restriction: " + e.Message);
                                return 0;
                            }
                        }
                    }
                }
            }

            _log.Warn("no restriction could be found");
            return 0;
        }

        public static XmlSchemaType getType(XmlSchemaAnnotated node)
        {
            XmlSchemaType type = null;
            if (node is XmlSchemaElement)
            {
                type = (node as XmlSchemaElement).ElementSchemaType;
            }
            else if (node is XmlSchemaAttribute)
            {
                type = (node as XmlSchemaAttribute).AttributeSchemaType;
            }
            return type;
        }

        /// <summary>
        /// Searches for a pattern restriction in the given schema type.
        /// If none is available, null will be returned.
        /// </summary>
        /// <param name="type">the schema type</param>
        /// <returns>The pattern restriction or null.</returns>
        public static string tryGetPatternRestriction(XmlSchemaAnnotated node)
        {
            XmlSchemaType type = getType(node);
            if (type is XmlSchemaSimpleType)
            {
                XmlSchemaSimpleType simpleType = type as XmlSchemaSimpleType;
                if (simpleType.Content is XmlSchemaSimpleTypeRestriction)
                {
                    XmlSchemaSimpleTypeRestriction restriction = simpleType.Content as XmlSchemaSimpleTypeRestriction;
                    if (restriction.Facets.Count > 0 && restriction.Facets[0] is XmlSchemaPatternFacet)
                    {
                        XmlSchemaPatternFacet pattern = restriction.Facets[0] as XmlSchemaPatternFacet;
                        return pattern.Value;
                    }
                }
            }
            _log.Warn("no pattern restriction was found");
            return null;
        }

        /// <summary>
        /// Searches for an unhandled attribute in the schema element and the schema type.
        /// The namespace of these unhandled attributes should be "uispecificattributes"
        /// If none was found, "" will be returned.
        /// </summary>
        /// <param name="elem">The schema element.</param>
        /// <param name="attName">The name of the unhandled attribute.</param>
        /// <returns>the value of the unhandled attribute.</returns>
        public static string tryGetUnhandledAttribute(XmlSchemaAnnotated elem, string attName)
        {
            string attValue = "";

            XmlAttribute[] attList;
            if (elem is XmlSchemaElement)
            {
                attList = (elem as XmlSchemaElement).ElementSchemaType.UnhandledAttributes;

                if (attList != null)
                    foreach (XmlAttribute att in attList)
                        if (att.NamespaceURI == "uispecificattributes")
                            if (att.LocalName == attName)
                            {
                                attValue = att.Value;
                                break;
                            }
            }


            attList = elem.UnhandledAttributes;
            if (attList != null)
                foreach (XmlAttribute att in attList)
                    if (att.NamespaceURI == "uispecificattributes")
                        if (att.LocalName == attName)
                        {
                            attValue = att.Value;
                            break;
                        }

            return attValue;
        }

        /// <summary>
        /// Searches for documentation in an xml schema type.
        /// Only the first entry of the documentation will be returned.
        /// returns "" if no documentation was found.
        /// </summary>
        /// <param name="elem">the schema type.</param>
        /// <returns>the documentation string</returns>
        public static string tryGetDocumentation(XmlSchemaAnnotated elem)
        {
            if (elem.Annotation != null)
            {
                foreach (XmlSchemaObject a in elem.Annotation.Items)
                {
                    if (a is XmlSchemaDocumentation)
                    {
                        XmlSchemaDocumentation doc = a as XmlSchemaDocumentation;
                        if (doc.Markup.GetLength(0) == 0)
                        {
                            return "";
                        }
                        else
                        {
                            return doc.Markup[0].InnerText;
                        }
                    }
                }
            }

            _log.Info("no documentation was found in the xml Schema.");
            return null;
        }

        public static XmlSchemaElement tryGetElement(XmlSchema schema)
        {
            foreach (XmlSchemaObject o in schema.Items)
            {
                if (o is XmlSchemaElement)
                {
                    return o as XmlSchemaElement;
                }
            }
            return null;
        }

        public static XmlSchema getXmlSchemaFromXml(string xmlpath)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                // TODO: save to local filesystem at start
                doc.Load(xmlpath);

                return getXmlSchemaFromXml(doc);
            }
            catch
            {
                return null;
            }
        }

        public static XmlSchema getXmlSchemaFromXml(XmlDocument doc)
        {
            try
            {
                // get schema file

                if (doc.DocumentElement.GetAttribute("noNamespaceSchemaLocation", xsiNamespace) == "")
                {
                    _log.Error("this xml does not contain a reference to a schema. Please add the attribute 'noNamespaceSchemaLocation'.");
                    return null;
                }
                string path = Environment.CurrentDirectory + @"\Resources\DocUI\" + doc.DocumentElement.GetAttribute("noNamespaceSchemaLocation", xsiNamespace);

                // compile schema
                XmlSchemaSet schemaSet = new XmlSchemaSet();
                XmlTextReader reader = new XmlTextReader(path);
                XmlSchema schema = XmlSchema.Read(reader, null);
                schemaSet.Add(schema);
                schemaSet.Compile();

                return schema;
            }
            catch
            {
                return null;
            }
        }


    }
}

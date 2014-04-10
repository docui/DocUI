using Common.Logging;
using Org.Filedrops.FileSystem;
using System;
using System.Xml;
using System.Xml.Schema;

namespace Org.DocUI.Tools
{
    class XmlValidator
    {
        private static string path = "";
        private static bool succes;
        private static readonly ILog Log;

        static XmlValidator()
        {
            Log = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// This method parses an xml file, and will return any errors if they are found.
        /// the schemafile will be found in the directory: "currentdirectory\xml_docuemtns\xmls\"
        /// followed by the value of the xsi:noNamespaceSchemaLocation attribute.
        /// </summary>
        /// <param name="f">The xml file</param>
        /// <returns>whether the file is Valid.</returns>
        public static Boolean Validate(FiledropsFile f, XmlSchema schema)
        {
            if (f == null || schema == null) { Log.Info("no schema/file was given"); return false; }

            succes = true;
            XmlDocument document = new XmlDocument();
            try
            {
                path = f.FullName;
                document.Load(f.FullName);

                return Validate(document, schema);
            }
            catch (Exception e)
            {
                Log.Info("Problem with xml file load. Perhaps the file isn't an xml document?");
                Log.Info(e.Message);
                return false;
            }
        }

        public static bool Validate(XmlDocument document, XmlSchema schema)
        {
            succes = true;
            document.Schemas.Add(schema);
            document.Validate(new ValidationEventHandler(ValidationCallBack));
            return succes;
        }

        /// <summary>
        /// If any error is found in the xml file, this method will be called.
        /// The callback will Log the errors.
        /// There will be a different message for errors and warnings. Only errors will cause an invalid file.
        /// </summary>
        /// <param name="sender">isn't used.</param>
        /// <param name="args">isn't used.</param>
        private static void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
            {
                Log.Debug("Validation warning found on the file: " + path + "\n" + args.Message);
            }
            else
            {
                Log.Debug("Error in file: " + path + " on line: " + args.Exception.LineNumber + "\n" + args.Message);
                succes = false;
            }
        }
    }
}

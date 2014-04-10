using System;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Schema;

namespace Org.DocUI.FormBuilder.Components
{
    public class DocUIFolder : DocUIFile
    {

        public DocUIFolder(XmlNode xmlNode, XmlSchemaAnnotated xsdNode, Panel contentpanel, Panel overlaypanel, DynamicProjectForm parentForm)
            : base(xmlNode, xsdNode, contentpanel, overlaypanel, parentForm)
        {
        }

        /// <summary>
        /// opens a folder dialog. Once a folder is selected, it's path will be entered in the textbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected override void fileChooser(object sender, EventArgs args)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Value = dialog.SelectedPath;
            }
        }
    }
}

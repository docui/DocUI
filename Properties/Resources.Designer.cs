﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34011
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Org.DocUI.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Org.DocUI.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;Components&gt;
        ///  &lt;Component schematype=&quot;boolean&quot; type=&quot;Org.DocUI.docui.components.BooleanDocUIComponent&quot;/&gt;
        ///  &lt;Component schematype=&quot;int&quot; type=&quot;Org.DocUI.docui.components.IntegerDocUIComponent&quot;/&gt;
        ///  &lt;Component schematype=&quot;string&quot; type=&quot;Org.DocUI.docui.components.StringDocUIComponent&quot;/&gt;
        ///  &lt;Component schematype=&quot;time&quot; type=&quot;Org.DocUI.docui.components.TimeDocUIComponent&quot;/&gt;
        ///  &lt;Component schematype=&quot;date&quot; type=&quot;Org.DocUI.docui.components.DateDocUIComponent&quot;/&gt;
        ///  &lt;Component schematype=&quot;dateTime&quot; type=&quot;Org.DocUI. [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Components {
            get {
                return ResourceManager.GetString("Components", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;UTF-8&quot;?&gt;
        ///&lt;!-- edited with XMLSpy v2007 rel. 3 sp1 (http://www.altova.com) by Lieven Janssen (TIBOTEC-VIRCO CVA) --&gt;
        ///&lt;xs:schema xmlns:xs=&quot;http://www.w3.org/2001/XMLSchema&quot; xmlns:uis=&quot;uispecificattributes&quot; elementFormDefault=&quot;qualified&quot; attributeFormDefault=&quot;unqualified&quot;&gt;
        ///	&lt;xs:simpleType name=&quot;GUIDType&quot;&gt;
        ///		&lt;xs:restriction base=&quot;xs:string&quot;/&gt;
        ///	&lt;/xs:simpleType&gt;
        ///	&lt;xs:complexType name=&quot;CredentialsType&quot;&gt;
        ///		&lt;xs:sequence&gt;
        ///			&lt;xs:element name=&quot;Username&quot; type=&quot;xs:string&quot; default=&quot;&quot; [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string ui {
            get {
                return ResourceManager.GetString("ui", resourceCulture);
            }
        }
    }
}

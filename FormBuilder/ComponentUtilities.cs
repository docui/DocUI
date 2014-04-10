using Org.DocUI.FormBuilder.Components;
using System;
using System.Reflection;

namespace Org.DocUI.FormBuilder
{
    /// <summary>
    /// Contains handy shortcuts/methods for DocUI Components
    /// </summary>
    public class ComponentUtilities
    {
        public static AbstractDocUIComponent TryGetComponent(string component, Object[] cparams)
        {
            try
            {
                Type t = Assembly.GetExecutingAssembly().GetType(component);
                if (t == null)//check entry Assembly for extra Components if not found in default set of Components
                {
                    t = Assembly.GetEntryAssembly().GetType(component);
                }
                if (t == null)
                {
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        t = assembly.GetType(component);
                        if (t != null)
                        {
                            break;
                        }
                    }
                }
                AbstractDocUIComponent comp = (AbstractDocUIComponent)Activator.CreateInstance(t, cparams);
                return comp;
            }
            catch { return null; }
        }
    }
}

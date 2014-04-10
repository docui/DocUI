using System.Reflection;
using System.Windows.Media.Imaging;

namespace Org.DocUI.Tools
{
    public static class EmbeddedResourceTools
    {
        public static BitmapImage GetImage(string resource, int size = -1, Assembly assembly = null)
        {
            BitmapImage bitmap;
            if (assembly == null)
            {
                assembly = Assembly.GetExecutingAssembly();
            }

            using (var stream = assembly.GetManifestResourceStream(resource))
            {
                if (stream != null)
                {
                    bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    if (size != -1)
                    {
                        bitmap.DecodePixelHeight = size;
                        bitmap.DecodePixelWidth = size;
                    }
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }

            return null;

        }

    }
}

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace Sample_Crunch
{
    public class ResourceHelper
    {
        // ******************************************************************
        /// <summary>
        /// Load a resource WPF-BitmapImage (png, bmp, ...) from embedded resource defined as 'Resource' not as 'Embedded resource'.
        /// </summary>
        /// <param name="pathInApplication">Path without starting slash</param>
        /// <param name="assembly">Usually 'Assembly.GetExecutingAssembly()'. If not mentionned, I will use the calling assembly</param>
        /// <returns></returns>
        public static BitmapImage LoadBitmapFromResource(string pathInApplication, Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = Assembly.GetCallingAssembly();
            }

            return new BitmapImage(ResourceHelper.GetLocationUri(pathInApplication, assembly));
        }

        // ******************************************************************
        /// <summary>
        /// The resource should be defined as 'Resource' not as 'Embedded resource'.
        /// </summary>
        /// <param name="pathWithoutLeadingSlash">The path start with folder name (if any) then '/', then ...</param>
        /// <param name="assembly">If null, then use calling assembly to find the resource</param>
        /// <returns></returns>
        public static Uri GetLocationUri(string pathWithoutLeadingSlash, Assembly assembly = null)
        {
            if (pathWithoutLeadingSlash[0] == '/')
            {
                pathWithoutLeadingSlash = pathWithoutLeadingSlash.Substring(1);
            }

            if (assembly == null)
            {
                assembly = Assembly.GetCallingAssembly();
            }

            return new Uri(@"pack://application:,,,/" + assembly.GetName().Name + ";component/" + pathWithoutLeadingSlash, UriKind.Absolute);
        }

        // ******************************************************************
        /// <summary>
        /// The resource should be defined as 'Resource' not as 'Embedded resource'.
        /// Example:            
        ///     StreamResourceInfo info = ResourceHelper.GetResourceStreamInfo(@"Resources/GraphicUserGuide.html");
        ///     if (info != null)
        ///     {
        ///         WebBrowser.NavigateToStream(info.Stream);
        ///     }
        /// </summary>
        /// <param name="path">The path start with folder name (if any) then '/', then ...</param>
        /// <param name="assembly">If null, then use calling assembly to find the resource</param>
        /// <returns></returns>
        public static StreamResourceInfo GetResourceStreamInfo(string path, Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = Assembly.GetCallingAssembly();
            }

            return Application.GetResourceStream(ResourceHelper.GetLocationUri(path, assembly));
        }

        // ******************************************************************


        public static ImageSource SetIconFromBitmap(System.Drawing.Bitmap bitmap)
        {
            return (ImageSource)Imaging.CreateBitmapSourceFromHBitmap(
               bitmap.GetHbitmap(),
               IntPtr.Zero,
               Int32Rect.Empty,
               BitmapSizeOptions.FromEmptyOptions());

        }

    }
}


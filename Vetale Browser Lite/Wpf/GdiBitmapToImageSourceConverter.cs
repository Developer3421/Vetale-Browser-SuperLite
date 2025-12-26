using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Vetale_Browser_Lite.Wpf
{
    public sealed class GdiBitmapToImageSourceConverter : IValueConverter
    {
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (value is ImageSource img)
                return img;

            if (value is Bitmap bitmap)
            {
                IntPtr hBitmap = bitmap.GetHbitmap();
                try
                {
                    var source = Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap,
                        IntPtr.Zero,
                        System.Windows.Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());

                    source.Freeze();
                    return source;
                }
                finally
                {
                    DeleteObject(hBitmap);
                }
            }

            if (value is Uri uri)
                return new BitmapImage(uri);

            if (value is string s && !string.IsNullOrWhiteSpace(s))
            {
                if (Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out var u))
                    return new BitmapImage(u);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

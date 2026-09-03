using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Vetale_Browser_Lite.Wpf
{
    public sealed class BytesToImageSourceConverter : IValueConverter
    {
        // Display size is 20px: decode at exactly 2x so the 2:1 downscale
        // stays crisp (integer-ratio resampling, sharp on 100% and 200% DPI).
        private const int DecodeWidth = 40;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not byte[] bytes || bytes.Length == 0)
                return null;
            try
            {
                // ICO containers hold several frames (16/32/48px...). WPF takes
                // the first one (usually the tiny 16px) and stretches it —
                // that was the "soap". Extract the largest frame ourselves.
                bytes = ExtractLargestIcoFrame(bytes);
                using var ms = new MemoryStream(bytes);
                var img = new BitmapImage();
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.StreamSource = ms;
                img.DecodePixelWidth = DecodeWidth;
                img.EndInit();
                img.Freeze();
                return img;
            }
            catch { return null; }
        }

        private static byte[] ExtractLargestIcoFrame(byte[] ico)
        {
            // ICO header: reserved(2)=0, type(2)=1, count(2)
            if (ico.Length < 6 || ico[0] != 0 || ico[1] != 0 || ico[2] != 1 || ico[3] != 0)
                return ico;
            int count = ico[4] | (ico[5] << 8);
            if (count <= 0 || count > 64 || ico.Length < 6 + count * 16)
                return ico;

            int bestSize = -1, bestOffset = 0, bestBytes = 0;
            for (int i = 0; i < count; i++)
            {
                int o = 6 + i * 16;
                int w = ico[o] == 0 ? 256 : ico[o];
                int h = ico[o + 1] == 0 ? 256 : ico[o + 1];
                int sizeInBytes = BitConverter.ToInt32(ico, o + 8);
                int offset = BitConverter.ToInt32(ico, o + 12);
                int side = Math.Min(w, h);
                if (side > bestSize && sizeInBytes > 0 && offset > 0 &&
                    (long)offset + sizeInBytes <= ico.Length)
                {
                    bestSize = side;
                    bestOffset = offset;
                    bestBytes = sizeInBytes;
                }
            }
            if (bestSize < 0)
                return ico;

            var frame = new byte[bestBytes];
            Buffer.BlockCopy(ico, bestOffset, frame, 0, bestBytes);
            return frame; // usually an embedded PNG/BMP — decodable standalone
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

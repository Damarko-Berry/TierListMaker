using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;


namespace TierListMaker
{
    /// <summary>
    /// Interaction logic for TierItem.xaml
    /// </summary>
    public partial class TierItem : UserControl
    {
        public static double Size = 50;
        public TierItem()
        {
            InitializeComponent();
        }
        public TierItem(string path): this()
        {
            BitmapImage bitmap = new BitmapImage(new Uri(path));
            image.Source = BitmapToImageSource(MakeSquare(bitmap));
            SetSize(Size, Size);
        }
        public TierItem(Bitmap map): this()
        {
            image.Source = BitmapToImageSource(map);
            SetSize(Size, Size);
        }
        public void SetSize(double width, double height)
        {
            
            image.Width = width;
            image.Height = height;
        }
        private Bitmap MakeSquare(BitmapImage bitmapImage)
        {
            int size = Math.Min(bitmapImage.PixelWidth, bitmapImage.PixelHeight);
            Bitmap squareBitmap = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(squareBitmap))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                    encoder.Save(memoryStream);
                    using (Bitmap bitmap = new Bitmap(memoryStream))
                    {
                        g.DrawImage(bitmap, new Rectangle(0, 0, size, size), new Rectangle(0, 0, size, size), GraphicsUnit.Pixel);
                    }
                }
            }
            return squareBitmap;
        }
        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Png);
                memoryStream.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
    }
}

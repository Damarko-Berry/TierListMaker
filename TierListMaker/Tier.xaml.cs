using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
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
using System.Windows.Shapes;

namespace TierListMaker
{
    /// <summary>
    /// Interaction logic for Tier.xaml
    /// </summary>
    public partial class Tier : UserControl
    {
        public Bitmap[] getitems
        {
            get
            {
                Bitmap[] result = new Bitmap[TierPanel.Children.Count];
                for (int i = 0; i < result.Length; i++)
                {
                    var Child = (TierItem)TierPanel.Children[i];
                    result[i] = ConvertImageSourceToBitmap((BitmapImage)Child.image.Source);
                }
                return result;
            }
        }
        public static Bitmap ConvertImageSourceToBitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);
                return new Bitmap(bitmap);

            }
        }
        public Tier()
        {
            InitializeComponent();
        }
        public Tier(TierData data): this()
        {
            Title = data.Title;
            for (int i = 0; i < data.bitmaps.Length; i++)
            {
                TierPanel.Children.Add(new TierItem(data.GetBitmaps()[i]));
            }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(Tier), new PropertyMetadata());
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        private void Tier_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("TierItem"))
            {
                TierItem droppedItem = e.Data.GetData("TierItem") as TierItem;
                if (droppedItem != null)
                {
                    Tier destinationTier = this;
                    if (destinationTier != null)
                    {
                        // Remove dropped item from its original parent
                        if (droppedItem.Parent is Panel originalParent)
                        {
                            originalParent.Children.Remove(droppedItem);
                        }

                        // Add dropped item to the destination tier
                        destinationTier.TierPanel.Children.Add(droppedItem);
                    }
                }
            }
        }

        private void TierPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is TierItem item)
            {
                DragDrop.DoDragDrop(item, new DataObject("TierItem", item), DragDropEffects.Move);
                //ItemList.Children.Remove(item);
            }
        }


        public TierData GetData()
        {
            return new()
            {
                Title = this.Title,
                bitmaps = getitems.Select(TierData.BitmapToBase64).ToArray(),
            };
        }

        private void additem_Click(object sender, RoutedEventArgs e)
        {
            var view = MainWindow.Instance.view.Children;
            for (int i = 0; i < view.Count; i++)
            {
                if (view[i] == this)
                {
                    MainWindow.Instance.Insert_tier(new Tier(), i + 1);
                    return;
                }
            }
            MainWindow.Instance.AddTier_Click(sender,e);
        }

        private void removeitem_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.view.Children.Remove(this);
            MainWindow.Instance.CorrectColors();
        }
    }
}

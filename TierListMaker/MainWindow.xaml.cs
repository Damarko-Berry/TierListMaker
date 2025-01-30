using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;
using System.Text.Json;
using Microsoft.Win32;
using Brushes = System.Windows.Media.Brushes;
using Brush = System.Windows.Media.Brush;


namespace TierListMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance = null;
        Progress progress => Progress.FromTiers(GetAllTiers(), getitems);

        Tier[] GetAllTiers()
        {
            List<Tier> result = new();
            var v = view.Children;
            foreach (var item in v)
            {
                if (item is Tier)
                    result.Add((Tier)item);
            }

            return [.. result];
        }
        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            LoadToList(Progress.Default);
        }
        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    if (IsImageFile(file))
                    {
                        TierItem tierItem = new(file);
                        ItemList.Children.Add(tierItem);
                    }
                }
            }
        }

        private bool IsImageFile(string file)
        {
            string extension = Path.GetExtension(file).ToLower();
            return extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".bmp";
        }

        private void ItemList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is TierItem item)
            {
                DragDrop.DoDragDrop(item, new DataObject("TierItem", item), DragDropEffects.Move);
                
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            
            Directory.CreateDirectory("Data");
            File.WriteAllText(Path.Combine("Data",$"{MakeFileNameSafe(Uniname.Text)}.json"), progress.ToJson());
        }

        public Bitmap[] getitems
        {
            get
            {
                Bitmap[] result = new Bitmap[ItemList.Children.Count];
                for (int i = 0; i < result.Length; i++)
                {
                    var Child = (TierItem)ItemList.Children[i];
                    result[i] = ConvertImageSourceToBitmap((BitmapImage)Child.image.Source);
                }
                return result;
            }
        }
        private Bitmap ConvertImageSourceToBitmap(BitmapImage bitmapImage)
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

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            
            ofd.Filter="textfiles|*.json";
            ofd.Multiselect = false;
            if (!ofd.ShowDialog().Value) return;
            clear();
            try
            {
                var jsonString = File.ReadAllText(ofd.FileName);
                Progress deserializedProgress = JsonSerializer.Deserialize<Progress>(jsonString);
                LoadToList(deserializedProgress);
                Uniname.Text = new FileInfo(ofd.FileName).Name.Replace(".json", string.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void clear()
        {
            var v = view.Children;
            for (int i = 0; i < v.Count; i++)
            {
                if (v[i] is Tier)
                {
                    view.Children.Remove(v[i]);
                    i--;
                }
            }
        }
        void LoadToList(Progress progress)
        {
            foreach (var tier in progress.Tiers)
            {
                view.Children.Add(new Tier(tier));
            }
            foreach (var item in progress.NotDecided)
            {
                ItemList.Children.Add(new TierItem(item));
            }
            CorrectColors();
        }
        private void Screenschot_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory("Lists");
            var SSpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),"Tier List Maker", $"{Uniname.Text}.png");
            SaveScreenshot(view, Path.Combine("Lists",$"{Uniname.Text}.png"));
        }
        private void SaveScreenshot(FrameworkElement element, string filename)
        {
            var width = (int)element.ActualWidth;
            var height = (int)element.ActualHeight;

            var renderTarget = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            var visual = new DrawingVisual();

            using (var context = visual.RenderOpen())
            {
                var brush = new VisualBrush(element);
                context.DrawRectangle(brush, null, new Rect(new System.Windows.Point(0, 0), new System.Windows.Size(width, height)));
            }

            renderTarget.Render(visual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTarget));

            using (var stream = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                encoder.Save(stream);
            }

            MessageBox.Show("Screenshot saved to " + MakeFileNameSafe(filename));
        }

        private void Size_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int size = (2+Size.SelectedIndex)*25;
            TierItem.Size = size;
            List<TierItem> tierItems = new List<TierItem>();
            var v = GetAllTiers();
            foreach (var item in v)
            {
                foreach (var child in item.TierPanel.Children)
                {
                    if (child is TierItem )
                    {
                        tierItems.Add((TierItem)child);
                    }
                }
            }
            var ti = ItemList.Children;
            foreach (var item in ti)
            {
                if (item is TierItem)
                {
                    tierItems.Add((TierItem)item);
                }
            }
            foreach (var item in tierItems)
            {
                item.SetSize(size, size);
            }

        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "textfiles|*.json";
            ofd.Multiselect = false;
            if (!ofd.ShowDialog().Value) return;
            try
            {
                var GG = GetAllTiers();
                var jsonString = File.ReadAllText(ofd.FileName);
                Progress deserializedProgress = JsonSerializer.Deserialize<Progress>(jsonString);
                for (int i = 0; i < deserializedProgress.Tiers.Length; i++)
                {
                    bool found = false;
                    for (int j = 0; j < GG.Length; j++)
                    {
                        if (GG[j].GetData().Title == deserializedProgress.Tiers[i].Title)
                        {
                            found = true;
                            GG[j].Add(deserializedProgress.Tiers[i].GetBitmaps());  
                        }
                    }
                    if(!found)
                    {
                        var nT = new Tier(deserializedProgress.Tiers[i]);
                        view.Children.Add(nT);
                    }
                }
                foreach (var item in progress.NotDecided)
                {
                    ItemList.Children.Add(new TierItem(item));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void Insert_tier(Tier tier, int index)
        {
            view.Children.Insert(index, tier);
            CorrectColors();
        }
        public void CorrectColors()
        {
            var childs = GetAllTiers();
            for (int i = 0; i < childs.Length; i++)
            {
                childs[i].Background =  brushes[i % brushes.Length]; 
            }
        }
        public void AddTier_Click(object sender, RoutedEventArgs e)
        {
            var nT = new Tier();
            view.Children.Add(nT);
            CorrectColors();
        }
        public static Brush[] brushes =
 [
     Brushes.Red,
    Brushes.Orange,
    Brushes.Gold,
    Brushes.Yellow,
    Brushes.Lime,
    Brushes.Green,
    Brushes.Cyan,
    Brushes.Aqua,
    Brushes.Blue,
    Brushes.RoyalBlue,
    Brushes.DeepSkyBlue,
    Brushes.CornflowerBlue,
    Brushes.MediumSlateBlue,
    Brushes.MediumOrchid,
    Brushes.Purple,
    Brushes.Magenta,
    Brushes.Fuchsia,
    Brushes.DeepPink,
    Brushes.Pink,
    Brushes.LightPink,
    Brushes.HotPink,
    Brushes.LightCoral,
    Brushes.Salmon,
    Brushes.Coral,
    Brushes.Tomato,
    Brushes.OrangeRed,
    Brushes.LemonChiffon,
    Brushes.PapayaWhip,
    Brushes.Moccasin,
    Brushes.NavajoWhite,
    Brushes.PeachPuff,
    Brushes.Bisque,
    Brushes.BlanchedAlmond,
    Brushes.AntiqueWhite,
    Brushes.Azure,
    Brushes.MintCream,
    Brushes.Honeydew,
    Brushes.LightYellow,
    Brushes.LightGoldenrodYellow,
    Brushes.FloralWhite,
    Brushes.Ivory,
    Brushes.LavenderBlush,
    Brushes.White,
    Brushes.WhiteSmoke,
    Brushes.GhostWhite,
    Brushes.Snow
 ];

        private string MakeFileNameSafe(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }
    }
}
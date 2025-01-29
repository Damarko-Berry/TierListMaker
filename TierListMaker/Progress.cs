using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace TierListMaker
{
    public class Progress
    {
        public TierData[] Tiers { get; set; } = new TierData[0];

        public string[] NotDecided { get; set; } = new string[] { };

        public Progress() { }

        public static string BitmapToBase64(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public static Bitmap Base64ToBitmap(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                return new Bitmap(ms);
            }
        }

        public static Progress FromTiers(Tier[] tiers, Bitmap[] notDecided)
        {
            Progress progress = new Progress
            {
                NotDecided = notDecided.Select(BitmapToBase64).ToArray()
            };
            progress.Tiers = new TierData[tiers.Length];
            for (int i = 0; i < progress.Tiers.Length; i++)
            {
                progress.Tiers[i] = tiers[i].GetData();
                
            }
            return progress;
        }

        public Bitmap[] GetNotDecidedBitmaps() => NotDecided.Select(Base64ToBitmap).ToArray();

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public static Progress FromJson(string json)
        {
            return JsonSerializer.Deserialize<Progress>(json);
        }
        public static Progress Default=>
            new Progress()
            {
                Tiers = new TierData[5]
                {
                    new TierData("S", new Bitmap[0]),
                    new TierData("A", new Bitmap[0]),
                    new TierData("B", new Bitmap[0]),
                    new TierData("C", new Bitmap[0]),
                    new TierData("D", new Bitmap[0])
                },
                NotDecided = new string[0]
            };
    }
    public struct TierData
    {
        public string Title { get; set; }
        public string[] bitmaps { get; set; }
        public Bitmap[] GetBitmaps() => bitmaps.Select(Base64ToBitmap).ToArray();

        public TierData(string title, Bitmap[] bitmaps)
        {
            Title = title;
            this.bitmaps = bitmaps.Select(BitmapToBase64).ToArray();
        }

        public static string BitmapToBase64(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return Convert.ToBase64String(ms.ToArray());
            }
        }
        public static Bitmap Base64ToBitmap(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                return new Bitmap(ms);
            }
        }
    }
}

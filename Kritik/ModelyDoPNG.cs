using OxyPlot;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Kritik
{
    class ModelyDoPNG
    {
        public int Sirka { get; set; }
        public int Resolution { get { return Convert.ToInt32(Math.Round(Sirka / 884.0 * 96)); } }
        public int Okraj { get; set; }
        private List<Image> Obrazky { get; set; }
        public ModelyDoPNG()
        {
            Okraj = 25;
            Sirka = 1920 - 2 * Okraj;            
            Obrazky = new List<Image>();
        }
        public void Pridat(PlotModel model, int vyska)
        {
            PngExporter pngExporter = new PngExporter { Width = Sirka, Height = vyska, Resolution = Resolution };
            MemoryStream stream = new MemoryStream();
            pngExporter.Export(model, stream);
            Image img = Image.FromStream(stream);
            Obrazky.Add(img);
        }
        public void Ulozit(string soubor)
        {
            int celkovaVyska = 2 * Okraj;
            foreach (Image img in Obrazky)
            {
                celkovaVyska += img.Height;
            }
            Bitmap bitmap = new Bitmap(Sirka + 2 * Okraj, celkovaVyska);
            bitmap.SetResolution(Resolution, Resolution);
            Graphics g = Graphics.FromImage(bitmap);
            g.Clear(Color.White);
            int y = Okraj;
            foreach (Image img in Obrazky)
            {
                g.DrawImage(img, new Point(Okraj, y));
                y += img.Height;
            }
            bitmap.Save(soubor, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}

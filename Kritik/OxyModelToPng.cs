using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OxyPlot;
using OxyPlot.Wpf;
using System.IO;

namespace Kritik
{
    class OxyModelToPng
    {
        private readonly int width, padding, resolution;
        private int totalHeight;
        private List<Image> images = new List<Image>();
        
        public OxyModelToPng(int resolution, int imageWidth, int padding)
        {
            this.width = imageWidth;
            this.padding = padding;
            this.resolution = resolution;
            this.totalHeight = 2 * padding;
        }

        public void AddModel(PlotModel plotModel, int height)
        {
            totalHeight += height;
            PngExporter pngExporter = new PngExporter()
            {
                Width = width - 2 * padding,
                Height = height,
                Resolution = resolution
            };
            MemoryStream memoryStream = new MemoryStream();
            pngExporter.Export(plotModel, memoryStream);
            Image image = Image.FromStream(memoryStream);
            images.Add(image);
        }

        public void SaveToFile(string fileName)
        {
            Bitmap bitmap = new Bitmap(width, totalHeight);
            bitmap.SetResolution(resolution, resolution);
            Graphics g = Graphics.FromImage(bitmap);
            g.Clear(Color.White);
            int y = padding;
            foreach (Image image in images)
            {
                g.DrawImage(image, new Point(padding, y));
                y += image.Height;
            }
            bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}

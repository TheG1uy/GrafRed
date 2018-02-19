using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;

namespace PhotoRed
{
   abstract class Filters
    {
       public int Clamp(int value, int max, int min)
       {
           if (value < min) return min;
           if (value > max) return max;
           return value;
       }
       protected abstract Color calculateNewPixelColor(Bitmap im, int x, int y);
       public Bitmap processImage(Bitmap im,BackgroundWorker worker)
       {
           Bitmap resIm = new Bitmap(im.Width, im.Height);
           for (int i = 0; i < resIm.Width; i++)
           {
               worker.ReportProgress((int)((float)i / im.Width * 100));
               if (worker.CancellationPending) return null;
               for (int j = 0; j < resIm.Height; j++)
                   resIm.SetPixel(i, j, calculateNewPixelColor(im, i, j));
           }
           return resIm;
       }
    }
   class InvertFilter :  Filters 
   {
       protected override Color calculateNewPixelColor(Bitmap im, int x, int y)
       {
           Color sColor = im.GetPixel(x, y);
           Color rColor = Color.FromArgb(255-sColor.R,255-sColor.G,255-sColor.B);
           return rColor;
       }
   }
}

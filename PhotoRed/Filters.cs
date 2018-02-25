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
       public int Clamp(int value, int min, int max)
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
    class GrayScaleFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap im, int x, int y)
        {
            float col = im.GetPixel(x, y).R * 0.36f + im.GetPixel(x, y).G * 0.53f + im.GetPixel(x, y).B * 0.11f;
            return Color.FromArgb(Clamp((int)col,0,255), Clamp((int)col, 0, 255), Clamp((int)col, 0, 255));
        }
    }
    class SepiaFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap im, int x, int y)
        {
            int k=20;
            float col = im.GetPixel(x, y).R * 0.36f + im.GetPixel(x, y).G * 0.53f + im.GetPixel(x, y).B * 0.11f;
            return Color.FromArgb(Clamp((int)col+2*k, 0, 255), Clamp((int)(col+0.5f*k), 0, 255), Clamp((int)col-k, 0, 255));
        }
    }
    class BrightnessFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap im, int x, int y)
        {
            int k = 40;
            return Color.FromArgb(Clamp((int)(im.GetPixel(x, y).R + k), 0, 255), Clamp((int)(im.GetPixel(x, y).G + k), 0, 255), Clamp((int)(im.GetPixel(x, y).B + k), 0, 255));
        }
    }
 class MatrixFilter : Filters
    {
     protected float[,] kernel = null;
     protected MatrixFilter(){}
     public MatrixFilter(float[,] kernel)
     {
         this.kernel = kernel;
     }
     protected override Color calculateNewPixelColor(Bitmap im, int x, int y)
     {
         int radiusX = kernel.GetLength(0) / 2;
         int radiusY = kernel.GetLength(1) / 2;
         float resultR = 0;
         float resultG = 0;
         float resultB = 0;
         for(int l=-radiusY;l<=radiusY;l++)
             for (int k = -radiusX; k <= radiusX; k++)
             {
                 int idX = Clamp(x + k, 0, im.Width - 1);
                 int idY = Clamp(y + l, 0, im.Height - 1);
                 Color neighborColor = im.GetPixel(idX, idY);
                 resultR+=neighborColor.R*kernel[k+radiusX,l+radiusY];
                 resultG+=neighborColor.G*kernel[k+radiusX,l+radiusY];
                 resultB+=neighborColor.B*kernel[k+radiusX,l+radiusY];
             }
         return Color.FromArgb(Clamp((int)resultR, 0, 255), Clamp((int)resultG, 0, 255), Clamp((int)resultB, 0, 255));
     }

    }
 class BlurFilter : MatrixFilter
 {
     public BlurFilter()
     {
         int sizeX = 3;
         int sizeY = 3;
         kernel = new float[sizeX, sizeY];
         for (int i = 0; i < sizeX; i++)
             for (int j = 0; j < sizeY; j++)
                 kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
     }
 }
 class GaussianFilter : MatrixFilter 
 {
     public void createGaussianKernel(int radius,float sigma)
     {
         int size = radius * 2 + 1;
         kernel = new float[size, size];
         float norm = 0;
         for(int i=-radius;i<radius;i++)
             for (int j = -radius; j < radius; j++)
             { 
                 kernel[i+radius,j+radius]=(float)(Math.Exp(-(i*i+j*j)/(sigma*sigma)));
                 norm+= kernel[i+radius,j+radius];
             }
         for (int i = 0; i < size; i++)
             for (int j = 0; j < size; j++)
                 kernel[i, j] /=norm;

     }
     public GaussianFilter()
     {
         createGaussianKernel(3, 2);
     }
 }
 class SobelFilter : MatrixFilter
 {
     public SobelFilter() { }
     protected override Color calculateNewPixelColor(Bitmap im, int x, int y)
     {
         float resultR1 = 0;
         float resultG1 = 0;
         float resultB1 = 0;
         float resultR2 = 0;
         float resultG2 = 0;
         float resultB2 = 0;
         kernel = new float[3, 3]{
             {-1,-2,-1},
             {0,0,0},
             {1,2,1}
        };
         int radiusX = kernel.GetLength(0) / 2;
         int radiusY = kernel.GetLength(1) / 2;
         for (int l = -radiusY; l <= radiusY; l++)
             for (int k = -radiusX; k <= radiusX; k++)
             {
                 int idX = Clamp(x + k, 0, im.Width - 1);
                 int idY = Clamp(y + l, 0, im.Height - 1);
                 Color neighborColor = im.GetPixel(idX, idY);
                 resultR1 += neighborColor.R * kernel[k + radiusX, l + radiusY];
                 resultG1 += neighborColor.G * kernel[k + radiusX, l + radiusY];
                 resultB1 += neighborColor.B * kernel[k + radiusX, l + radiusY];
             }
         kernel = new float[3, 3]{
             {-1,0,1},
             {-2,0,2},
             {-1,0,1}
        };
         for (int l = -radiusY; l <= radiusY; l++)
             for (int k = -radiusX; k <= radiusX; k++)
             {
                 int idX = Clamp(x + k, 0, im.Width - 1);
                 int idY = Clamp(y + l, 0, im.Height - 1);
                 Color neighborColor = im.GetPixel(idX, idY);
                 resultR2 += neighborColor.R * kernel[k + radiusX, l + radiusY];
                 resultG2 += neighborColor.G * kernel[k + radiusX, l + radiusY];
                 resultB2 += neighborColor.B * kernel[k + radiusX, l + radiusY];
             }
         int sum = (int)Math.Sqrt(resultR1 * resultR1 + resultR2 * resultR2 + resultG1 * resultG1 + resultG2 * resultG2+resultB1 * resultB1 + resultB2 * resultB2)/3;
         return Color.FromArgb(Clamp((int)sum, 0, 255), Clamp((int)sum, 0, 255), Clamp((int)sum, 0, 255));
     }
 }
    class HarshnessFilter : MatrixFilter
    {
        public HarshnessFilter()
        {
            kernel = new float[3, 3]{
             {0,-1,0},
             {-1,5,-1},
             {0,-1,0}
        };
        }
    }
    class ShaarraFilter : MatrixFilter
    {
        public ShaarraFilter() { }
        protected override Color calculateNewPixelColor(Bitmap im, int x, int y)
        {
            float resultR1 = 0;
            float resultG1 = 0;
            float resultB1 = 0;
            float resultR2 = 0;
            float resultG2 = 0;
            float resultB2 = 0;
            kernel = new float[3, 3]{
             {3,10,3},
             {0,0,0},
             {-3,-10,-3}
        };
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, im.Width - 1);
                    int idY = Clamp(y + l, 0, im.Height - 1);
                    Color neighborColor = im.GetPixel(idX, idY);
                    resultR1 += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG1 += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB1 += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            kernel = new float[3, 3]{
             {3,0,-3},
             {10,0,-10},
             {3,0,-3}
        };
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, im.Width - 1);
                    int idY = Clamp(y + l, 0, im.Height - 1);
                    Color neighborColor = im.GetPixel(idX, idY);
                    resultR2 += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG2 += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB2 += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            int sum = (int)Math.Sqrt(resultR1 * resultR1 + resultR2 * resultR2 + resultG1 * resultG1 + resultG2 * resultG2 + resultB1 * resultB1 + resultB2 * resultB2) / 3;
            return Color.FromArgb(Clamp((int)sum, 0, 255), Clamp((int)sum, 0, 255), Clamp((int)sum, 0, 255));
        }
    }
    class PruittFilter:MatrixFilter
    {
        public PruittFilter() { }
        protected override Color calculateNewPixelColor(Bitmap im, int x, int y)
        {
            float resultR1 = 0;
            float resultG1 = 0;
            float resultB1 = 0;
            float resultR2 = 0;
            float resultG2 = 0;
            float resultB2 = 0;
            kernel = new float[3, 3]{
             {-1,-1,-1},
             {0,0,0},
             {1,1,1}
        };
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, im.Width - 1);
                    int idY = Clamp(y + l, 0, im.Height - 1);
                    Color neighborColor = im.GetPixel(idX, idY);
                    resultR1 += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG1 += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB1 += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            kernel = new float[3, 3]{
             {-1,0,1},
             {-1,0,1},
             {-1,0,1}
        };
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, im.Width - 1);
                    int idY = Clamp(y + l, 0, im.Height - 1);
                    Color neighborColor = im.GetPixel(idX, idY);
                    resultR2 += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG2 += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB2 += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            int sum = (int)Math.Sqrt(resultR1 * resultR1 + resultR2 * resultR2 + resultG1 * resultG1 + resultG2 * resultG2 + resultB1 * resultB1 + resultB2 * resultB2) / 3;
            return Color.FromArgb(Clamp((int)sum, 0, 255), Clamp((int)sum, 0, 255), Clamp((int)sum, 0, 255));
        }
    }
    class EmbossingFilter : MatrixFilter
    {
        public EmbossingFilter()
        {
            kernel = new float[3, 3]{
             {0,1,0},
             {1,0,-1},
             {0,-1,0}
        };
        }

        protected override Color calculateNewPixelColor(Bitmap im, int x, int y)
        {
            float resultR1 = 0;
            float resultG1 = 0;
            float resultB1 = 0;
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, im.Width - 1);
                    int idY = Clamp(y + l, 0, im.Height - 1);
                    Color neighborColor = im.GetPixel(idX, idY);
                    resultR1 += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG1 += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB1 += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }

         return Color.FromArgb(
            Clamp((int)((resultR1 + 128)*0.36f+ (resultG1 + 128) * 0.53f+ (resultB1 + 128) * 0.11f), 0, 255),
            Clamp((int)((resultR1 + 128) * 0.36f + (resultG1 + 128) * 0.53f + (resultB1 + 128) * 0.11f), 0, 255),
            Clamp((int)((resultR1 + 128) * 0.36f + (resultG1 + 128) * 0.53f + (resultB1 + 128) * 0.11f), 0, 255));
     }
    }
    class TransferFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap im, int x, int y)
        {
            int newX = Clamp((int)(x + 50 ), 0, im.Width - 1);
            int newY = y;
            return im.GetPixel(newX, newY);
        }
    }
    class TurnFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap im, int x, int y)
        {
            int x0, y0, k;
            x0 = 150;
            y0 = 150;
            k = 90;
            int newX = Clamp(((int)((x - x0) * Math.Cos(k) - (y - y0) * Math.Sin(k)+x0)), 0, im.Width - 1);
            int newY = Clamp(((int)((x - x0) * Math.Sin(k) - (y - y0) * Math.Cos(k))+y0), 0, im.Height - 1);
            return im.GetPixel(newX, newY);
        }
    }
    class GLASSFilter : Filters {
        private Random rnd;
        public GLASSFilter()
        {
            rnd = new Random();
        }
        protected override Color calculateNewPixelColor(Bitmap im, int x, int y)
        {
           
            int newX = Clamp(((int)(x + (rnd.NextDouble()-0.5f) * 10)), 0, im.Width - 1);
            int newY = Clamp(((int)(y + (rnd.NextDouble()-0.5f) * 10)), 0, im.Height - 1);
            return im.GetPixel(newX, newY);
        }
    }
}




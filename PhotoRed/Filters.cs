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
        public float minBrightness = 1.0f;
        public float maxBrightness = 0.0f;
        public float AveregeBrightnessR = 1.0f;
        public float AveregeBrightnessG = 1.0f;
        public float AveregeBrightnessB = 1.0f;
        public float AveregeBrightness = 1.0f;


        public void setMaxMinBrightness(Bitmap sourceImage)
        {
            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    if (minBrightness > sourceImage.GetPixel(i, j).GetBrightness())
                        minBrightness = sourceImage.GetPixel(i, j).GetBrightness();
                    if (maxBrightness < sourceImage.GetPixel(i, j).GetBrightness())
                        maxBrightness = sourceImage.GetPixel(i, j).GetBrightness();
                }
            }
        }

        public void setAveregeBrightness(Bitmap sourceImage)
        {
            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    AveregeBrightnessR += sourceImage.GetPixel(i, j).R;
                    AveregeBrightnessG += sourceImage.GetPixel(i, j).G;
                    AveregeBrightnessB += sourceImage.GetPixel(i, j).B;
                }
            }
            AveregeBrightnessR /= sourceImage.Width * sourceImage.Height;
            AveregeBrightnessG /= sourceImage.Width * sourceImage.Height;
            AveregeBrightnessB /= sourceImage.Width * sourceImage.Height;

            AveregeBrightness = (AveregeBrightnessR + AveregeBrightnessG + AveregeBrightnessB) / 3;

        }

        public int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        protected abstract Color calculateNewPixelColor(Bitmap im, int x, int y);
        virtual public Bitmap processImage(Bitmap im, BackgroundWorker worker)
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
    class InvertFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap im, int x, int y)
        {
            Color sColor = im.GetPixel(x, y);
            Color rColor = Color.FromArgb(255 - sColor.R, 255 - sColor.G, 255 - sColor.B);
            return rColor;
        }
    }
    class GrayScaleFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap im, int x, int y)
        {
            float col = im.GetPixel(x, y).R * 0.36f + im.GetPixel(x, y).G * 0.53f + im.GetPixel(x, y).B * 0.11f;
            return Color.FromArgb(Clamp((int)col, 0, 255), Clamp((int)col, 0, 255), Clamp((int)col, 0, 255));
        }
    }
    class SepiaFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap im, int x, int y)
        {
            int k = 20;
            float col = im.GetPixel(x, y).R * 0.36f + im.GetPixel(x, y).G * 0.53f + im.GetPixel(x, y).B * 0.11f;
            return Color.FromArgb(Clamp((int)col + 2 * k, 0, 255), Clamp((int)(col + 0.5f * k), 0, 255), Clamp((int)col - k, 0, 255));
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
        protected MatrixFilter() { }
        public MatrixFilter(float[,] kernel)
        {
            this.kernel = kernel;
        }
        public void setKernel(float[,] arr,int size)
        {
            kernel = new float[size, size];
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    kernel[i, j] = arr[i, j];
        }
        protected override Color calculateNewPixelColor(Bitmap im, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, im.Width - 1);
                    int idY = Clamp(y + l, 0, im.Height - 1);
                    Color neighborColor = im.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            return Color.FromArgb(Clamp((int)resultR, 0, 255), Clamp((int)resultG, 0, 255), Clamp((int)resultB, 0, 255));
        }
        public Color calculateNewPixelColorMin(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            Color min = Color.FromArgb(255, 255, 255);

            for (int i = -radiusY; i <= radiusY; i++)
            {
                for (int j = -radiusX; j <= radiusX; j++)
                {
                    Color curr = sourceImage.GetPixel(Clamp(x + i, 0, sourceImage.Width - 1), Clamp(y + j, 0, sourceImage.Height - 1));
                    if ((kernel[j + radiusX, i + radiusY] != 0) && (Math.Sqrt(curr.R * curr.R + curr.G * curr.G + curr.B * curr.B) <
                                                Math.Sqrt(min.R * min.R + min.G * min.G + min.B * min.B)))
                        min = curr;
                }
            }
            return min;
        }


        public Color calculateNewPixelColorMax(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            Color max = Color.FromArgb(0, 0, 0);

            for (int i = -radiusY; i <= radiusY; i++)
            {
                for (int j = -radiusX; j <= radiusX; j++)
                {
                    Color curr = sourceImage.GetPixel(Clamp(x + i, 0, sourceImage.Width - 1), Clamp(y + j, 0, sourceImage.Height - 1));
                    if ((kernel[j + radiusX, i + radiusY] != 0) && (Math.Sqrt(curr.R * curr.R + curr.G * curr.G + curr.B * curr.B) >
                                                Math.Sqrt(max.R * max.R + max.G * max.G + max.B * max.B)))
                        max = curr;
                }
            }
            return max;
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
        public void createGaussianKernel(int radius, float sigma)
        {
            int size = radius * 2 + 1;
            kernel = new float[size, size];
            float norm = 0;
            for (int i = -radius; i < radius; i++)
                for (int j = -radius; j < radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    kernel[i, j] /= norm;

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
            int sum = (int)Math.Sqrt(resultR1 * resultR1 + resultR2 * resultR2 + resultG1 * resultG1 + resultG2 * resultG2 + resultB1 * resultB1 + resultB2 * resultB2) / 3;
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
    class PruittFilter : MatrixFilter
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
               Clamp((int)((resultR1 + 128) * 0.36f + (resultG1 + 128) * 0.53f + (resultB1 + 128) * 0.11f), 0, 255),
               Clamp((int)((resultR1 + 128) * 0.36f + (resultG1 + 128) * 0.53f + (resultB1 + 128) * 0.11f), 0, 255),
               Clamp((int)((resultR1 + 128) * 0.36f + (resultG1 + 128) * 0.53f + (resultB1 + 128) * 0.11f), 0, 255));
        }
    }
    class TransferFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap im, int x, int y)
        {
            int newX = Clamp((int)(x + 50), 0, im.Width - 1);
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
            int newX = Clamp(((int)((x - x0) * Math.Cos(k) - (y - y0) * Math.Sin(k) + x0)), 0, im.Width - 1);
            int newY = Clamp(((int)((x - x0) * Math.Sin(k) - (y - y0) * Math.Cos(k)) + y0), 0, im.Height - 1);
            return im.GetPixel(newX, newY);
        }
    }
    class GLASSFilter : Filters
    {
        private Random rnd;
        public GLASSFilter()
        {
            rnd = new Random();
        }
        protected override Color calculateNewPixelColor(Bitmap im, int x, int y)
        {

            int newX = Clamp(((int)(x + (rnd.NextDouble() - 0.5f) * 10)), 0, im.Width - 1);
            int newY = Clamp(((int)(y + (rnd.NextDouble() - 0.5f) * 10)), 0, im.Height - 1);
            return im.GetPixel(newX, newY);
        }
    }
    class MedianFilter : MatrixFilter
    {
        public MedianFilter()
        {
            int sizeX = 9;
            int sizeY = 9;
            kernel = new float[sizeX, sizeY];

        }
        protected override Color calculateNewPixelColor(Bitmap im, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            double[] arr = new double[(kernel.GetLength(0)) * (kernel.GetLength(1))];
            int i = 0;
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, im.Width - 1);
                    int idY = Clamp(y + l, 0, im.Height - 1);
                    arr[i] = (im.GetPixel(idX, idY)).ToArgb();
                    i++;
                }
            arr = sortins(arr, (kernel.GetLength(0)) * (kernel.GetLength(1)));
            double med = arr[((kernel.GetLength(0)) * (kernel.GetLength(1))) / 2];
            i = 0;
            Color resultColor = Color.FromArgb(0, 50, 0);
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, im.Width - 1);
                    int idY = Clamp(y + l, 0, im.Height - 1);
                    if (med == (im.GetPixel(idX, idY)).ToArgb())
                        resultColor = im.GetPixel(idX, idY);
                }
            return resultColor;
        }
        double[] sortins(double[] arr, int size)
        {
            double a = 0;
            int pos;
            for (int i = 1; i < size; i++)
            {
                a = arr[i];
                pos = i - 1;
                while (pos >= 0 && arr[pos] > a)
                {
                    arr[pos + 1] = arr[pos];
                    pos = pos - 1;
                }
                arr[pos + 1] = a;
            }
            return arr;
        }
    }
    class LinearStretching : MatrixFilter
    {
        private int maxR, minR, maxG, minG, maxB, minB;
        public LinearStretching()
        {
            maxR = new int();
            minR = new int();
            maxG = new int();
            minG = new int();
            maxB = new int();
            minB = new int();
        }
    
    public override Bitmap processImage(Bitmap im, BackgroundWorker worker)
    {
        maxR = minR = im.GetPixel(0, 0).R;
        maxG = minG = im.GetPixel(0, 0).G;
        maxB = minB = im.GetPixel(0, 0).B;

        for (int i = 0; i < im.Width; i++)
            for (int j = 0; j < im.Height; j++)
            {
                if (im.GetPixel(i, j).R > maxR)
                    maxR = im.GetPixel(i, j).R;
                if (im.GetPixel(i, j).G > maxG)
                    maxG = im.GetPixel(i, j).G;
                if (im.GetPixel(i, j).B > maxB)
                    maxB = im.GetPixel(i, j).B;
                if (im.GetPixel(i, j).R < minR)
                    minR = im.GetPixel(i, j).R;
                if (im.GetPixel(i, j).G < minG)
                    minG = im.GetPixel(i, j).G;
                if (im.GetPixel(i, j).B < minB)
                    minB = im.GetPixel(i, j).B;
            }

        Bitmap resultImage = new Bitmap(im.Width, im.Height);

        for (int i = 0; i < im.Width; i++)
        {
            worker.ReportProgress((int)((float)i / resultImage.Width * 100));
            if (worker.CancellationPending)
                return null;
            for (int j = 0; j < im.Height; j++)
            {
                resultImage.SetPixel(i, j,
                    Color.FromArgb(Clamp((int)(255 * (im.GetPixel(i, j).R - minR) / (maxR - minR)), 0, 255),
                                   Clamp((int)(255 * (im.GetPixel(i, j).G - minG) / (maxG - minG)), 0, 255),
                                   Clamp((int)(255 * (im.GetPixel(i, j).B - minB) / (maxB - minB)), 0, 255)));
            }
        }
        return resultImage;
    }
}
    class GreyWorldFilter : Filters
    {

        protected override Color calculateNewPixelColor(Bitmap im, int x, int y)
        {
            Color sourceColor = im.GetPixel(x, y);
            Color resultColor = Color.FromArgb(Clamp((int)(sourceColor.R * AveregeBrightness / AveregeBrightnessR), 0, 255),
                Clamp((int)(sourceColor.G * AveregeBrightness / AveregeBrightnessG), 0, 255),
                Clamp((int)(sourceColor.B * AveregeBrightness / AveregeBrightnessB), 0, 255));
            return resultColor;
        }
    }
    class DilationFilter : MatrixFilter
    {
        protected override Color calculateNewPixelColor(Bitmap im, int x, int y)

        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            Color max = Color.FromArgb(0, 0, 0);

            for (int i = -radiusY; i <= radiusY; i++)
            {
                for (int j = -radiusX; j <= radiusX; j++)
                {
                    Color curr = im.GetPixel(Clamp(x + i, 0, im.Width - 1), Clamp(y + j, 0, im.Height - 1));
                    if ((kernel[j + radiusX, i + radiusY] != 0) && (Math.Sqrt(curr.R * curr.R + curr.G * curr.G + curr.B * curr.B) >
                                                Math.Sqrt(max.R * max.R + max.G * max.G + max.B * max.B)))
                        max = curr;
                }
            }
            return max;
        }
    }
    class ErosionFilter : MatrixFilter

    {
     
        protected override Color calculateNewPixelColor(Bitmap im, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            Color min = Color.FromArgb(255, 255, 255);

            for (int i = -radiusY; i <= radiusY; i++)
            {
                for (int j = -radiusX; j <= radiusX; j++)
                {
                    Color curr = im.GetPixel(Clamp(x + i, 0, im.Width - 1), Clamp(y + j, 0, im.Height - 1));
                    if ((kernel[j + radiusX, i + radiusY] != 0) && (Math.Sqrt(curr.R * curr.R + curr.G * curr.G + curr.B * curr.B) <
                                                Math.Sqrt(min.R * min.R + min.G * min.G + min.B * min.B)))
                        min = curr;
                }
            }
            return min;
        }
    }
    class Opening : MatrixFilter
    {
      


        public override Bitmap processImage(Bitmap im, BackgroundWorker worker)
        {

            Bitmap resultImage = new Bitmap(im.Width, im.Height);

            for (int i = 0; i < im.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 50));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < im.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColorMin(im, i, j));
                }
            }

            Bitmap resultImage1 = new Bitmap(im.Width, im.Height);

            for (int i = 0; i < im.Width; i++)
            {
                worker.ReportProgress((int)(50 + (float)i / resultImage.Width * 50));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < im.Height; j++)
                {
                    resultImage1.SetPixel(i, j, calculateNewPixelColorMax(im, i, j));
                }
            }

            return resultImage1;

        }
    }
    class Closing : MatrixFilter
    {
        public override Bitmap processImage(Bitmap im, BackgroundWorker worker)
        {

            Bitmap resultImage = new Bitmap(im.Width, im.Height);

            for (int i = 0; i < im.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 50));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < im.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColorMax(im, i, j));
                }
            }

            Bitmap resultImage1 = new Bitmap(im.Width, im.Height);

            for (int i = 0; i < im.Width; i++)
            {
                worker.ReportProgress((int)(50 + (float)i / resultImage.Width * 50));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < im.Height; j++)
                {
                    resultImage1.SetPixel(i, j, calculateNewPixelColorMin(resultImage, i, j));
                }
            }

            return resultImage1;

        }
    }
    class GradFilter : MatrixFilter
    {

        public override Bitmap processImage(Bitmap im, BackgroundWorker worker)
        {

            Bitmap resultImageD = new Bitmap(im.Width, im.Height);

            for (int i = 0; i < im.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImageD.Width * 33));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < im.Height; j++)
                {
                    resultImageD.SetPixel(i, j, calculateNewPixelColorMax(im, i, j));
                }
            }

            Bitmap resultImageE = new Bitmap(im.Width, im.Height);

            for (int i = 0; i < im.Width; i++)
            {
                worker.ReportProgress((int)(33 + (float)i / resultImageE.Width * 33));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < im.Height; j++)
                {
                    resultImageE.SetPixel(i, j, calculateNewPixelColorMin(im, i, j));
                }
            }

            for (int i = 0; i < im.Width; i++)
            {
                worker.ReportProgress((int)(66 + (float)i / resultImageE.Width * 34));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < im.Height; j++)
                {
                    resultImageD.SetPixel(i, j, calculateNewPixelColorMin(resultImageE, i, j));
                }
            }


            return resultImageD;

        }
    }
    class NewFilter : Filters
    {
        int gran;
        public void cal(Bitmap im)
        {
            gran =(int) (float)(im.Height * 3) / 4;
        }
        protected override Color calculateNewPixelColor(Bitmap im, int x, int y)
        {
            if (y > gran)
            {
                Color sColor = im.GetPixel(x, y);
                Color rColor = Color.FromArgb(255 - sColor.R, 255 - sColor.G, 255 - sColor.B);

                return rColor;
            }
            else return im.GetPixel(x, y);
        }
    }

}




using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PhotoRed
{
    public partial class Form1 : Form
    {
        private float[,] mas;
        Form2 fm;
        Bitmap image;
        TStack<Bitmap> st,stO;
        public Form1()
        {
            InitializeComponent();
            st = new TStack<Bitmap>(10);
            stO = new TStack<Bitmap>(10);
            отменаToolStripMenuItem.Enabled = false;
            вернутьToolStripMenuItem.Enabled = false;
            матМорфологияToolStripMenuItem.Enabled = false;
        }
        public void getMas(float[,] arr,int size)
        {
            mas = new float[size, size];
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    mas[i, j] = arr[i, j];
            fm.Dispose();
        }
        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files | *.png; *.jpg; *.bmp; | All Files (*.*) | *.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(dialog.FileName);
                pictureBox1.Image = image;
                pictureBox1.Refresh();
            }
        }

        private void инверсияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InvertFilter filter = new InvertFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }


        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!st.isfull()) { st.push(image); отменаToolStripMenuItem.Enabled = true; }
            Bitmap newImage = ((Filters)e.Argument).processImage(image, backgroundWorker1);
            if (backgroundWorker1.CancellationPending != true) image = newImage;

        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                pictureBox1.Image = image;
                pictureBox1.Refresh();
            }
            progressBar1.Value = 0;
        }

        private void Отмена_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }

        private void размытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BlurFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void gaussToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GaussianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void sobelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new SobelFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void grayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GrayScaleFilter filter = new GrayScaleFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void sepiaFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SepiaFilter filter = new SepiaFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void brightnessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BrightnessFilter filter = new BrightnessFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void harshnessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HarshnessFilter filter = new HarshnessFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void вернутьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!stO.isempty())
            {
                if (!st.isfull()) { st.push(image); отменаToolStripMenuItem.Enabled = true; }
                image = stO.pop();               
                pictureBox1.Image = image;
                pictureBox1.Refresh();
            }
            if (stO.isempty()) вернутьToolStripMenuItem.Enabled = false;
        }

        private void shaarraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShaarraFilter filter = new ShaarraFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void прюиттаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PruittFilter filter = new PruittFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void embossingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EmbossingFilter filter = new EmbossingFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void transferToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TransferFilter filter = new TransferFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void turnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TurnFilter filter = new TurnFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void gLASSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GLASSFilter filter = new GLASSFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "PNG|*.png|JPEG|*.jpg|BMP|*.bmp";
            if (dialog.ShowDialog() == DialogResult.OK && dialog.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)dialog.OpenFile();
                switch (dialog.FilterIndex)
                {
                    case 1:
                        pictureBox1.Image.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                    case 2:
                        pictureBox1.Image.Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                    case 3:
                        pictureBox1.Image.Save(fs, System.Drawing.Imaging.ImageFormat.Bmp);
                        break;
                }
                fs.Close();
            }
        }

        private void medianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MedianFilter filter = new MedianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void dilationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DilationFilter filter = new DilationFilter();
            filter.setKernel(mas, mas.GetLength(0));
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void erosionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErosionFilter filter = new ErosionFilter();
            filter.setKernel(mas, mas.GetLength(0));
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void openingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Opening filter = new Opening();
            filter.setKernel(mas, mas.GetLength(0));
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void closingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Closing filter = new Closing();
            filter.setKernel(mas, mas.GetLength(0));
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void gradFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GradFilter filter = new GradFilter();
            filter.setKernel(mas, mas.GetLength(0));
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void настроитьСтруктурныйЭлементToolStripMenuItem_Click(object sender, EventArgs e)
        {
            матМорфологияToolStripMenuItem.Enabled = true;
            fm = new Form2(this);
            fm.ShowDialog();

        }

        private void linearStretchingToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            LinearStretching filter = new LinearStretching();
            filter.setMaxMinBrightness(image);
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void greyWorldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GreyWorldFilter filter = new GreyWorldFilter();
            filter.setAveregeBrightness(image);
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void CancelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!st.isempty()) {
                if (!stO.isfull()) { stO.push(image); вернутьToolStripMenuItem.Enabled = true; }
                image = st.pop();
                pictureBox1.Image = image;
                pictureBox1.Refresh();
              
            }
            if (st.isempty()) отменаToolStripMenuItem.Enabled = false;


        }

        private void nEWFILTERToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewFilter filter = new NewFilter();
            filter.cal(image);
            backgroundWorker1.RunWorkerAsync(filter);
        }
    }

}
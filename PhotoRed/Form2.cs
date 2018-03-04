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
    public partial class Form2 : Form
    {
        Form1 main;
        int size;
        public Form2(Form1 f)
        {
            InitializeComponent();
            main = f;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            size =Convert.ToInt32(textBox1.Text);
            dataGridView1.ColumnCount = size;
            dataGridView1.RowCount = size;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            float[,] res=new float[size,size];
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    res[i, j] = Convert.ToInt32(dataGridView1.Rows[i].Cells[j].Value);
            main.getMas(res, size);
        }
    }
}

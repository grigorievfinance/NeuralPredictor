using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
            numericUpDown1.Value = Form4.hidden;
            numericUpDown2.Value = Form4.layers;
            numericUpDown3.Value = Form4.WindowSize;
            numericUpDown4.Value = Form4.future;
            numericUpDown5.Value = Form4.trainsize;
            numericUpDown5.Maximum = Form4.trainsize;
            numericUpDown6.Value = Form4.predictsize;
            numericUpDown6.Maximum = Form4.predictsize;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Form4.WindowSize = (int)numericUpDown3.Value;
            Form4.hidden = (int)numericUpDown1.Value;
            Form4.layers = (int)numericUpDown2.Value;
            Form4.future = (int)numericUpDown4.Value;
            Form4.trainsize = (int)numericUpDown5.Value;
            Form4.predictsize = (int)numericUpDown6.Value;
            Form4.Init();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

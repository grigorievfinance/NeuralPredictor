using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Windows.Forms.DataVisualization.Charting;
using Encog.Persist;
using Encog.Engine.Network.Activation;
using Encog.ML;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.ML.Data.Temporal;
using Encog.ML.Train;
using Encog.ML.Train.Strategy;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Encog.Neural.Networks.Training;
using Encog.Neural.Networks.Training.Anneal;
using Encog.Neural.Networks.Training.Lma;
using Encog.Neural.Networks.Training.Propagation.Back;
using Encog.Neural.Networks.Training.Propagation.Resilient;
using Encog.Neural.Pattern;
using Encog.Util;
using Encog.Util.Simple;
using Encog.Util.Arrayutil;
using Encog.Util.CSV;
using Encog.Util.File;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Create();
        }

        public static int len;

        public static List<double> QuickParseCSV(string file, string Name, int size)
        {
            List<double> returnedArrays = new List<double>();
            ReadCSV csv = new ReadCSV(file, true, CSVFormat.English);
            int currentRead = 0;

            while (csv.Next() && currentRead < size)
            {
                returnedArrays.Add(csv.GetDouble(Name));
                currentRead++;
            }
            
            return returnedArrays;
        }
        
        private void openCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stream myStream;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = openFileDialog1.OpenFile()) != null)
                {

                    if (!File.Exists(openFileDialog1.FileName + "2"))
                        File.Copy(openFileDialog1.FileName, openFileDialog1.FileName + "2");

                    string[] fileContent = File.ReadAllLines(openFileDialog1.FileName);
                    len = fileContent.Length;

                    Form fm = Form4.ActiveForm.ActiveMdiChild;
                    if (fm == null) Create();

                    Form4.Sunspots = QuickParseCSV(openFileDialog1.FileName, "Close", len).ToArray();
                    Form4.Open = QuickParseCSV(openFileDialog1.FileName, "Open", len).ToArray();
                    Form4.High = QuickParseCSV(openFileDialog1.FileName, "High", len).ToArray();
                    Form4.Low = QuickParseCSV(openFileDialog1.FileName, "Low", len).ToArray();
                    
                    Form4 frm = (Form4)this.ActiveMdiChild;
                    frm.Graphic();

                    FileInfo name = new FileInfo(openFileDialog1.FileName);
                    frm.Text = name.Name;

                    toolStripStatusLabel1.Text = "CSV loaded";

                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void openNetworkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stream myStream;

            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = openFileDialog2.OpenFile()) != null)
                {

                    if (!File.Exists(openFileDialog2.FileName + "2"))
                        File.Copy(openFileDialog2.FileName, openFileDialog2.FileName + "2");

                    Form4 frm = (Form4)this.ActiveMdiChild;
                    FileInfo networkFile = new FileInfo(openFileDialog2.FileName);
                    frm.network = (BasicNetwork)EncogDirectoryPersistence.LoadObject(networkFile);
                    Form4.networkflag = 1;
                    toolStripStatusLabel1.Text = "Network loaded";
                }


            }

        }

        private void openTrainingToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Stream myStream;

            if (openFileDialog3.ShowDialog() == DialogResult.OK)
            {
                if (Form4.networkflag == 0)
                {
                    toolStripStatusLabel1.Text = "Load CSV first!";
                    return;
                }
                if ((myStream = openFileDialog3.OpenFile()) != null)
                {
                    Form4 frm = (Form4)this.ActiveMdiChild;
                    FileInfo trainFile = new FileInfo(openFileDialog3.FileName);
                    frm.training = EncogUtility.LoadEGB2Memory(trainFile);
                    Form4.trainflag = 1;
                    toolStripStatusLabel1.Text = "Training loaded";
                }
            }
        }

        private void saveNetworkToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (Form4.networkflag == 0)
            {
                toolStripStatusLabel1.Text = "Create the network first!";
                return;
            }
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Form4 frm = (Form4)this.ActiveMdiChild;
                FileInfo networkFile = new FileInfo(saveFileDialog1.FileName);
                EncogDirectoryPersistence.SaveObject(networkFile, frm.network);
                toolStripStatusLabel1.Text = "Network saved";
            }
        }

        private void saveTraningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Form4.trainflag == 0)
            {
                toolStripStatusLabel1.Text = "Training first!";
                return;
            }
            if (saveFileDialog2.ShowDialog() == DialogResult.OK)
            {
                Form4 frm = (Form4)this.ActiveMdiChild;
                FileInfo trainingFile = new FileInfo(saveFileDialog2.FileName);
                EncogUtility.SaveEGB(trainingFile, frm.training);
                toolStripStatusLabel1.Text = "Training saved";
            }
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form4 frm = (Form4)this.ActiveMdiChild;
            frm.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form4 frm = (Form4)this.ActiveMdiChild;
            frm.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form4 frm = (Form4)this.ActiveMdiChild;
            frm.Paste();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form4 frm = (Form4)this.ActiveMdiChild;
            frm.Deselectall();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form4 frm = (Form4)this.ActiveMdiChild;
            frm.Selectall();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form4 frm = (Form4)this.ActiveMdiChild;
            frm.Clear();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form3 frm = new Form3();
            frm.Show();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 frm = new Form2();
            frm.Show();
        }

        private void helpF1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, "help.chm");
          
        }

        private void arrangeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.ArrangeIcons);
        }

        private void cascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.Cascade);
        }

        private void titleHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void titleVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.TileVertical);
        }
        
        private void trainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Form4.networkflag == 0)
            {
                toolStripStatusLabel1.Text = "Load CSV first!";
                return;
            }
            toolStripStatusLabel1.Text = "Training";
            Form4 frm = (Form4)this.ActiveMdiChild;
            frm.CreateNetwork();
            frm.Train();
            toolStripStatusLabel1.Text = "Ready";
        }

        private void predictToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Form4.trainflag == 0)
            {
                toolStripStatusLabel1.Text = "Training first!";
                return;
            }
            Form4 frm = (Form4)this.ActiveMdiChild;
            frm.Predict();
            
        }

        private void futureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Form4.trainflag == 0)
            {
                toolStripStatusLabel1.Text = "Training first!";
                return;
            }
            Form4 frm = (Form4)this.ActiveMdiChild;
            frm.Future();
            toolStripStatusLabel1.Text = "Future";
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Create();
        }
       
        public void Create()
        {
            Form4 frm = new Form4();
            frm.MdiParent = this;
            frm.Text = "Untitled";
            frm.Show();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form4 frm = (Form4)this.ActiveMdiChild;
            frm.Undo();
        }

   }
}
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
using Encog.Neural.Networks.Training.Propagation;
using Encog.Neural.NEAT;
using Encog.Neural.NEAT.Training;
using Encog.Neural.Pattern;
using Encog.Util;
using Encog.Util.Simple;
using Encog.Util.Arrayutil;
using Encog.Util.CSV;
using Encog.Util.File;

namespace WindowsFormsApplication1
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
            toolStripStatusLabel1.Text = Convert.ToString(System.DateTime.Now.ToLongDateString()) + " " + Convert.ToString(System.DateTime.Now.ToLongTimeString());
        }

        public static double[] Sunspots = { 0 };
        public static double[] Percents = { 0 };
        public static double[] Open = { 0 };
        public static double[] High = { 0 };
        public static double[] Low = { 0 };

        public static int future = 5;
        public static int layers = 4;
        public static int hidden = 80;
        public static int WindowSize = 20;

        public static int TrainStart;
        public static int TrainEnd;
        public static int EvaluateStart;
        public static int EvaluateEnd;
        public static int trainsize = 100;
        public static int predictsize = 100;

        public static int networkflag = 0;
        public static int trainflag = 0;
        public BasicNetwork network;
        public IMLDataSet training;

        public void Cut()
        {
        richTextBox1.Cut();
        }

        public void Paste()
        {
            richTextBox1.Paste();
        }

        public void Copy()
        {
            richTextBox1.Copy();
        }

        public void Deselectall()
        {
            richTextBox1.DeselectAll();
        }

        public void Selectall()
        {
            richTextBox1.SelectAll();
        }

        public void Undo()
        {
            richTextBox1.Undo();
        }

        public void Clear()
        {
            richTextBox1.Clear();
        }

        public void CreateNetwork()
        {
            if (networkflag == 0)
            {
                toolStripStatusLabel1.Text = "Load CSV first!";
                return;
            }
            network = (BasicNetwork)ElmanNetwork(WindowSize, future, hidden);
            //network = new BasicNetwork();
            network.AddLayer(new BasicLayer(null, true, WindowSize));
                      for (int i = 0; i < layers; i++)
            network.AddLayer(new BasicLayer(new ActivationTANH(), true, hidden));
            network.AddLayer(new BasicLayer(new ActivationLinear(), true, future));
            network.Structure.FinalizeStructure();
            network.Reset();
            training = GenerateTraining();
            toolStripStatusLabel1.Text = "Inputs " + WindowSize + ", Hidden " + hidden + ", Layers " + layers + ", Outputs " + future;
        }

        public void Train()
        {

            if (networkflag == 0)
            {
                toolStripStatusLabel1.Text = "Load CSV first!";
                return;
            }
           //IMLTrain trainMain = new ResilientPropagation(network, training);
           IMLTrain trainMain = new Backpropagation(network, training, 0.0001, 0.01);
           trainflag = 1;
           // var stop = new StopTrainingStrategy();
           //StopTrainingStrategy stop = new StopTrainingStrategy(0.0000001, 200);
           //ICalculateScore score = new TrainingSetScore(training); //trainMain.Training
           //IMLTrain trainAlt = new NeuralSimulatedAnnealing(network, score, 10, 2, 100);
           //trainMain.AddStrategy(new Greedy());
           //trainMain.AddStrategy(new HybridStrategy(trainAlt));
           //trainMain.AddStrategy(stop);
            EncogUtility.TrainDialog(trainMain, network, training);
        }

        public static IMLDataSet GenerateTraining()
        {
            var result = new TemporalMLDataSet(WindowSize, future);

            var desc = new TemporalDataDescription(TemporalDataDescription.Type.Raw, true, true);
            result.AddDescription(desc);

            NormalizeArray array = new NormalizeArray { NormalizedHigh = 1, NormalizedLow = -1 };

            // create arrays to hold the normalized sunspots
            double[] _normalizedSunspots = array.Process(Sunspots);
            double[] _closedLoopSunspots = EngineArray.ArrayCopy(_normalizedSunspots);
            double[] _normalizedOpen = array.Process(Open);
            double[] _closedLoopOpen = EngineArray.ArrayCopy(_normalizedOpen);
            double[] _normalizedHigh = array.Process(High);
            double[] _closedLoopHigh = EngineArray.ArrayCopy(_normalizedHigh);
            double[] _normalizedLow = array.Process(Low);
            double[] _closedLoopLow = EngineArray.ArrayCopy(_normalizedLow);

         

            for (int year = TrainStart; year < TrainEnd; year++)
            {
                var point = new TemporalPoint(4) { Sequence = year };
                point.Data[0] = _normalizedSunspots[year];
                point.Data[1] = _normalizedOpen[year];
                point.Data[2] = _normalizedHigh[year];
                point.Data[3] = _normalizedLow[year];
                result.Points.Add(point);
            }

            result.Generate();

            return result;
        }

        public void Predict()
        {
            if (trainflag == 0) 
            {
                toolStripStatusLabel1.Text = "Train first!";
                return;
            }
            NormalizeArray array = new NormalizeArray { NormalizedHigh = 1, NormalizedLow = -1 };

            // create arrays to hold the normalized sunspots
            double[] _normalizedSunspots = array.Process(Sunspots);
            double[] _closedLoopSunspots = EngineArray.ArrayCopy(_normalizedSunspots);
            double[] _normalizedOpen = array.Process(Open);
            double[] _closedLoopOpen = EngineArray.ArrayCopy(_normalizedOpen);
            double[] _normalizedHigh = array.Process(High);
            double[] _closedLoopHigh = EngineArray.ArrayCopy(_normalizedHigh);
            double[] _normalizedLow = array.Process(Low);
            double[] _closedLoopLow = EngineArray.ArrayCopy(_normalizedLow);

            int correct = 0;
            int count = 0;
            double accOpen = 0;
            double accHigh = 0;
            double accLow = 0;
            double accurancy = 0;
            double aver = 0;
            double open;
            double high;
            double low;
            double close;

            for (int year = EvaluateStart + 1; year < EvaluateEnd; year++)
            {
                // calculate based on actual data
                IMLData input = new BasicMLData(WindowSize);
                for (var i = 0; i < input.Count; i++)
                {
                    input.Data[i] = _normalizedSunspots[(year - WindowSize) + i];
                }
                IMLData output = network.Compute(input);
                double prediction = output.Data[0];
                _closedLoopSunspots[year] = prediction;

                // calculate "closed loop", based on predicted data
                for (var i = 0; i < input.Count; i++)
                {
                    input.Data[i] = _closedLoopSunspots[(year - WindowSize) + i];
                }
                output = network.Compute(input);
                double closedLoopPrediction = output.Data[0];

                //Open
                IMLData input1Open = new BasicMLData(WindowSize);
                for (var i = 0; i < input.Count; i++)
                {
                    input.Data[i] = _normalizedOpen[(year - WindowSize) + i];
                }
                IMLData outputOpen = network.Compute(input);
                double predictionOpen = output.Data[0];
                _closedLoopOpen[year] = prediction;

                // calculate "closed loop", based on predicted data
                for (var i = 0; i < input.Count; i++)
                {
                    input.Data[i] = _closedLoopOpen[(year - WindowSize) + i];
                }
                output = network.Compute(input);
                double closedLoopPredictionOpen = output.Data[0];

                //High
                IMLData inputHigh = new BasicMLData(WindowSize);
                for (var i = 0; i < input.Count; i++)
                {
                    input.Data[i] = _normalizedHigh[(year - WindowSize) + i];
                }
                IMLData outputHigh = network.Compute(input);
                double predictionHigh = output.Data[0];
                _closedLoopHigh[year] = prediction;

                // calculate "closed loop", based on predicted data
                for (var i = 0; i < input.Count; i++)
                {
                    input.Data[i] = _closedLoopHigh[(year - WindowSize) + i];
                }
                output = network.Compute(input);
                double closedLoopPredictionHigh = output.Data[0];

                //Low
                IMLData inputLow = new BasicMLData(WindowSize);
                for (var i = 0; i < input.Count; i++)
                {
                    input.Data[i] = _normalizedLow[(year - WindowSize) + i];
                }
                IMLData outputLow = network.Compute(input);
                double predictionLow = output.Data[0];
                _closedLoopLow[year] = prediction;

                // calculate "closed loop", based on predicted data
                for (var i = 0; i < input.Count; i++)
                {
                    input.Data[i] = _closedLoopLow[(year - WindowSize) + i];
                }
                output = network.Compute(input);
                double closedLoopPredictionLow = output.Data[0];

                string diractual = "Flat";
                string dirpredict = "Flat";

                if (_normalizedOpen[year] < -0.1)
                    diractual = "down";
                if (_normalizedOpen[year] > 0.1)
                    diractual = "up";
                if (_normalizedOpen[year] > -0.1 && _normalizedOpen[year] < 0.1)
                    diractual = "Flat";


                if (predictionOpen < -0.1)
                    dirpredict = "down";
                if (predictionOpen > 0.1)
                    dirpredict = "up";
                if (predictionOpen > -0.1 && predictionOpen < 0.1)
                    dirpredict = "Flat";


                if (diractual == dirpredict)
                    correct++;

                open = array.Stats.DeNormalize(predictionOpen);
                high = array.Stats.DeNormalize(predictionHigh);
                low = array.Stats.DeNormalize(predictionLow);
                close = array.Stats.DeNormalize(prediction);

                accOpen = Math.Abs(Open[year] - open);
                accHigh = Math.Abs(High[year] - high);
                accLow = Math.Abs(Low[year] - low);
                accurancy = Math.Abs(Sunspots[year] - close);

                aver = aver + (accOpen + accHigh + accLow + accurancy) / 4;

                chart1.Series[1].Points.AddXY(year, open, high, low, close);
                count++;
            }
            
            double percent = ((double)correct / (double)count) * 100;
            double aveg = aver / (double)count;

            toolStripStatusLabel1.Text = "Direction correct: " + correct + "/" + count + ", Directional accuracy: " + Format.FormatDouble(percent, 5) + "%, Average accuracy: " + Format.FormatDouble(aveg, 5);
        }

        public void Future()
        {

            if (trainflag == 0)
            {
                toolStripStatusLabel1.Text = "Train first!";
                return;
            }

            richTextBox1.Clear();

            //future predict
            NormalizeArray array = new NormalizeArray { NormalizedHigh = 1, NormalizedLow = -1 };

            // create arrays to hold the normalized sunspots
            double[] _normalizedSunspots = array.Process(Sunspots);
            double[] _closedLoopSunspots = EngineArray.ArrayCopy(_normalizedSunspots);
            double[] _normalizedOpen = array.Process(Open);
            double[] _closedLoopOpen = EngineArray.ArrayCopy(_normalizedOpen);
            double[] _normalizedHigh = array.Process(High);
            double[] _closedLoopHigh = EngineArray.ArrayCopy(_normalizedHigh);
            double[] _normalizedLow = array.Process(Low);
            double[] _closedLoopLow = EngineArray.ArrayCopy(_normalizedLow);

            string dirpred = "Flat";
            int yr = EvaluateEnd;

            IMLData inp = new BasicMLData(WindowSize);
            for (var i = 0; i < inp.Count; i++)
            {
                inp.Data[i] = _normalizedSunspots[(yr - WindowSize) + i];
            }
            IMLData outp = network.Compute(inp);

            IMLData inpOpen = new BasicMLData(WindowSize);
            for (var i = 0; i < inp.Count; i++)
            {
                inp.Data[i] = _normalizedOpen[(yr - WindowSize) + i];
            }
            IMLData outpOpen = network.Compute(inp);

            IMLData inpHigh = new BasicMLData(WindowSize);
            for (var i = 0; i < inp.Count; i++)
            {
                inp.Data[i] = _normalizedHigh[(yr - WindowSize) + i];
            }
            IMLData outpHigh = network.Compute(inp);

            IMLData inpLow = new BasicMLData(WindowSize);
            for (var i = 0; i < inp.Count; i++)
            {
                inp.Data[i] = _normalizedLow[(yr - WindowSize) + i];
            }
            IMLData outpLow = network.Compute(inp);

            toolStripStatusLabel1.Text = "Predict in future " + future + " vaue";

            for (int i = 0; i < future; i++)
            {
                double pred = outp.Data[i];
                double predOpen = outpOpen.Data[i];
                double predHigh = outpHigh.Data[i];
                double predLow = outpLow.Data[i];

                if (predOpen < -0.1)
                    dirpred = "down";
                if (predOpen > 0.1)
                    dirpred = "up";
                if (predOpen < 0.1 && predOpen > -0.1)
                    dirpred = "Flat";

                double futureOpen = array.Stats.DeNormalize(predOpen);
                double futureHigh = array.Stats.DeNormalize(predHigh);
                double futureLow = array.Stats.DeNormalize(predLow);
                double futureClose = array.Stats.DeNormalize(pred);

                

                richTextBox1.AppendText("\nFuture: " + (i) + "\nOpen: " + Format.FormatDouble(futureOpen, 5) + "\nHigh: " + Format.FormatDouble(futureHigh, 5) 
                  + "\nLow: " + Format.FormatDouble(futureLow, 5) + "\nClose: " + Format.FormatDouble(futureClose, 5) + "\nDirection: " + dirpred + "\n");
                chart1.Series[2].Points.AddXY(yr + i, futureOpen, futureHigh, futureLow, futureClose);
            }
        }

        public void Graphic()
        {
            Init();

            double max = (double)Low.Min();
            double min = (double)High.Max();

            for (int j = (EvaluateEnd - 50); j < (EvaluateEnd); j++)
            {
                if (High[j] > max) max = High[j];
                if (Low[j] < min) min = Low[j];
            }
            
            chart1.ChartAreas["ChartArea1"].AxisY.Minimum = min;
            chart1.ChartAreas["ChartArea1"].AxisY.Maximum = max;
            chart1.ChartAreas["ChartArea1"].AxisX.Minimum = EvaluateEnd - 50;
            chart1.ChartAreas["ChartArea1"].AxisX.Maximum = EvaluateEnd + future;
            chart1.Update();
            
            for (int z = (EvaluateEnd - 50); z < (EvaluateEnd); z++)
            {
                chart1.Series[0].Points.AddXY(z, Open[z], High[z], Low[z], Sunspots[z]);
            }
        }

        public static void Init()
        {
            EvaluateEnd = Form1.len - 1;
            EvaluateStart = EvaluateEnd - predictsize;
            TrainEnd = EvaluateStart;
            TrainStart = TrainEnd - trainsize;
            networkflag = 1;
            predictsize = (int)(0.2 * Form1.len);
            trainsize = (int)(0.2 * Form1.len);
        }

        public static IMLMethod ElmanNetwork(int inputsize, int outputsize, int hidden1)
        {
            // construct an Elman type network
            ElmanPattern pattern = new ElmanPattern();
            pattern.ActivationFunction = new ActivationTANH();
            pattern.InputNeurons = inputsize;
            pattern.AddHiddenLayer(hidden1);
            pattern.OutputNeurons = outputsize;
            return pattern.Generate();
        }
  }
}

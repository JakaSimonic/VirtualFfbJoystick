using vJoyWrapper;
    using System;
using System.Collections.Generic;

//using sim_onix;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gugalnik
{
    public partial class Form1 : Form
    {
        private bool start = false;
        private volatile bool run = true;
        private double frequency = 0;
        private double amplitude = 0;
        private double pfl, pfr, prl, prr = 0;
        private int sleep = 250;

        public Form1()
        {
            InitializeComponent();

            vw = new vJoyFunctions();
            vw.Init();
            flP.Text = "0";
            frP.Text = "0";
            rlP.Text = "0";
            rrP.Text = "0";

            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);

            Thread t = new Thread(Process);
            t.Start();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            amplitude = (double)Amplitude.Value / 10D;
            label9.Text = amplitude.ToString();
        }

        private void Process()
        {
            while (true)
            {
                var result = vw.GetFfbForce(new List<double> { 255d*xtrans, 255d*ytrans });
                UpdateChart3(result);
                Thread.Sleep(sleep);
            }
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            frequency = Math.PI * 2D / (double)(10100 - Frequency.Value * 200);
            float temp = ((float)(1000F / (10100F - (float)Frequency.Value * 200F)));
            label8.Text = temp.ToString("0.00") + " Hz";
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        public void UpdateChart2(double[] position)
        {
            try
            {
                var updatePlotInput = new Action(() =>
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (chart2.Series[i].Points.Count > 75) chart2.Series[i].Points.RemoveAt(0);
                        chart2.Series[i].Points.AddY(position[i]);
                    }
                });

                this.Invoke(updatePlotInput);
            }
            catch { }
        }

        public void UpdateChart3(List<double> dl)
        {
            double[] torque = { dl[0], dl[1] };
            try
            {
                var updatePlotInput = new Action(() =>
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (chart3.Series[i].Points.Count > 75) chart3.Series[i].Points.RemoveAt(0);
                        chart3.Series[i].Points.AddY(torque[i]);
                    }
                });

                this.Invoke(updatePlotInput);
            }
            catch { }
        }

        private void flP_TextChanged(object sender, EventArgs e)
        {
            try
            {
                pfl = Double.Parse(flP.Text);
            }
            catch { pfl = 0; }
        }

        private void frP_TextChanged(object sender, EventArgs e)
        {
            try
            {
                pfr = Double.Parse(frP.Text);
            }
            catch
            { pfr = 0; }
        }

        private void rlP_TextChanged(object sender, EventArgs e)
        {
            try
            {
                prl = Double.Parse(rlP.Text);
            }
            catch
            {
                prl = 0;
            }
        }

        private void rrP_TextChanged(object sender, EventArgs e)
        {
            try
            {
                prr = Double.Parse(rrP.Text);
            }
            catch
            {
                prr = 0;
            }
        }
    }
}
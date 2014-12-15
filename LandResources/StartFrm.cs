using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace LandResources
{
    public partial class StartFrm : System.Windows.Forms.Form
    {
        private double OPACITY_STEP1 = 0.01;
        private double OPACITY_STEP2 = 0.03;
        private double OPACITY_STEP3 = 0.05;
        private MainForm mainFrm;
        public StartFrm(MainForm _MainForm)
        {
            InitializeComponent();
            mainFrm = _MainForm;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (this.Opacity >= 0.6)
                {
                    this.Opacity -= OPACITY_STEP1;
                }
                else if (this.Opacity < 0.6 && this.Opacity >= 0.3 && this.Opacity >= OPACITY_STEP2)
                {
                    this.Opacity -= OPACITY_STEP2;
                }
                else if (this.Opacity < 0.3 && this.Opacity >= OPACITY_STEP3)
                {
                    this.Opacity -= OPACITY_STEP3;
                }
                else
                {
                    this.Close();
                }
            }
            catch
            {
            }
        }

        private void StartFrm_Load(object sender, EventArgs e)
        {
            this.Refresh();
            mainFrm.Opacity = 0;
            this.timer1.Interval = 100;
            this.timer1.Start();            
        }

        private void StartFrm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.timer1.Stop();
            mainFrm.Opacity = 1;
        }
    }
}
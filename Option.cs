using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UTTTWithGUI
{
    public partial class Options : Form
    {
        public Options(int md, int stfv, int[,] mig, int[] mag, int us)
        {
            InitializeComponent();
            MaxDepth = md;
            StepsToFullView = stfv;
            maxDepth.Text = MaxDepth.ToString();
            stepsToFullView.Text = StepsToFullView.ToString();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    microGame.Text += mig[i,j].ToString()+" ";
                }
                microGame.Text += Environment.NewLine;
                macroGame.Text += mag[i].ToString()+" ";
            }
            userSquare.Text = us.ToString();
        }
        public int MaxDepth;
        public int StepsToFullView;
        private void Accept_Click(object sender, EventArgs e)
        {
            MaxDepth = Convert.ToInt32(maxDepth.Text);
            StepsToFullView = Convert.ToInt32(stepsToFullView.Text);
            this.Hide();
        }
    }
}
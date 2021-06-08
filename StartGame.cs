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
    public partial class StartGame : Form
    {
        public int player=0;
        public StartGame()
        {
            InitializeComponent();
        }

        private void cross_Click(object sender, EventArgs e)
        {
            player = -1;
            this.Hide();
        }

        private void nul_Click(object sender, EventArgs e)
        {
            player = 1;
            this.Hide();
        }
    }
}
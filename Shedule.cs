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
    public partial class Shedule : Form
    {
        Graphics g;
        LinkedListNode<int> Evaluated;
        PointF[] points;
        public Shedule(LinkedList<int> Eva)
        {
            InitializeComponent();
            this.Evaluated = Eva.First;
            points = new PointF[Eva.Count+1];
            points[0].X = 0;
            points[0].Y = 385;
            for (int i = 1; i <= Eva.Count(); i++)
            {
                points[i].X = i * 878 / Eva.Count();
                points[i].Y = 385-((float)Evaluated.Value)/10;
                Evaluated = Evaluated.Next;
            }
        }

        private void onPaint(object sender, PaintEventArgs e)
        {
            g = e.Graphics;
            Pen p1 = new Pen(Color.Black,2);
            Pen p2 = new Pen(Color.Green, 1);
            g.DrawLine(p1, 0, 386, 878, 386);
            for (int i = 1; i < points.Length; i++)
            {
                g.DrawLine(p2, points[i - 1], points[i]);
            }
        }
    }
}
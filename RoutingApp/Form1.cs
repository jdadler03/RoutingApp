using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;

namespace RoutingApp
{
    public partial class Form1 : Form
    {
        Graphics g;
        Pen pen;
        Brush brush;
        Routes R;
        List <Vector2> pts = new List<Vector2>();
        double[,] mtx;

        public Form1()
        {
            InitializeComponent();
            g = drawPanel.CreateGraphics();

            pen = Pens.Black;
            brush = Brushes.Black;
        }

        void drawPts(List<Vector2> pts)
        {
            int ptRadius = 2;
            foreach(Vector2 pt in pts)
            {
                g.FillEllipse(brush, pt.X - ptRadius, pt.Y - ptRadius, ptRadius * 2, ptRadius * 2);
            }
        }

        double[,] generateMtx(int n)
        {
            Random rand = new Random();
            double[,] mtx = new double[n, n];
            pts.Clear();
            pts.Add(new Vector2(850, 450));
            for (int i = 1; i < n; i++) pts.Add(new Vector2(rand.Next(0, 1700), rand.Next(0, 900)));
            for (int i = 0; i < pts.Count; i++)
                for (int j = i; j < pts.Count; j++)
                {
                    double dist = Vector2.Distance(pts[i], pts[j]);
                    mtx[i, j] = dist;
                    mtx[j, i] = dist;
                }

            return mtx;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            mtx = generateMtx(21);
            R = new Routes(mtx);
            g = drawPanel.CreateGraphics();
            g.Clear(Color.White);
            drawAll();
        }

        void drawAll()
        {
            drawPts(pts);
            drawRoutes();
        }
        void drawRoutes()
        {
            List<List<int>> rts = R.getRoutes();
            foreach (List<int> rt in rts)
            {
                int h = 0;
                foreach (int i in rt)
                {
                    drawArrow(pts[h], pts[i]);
                    h = i;
                }
                drawArrow(pts[h],pts[0]);
            }
        }
        void drawArrow(Vector2 a, Vector2 b)
        {
            int arrowSize = 12;

            double aAngle = Math.Atan((b.Y - a.Y) / (b.X - a.X));
            if (b.X - a.X < 0) aAngle += Math.PI;
            Vector2 c = new Vector2(
                (float) (b.X - arrowSize * Math.Cos(aAngle - Math.PI / 6)),
                (float) (b.Y - arrowSize * Math.Sin(aAngle - Math.PI / 6))
            );
            Vector2 d = new Vector2(
                (float) (b.X - arrowSize * Math.Cos(aAngle + Math.PI / 6)),
                (float) (b.Y - arrowSize * Math.Sin(aAngle + Math.PI / 6))
            );

            g.DrawLine(pen, a.X, a.Y, b.X, b.Y);
            g.DrawLine(pen, b.X, b.Y, c.X, c.Y);
            g.DrawLine(pen, b.X, b.Y, d.X, d.Y);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void drawPanel_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}

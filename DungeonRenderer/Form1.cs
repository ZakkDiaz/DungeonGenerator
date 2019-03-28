using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DungeonRenderer.Models;

namespace DungeonRenderer
{
    public partial class Form1 : Form
    {
        private IDungeonGenerator generator;
        public Form1()
        {
            InitializeComponent();
            panel1.Paint += Panel1_Paint;
            generator = new CellGenerator();
        }

        private void Panel1_Paint(object sender, PaintEventArgs e)
        {
            using (var g = e.Graphics)
            {
                g.Clear(Color.Black);
                g.DrawImage(GenerateDungeon(), new PointF(0,0));
            }
        }

        private Bitmap GenerateDungeon()
        {
            var seed = textBox1.Text;
            var size = textBox2.Text;
            int x = 10;
            Int32.TryParse(size, out x);
            x = x == 0 ? 10 : x;
            generator = new CellGenerator();
            generator.GenerateDungeon(seed, x, trackBar1.Value);
            return generator.Draw(panel1.Width, panel1.Height);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            panel1.Invalidate();
        }
    }
}


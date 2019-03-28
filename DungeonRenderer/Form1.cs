using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DungeonRenderer
{
    public partial class Form1 : Form
    {
        private IDungeonGenerator generator;
        public Form1()
        {
            InitializeComponent();
            panel1.Paint += Panel1_Paint;
            generator = new CellGenerator(10);
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
            var xVal = textBox1.Text;
            var yVal = textBox2.Text;
            int x = 10;
            int y = 10;
            Int32.TryParse(xVal, out x);
            Int32.TryParse(yVal, out y);
            x = x == 0 ? 10 : x;
            y = y == 0 ? 10 : y;
            generator.GenerateDungeon(x, y);
            return generator.Draw(panel1.Width, panel1.Height);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            panel1.Invalidate();
        }
    }

    public class DungeonGenerator : IDungeonGenerator
    {
        private List<ITile> tiles;
        private Random r;

        public DungeonGenerator()
        {
            tiles = new List<ITile>();
            r = new Random();
        }

        public DungeonGenerator(int seed)
        {
            tiles = new List<ITile>();
            r = new Random(seed);
        }

        public Bitmap Draw(int width, int height)
        {
            var min = Math.Min(width, height);
            Bitmap bmp = new Bitmap(min, min);

            DrawDungeon(ref bmp);

            return bmp;
        }

        public void DrawDungeon(ref Bitmap bmp)
        {
            var maxX = tiles.Max(m => m.GetLocation().X);
            var maxY = tiles.Max(m => m.GetLocation().Y);
            var width = bmp.Width;
            var height = bmp.Height;
            var tileWidth = tiles.Count(c => c.GetLocation().X == 0);
            var tileHeight =tiles.Count(c => c.GetLocation().Y == 0);
            tileWidth = width / maxX;
            tileHeight = height / maxY;
            using (var g = Graphics.FromImage(bmp))
            {
                foreach (var tile in tiles)
                {
                    Point p = tile.GetLocation();
                    Color c = tile.GetColor();
                    var xStep = p.X / (float)maxX;
                    var yStep = p.Y / (float)maxY;
                    xStep *= width;
                    yStep *= height;
                    g.FillRectangle(new SolidBrush(c), xStep, yStep, tileWidth, tileHeight);
                }
            }
        }

        public void GenerateDungeon(int rows, int columns)
        {
            tiles.Clear();
            for (var i = 0; i < rows; i++)
            {
                for (var ii = 0; ii < columns; ii++)
                {
                    tiles.Add(GetTile(i, ii));
                }
            }
        }

        public ITile GetTile(int i, int ii)
        {
            if (i < ii)
                return new Tile(i, ii, Color.Green);
            else if(i > ii + 5)
                return new Tile(i, ii, Color.DarkGreen);
            else
                return new Tile(i, ii, Color.Blue);
        }
    }

    public class CellGenerator : IDungeonGenerator
    {
        List<Cell> cells = new List<Cell>();

        public CellGenerator(int squareLength)
        {
            this.Generate(squareLength);
            var removedList = this.Trim();
            cells = cells.Except(removedList).ToList();
        }

        private List<Cell> Trim()
        {
            List<Cell> ret = new List<Cell>();
            foreach (var c in cells)
            {
                var val = c.Trim();
                if (val != null)
                    ret.Add(val);
            }

            return ret;
        }

        private Cell Generate(int squareLength)
        {
            int roomCount = squareLength * squareLength;


            //Create cells
            for (var i = 1; i < roomCount + 1; i++)
            {
                cells.Add(new Cell(i));
            }

            //Sort them randomly
            Random r = new Random();
            cells = cells.OrderBy(o => r.Next()).ToList();

            //Assign coordinates
            int cnt = 0;
            for (var x = 0; x < squareLength; x++)
            {
                for (var y = 0; y < squareLength; y++)
                {
                    var curCell = cells[cnt];
                    curCell.SetCoordinate(new Tuple<int, int>(x, y));
                    cnt++;
                }
            }

            Cell head = cells.OrderBy(o => o.Id()).FirstOrDefault();
            head.Connect(new List<Cell>(cells));

            return head;
        }

        public Bitmap Draw(int width, int height)
        {
            var min = Math.Min(width, height);
            Bitmap bmp = new Bitmap(min, min);

            DrawDungeon(ref bmp);

            return bmp;
        }

        public void DrawDungeon(ref Bitmap bmp)
        {
            var maxX = cells.Max(m => m.GetLocation().X) + 1;
            var maxY = cells.Max(m => m.GetLocation().Y) + 1;
            var width = bmp.Width;
            var height = bmp.Height;
            var tileWidth = cells.Count(c => c.GetLocation().X == 0);
            var tileHeight = cells.Count(c => c.GetLocation().Y == 0);
            tileWidth = width / maxX;
            tileHeight = height / maxY;

            var renderCells = new List<Cell>(cells);

            using (var g = Graphics.FromImage(bmp))
            {
                float lastX = -1;
                float lastY = -1;

                var headTile = cells.OrderBy(o => o.Id()).FirstOrDefault();

                foreach (var t in cells)
                {
                    RenderTile(maxX, maxY, width, height, tileWidth, tileHeight, g, lastX, lastY, t, ref renderCells);
                }

            }
        }

        private static void RenderTile(int maxX, int maxY, int width, int height, int tileWidth, int tileHeight, Graphics g, float lastX, float lastY, Cell headTile, ref List<Cell> renderCells)
        {
            if (renderCells.IndexOf(headTile) == -1)
                return;

            renderCells.Remove(headTile);

            var tile = headTile;
            Point p = tile.GetLocation();
            Color c = tile.GetColor();
            var xStep = p.X / (float)maxX;
            var yStep = p.Y / (float)maxY;
            xStep *= width;
            yStep *= height;
            //g.DrawRectangle(new Pen(c), xStep, yStep, tileWidth, tileHeight);
            g.FillRectangle(new SolidBrush(c), xStep, yStep, tileWidth, tileHeight);
            g.DrawString($"{tile.Id()}", SystemFonts.DefaultFont, Brushes.WhiteSmoke, xStep, yStep);
            g.FillEllipse(new SolidBrush(Color.Red), xStep + tileWidth / 2, yStep + tileHeight / 2, 10, 10);
            if (lastX != -1)
            {
                //g.DrawArc(Pens.Blue, lastX + tileWidth / 2, lastY + tileHeight / 2, xStep + tileWidth / 2, yStep + tileHeight / 2, -10, 10);
                //g.DrawLine(Pens.Blue, lastX + tileWidth / 2, lastY + tileHeight / 2, xStep + tileWidth / 2, yStep + tileHeight / 2);
            }

            lastX = xStep;
            lastY = yStep;

            if (headTile.West())
            {
                var cell = headTile.GetWest();
                DoWalk(maxX, maxY, width, height, tileWidth, tileHeight, g, lastX, lastY, ref renderCells, xStep, yStep, cell);
            }


            if (headTile.South())
            {
                var cell = headTile.GetSouth();
                DoWalk(maxX, maxY, width, height, tileWidth, tileHeight, g, lastX, lastY, ref renderCells, xStep, yStep, cell);
            }
            

            if (headTile.East())
            {
                var cell = headTile.GetEast();
                DoWalk(maxX, maxY, width, height, tileWidth, tileHeight, g, lastX, lastY, ref renderCells, xStep, yStep, cell);
            }
            

            if (headTile.North())
            {
                var cell = headTile.GetNorth();
                DoWalk(maxX, maxY, width, height, tileWidth, tileHeight, g, lastX, lastY, ref renderCells, xStep, yStep, cell);
            }

        }

        private static void DoWalk(int maxX, int maxY, int width, int height, int tileWidth, int tileHeight, Graphics g, float lastX, float lastY, ref List<Cell> renderCells, float xStep, float yStep, Cell cell)
        {
            Point p2 = cell.GetLocation();
            Color c2 = cell.GetColor();
            var xStep2 = p2.X / (float)maxX;
            var yStep2 = p2.Y / (float)maxY;
            xStep2 *= width;
            yStep2 *= height;

            g.DrawLine(Pens.DarkGreen, xStep2 + tileWidth / 2, yStep2 + tileHeight / 2, xStep + tileWidth / 2, yStep + tileHeight / 2);

            //RenderTile(maxX, maxY, width, height, tileWidth, tileHeight, g, ref lastX, ref lastY,
            //    cell, ref renderCells);
        }

        public void GenerateDungeon(int rows, int columns)
        {
            //already done in constructor
        }

        public ITile GetTile(int i, int ii)
        {
            return cells.FirstOrDefault(c => c.Coordinates().Item1 == i && c.Coordinates().Item2 == ii);
        }
    }

    internal class Tile : ITile
    {
        private int _x;
        private int _y;
        private Color _color;

        public Tile(int x, int y, Color color)
        {
            _x = x;
            _y = y;
            _color = color;
        }

        public Color GetColor()
        {
            return _color;
        }

        public Point GetLocation()
        {
            return new Point(_x, _y);
        }
    }

    public interface ITile
    {
        Point GetLocation();
        Color GetColor();
    }

    public interface IDungeonGenerator
    {
        Bitmap Draw(int width, int height);
        void GenerateDungeon(int rows, int columns);
        ITile GetTile(int i, int ii);
    }



    public class Cell : ITile
    {
        //base value
        private int _id { get; set; }

        //total value
        private int _value { get; set; }

        //door placements
        private Cell _west { get; set; }
        private Cell _north { get; set; }
        private Cell _east { get; set; }
        private Cell _south { get; set; }

        public int Id() => _id;
        public int Value() => _value;
        public bool West() => _west != null;
        public bool North() => _north != null;
        public bool East() => _east != null;
        public bool South() => _south != null;

        //grid id
        private Tuple<int, int> _coordinate { get; set; }
        public Tuple<int, int> Coordinates() => _coordinate != null ? _coordinate : new Tuple<int, int>(0, 0);

        public Cell(int Id)
        {
            _id = Id;
            _value = _id;
        }

        public void SetCoordinate(Tuple<int, int> coordinate)
        {
            _coordinate = coordinate;
        }

        internal void Connect(List<Cell> cells)
        {
            var trackedCells = new List<Cell>(cells);
            trackedCells.Remove(this);
            var potentialCells = cells.Where(c => this.IsNear(c)).ToList();
            potentialCells.Remove(this);
            var lowest = potentialCells.OrderBy(o => o.Id()).FirstOrDefault();
            this.Join(lowest);
            lowest.Join(this);
            lowest.Connect(this, trackedCells, potentialCells, cells);
        }

        private void Connect(Cell from, List<Cell> trackedCells, List<Cell> potentialCells, List<Cell> MASTER_CELL_LIST)
        {
            trackedCells.Remove(this);
            this._value += from._id + from._value;

            var nextCells = trackedCells.Where(c => this.IsNear(c)).ToList();
            potentialCells.AddRange(nextCells);
            potentialCells = potentialCells.Except(MASTER_CELL_LIST.Except(trackedCells)).ToList();
            //potentialCells = potentialCells.Except(trackedCells).ToList();

            var lowest = potentialCells.OrderBy(o => o.Id()).FirstOrDefault();

            var line = MASTER_CELL_LIST.Except(trackedCells).Where(l => l.IsNear(lowest)).OrderBy(l => l._id)
                .FirstOrDefault();

            if (lowest != null)
            {
                line.Join(lowest);
                lowest.Join(line);
                lowest.Connect(line, trackedCells, potentialCells, MASTER_CELL_LIST);
            }
        }

        private void Join(Cell from)
        {
            if (this.Coordinates().Item1 -1 == from.Coordinates().Item1)
                this._west = from;
            if (this.Coordinates().Item2 - 1 == from.Coordinates().Item2)
                this._north = from;
            if (this.Coordinates().Item1 + 1 == from.Coordinates().Item1)
                this._east = from;
            if (this.Coordinates().Item2 + 1 == from.Coordinates().Item2)
                this._south = from;
        }

        private bool IsNear(Cell c)
        {
            if (c == null)
                return false;
            //Check the immediate sides
            return
                //X - 1 = look west
                ((this._coordinate.Item1 - 1 == c._coordinate.Item1) && (this._coordinate.Item2 == c._coordinate.Item2))
                ||
                //X + 1 = look east
                ((this._coordinate.Item1 + 1 == c._coordinate.Item1) && (this._coordinate.Item2 == c._coordinate.Item2))
                ||
                //Y - 1 = look east
                ((this._coordinate.Item2 - 1 == c._coordinate.Item2) && (this._coordinate.Item1 == c._coordinate.Item1))
                ||
                //Y + 1 = look east
                ((this._coordinate.Item2 + 1 == c._coordinate.Item2) && (this._coordinate.Item1 == c._coordinate.Item1))
                ;


            //This does all 8 directions
            //return Math.Abs(this._coordinate.Item1 - c.Coordinates().Item1) <= 1 &&
            //       Math.Abs(this._coordinate.Item2 - c.Coordinates().Item2) <= 1;
        }

        public Point GetLocation()
        {
            return new Point(_coordinate.Item1, _coordinate.Item2);
        }

        public Color GetColor()
        {
            var red = (_id + _value)/5;
            var green = 127;
            var blue = 127;
            //if (East())
            //    blue += 80;
            //if (West())
            //    blue -= 80;
            //if (North())
            //    red += 80;
            //if (South())
            //    red -= 80;

            return Color.FromArgb(Clamp(red), Clamp(green), Clamp(blue));
        }

        private int Clamp(int val)
        {
            if (val < 0)
                val = 0;
            if (val > 255)
                val = 255;
            if(!(val >= 0 && val <= 255))
                val = 0;
            return val;
        }

        internal Cell GetWest()
        {
            return _west;
        }
        internal Cell GetNorth()
        {
            return _north;
        }
        internal Cell GetEast()
        {
            return _east;
        }
        internal Cell GetSouth()
        {
            return _south;
        }

        internal Cell Trim()
        {
            var cnt = 0;
            if (North())
                cnt++;
            if (East())
                cnt++;
            if (South())
                cnt++;
            if (West())
                cnt++;
            if (cnt <= 1)
            {
                return Prune();
            }

            return null;
        }

        private Cell Prune()
        {
            if (this.West())
            {
                this._west._east = null;
                this._west = null;
            }

            if (this.East())
            {
                this._east._west= null;
                this._east = null;
            }
            if (this.South())
            {
                this._south._north= null;
                this._south = null;
            }
            if (this.North())
            {
                this._north._south = null;
                this._north = null;
            }

            return this;
        }
    }
}


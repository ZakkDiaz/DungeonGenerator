using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DungeonRenderer.Models
{
    //public class DungeonGenerator : IDungeonGenerator
    //{
    //    private List<ITile> tiles;
    //    private Random r;

    //    public DungeonGenerator()
    //    {
    //        tiles = new List<ITile>();
    //        r = new Random();
    //    }

    //    public DungeonGenerator(int seed)
    //    {
    //        tiles = new List<ITile>();
    //        r = new Random(seed);
    //    }

    //    public Bitmap Draw(int width, int height)
    //    {
    //        var min = Math.Min(width, height);
    //        Bitmap bmp = new Bitmap(min, min);

    //        DrawDungeon(ref bmp);

    //        return bmp;
    //    }

    //    public void DrawDungeon(ref Bitmap bmp)
    //    {
    //        var maxX = tiles.Max(m => m.GetLocation().X);
    //        var maxY = tiles.Max(m => m.GetLocation().Y);
    //        var width = bmp.Width;
    //        var height = bmp.Height;
    //        var tileWidth = tiles.Count(c => c.GetLocation().X == 0);
    //        var tileHeight =tiles.Count(c => c.GetLocation().Y == 0);
    //        tileWidth = width / maxX;
    //        tileHeight = height / maxY;
    //        using (var g = Graphics.FromImage(bmp))
    //        {
    //            foreach (var tile in tiles)
    //            {
    //                Point p = tile.GetLocation();
    //                Color c = tile.GetColor();
    //                var xStep = p.X / (float)maxX;
    //                var yStep = p.Y / (float)maxY;
    //                xStep *= width;
    //                yStep *= height;
    //                g.FillRectangle(new SolidBrush(c), xStep, yStep, tileWidth, tileHeight);
    //            }
    //        }
    //    }

    //    public void GenerateDungeon(int rows, int columns)
    //    {
    //        tiles.Clear();
    //        for (var i = 0; i < rows; i++)
    //        {
    //            for (var ii = 0; ii < columns; ii++)
    //            {
    //                tiles.Add(GetTile(i, ii));
    //            }
    //        }
    //    }

    //    public ITile GetTile(int i, int ii)
    //    {
    //        if (i < ii)
    //            return new Tile(i, ii, Color.Green);
    //        else if(i > ii + 5)
    //            return new Tile(i, ii, Color.DarkGreen);
    //        else
    //            return new Tile(i, ii, Color.Blue);
    //    }
    //}

    public class CellGenerator : IDungeonGenerator
    {
        List<Cell> cells = new List<Cell>();
        private string _seed = "empty";
        public CellGenerator()
        {
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

            int sd = -1;
            Int32.TryParse(_seed, out sd);
            Random r = new Random(sd);
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

        public void GenerateDungeon(string seed, int squareLength, int pruneLength)
        {
            _seed = seed;
            this.Generate(squareLength);

            for (var i = 0; i < pruneLength; i++)
            {
                var removedList = this.Trim();
                cells = cells.Except(removedList).ToList();
            }
        }

        public ITile GetTile(int i, int ii)
        {
            return cells.FirstOrDefault(c => c.Coordinates().Item1 == i && c.Coordinates().Item2 == ii);
        }
    }
}


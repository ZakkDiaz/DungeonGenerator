using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static System.Int32;

namespace DungeonRenderer.Models
{
    public class CellGenerator : IDungeonGenerator
    {
        List<Cell> cells = new List<Cell>();
        private string _seed = "empty";
        private int _sd;
        private IEnumerable<Cell> Trim()
        {
            var ret = new List<Cell>();
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
            var roomCount = squareLength * squareLength;
            
            for (var i = 1; i < roomCount + 1; i++)
            {
                cells.Add(new Cell(i));
            }

            if (TryParse(_seed, out var sd))
                _sd = sd;
            else if(_sd == default(int))
                _sd = DateTime.Now.Millisecond;
            var r = new Random(_sd);
            cells = cells.OrderBy(o => r.Next()).ToList();
            
            

            var cnt = 0;
            for (var x = 0; x < squareLength; x++)
            {
                for (var y = 0; y < squareLength; y++)
                {
                    var curCell = cells[cnt];
                    curCell.SetCoordinate(new Tuple<int, int>(x, y));
                    cnt++;
                }
            }
            var head = cells.OrderBy(o => o.Id()).FirstOrDefault();
            if (head == null) return null;
            head.Connect(new List<Cell>(cells));
            return head;

        }
        public Bitmap Draw(int width, int height)
        {
            var min = Math.Min(width, height);
            var bmp = new Bitmap(min, min);
            DrawDungeon(ref bmp);
            return bmp;
        }
        public void DrawDungeon(ref Bitmap bmp)
        {
            var maxX = cells.Max(m => m.GetLocation().X) + 1;
            var maxY = cells.Max(m => m.GetLocation().Y) + 1;
            var width = bmp.Width;
            var height = bmp.Height;
            var tileWidth = width / maxX;
            var tileHeight = height / maxY;
            var renderCells = new List<Cell>(cells);
            using (var g = Graphics.FromImage(bmp))
            {
                foreach (var t in cells)
                {
                    RenderTile(maxX, maxY, width, height, tileWidth, tileHeight, g, t, ref renderCells);
                }
            }
        }
        private static void RenderTile(int maxX, int maxY, int width, int height, int tileWidth, int tileHeight,
            Graphics g, Cell headTile, ref List<Cell> renderCells)
        {
            if (renderCells.IndexOf(headTile) == -1)
                return;
            renderCells.Remove(headTile);
            var tile = headTile;
            var p = tile.GetLocation();
            var c = tile.GetColor();
            var xStep = p.X / (float) maxX;
            var yStep = p.Y / (float) maxY;
            xStep *= width;
            yStep *= height;
            
            g.FillRectangle(new SolidBrush(c), xStep, yStep, tileWidth, tileHeight);
            g.DrawString($"{tile.Id()}", SystemFonts.DefaultFont, Brushes.WhiteSmoke, xStep, yStep);
            g.FillEllipse(new SolidBrush(Color.Red), xStep + tileWidth / 2, yStep + tileHeight / 2, 10, 10);
            
            if (headTile.West())
            {
                var cell = headTile.GetWest();
                DoWalk(maxX, maxY, width, height, tileWidth, tileHeight, g, xStep, yStep,
                    cell);
            }
            if (headTile.South())
            {
                var cell = headTile.GetSouth();
                DoWalk(maxX, maxY, width, height, tileWidth, tileHeight, g, xStep, yStep,
                    cell);
            }
            if (headTile.East())
            {
                var cell = headTile.GetEast();
                DoWalk(maxX, maxY, width, height, tileWidth, tileHeight, g, xStep, yStep,
                    cell);
            }
            if (headTile.North())
            {
                var cell = headTile.GetNorth();
                DoWalk(maxX, maxY, width, height, tileWidth, tileHeight, g, xStep, yStep,
                    cell);
            }
        }
        private static void DoWalk(int maxX, int maxY, int width, int height, int tileWidth, int tileHeight, Graphics g,
            float xStep, float yStep, Cell cell)
        {
            Point p2 = cell.GetLocation();
            var xStep2 = p2.X / (float) maxX;
            var yStep2 = p2.Y / (float) maxY;
            xStep2 *= width;
            yStep2 *= height;
            g.DrawLine(Pens.DarkGreen, xStep2 + tileWidth / 2, yStep2 + tileHeight / 2, xStep + tileWidth / 2,
                yStep + tileHeight / 2);
            
            
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

        public void Clear()
        {
            cells.Clear();
        }
    }
}
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

        //private static Dictionary<string, Bitmap> _bmpDictionary = new Dictionary<string, Bitmap>();

        public CellGenerator()
        {
        }

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

        public ITile GetEntrance()
        {
            return cells.FirstOrDefault();
        }
    }
}
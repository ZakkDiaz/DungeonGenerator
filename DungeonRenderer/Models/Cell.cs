using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DungeonRenderer.Models
{
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
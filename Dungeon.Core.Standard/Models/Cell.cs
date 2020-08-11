using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
namespace DungeonRenderer.Models
{
    public class Cell : ITile
    {
        
        private int _id { get; set; }
        
        private int _value { get; set; }
        
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
            var potentialCells = cells.Where(this.IsNear).ToList();
            potentialCells.Remove(this);
            var lowest = potentialCells.OrderBy(o => o.Id()).FirstOrDefault();
            this.Join(lowest);
            if (lowest != null)
            {
                lowest.Join(this);
                lowest.Connect(this, trackedCells, potentialCells, cells);
            }
        }
        private void Connect(Cell from, List<Cell> trackedCells, List<Cell> potentialCells, List<Cell> masterCellList)
        {
            trackedCells.Remove(this);
            this._value += from._id + from._value;
            var nextCells = trackedCells.Where(this.IsNear).ToList();
            potentialCells.AddRange(nextCells);
            potentialCells = potentialCells.Except(masterCellList.Except(trackedCells)).ToList();
            
            var lowest = potentialCells.OrderBy(o => o.Id()).FirstOrDefault();
            var line = masterCellList.Except(trackedCells).Where(l => l.IsNear(lowest)).OrderBy(l => l._id)
                .FirstOrDefault();
            if (lowest != null)
            {
                if (line != null)
                {
                    line.Join(lowest);
                    lowest.Join(line);
                    lowest.Connect(line, trackedCells, potentialCells, masterCellList);
                }
            }
        }
        private void Join(Cell from)
        {
            if (this.Coordinates().Item1 - 1 == from.Coordinates().Item1)
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
            if (c == null || c._coordinate == null)
                return false;
            
            return
                _coordinate != null && ((_coordinate.Item1 - 1 == c._coordinate.Item1 && _coordinate.Item2 == c._coordinate.Item2)||
                                        (_coordinate.Item1 + 1 == c._coordinate.Item1 && _coordinate.Item2 == c._coordinate.Item2)||
                                        (_coordinate.Item2 - 1 == c._coordinate.Item2 && _coordinate.Item1 == c._coordinate.Item1)||
                                        (_coordinate.Item2 + 1 == c._coordinate.Item2 && _coordinate.Item1 == c._coordinate.Item1));
        }
        public Point GetLocation()
        {
            if (_coordinate == null)
                return new Point(-1000, 1000);
            return new Point(_coordinate.Item1, _coordinate.Item2);
        }
        public Color GetColor()
        {
            var red = (_id + _value) / 5;
            var green = 127;
            var blue = 127;
            return Color.FromArgb(25, Clamp(red), Clamp(green), Clamp(blue));
        }
        private int Clamp(int val)
        {
            if (val < 0)
                val = 0;
            if (val > 255)
                val = 255;
            if (!(val >= 0 && val <= 255))
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
            return cnt <= 1 ? Prune() : null;
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
                this._east._west = null;
                this._east = null;
            }
            if (this.South())
            {
                this._south._north = null;
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
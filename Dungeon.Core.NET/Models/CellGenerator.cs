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


        //Vector3 currentPosition = new Vector3() { 0, 0, 0 };
        public Direction GetDirection(Vector3 myVector)
        {
            DirectionCube cube = new DirectionCube(myVector);

            var allDirections = cube.GetDirections();

            return cube.GetImmediateDirection();
        }

        public class Vector3
        {
            public int x, y, z;

        }

        public class DirectionCube
        {
            Vector3 offset;

            public DirectionCube(Vector3 _init)
            {
                offset = _init;
            }

            private bool DirectionX { get { return offset.x != 0; } }
            //private bool DirectionY { get { return offset.y != 0; } }
            private bool DirectionZ { get { return offset.z != 0; } }

            public bool Up { get { return !DirectionX & offset.z > 0; } }
            public bool Down { get { return !DirectionX & offset.z < 0; } }
            public bool Left { get { return !DirectionZ & offset.x > 0; } }
            public bool Right { get { return !DirectionZ & offset.x < 0; } }

            public Direction GetImmediateDirection()
            {
                if (Up)
                    return Direction.up;
                if (Down)
                    return Direction.down;
                if (Left)
                    return Direction.left;
                if (Right)
                    return Direction.right;
                return Direction.none;
            }

            internal IEnumerable<Direction> GetDirections()
            {
                List<Direction> directions = new List<Direction>();
                if (Up)
                    directions.Add(Direction.up);
                if (Down)
                    directions.Add(Direction.down);
                if (Left)
                    directions.Add(Direction.left);
                if (Right)
                    directions.Add(Direction.right);
                return directions;
            }
        }

        public enum Direction
        {
            up, down, left, right, front, back, none
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
                    curCell.SetCoordinate(new Point(x, y));
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
            return cells.FirstOrDefault(c => c.Coordinates().X == i && c.Coordinates().Y == ii);
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
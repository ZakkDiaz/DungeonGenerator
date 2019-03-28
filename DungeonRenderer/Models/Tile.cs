using System.Drawing;

namespace DungeonRenderer.Models
{
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
}


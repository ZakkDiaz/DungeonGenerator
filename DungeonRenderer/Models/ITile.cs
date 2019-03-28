using System.Drawing;

namespace DungeonRenderer.Models
{
    public interface ITile
    {
        Point GetLocation();
        Color GetColor();
    }
}


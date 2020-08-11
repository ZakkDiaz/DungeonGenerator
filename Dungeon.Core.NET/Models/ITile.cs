using System.Drawing;
namespace DungeonRenderer.Models
{
    public interface ITile
    {
        int GetLocationX();
        int GetLocationY();
        Color GetColor();
    }
}
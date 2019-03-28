using System.Drawing;
namespace DungeonRenderer.Models
{
    public interface IDungeonGenerator
    {
        Bitmap Draw(int width, int height);
        void GenerateDungeon(string seed, int squareLength, int pruneLength);
        ITile GetTile(int i, int ii);
        void Clear();
    }
}
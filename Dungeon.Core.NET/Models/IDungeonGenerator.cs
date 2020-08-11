using System.Drawing;
namespace DungeonRenderer.Models
{
    public interface IDungeonGenerator
    {
        void GenerateDungeon(string seed, int squareLength, int pruneLength);
        ITile GetTile(int i, int ii);
        void Clear();
        ITile GetEntrance();
    }
}
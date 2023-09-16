using System.Collections.Generic;

namespace MazeDatatype
{
    public class MazeGraph
    {
        public int Width { get; }
        public int Height { get; }

        public MazeCell[,] Cells { get; }
        public List<MazeWall> Walls { get; }

        public MazeGraph(int width, int height, bool withWalls = true)
        {
            Width = width;
            Height = height;
            Cells = new MazeCell[width, height];
            Walls = new List<MazeWall>();
            
            // Initialize the maze cells
            for (var x = 0; x < Width; x++)
            {
                for (var z = 0; z < Height; z++)
                {
                    Cells[x, z] = new MazeCell(x, z, withWalls);
                }
            }
        }

        public MazeCell GetCell(int x, int z)
        {
            return Cells[x, z];
        }

        public void SetCell(int x, int z, MazeCell cell)
        {
            Cells[x, z] = cell;
        }
    }
}

using System.Collections.Generic;

namespace MazeDatatype
{
    public class MazeCell
    {
        public int X { get; }
        public int Z { get; }

        public List<MazeCell> Neighbours { get; }
        public List<MazeWall> Walls { get; }
        
        public MazeCorner TopLeftCorner { get; set; }
        public MazeCorner TopRightCorner { get; set; }
        public MazeCorner BottomLeftCorner { get; set; }
        public MazeCorner BottomRightCorner { get; set; }
        
        public bool Visited { get; set; }

        public MazeCell(int x, int z)
        {
            X = x;
            Z = z;
            Walls = new List<MazeWall>();
            Neighbours = new List<MazeCell>();
            Visited = false;
        }

        public void AddWall(MazeWall wall)
        {
            Walls.Add(wall);
        }

        public void AddNeighbour(MazeCell mazeCell)
        {
            Neighbours.Add(mazeCell);
        }
    }
}

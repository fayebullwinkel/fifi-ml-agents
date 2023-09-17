using System;
using System.Collections.Generic;

namespace MazeDatatype
{
    public class MazeCell
    {
        public int X { get; }
        public int Z { get; }

        public MazeWall LeftWall { get; set; }
        public MazeWall RightWall { get; set; }
        public MazeWall TopWall { get; set; }
        public MazeWall BottomWall { get; set; }
        
        public MazeCorner TopLeftCorner { get; set; }
        public MazeCorner TopRightCorner { get; set; }
        public MazeCorner BottomLeftCorner { get; set; }
        public MazeCorner BottomRightCorner { get; set; }
        
        private bool Visited;
        public List<MazeCell> Neighbours { get; }
        
        public MazeCell(int x, int z)
        {
            X = x;
            Z = z;
            Visited = false;
            Neighbours = new List<MazeCell>();
        }

        public MazeWall GetWall(WallOrientation orientation)
        {
            return orientation switch
            {
                WallOrientation.Left => LeftWall,
                WallOrientation.Right => RightWall,
                WallOrientation.Top => TopWall,
                WallOrientation.Bottom => BottomWall,
                _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null)
            };
        }
        
        public void SetWall(WallOrientation orientation, MazeWall wall)
        {
            switch (orientation)
            {
                case WallOrientation.Left:
                    LeftWall = wall;
                    break;
                case WallOrientation.Right:
                    RightWall = wall;
                    break;
                case WallOrientation.Top:
                    TopWall = wall;
                    break;
                case WallOrientation.Bottom:
                    BottomWall = wall;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null);
            }
        }

        public void AddNeighbour(MazeCell mazeCell)
        {
            Neighbours.Add(mazeCell);
        }
    }
}

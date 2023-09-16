using System;
using System.Collections.Generic;

namespace MazeDatatype
{
    public enum Wall
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public enum Corner
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
    
    public class MazeCell
    {
        private readonly int _x;
        private readonly int _z;

        private bool _leftWall;
        private bool _rightWall;
        private bool _topWall;
        private bool _bottomWall;
        
        private int _topLeftCorner;
        private int _topRightCorner;
        private int _bottomLeftCorner;
        private int _bottomRightCorner;
        
        private bool _visited;
        
        private readonly List<MazeCell> _neighbours;
        
        public MazeCell(int x, int z, bool withWalls = true)
        {
            _x = x;
            _z = z;
            _leftWall = withWalls;
            _rightWall = withWalls;
            _topWall = withWalls;
            _bottomWall = withWalls;
            _topLeftCorner = withWalls ? 4 : 0;
            _topRightCorner = withWalls ? 4 : 0;
            _bottomLeftCorner = withWalls ? 4 : 0;
            _bottomRightCorner = withWalls ? 4 : 0;
            _visited = false;
            _neighbours = new List<MazeCell>();
        }
        
        public int GetX()
        {
            return _x;
        }
        
        public int GetZ()
        {
            return _z;
        }

        public bool GetWall(Wall wall)
        {
            return wall switch
            {
                Wall.Left => _leftWall,
                Wall.Right => _rightWall,
                Wall.Top => _topWall,
                Wall.Bottom => _bottomWall,
                _ => false
            };
        }
        
        public void SetWall(Wall wall, bool value)
        {
            switch (wall)
            {
                case Wall.Left:
                    _leftWall = value;
                    break;
                case Wall.Right:
                    _rightWall = value;
                    break;
                case Wall.Top:
                    _topWall = value;
                    break;
                case Wall.Bottom:
                    _bottomWall = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(wall), wall, null);
            }
        }
        
        public int GetCorner(Corner corner)
        {
            return corner switch
            {
                Corner.TopLeft => _topLeftCorner,
                Corner.TopRight => _topRightCorner,
                Corner.BottomLeft => _bottomLeftCorner,
                Corner.BottomRight => _bottomRightCorner,
                _ => 0
            };
        }
        
        public void SetCorner(Corner corner, int value)
        {
            switch (corner)
            {
                case Corner.TopLeft:
                    _topLeftCorner = value;
                    break;
                case Corner.TopRight:
                    _topRightCorner = value;
                    break;
                case Corner.BottomLeft:
                    _bottomLeftCorner = value;
                    break;
                case Corner.BottomRight:
                    _bottomRightCorner = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(corner), corner, null);
            }
        }
        
        public bool GetVisited()
        {
            return _visited;
        }
        
        public void SetVisited(bool value)
        {
            _visited = value;
        }
        
        public List<MazeCell> GetNeighbours()
        {
            return _neighbours;
        }
        
        public void AddNeighbour(MazeCell mazeCell)
        {
            _neighbours.Add(mazeCell);
        }
    }
}

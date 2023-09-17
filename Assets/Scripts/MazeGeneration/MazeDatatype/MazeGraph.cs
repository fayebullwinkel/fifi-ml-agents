using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MazeDatatype
{
    public class MazeGraph
    {
        public int Width { get; }
        public int Height { get; }

        public MazeCell[,] Cells { get; }
        public List<MazeWall> Walls { get; }
        public List<MazeCorner> Corners { get; }
        
        private MazeManager mazeManager = MazeManager.Singleton;
        
        public MazeCell StartCell { get; private set; }
        public MazeCell EndCell { get; private set; }

        public MazeGraph(int width, int height)
        {
            Width = width;
            Height = height;
            Cells = new MazeCell[width, height];
            Walls = new List<MazeWall>();
            Corners = new List<MazeCorner>();
            
            // Initialize the maze cells
            for (var x = 0; x < Width; x++)
            {
                for (var z = 0; z < Height; z++)
                {
                    Cells[x, z] = new MazeCell(x, z);
                }
            }
            
            InitializeNeighbours();
            InitializeWalls();
            InitializeCorners();
        }
        
        private void InitializeNeighbours()
        {
            for (var x = 0; x < Width; x++)
            {
                for (var z = 0; z < Height; z++)
                {
                    var cell = Cells[x, z];
                    // if the cell is not on the left edge, add the cell to the left as a neighbour
                    if (x > 0)
                    {
                        cell.Neighbours.Add(Cells[x - 1, z]);
                    }
                    // if the cell is not on the right edge, add the cell to the right as a neighbour
                    if (x < Width - 1)
                    {
                        cell.Neighbours.Add(Cells[x + 1, z]);
                    }
                    // if the cell is not on the bottom edge, add the cell to the bottom as a neighbour
                    if (z > 0)
                    {
                        cell.Neighbours.Add(Cells[x, z - 1]);
                    }
                    // if the cell is not on the top edge, add the cell to the top as a neighbour
                    if (z < Height - 1)
                    {
                        cell.Neighbours.Add(Cells[x, z + 1]);
                    }
                }
            }
        }

        private void InitializeWalls()
        {
            var wallParent = new GameObject("wallParent");
            wallParent.transform.parent = mazeManager.transform;
            wallParent.transform.localPosition = new Vector3(0, 0.5f, 0);
            
            for (var x = 0; x < Width; x++)
            {
                for (var z = 0; z < Height; z++)
                {
                    var cell = Cells[x, z];
                    // if the cell is not on the left edge, add the left wall
                    if (x > 0)
                    {
                        PlaceWall(cell, WallOrientation.Left, WallType.Vertical, wallParent);
                    }
                    // if the cell is not on the right edge, add the right wall
                    if (x < Width - 1)
                    {
                        PlaceWall(cell, WallOrientation.Right, WallType.Vertical, wallParent);
                    }
                    // if the cell is not on the bottom edge, add the bottom wall
                    if (z > 0)
                    {
                        PlaceWall(cell, WallOrientation.Bottom, WallType.Horizontal, wallParent);
                    }
                    // if the cell is not on the top edge, add the top wall
                    if (z < Height - 1)
                    {
                        PlaceWall(cell, WallOrientation.Top, WallType.Horizontal, wallParent);
                    }
                }
            }
        }
        
        private void InitializeCorners()
        {
            for (var x = 0; x < Width - 1; x++)
            {
                for (var z = 0; z < Height - 1; z++)
                {
                    var cells = new List<MazeCell>
                    {
                        Cells[x, z],
                        Cells[x + 1, z],
                        Cells[x, z + 1],
                        Cells[x + 1, z + 1]
                    };
                    // find all walls that are in the corner
                    var walls = new List<MazeWall>();
                    walls.Add(Walls.Find(x => x.Cells.Contains(cells[0]) && x.Cells.Contains(cells[1])));
                    walls.Add(Walls.Find(x => x.Cells.Contains(cells[0]) && x.Cells.Contains(cells[2])));
                    walls.Add(Walls.Find(x => x.Cells.Contains(cells[1]) && x.Cells.Contains(cells[3])));
                    walls.Add(Walls.Find(x => x.Cells.Contains(cells[2]) && x.Cells.Contains(cells[3])));
                    
                    var corner = new MazeCorner(cells, walls);
                    Corners.Add(corner);
                    
                    Cells[x, z].TopRightCorner = corner;
                    Cells[x + 1, z].TopLeftCorner = corner;
                    Cells[x, z + 1].BottomRightCorner = corner;
                    Cells[x + 1, z + 1].BottomLeftCorner = corner;
                }
            }
        }

        private void PlaceWall(MazeCell currentCell, WallOrientation orientation, WallType wallType,
            GameObject wallParent)
        {
            var neighborCell = GetNeighborCell(currentCell, orientation);
            var position = GetWallPosition(currentCell, orientation);
            var existingWall = GetExistingWall(position);

            if (existingWall != null)
            {
                return;
            }
            var wallObject = Object.Instantiate(mazeManager.wallPrefab, wallParent.transform);
            wallObject.transform.localPosition = position;
            wallObject.transform.localScale = GetWallScale(wallType);
            var wall = wallObject.GetComponent<MazeWall>();
            var cells = new List<MazeCell> {currentCell, neighborCell};
            wall.InitMazeWall(wallType, cells);
            currentCell.AddWall(wall);
            neighborCell.AddWall(wall);
            Walls.Add(wall);
        }
        
        public MazeWall GetExistingWall(Vector3 position)
        {
            return Walls.Find(x => x.gameObject.transform.localPosition == position);
        }
        
        private Vector3 GetWallPosition(MazeCell cell, WallOrientation wall)
        {
            var cellSize = mazeManager.GetCellSize();
            float xOffset = 0;
            float zOffset = 0;

            switch (wall)
            {
                case WallOrientation.Top:
                    zOffset = cellSize / 2;
                    break;
                case WallOrientation.Bottom:
                    zOffset = -cellSize / 2;
                    break;
                case WallOrientation.Left:
                    xOffset = -cellSize / 2;
                    break;
                case WallOrientation.Right:
                    xOffset = cellSize / 2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(wall), wall, null);
            }

            return new Vector3(cell.X * cellSize + xOffset, 0, cell.Z * cellSize + zOffset);
        }
        
        private Vector3 GetWallScale(WallType type)
        {
            return type == WallType.Horizontal ? new Vector3(mazeManager.GetCellSize(), 1, 0.1f) : new Vector3(0.1f, 1, mazeManager.GetCellSize());
        }

        private MazeCell GetNeighborCell(MazeCell currentCell, WallOrientation orientation)
        {
            var (x, z) = GetNeighborCoordinates(currentCell.X, currentCell.Z, orientation);
            var cell = Cells[x, z];
            return cell;
        }

        private (int, int) GetNeighborCoordinates(int x, int z, WallOrientation orientation)
        {
            switch (orientation)
            {
                case WallOrientation.Left:
                    return (x - 1, z);
                case WallOrientation.Right:
                    return (x + 1, z);
                case WallOrientation.Bottom:
                    return (x, z - 1);
                case WallOrientation.Top:
                    return (x, z + 1);
                default:
                    throw new ArgumentException("Invalid wall orientation");
            }
        }

        public void RemoveWall(MazeWall wall)
        {
            Walls.Remove(wall);
            // Remove wall from cells
            foreach (var cell in wall.Cells)
            {
                cell.Walls.Remove(wall);
            }
            // Remove wall from corners
            var corner = Corners.FindAll(x => x.Walls.Contains(wall));
            foreach (var c in corner)
            {
                c.Walls.Remove(wall);
            }
        }
        
        public void MarkCellVisited(int x, int z)
        {
            Cells[x, z].Visited = true;
        }

        public MazeCell GetCellFromAgentPosition(Vector3 position)
        {
            var cellSize = mazeManager.GetCellSize();
            foreach (var cell in Cells)
            {
                var minX = cell.X * cellSize - cellSize / 2;
                var maxX = cell.X * cellSize + cellSize / 2;
                var isInRangeX = minX <= position.x && position.x <= maxX;
                var minZ = cell.Z * cellSize - cellSize / 2;
                var maxZ = cell.Z * cellSize + cellSize / 2;
                var isInRangeZ = minZ <= position.z && position.z <= maxZ;
                if (isInRangeX && isInRangeZ)
                {
                    return cell;
                }
            }
            return null;
        }
        
        public void PlaceStart(Vector3 position)
        {
            var cell = GetCellFromAgentPosition(position);
            if (cell != null && StartCell == null)
            {
                StartCell = cell;
                var cellSize = mazeManager.GetCellSize();
                var startCellObject = Object.Instantiate(mazeManager.startCellPrefab, mazeManager.transform);
                startCellObject.transform.localPosition = new Vector3(cell.X * cellSize, 0, cell.Z * cellSize);
                startCellObject.transform.localScale = new Vector3(cellSize, 0.01f, cellSize);
            }
        }
        
        public void PlaceGoal(Vector3 position)
        {
            var cell = GetCellFromAgentPosition(position);
            if (cell != null && EndCell == null && StartCell != cell)
            {
                EndCell = cell;
                var cellSize = mazeManager.GetCellSize();
                var goalCellObject = Object.Instantiate(mazeManager.goalCellPrefab, mazeManager.transform);
                goalCellObject.transform.localPosition = new Vector3(cell.X * cellSize, 0, cell.Z * cellSize);
                goalCellObject.transform.localScale = new Vector3(cellSize, 0.01f, cellSize);
                
                Debug.Log("Maze is valid: " + IsMazeValid());
            }
        }

        public bool IsMazeValid()
        {
            // Check if all cells are visited -> maze is connected
            if (Cells.Cast<MazeCell>().Any(cell => !cell.Visited))
            {
                return false;
            }
            // Check if maze has a start and end cell
            if (StartCell == null || EndCell == null)
            {
                return false;
            }
            // Check if there is a path from start to end cell -> maze is solvable
            var path = FindPath(StartCell, EndCell);
            return path.Contains(StartCell) && path.Contains(EndCell);
        }
        
        // Find a path from start to end cell using the breadth-first search algorithm
        public List<MazeCell> FindPath(MazeCell startCell, MazeCell endCell)
        {
            var queue = new Queue<MazeCell>();
            var visited = new HashSet<MazeCell>();
            var parent = new Dictionary<MazeCell, MazeCell>();
            queue.Enqueue(startCell);
            visited.Add(startCell);
            while (queue.Count > 0)
            {
                var currentCell = queue.Dequeue();
                if (currentCell == endCell)
                {
                    break;
                }
                foreach (var neighbour in currentCell.Neighbours)
                {
                    if (!visited.Contains(neighbour))
                    {
                        // check if there is a wall between the current cell and the neighbour
                        var wall = Walls.Find(x => x.Cells.Contains(currentCell) && x.Cells.Contains(neighbour));
                        if (wall != null)
                        {
                            continue;
                        }
                        queue.Enqueue(neighbour);
                        visited.Add(neighbour);
                        parent[neighbour] = currentCell;
                    }
                }
            }
            var path = new List<MazeCell>();
            var cell = endCell;
            while (cell != startCell)
            {
                path.Add(cell);
                cell = parent[cell];
            }
            path.Add(startCell);
            path.Reverse();
            return path;
        }
        
        public bool MazeMeetsRequirements()
        {
            // Check if all corners have at least one wall -> maze is not empty
            if (Corners.Any(corner => corner.Walls.Count == 0))
            {
                return false;
            }
            // TODO: add more requirements
            return true;
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

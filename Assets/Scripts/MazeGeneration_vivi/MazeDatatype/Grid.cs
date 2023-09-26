﻿using System;
using System.Collections.Generic;
using System.Linq;
using MazeGeneration_vivi.MazeDatatype.Enums;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MazeGeneration_vivi.MazeDatatype
{
    public class Grid
    {
        private int Size { get; }
        public ECubeFace Face { get; }

        public MazeCell[,] Cells { get; }
        public List<MazeWall> Walls { get; }
        public List<MazeCorner> Corners { get; }

        public Maze Maze { get; }
        
        public MazeCell StartCell { get; private set; }
        public MazeCell EndCell { get; private set; }

        internal GameObject Parent;

        private PrefabCollection prefabCollection;

        public Grid(Maze maze, int size, GameObject parent, ECubeFace face = ECubeFace.None)
        {
            Maze = maze;
            Size = size;
            Face = face;
            Cells = new MazeCell[size, size];
            Walls = new List<MazeWall>();
            Corners = new List<MazeCorner>();
            Parent = parent;
            prefabCollection = Maze.prefabCollection;
            
            // Initialize the maze cells
            for (var x = 0; x < Size; x++)
            {
                for (var z = 0; z < Size; z++)
                {
                    Cells[x, z] = new MazeCell(x, z, this);
                }
            }
        }

        public void SetupGrid()
        {
            InitializeNeighbours();
            InitializeWalls();
            InitializeCorners();

            if (Maze.debugMode)
            {
                PrintAllCells();
                ShowAllCells();
            }
        }
        
        private void InitializeNeighbours()
        {
            for (var x = 0; x < Size; x++)
            {
                for (var z = 0; z < Size; z++)
                {
                    var cell = Cells[x, z];
                    // if the cell is not on the left edge, add the cell to the left as a neighbour
                    if (x > 0)
                    {
                        cell.AddNeighbour(Cells[x - 1, z]);
                    }
                    // if the cell is on the left edge, add the cell on the right edge from the left neighbour graph as a neighbour
                    else if (x == 0 && Maze.mazeType == EMazeType.ThreeDimensional)
                    {
                        var neighbourGraph = GetNeighbourGraph(this, EDirection.Left);
                        var neighbourCell = neighbourGraph.Cells[Size - 1, z];
                        cell.AddNeighbour(neighbourCell);
                    }
                    // if the cell is not on the right edge, add the cell to the right as a neighbour
                    if (x < Size - 1)
                    {
                        cell.AddNeighbour(Cells[x + 1, z]);
                    }
                    else if (x == Size - 1 && Maze.mazeType == EMazeType.ThreeDimensional)
                    {
                        var neighbourGraph = GetNeighbourGraph(this, EDirection.Right);
                        var neighbourCell = neighbourGraph.Cells[0, z];
                        cell.AddNeighbour(neighbourCell);
                    }
                    // if the cell is not on the bottom edge, add the cell to the bottom as a neighbour
                    if (z > 0)
                    {
                        cell.AddNeighbour(Cells[x, z - 1]);
                    }
                    else if (z == 0 && Maze.mazeType == EMazeType.ThreeDimensional)
                    {
                        var neighbourGraph = GetNeighbourGraph(this, EDirection.Bottom);
                        var neighbourCell = neighbourGraph.Cells[x, Size - 1];
                        cell.AddNeighbour(neighbourCell);
                    }
                    // if the cell is not on the top edge, add the cell to the top as a neighbour
                    if (z < Size - 1)
                    {
                        cell.AddNeighbour(Cells[x, z + 1]);
                    }
                    else if (z == Size - 1 && Maze.mazeType == EMazeType.ThreeDimensional)
                    {
                        var neighbourGraph = GetNeighbourGraph(this, EDirection.Top);
                        var neighbourCell = neighbourGraph.Cells[x, 0];
                        cell.AddNeighbour(neighbourCell);
                    }
                }
            }
        }

        private void InitializeWalls()
        {
            var wallParent = new GameObject("wallParent");
            wallParent.transform.parent = Parent.transform;
            wallParent.transform.localPosition = new Vector3(0, 0.5f, 0);
            
            for (var x = 0; x < Size; x++)
            {
                for (var z = 0; z < Size; z++)
                {
                    var cell = Cells[x, z];
                    PlaceWall(cell, EDirection.Left, WallType.Vertical, wallParent);
                    PlaceWall(cell, EDirection.Right, WallType.Vertical, wallParent);
                    PlaceWall(cell, EDirection.Bottom, WallType.Horizontal, wallParent);
                    PlaceWall(cell, EDirection.Top, WallType.Horizontal, wallParent);
                }
            }
        }
        
        private void InitializeCorners()
        {
            for (var x = 0; x < Size - 1; x++)
            {
                for (var z = 0; z < Size - 1; z++)
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
        
        private Grid GetNeighbourGraph(Grid graph, EDirection direction)
        {
            var thisFace = graph.Face;
            if (!NeighbourMap.Map.TryGetValue(thisFace, out var neighbourMap) ||
                !neighbourMap.TryGetValue(direction, out var neighbourFace))
            {
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            return Maze.Grids[neighbourFace];
        }

        private void PlaceWall(MazeCell currentCell, EDirection direction, WallType wallType,
            GameObject wallParent)
        {
            var neighborCell = GetNeighborCell(currentCell, direction);
            var position = GetWallPosition(currentCell, direction);
            var grid = currentCell.Grid;
            var existingWall = GetExistingWall(position);

            if (existingWall != null || neighborCell == null)
            {
                return;
            }
            var wallObject = Object.Instantiate(prefabCollection.wallPrefab, wallParent.transform);
            wallObject.transform.localPosition = position;
            wallObject.transform.localScale = GetWallScale(wallType);
            var wall = wallObject.GetComponent<MazeWall>();
            var cells = new List<MazeCell> {currentCell};
            wall.InitMazeWall(grid, wallType, cells);
            currentCell.AddWall(wall);
            neighborCell.AddWall(wall);
            Walls.Add(wall);
        }
        
        public MazeWall GetExistingWall(Vector3 position)
        {
            return Walls.Find(x => x.gameObject.transform.localPosition == position);
        }
        
        private Vector3 GetWallPosition(MazeCell cell, EDirection direction)
        {
            var cellSize = Maze.cellSize;
            float xOffset = 0;
            float zOffset = 0;

            switch (direction)
            {
                case EDirection.Top:
                    zOffset = cellSize / 2;
                    break;
                case EDirection.Bottom:
                    zOffset = -cellSize / 2;
                    break;
                case EDirection.Left:
                    xOffset = -cellSize / 2;
                    break;
                case EDirection.Right:
                    xOffset = cellSize / 2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            return new Vector3(cell.X * cellSize + xOffset, 0, cell.Z * cellSize + zOffset);
        }
        
        private Vector3 GetWallScale(WallType type)
        {
            return type == WallType.Horizontal ? new Vector3(Maze.cellSize, 1, 0.1f) : new Vector3(0.1f, 1, Maze.cellSize);
        }

        private MazeCell GetNeighborCell(MazeCell currentCell, EDirection direction)
        {
            var (x, z) = GetNeighborCoordinates(currentCell.X, currentCell.Z, direction);
            // check if coordinates are out of bounds
            if (x < 0 || x >= Size || z < 0 || z >= Size)
            {
                // if the maze is three dimensional, get the neighbour cell from the neighbour graph
                if (Maze.mazeType == EMazeType.ThreeDimensional)
                {
                    var neighbourGraphFace = GetNeighbourGraph(currentCell.Grid, direction).Face;
                    var neighbour = currentCell.Neighbours.Find(c => c.Grid.Face == neighbourGraphFace);
                    return neighbour;
                }
                // if the maze is two dimensional, return null
                return null;
            }
            var cell = Cells[x, z];
            return cell;
        }

        private (int, int) GetNeighborCoordinates(int x, int z, EDirection direction)
        {
            switch (direction)
            {
                case EDirection.Left:
                    return (x - 1, z);
                case EDirection.Right:
                    return (x + 1, z);
                case EDirection.Bottom:
                    return (x, z - 1);
                case EDirection.Top:
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

        public int GetVisitedCells()
        {
            return Cells.Cast<MazeCell>().Count(cell => cell.Visited);
        }

        public MazeCell GetCellFromPosition(Vector3 position)
        {
            var cellSize = Maze.cellSize;
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

        public Vector3 GetPositionFromCell(MazeCell cell)
        {
            var cellSize = Maze.cellSize;
            const float y = 0.5f;

            var x = cell.X * cellSize;
            var z = cell.Z * cellSize;

            return new Vector3(x, y, z);
        }
        
        public void PlaceStart(Vector3 position)
        {
            var cell = GetCellFromPosition(position);
            if (cell != null && StartCell == null)
            {
                StartCell = cell;
                var cellSize = Maze.cellSize;
                var startCellObject = Object.Instantiate(prefabCollection.startCellPrefab, Parent.transform);
                startCellObject.transform.localPosition = position;
                startCellObject.transform.localScale = new Vector3(cellSize, 0.01f, cellSize);
            }
        }
        
        public void PlaceGoal(Vector3 position)
        {
            var cell = GetCellFromPosition(position);
            if (cell != null && EndCell == null && StartCell != cell)
            {
                EndCell = cell;
                var cellSize = Maze.cellSize;
                var goalCellObject = Object.Instantiate(prefabCollection.goalCellPrefab, Parent.transform);
                goalCellObject.transform.localPosition = new Vector3(cell.X * cellSize, 0, cell.Z * cellSize);
                goalCellObject.transform.localScale = new Vector3(cellSize, 0.01f, cellSize);
                
                Debug.Log("Maze is valid: " + MazeIsValid());
            }
        }

        public bool MazeIsValid()
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
            var isSolvable = path.Count > 0 && path.First() == StartCell && path.Last() == EndCell;
            if (isSolvable)
            {
                ShowPath(path);
            }
            return isSolvable;
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
        
        // Finds the longest path from the start cell without an end cell using the breadth-first search algorithm
        public List<MazeCell> FindLongestPath(MazeCell startCell)
        {
            var queue = new Queue<MazeCell>();
            var visited = new HashSet<MazeCell>();
            var parent = new Dictionary<MazeCell, MazeCell>();
            queue.Enqueue(startCell);
            visited.Add(startCell);
            while (queue.Count > 0)
            {
                var currentCell = queue.Dequeue();
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
            // build a path for each cell in the dictionary from that cell to the start cell
            var paths = new List<List<MazeCell>>();
            foreach (var cell in parent.Keys)
            {
                if(cell == startCell)
                {
                    continue;
                }
                var path = new List<MazeCell>();
                var c = cell;
                while (c != startCell)
                {
                    path.Add(c);
                    c = parent[c];
                }
                path.Add(startCell);
                path.Reverse();
                paths.Add(path);
            }
            if(paths.Count == 0)
            {
                return new List<MazeCell>();
            }
            // return the longest path
            return paths.OrderByDescending(x => x.Count).First();
        }

        private void ShowPath(List<MazeCell> path)
        {
            if (!Maze.showPath)
            {
                return;
            }
            foreach (var cell in path)
            {
                // skip start and end cell
                if (cell == StartCell || cell == EndCell)
                {
                    continue;
                }
                var cellSize = Maze.cellSize;
                var pathCellObject = Object.Instantiate(prefabCollection.pathCellPrefab, Parent.transform);
                pathCellObject.transform.localPosition = new Vector3(cell.X * cellSize, 0, cell.Z * cellSize);
                pathCellObject.transform.localScale = new Vector3(cellSize, 0.01f, cellSize);
            }
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
        
        public float GetPercentageOfVisitedCells()
        {
            return (float) GetVisitedCells() / (Size * Size);
        }
        
        public float GetPercentageOfLongestPath()
        {
            var longestPath = FindLongestPath(StartCell);
            return (float) longestPath.Count / (Size * Size);
        }
        
        public void PrintAllCells()
        {
            foreach (var cell in Cells)
            {
                Debug.Log($"------- Face: {Face} -------");
                Debug.Log($"Cell: ({cell.X}, {cell.Z})");

                // Print neighbors
                foreach (var neighbor in cell.Neighbours)
                {
                    Debug.Log($"Neighbor: ({neighbor.X}, {neighbor.Z}), Face: {neighbor.Grid.Face}");
                }
            }
        }
        
        public void ShowAllCells()
        {
            foreach (var cell in Cells)
            {
                var position = GetPositionFromCell(cell);
                var cellObject = Object.Instantiate(prefabCollection.cellWithCoordinatesPrefab, Parent.transform);
                cellObject.transform.localPosition = position;
                cellObject.transform.localScale = new Vector3(Maze.cellSize, 0.01f, Maze.cellSize);

                var textMesh = cellObject.GetComponentInChildren<TextMeshPro>();
                textMesh.text = $"({cell.X}, {cell.Z})";
                textMesh.alignment = TextAlignmentOptions.Center;
            }
        }
    }
}

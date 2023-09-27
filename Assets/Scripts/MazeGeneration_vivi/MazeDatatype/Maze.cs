using System;
using System.Collections.Generic;
using System.Linq;
using MazeGeneration_vivi.MazeDatatype.Enums;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MazeGeneration_vivi.MazeDatatype
{
    public class Maze : MonoBehaviour
    {
        [Header("Maze Settings")]
        [Tooltip("If TwoDimensional, the maze will be generated on a plane. If ThreeDimensional, the maze will be generated on a cube.")]
        public EMazeType mazeType;
        [Tooltip("The number of cells in vertical and horizontal direction on each face of the cube.")]
        public int size;
        [Tooltip("The size of each cell in the maze.")]
        public float cellSize;
        [Tooltip("If true, the path from start to goal will be shown.")]
        public bool showPath;
        [Tooltip("If true, the visited cells will be shown.")]
        public bool showVisitedCells;
        [Tooltip("If true, multiple debug things will be shown.")]
        public bool debugMode;

        [Header("Resources")]
        public PrefabCollection prefabCollection = null!;
        [SerializeField]
        private GameObject mainCamera = null!;
        [SerializeField]
        private GameObject agent2D = null!;
        [SerializeField]
        private GameObject agent3D = null!;

        public Dictionary<ECubeFace, Grid> Grids { get; private set; }
        public MazeCell StartCell { get; private set; }
        public GameObject StartCellObject { get; private set; }
        public MazeCell EndCell { get; private set; }
        public GameObject EndCellObject { get; private set; }
        private GameObject maze;
        private GameObject cube;
        private GameObject agent;

        private void Awake()
        {
            Grids = new Dictionary<ECubeFace, Grid>();
            cube = transform.GetComponentInChildren<BoxCollider>().gameObject;
            maze = gameObject;
            agent = mazeType == EMazeType.ThreeDimensional ? agent3D : agent2D;
            agent.SetActive(true);
            var otherAgent = mazeType == EMazeType.ThreeDimensional ? agent2D : agent3D;
            otherAgent.SetActive(false);
            
            SetCameraPosition();

            // GenerateMaze();
            // PlaceAgent();
        }

        public void GenerateMaze()
        {
            switch (mazeType)
            {
                case EMazeType.TwoDimensional:
                    Generate2DMaze();
                    break;
                case EMazeType.ThreeDimensional:
                    Generate3DMaze();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Generate2DMaze()
        {
            // Position and scale the cube
            cube.transform.localScale = new Vector3(size * cellSize, 0.001f, size * cellSize);
            
            // Generate a grid
            var gridParent = new GameObject("Grid");
            gridParent.transform.SetParent(maze.transform);
            gridParent.tag = "Grid";
            var grid = new Grid(this, size, gridParent);
            Grids.Add(ECubeFace.None, grid);
            
            grid.SetupGrid();
            PositionGrid(grid);

            SetOuterWalls();
        }
        
        private void Generate3DMaze()
        {
            // Position and scale the cube
            cube.transform.localScale = new Vector3(size * cellSize, size * cellSize, size * cellSize);
            
            // Generate a grid for each face of the cube
            foreach (ECubeFace face in Enum.GetValues(typeof(ECubeFace)))
            {
                if (face.Equals(ECubeFace.None))
                {
                    continue;
                }
                var gridParent = new GameObject("Grid" + face);
                gridParent.transform.SetParent(maze.transform);
                gridParent.tag = "Grid";
                var grid = new Grid(this, size, gridParent, face);
                Grids.Add(face, grid);
            }
            foreach (var grid in Grids.Values)
            {
                grid.SetupGrid();
                PositionGrid(grid);
            }
        }

        private void PositionGrid(Grid grid)
        {
            var gridParent = grid.Parent;
            var posOffset = size * cellSize / 2;
            switch (grid.Face)
            {
                case ECubeFace.None:
                    break;
                case ECubeFace.Front:
                    gridParent.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                    gridParent.transform.localPosition = new Vector3(0, 0, -posOffset);
                    break;
                case ECubeFace.Back:
                    gridParent.transform.localRotation = Quaternion.Euler(-90, 0, 180);
                    gridParent.transform.localPosition = new Vector3(0, 0, posOffset);
                    break;
                case ECubeFace.Left:
                    gridParent.transform.localRotation = Quaternion.Euler(-90, 0, 90);
                    gridParent.transform.localPosition = new Vector3(-posOffset, 0, 0);
                    break;
                case ECubeFace.Right:
                    gridParent.transform.localRotation = Quaternion.Euler(-90, 0, -90);
                    gridParent.transform.localPosition = new Vector3(posOffset, 0, 0);
                    break;
                case ECubeFace.Top:
                    gridParent.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    gridParent.transform.localPosition = new Vector3(0, posOffset, 0);
                    break;
                case ECubeFace.Bottom:
                    gridParent.transform.localRotation = Quaternion.Euler(180, 0, 0);
                    gridParent.transform.localPosition = new Vector3(0, -posOffset, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetOuterWalls()
        {
            var outerWalls = Instantiate(prefabCollection.mazeBoundsPrefab, maze.transform);
            var scale = outerWalls.transform.localScale;
            scale = new Vector3(scale.x * cellSize * size, scale.y, scale.z * cellSize * size);
            outerWalls.transform.localScale = scale;
            var position = outerWalls.transform.localPosition;
            outerWalls.transform.localPosition = position;
        }

        private void SetCameraPosition()
        {
            var pos = mainCamera.transform.localPosition;
            pos.x = size - 1;
            pos.y *= size * cellSize / 2;
            pos.z *= size * cellSize / 2 + 1;
            mainCamera.transform.localPosition = pos;
            switch (mazeType)
            {
                case EMazeType.TwoDimensional:
                    // mainCamera.transform.localPosition = new Vector3(size - cellSize / 2, (size - cellSize / 2) * 2, -cellSize / 2);
                    break;
                case EMazeType.ThreeDimensional:
                    var cams = GameObject.FindGameObjectsWithTag("CubeViewCamera");
                    foreach (var cam in cams)
                    {
                        var position = cam.transform.localPosition;
                        position.x *= size * cellSize / 2;
                        position.y *= size * cellSize / 2;
                        position.z *= size * cellSize / 2;
                        cam.transform.localPosition = position;
                        var c = cam.GetComponent<Camera>();
                        c.orthographicSize = size * cellSize / 2;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void PlaceAgent()
        {
            // get random grid from Grids
            var gridIndex = mazeType == EMazeType.ThreeDimensional
                ? Random.Range(1, Enum.GetValues(typeof(ECubeFace)).Length)
                : 0;

            var cubeFace = (ECubeFace)gridIndex;
            
            var grid = Grids[cubeFace];
            var randomX = Random.Range(0, size);
            var randomZ = Random.Range(0, size);
            var randomCell = grid.Cells[randomX, randomZ];
            var position = grid.GetPositionFromCell(randomCell);

            agent.transform.SetParent(grid.Parent.transform);
            agent.transform.localPosition = position;
            agent.GetComponent<MazeGenerationAgent>().Grid = grid;
            agent.transform.parent = grid.Parent.transform;
            
            Debug.Log("Placed agent at " + position + " in grid " + cubeFace + " at cell " + randomX + ", " + randomZ + ".");
                
            PlaceStart(position);
            grid.MarkCellVisited(randomX, randomZ);
        }
        
        public void PlaceStart(Vector3 position)
        {
            var grid = agent.GetComponent<MazeGenerationAgent>().Grid;
            var cell = grid.GetCellFromPosition(position); 
            if (cell != null)
            {
                if (StartCell == null)
                {
                    StartCellObject = Instantiate(prefabCollection.startCellPrefab, grid.Parent.transform);
                }
                StartCellObject.transform.localPosition = position;
                StartCellObject.transform.localScale = new Vector3(cellSize, 0.01f, cellSize);
                StartCell = cell;
            }
        }
        
        public void PlaceGoal(Vector3 position)
        {
            var grid = agent.GetComponent<MazeGenerationAgent>().Grid;
            var cell = grid.GetCellFromPosition(position); 
            if (cell != null)
            {
                if (EndCell == null)
                {
                    EndCellObject = Instantiate(prefabCollection.startCellPrefab, grid.Parent.transform);
                }
                EndCellObject.transform.localPosition = position;
                EndCellObject.transform.localScale = new Vector3(cellSize, 0.01f, cellSize);
                EndCell = cell;
                
                Debug.Log("Maze is solvable: " + MazeIsValid());
            }
        }

        public void ClearMaze()
        {
            var grids = GameObject.FindGameObjectsWithTag("Grid");
            foreach (var grid in grids)
            {
                Destroy(grid);
            }
            
            StartCell = null;
            EndCell = null;
            Grids = new Dictionary<ECubeFace, Grid>();
        }
        
        #region MazeValidationMethods
        
        public bool MazeIsValid()
        {
            // Check if all cells are visited -> maze is connected
            if (Grids.Values.Any(grid => grid.GetVisitedCells() != grid.Cells.Length))
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
        
        public bool MazeMeetsRequirements()
        {
            // Check if all corners have at least one wall -> maze is not empty
            if (Grids.Values.Any(grid => grid.Corners.Any(corner => corner.Walls.Count == 0)))
            {
                return false;
            }
            // TODO: add more requirements
            return true;
        }
        
        #endregion
        
        #region PathFindingMethods
        
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
                        var grid = currentCell.Grid;
                        var neighbourGrid = neighbour.Grid;
                        var wall = grid.Walls.Find(x => x.Cells.Contains(currentCell) && x.Cells.Contains(neighbour));
                        var neighbourWall = neighbourGrid.Walls.Find(x => x.Cells.Contains(currentCell) && x.Cells.Contains(neighbour));
                        if (wall != null || neighbourWall != null)
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
                        var grid = currentCell.Grid;
                        var neighbourGrid = neighbour.Grid;
                        var wall = grid.Walls.Find(x => x.Cells.Contains(currentCell) && x.Cells.Contains(neighbour));
                        var neighbourWall = neighbourGrid.Walls.Find(x => x.Cells.Contains(currentCell) && x.Cells.Contains(neighbour));
                        if (wall != null || neighbourWall != null)
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
            if (!showPath)
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

                var grid = cell.Grid;
                var pathCellObject = Instantiate(prefabCollection.pathCellPrefab, grid.Parent.transform);
                pathCellObject.transform.localPosition = grid.GetPositionFromCell(cell);
                pathCellObject.transform.localScale = new Vector3(cellSize, 0.01f, cellSize);
            }
        }
        
        #endregion

        public int GetCellCount()
        {
            return size * size * 6;
        }
        
        public int GetVisitedCells() => Grids.Values.Sum(grid => grid.GetVisitedCells());
        
        public float GetPercentageOfVisitedCells()
        {
            return (float) GetVisitedCells() / GetCellCount();
        }
        
        public bool GetIsMazeEmpty() => Grids.Count == 0;
    }
}
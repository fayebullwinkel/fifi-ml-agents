using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MazeGeneration_vivi.MazeDatatype.Enums;
using TMPro;
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
        [Tooltip("If true, the maze requires all cells to be visited to be valid.")]
        public bool requireAllCellsToBeVisited;
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
        [SerializeField]
        private GameObject frontView = null!;
        [SerializeField]
        private GameObject backView = null!;
        [SerializeField]
        private GameObject leftView = null!;
        [SerializeField]
        private GameObject rightView = null!;
        [SerializeField]
        private GameObject topView = null!;
        [SerializeField]
        private GameObject bottomView = null!;

        public Dictionary<ECubeFace, Grid> Grids { get; private set; }
        public MazeCell StartCell { get; private set; }
        public GameObject StartCellObject { get; private set; }
        public MazeCell EndCell { get; private set; }
        public GameObject EndCellObject { get; private set; }
        public bool AgentIsMoving { get; set; }
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
            // var otherAgent = mazeType == EMazeType.ThreeDimensional ? agent2D : agent3D;
            // otherAgent.SetActive(false);
            
            SetCameraPosition();

            // GenerateMaze();
            // PlaceAgent();
        }

        #region GenerateMethods

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
                // get the VisitedCellsText object from the corresponding face
                var visitedCellsText = face switch
                {
                    ECubeFace.None => null,
                    ECubeFace.Front => frontView.transform.Find("VisitedCellsText").GetComponent<TextMeshProUGUI>(),
                    ECubeFace.Back => backView.transform.Find("VisitedCellsText").GetComponent<TextMeshProUGUI>(),
                    ECubeFace.Left => leftView.transform.Find("VisitedCellsText").GetComponent<TextMeshProUGUI>(),
                    ECubeFace.Right => rightView.transform.Find("VisitedCellsText").GetComponent<TextMeshProUGUI>(),
                    ECubeFace.Top => topView.transform.Find("VisitedCellsText").GetComponent<TextMeshProUGUI>(),
                    ECubeFace.Bottom => bottomView.transform.Find("VisitedCellsText").GetComponent<TextMeshProUGUI>(),
                    _ => throw new ArgumentOutOfRangeException()
                };

                var grid = new Grid(this, size, gridParent, face, visitedCellsText);
                Grids.Add(face, grid);
            }
            foreach (var grid in Grids.Values)
            {
                grid.SetupGrid();
                PositionGrid(grid);
            }
        }
        
        #endregion

        #region GeneralMethods

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
        
        #endregion

        #region PlacingMethods

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
            position.y = 0.5f;

            agent.transform.SetParent(grid.Parent.transform);
            agent.transform.localPosition = position;
            var mazeAgent = agent.GetComponent<MazeGenerationAgent>();
            mazeAgent.CurrentGrid = grid;
            mazeAgent.CurrentCell = randomCell;
            
            agent.transform.parent = grid.Parent.transform;
            
            // Debug.Log("Placed agent at " + position + " in grid " + cubeFace + " at cell " + randomX + ", " + randomZ + ".");
                
            position.y = 0;
            PlaceStart(position);
            MarkCellVisited(randomCell);
        }
        
        public void PlaceStart(Vector3 position)
        {
            var grid = agent.GetComponent<MazeGenerationAgent>().CurrentGrid;
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
                MarkCellVisited(cell);
            }
        }
        
        public void PlaceGoal(Vector3 position)
        {
            var grid = agent.GetComponent<MazeGenerationAgent>().CurrentGrid;
            var cell = grid.GetCellFromPosition(position); 
            // Goal can not be placed on the start cell
            if(cell == StartCell)
            {
                return;
            }
            if (cell != null)
            {
                if (EndCell == null)
                {
                    EndCellObject = Instantiate(prefabCollection.goalCellPrefab, grid.Parent.transform);
                }
                EndCellObject.transform.localPosition = position;
                EndCellObject.transform.localScale = new Vector3(cellSize, 0.01f, cellSize);
                EndCell = cell;
                
                // remove PathCell object if it exists
                var pathCell = grid.VisitedCellsPath.Find(x => x.transform.localPosition == position);
                if (pathCell != null)
                {
                    grid.VisitedCellsPath.Remove(pathCell);
                    Destroy(pathCell);
                }
                MarkCellVisited(cell);
                
                Debug.Log("Maze is Valid: " + IsValid());
            }
        }
        
        #endregion

        #region MovementMethods

        public void MoveAgent(EDirection direction)
        {
            AgentIsMoving = true;
            var mazeAgent = agent.GetComponent<MazeGenerationAgent>();
            var currentGrid = mazeAgent.CurrentGrid;
            var currentCell = currentGrid.GetCellFromPosition(agent.transform.localPosition);
            var nextCell = currentGrid.GetNeighborCell(currentCell, direction);
            var nextGrid = nextCell?.Grid;
    
            if (nextGrid == null)
            {
                return;
            }
    
            var nextPosition = nextGrid.GetPositionFromCell(nextCell);
            nextPosition.y = 0.5f;
    
            // set the parent of the agent to the next grid if it is not the same as the current grid
            if (currentGrid != nextGrid)
            {
                agent.transform.SetParent(nextGrid.Parent.transform);
                mazeAgent.CurrentGrid = nextGrid;
            }
            mazeAgent.CurrentCell = nextCell;
            StartCoroutine(MoveAgentCoroutine(nextPosition));
        }

        private IEnumerator MoveAgentCoroutine(Vector3 targetPosition)
        {
            var agentTransform = agent.transform;
            var startPosition = agentTransform.localPosition;
            var elapsedTime = 0f;
            var moveDuration = 1f; // Time to move to the center of the cell
    
            while (elapsedTime < moveDuration)
            {
                elapsedTime += Time.deltaTime;
                var t = elapsedTime / moveDuration;
                agentTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }
    
            // Ensure the agent is precisely at the target position
            agentTransform.localPosition = targetPosition;
            AgentIsMoving = false;
        }
        
        #endregion

        #region MazeMethods

        public void MarkCellVisited(MazeCell cell)
        {
            if(cell.Visited)
            {
                return;
            }
            cell.Visited = true;
            var grid = cell.Grid;
            grid.UpdateVisitedCellsText();
            if (!showVisitedCells)
            {
                return;
            }
            if (cell == StartCell || cell == EndCell)
            {
                return;
            }
            var position = grid.GetPositionFromCell(cell);
            if(grid.VisitedCellsPathParent == null)
            {
                grid.VisitedCellsPathParent = new GameObject("PathCellParent");
                grid.VisitedCellsPathParent.transform.parent = grid.Parent.transform;
                grid.VisitedCellsPathParent.transform.localPosition = Vector3.zero;
                grid.VisitedCellsPathParent.transform.localScale = Vector3.one;
                grid.VisitedCellsPathParent.transform.localRotation = Quaternion.identity;
            }
            var pathCellObject = Instantiate(prefabCollection.pathCellPrefab, grid.VisitedCellsPathParent.transform);
            pathCellObject.transform.localPosition = position;
            pathCellObject.transform.localScale = new Vector3(cellSize, 0.01f, cellSize);
            grid.VisitedCellsPath.Add(pathCellObject);
        }

        public void ClearMaze()
        {
            agent.transform.SetParent(null);
            foreach (var grid in Grids.Values)
            {
                grid.UpdateVisitedCellsText();
            }
            var grids = GameObject.FindGameObjectsWithTag("Grid");
            foreach (var grid in grids)
            {
                Destroy(grid);
            }
            StartCell = null;
            EndCell = null;
            Grids = new Dictionary<ECubeFace, Grid>();
        }
        
        #endregion
        
        #region MazeValidationMethods

        public bool IsFinished() => StartCell != null && EndCell != null;
        
        public bool IsValid()
        {
            if (requireAllCellsToBeVisited)
            {
                // Check if all cells are visited -> maze is connected
                if (Grids.Values.Any(grid => grid.GetVisitedCells() != grid.Cells.Length))
                {
                    return false;
                }
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
        
        public bool MeetsRequirements()
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
            // check if end cell is in parent dictionary -> there is a path from start to end cell
            if (!parent.ContainsKey(cell))
            {
                return path;
            }
            while (cell != startCell)
            {
                path.Add(cell);
                cell = parent[cell];
            }
            path.Add(startCell);
            path.Reverse();
            return path;
        }

        private void ShowPath(List<MazeCell> path)
        {
            if (!showPath)
            {
                return;
            }
            // delete all visited cells from every grid
            foreach (var grid in Grids.Values)
            {
                Destroy(grid.VisitedCellsPathParent);
                grid.VisitedCellsPath = new List<GameObject>();
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

        #region Getters

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
        
        #endregion
    }
}
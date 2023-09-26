using System;
using System.Collections.Generic;
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
        [SerializeField]
        [Tooltip("If true, the path from start to goal will be shown.")]
        internal bool showPath;
        [SerializeField]
        [Tooltip("If true, multiple debug things will be shown.")]
        internal bool debugMode;

        [Header("Resources")]
        public PrefabCollection prefabCollection = null!;
        [SerializeField]
        private GameObject mainCamera = null!;
        [SerializeField]
        private GameObject agent2D = null!;
        [SerializeField]
        private GameObject agent3D = null!;

        public Dictionary<ECubeFace, Grid> Grids { get; private set; }
        private GameObject maze;
        private GameObject cube;
        private GameObject agent;

        private void Awake()
        {
            Grids = new Dictionary<ECubeFace, Grid>();
            cube = transform.GetComponentInChildren<BoxCollider>().gameObject;
            maze = gameObject;
            // agent = cubeAgent ? agentCube : agentSphere;
            // agent.SetActive(true);
            // var otherAgent = cubeAgent ? agentSphere : agentCube;
            // otherAgent.SetActive(false);

            GenerateMaze();
            PlaceAgent();
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
            var grid = new Grid(this, size, gridParent);
            Grids.Add(ECubeFace.None, grid);
            
            grid.SetupGrid();
            PositionGrid(grid);

            SetOuterWalls();
            SetCameraPosition();
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
            mainCamera.transform.localPosition = new Vector3(size - cellSize / 2, (size - cellSize / 2) * 2, -cellSize / 2);
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
            // if (cubeFace is ECubeFace.Bottom or ECubeFace.Left or ECubeFace.Front)
            // {
            //     position.y = 1;
            // }

            // agent.transform.localPosition = position;
            var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.SetParent(grid.Parent.transform);
            c.transform.localPosition = position;
            
            Debug.Log("Placed agent at " + position + " in grid " + cubeFace + " at cell " + randomX + ", " + randomZ + ".");
                
            grid.MarkCellVisited(randomX, randomZ);
            grid.PlaceStart(position);
        }

        public void ClearMaze()
        {
            var grids = GameObject.FindGameObjectsWithTag("Grid");
            foreach (var grid in grids)
            {
                Destroy(grid);
            }
            
            Grids = new Dictionary<ECubeFace, Grid>();
        }

        public int GetCellCount()
        {
            return size * size * 6;
        }
    }
}
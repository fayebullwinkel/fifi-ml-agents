using System;
using System.Collections.Generic;
using MazeGeneration_vivi.MazeDatatype.Enums;
using UnityEngine;

namespace MazeGeneration_vivi.MazeDatatype
{
    public class Maze : MonoBehaviour
    {
        [Header("Maze Settings")]
        [Tooltip("If TwoDimensional, the maze will be generated on a plane. If ThreeDimensional, the maze will be generated on a cube.")]
        public EMazeType mazeType;
        [SerializeField]
        [Tooltip("The number of cells in vertical and horizontal direction on each face of the cube.")]
        private int size;
        [SerializeField]
        [Tooltip("The size of each cell in the maze.")]
        private float cellSize;
        [SerializeField]
        [Tooltip("If true, the path from start to goal will be shown.")]
        internal bool showPath;

        [Header("Resources")]
        public PrefabCollection prefabCollection = null!;
        [SerializeField]
        private GameObject mainCamera = null!;
        [SerializeField]
        private GameObject agent2D = null!;
        [SerializeField]
        private GameObject agent3D = null!;

        public Dictionary<ECubeFace, Grid> Grids { get; private set; }

        private GameObject cube;

        private GameObject agent;

        private GameObject maze;

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
            cube.transform.localPosition = new Vector3(size - cellSize / 2, 0, size - cellSize / 2);
            cube.transform.localScale = new Vector3(size * cellSize, 0.001f, size * cellSize);
            
            // Generate a grid for the top face of the cube
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
            cube.transform.localPosition = new Vector3(size - cellSize / 2, 0, size - cellSize / 2);
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
            switch (grid.Face)
            {
                case ECubeFace.None:
                    break;
                case ECubeFace.Front:
                    gridParent.transform.localRotation = Quaternion.Euler(90, 0, 0);
                    gridParent.transform.localPosition = new Vector3(0, size * cellSize / 2 - 1, -2);
                    break;
                case ECubeFace.Back:
                    gridParent.transform.localRotation = Quaternion.Euler(90, 0, 0);
                    gridParent.transform.localPosition = new Vector3(0, size * cellSize / 2 - 1, size * cellSize - 1);
                    break;
                case ECubeFace.Left:
                    gridParent.transform.localRotation = Quaternion.Euler(90, 90, 0);
                    gridParent.transform.localPosition = new Vector3(-2, size * cellSize / 2 - 1, size * cellSize - 2);
                    break;
                case ECubeFace.Right:
                    gridParent.transform.localRotation = Quaternion.Euler(90, 90, 0);
                    gridParent.transform.localPosition = new Vector3(size * cellSize - 1, size * cellSize / 2 - 1,
                        size * cellSize - 2);
                    break;
                case ECubeFace.Top:
                    gridParent.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    gridParent.transform.localPosition = new Vector3(0, size * cellSize / 2, 0);
                    break;
                case ECubeFace.Bottom:
                    gridParent.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    gridParent.transform.localPosition = new Vector3(0, -size * cellSize / 2 - 1, 0);
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
            position = new Vector3(size - cellSize / 2, position.y, size - cellSize / 2);
            outerWalls.transform.localPosition = position;
        }

        private void SetCameraPosition()
        {
            mainCamera.transform.localPosition = new Vector3(size - cellSize / 2, (size - cellSize / 2) * 2, -cellSize / 2);
        }

        // public void PlaceAgent()
        // {
        //     var randomX = Random.Range(0, xSize);
        //     var randomZ = Random.Range(0, zSize);
        //
        //     agent.transform.localPosition =
        //         new Vector3( randomX * cellSize, 0.6f, randomZ * cellSize);
        //     mazeGraph.MarkCellVisited(randomX, randomZ);
        //     mazeGraph.PlaceStart(agent.transform.localPosition);
        // }

        public void ClearMaze()
        {
            // TODO: clear maze
        }

        public int GetCellSize()
        {
            return (int)cellSize;
        }

        public int GetCellCount()
        {
            return size * size * 6;
        }
    }
}
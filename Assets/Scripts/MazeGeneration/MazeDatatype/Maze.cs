using System;
using System.Collections.Generic;
using MazeDatatype.Enums;
using UnityEngine;
using Color = UnityEngine.Color;
using Grid = MazeDatatype.Grid;

public class Maze : MonoBehaviour
{
    [Header("Maze Settings")]
    [Tooltip("If TwoDimensional, the maze will be generated on a plane. If ThreeDimensional, the maze will be generated on a cube.")]
    public EMazeType mazeType;

    [SerializeField]
    [Tooltip("The number of cells in vertical and horizontal direction on each face of the cube.")]
    private int size;

    [SerializeField]
    private float cellSize;

    [SerializeField]
    internal bool showPath;

    [Header("Resources")]
    [SerializeField]
    internal GameObject wallPrefab;

    [SerializeField]
    private GameObject mazeBoundsPrefab;

    [SerializeField]
    internal GameObject startCellPrefab;

    [SerializeField]
    internal GameObject goalCellPrefab;

    [SerializeField]
    internal GameObject pathCellPrefab;

    [SerializeField]
    private GameObject camera;

    [SerializeField]
    private GameObject agentSphere = null!;
    [SerializeField]
    private GameObject agentCube = null!;

    public static Maze Singleton { get; private set; } = null!;

    public Grid Grid { get; private set; }

    public Dictionary<ECubeFace, Grid> cubeGraphs { get; private set; }

    private GameObject cube;

    public GameObject OuterWalls { get; private set; } = null!;

    private GameObject agent;

    private GameObject maze;

    private void Awake()
    {
        Singleton = this;
        cubeGraphs = new Dictionary<ECubeFace, Grid>();
        // agent = cubeAgent ? agentCube : agentSphere;
        // agent.SetActive(true);
        // var otherAgent = cubeAgent ? agentSphere : agentCube;
        // otherAgent.SetActive(false);

        GenerateMaze();
    }

    public void GenerateMaze()
    {
        maze = new GameObject("Maze");
        switch (mazeType)
        {
            case EMazeType.TwoDimensional:
                var grid = new GameObject("Grid");
                grid.transform.SetParent(maze.transform);
                Grid = new Grid(size, grid);
                Grid.SetupMazeGraph();
                SetOuterWalls();
                SetCameraPosition();
                break;
            case EMazeType.ThreeDimensional:
                // Generate a cube where each face is a grid with cells and walls in horizontal and vertical direction
                cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.SetParent(maze.transform);
                cube.GetComponent<MeshRenderer>().material.color = Color.grey;
                var position = new Vector3(size - cellSize / 2, 0, size - cellSize / 2);
                cube.transform.localPosition = position;
                cube.transform.localScale = new Vector3(size * cellSize, size * cellSize, size * cellSize);
                // Generate a grid for each face of the cube
                foreach (ECubeFace face in Enum.GetValues(typeof(ECubeFace)))
                {
                    if (face.Equals(ECubeFace.None))
                    {
                        continue;
                    }
                    var faceGrid = new GameObject("Grid" + face);
                    faceGrid.transform.SetParent(maze.transform);
                    faceGrid.transform.localPosition = Vector3.zero;
                    faceGrid.transform.localScale = Vector3.one;
                    var graph = new Grid(size, faceGrid, face);
                    cubeGraphs.Add(face, graph);
                }
                foreach (var graph in cubeGraphs.Values)
                {
                    graph.SetupMazeGraph();
                }
                PositionFaceGrids();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void PositionFaceGrids()
    {
        foreach (var cubeFace in cubeGraphs.Keys)
        {
            var graph = cubeGraphs[cubeFace];
            var mazeParent = graph.MazeParent;
            switch (graph.Face)
            {
                case ECubeFace.None:
                    break;
                case ECubeFace.Front:
                    mazeParent.transform.localRotation = Quaternion.Euler(90, 0, 0);
                    mazeParent.transform.localPosition = new Vector3(0, size * cellSize / 2 - 1, -2);
                    break;
                case ECubeFace.Back:
                    mazeParent.transform.localRotation = Quaternion.Euler(90, 0, 0);
                    mazeParent.transform.localPosition = new Vector3(0, size * cellSize / 2 - 1, size * cellSize - 1);
                    break;
                case ECubeFace.Left:
                    mazeParent.transform.localRotation = Quaternion.Euler(90, 90, 0);
                    mazeParent.transform.localPosition = new Vector3(-2, size * cellSize / 2 - 1, size * cellSize - 2);
                    break;
                case ECubeFace.Right:
                    mazeParent.transform.localRotation = Quaternion.Euler(90, 90, 0);
                    mazeParent.transform.localPosition = new Vector3(size * cellSize - 1, size * cellSize / 2 - 1,
                        size * cellSize - 2);
                    break;
                case ECubeFace.Top:
                    mazeParent.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    mazeParent.transform.localPosition = new Vector3(0, size * cellSize / 2, 0);
                    break;
                case ECubeFace.Bottom:
                    mazeParent.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    mazeParent.transform.localPosition = new Vector3(0, -size * cellSize / 2 - 1, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void SetOuterWalls()
    {
        OuterWalls = Instantiate(mazeBoundsPrefab, maze.transform);
        var scale = OuterWalls.transform.localScale;
        scale = new Vector3(scale.x * cellSize * size, scale.y, scale.z * cellSize * size);
        OuterWalls.transform.localScale = scale;
        var position = OuterWalls.transform.localPosition;
        position = new Vector3(size - cellSize / 2, position.y, size - cellSize / 2);
        OuterWalls.transform.localPosition = position;
    }

    private void SetCameraPosition()
    {
        camera.transform.localPosition = new Vector3(size - cellSize / 2, (size - cellSize / 2) * 2, -cellSize / 2);
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
        if (maze != null)
        {
            Destroy(maze);
        }

        Grid = null!;
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
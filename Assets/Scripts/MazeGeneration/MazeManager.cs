using System.Collections.Generic;
using System.Linq;
using MazeDatatype;
using Unity.MLAgents.Policies;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MazeManager : MonoBehaviour
{
    [Header("Maze Settings")]
    
    [Tooltip("If true, the agent will be a cube. If false, the agent will be a sphere.")]
    public bool cubeAgent;

    [SerializeField]
    private int xSize;

    [SerializeField]
    private int zSize;

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

    public static MazeManager Singleton { get; private set; } = null!;
    
    public MazeGraph mazeGraph { get; private set; }
    
    public GameObject OuterWalls { get; private set; } = null!;
    
    private GameObject agent;

    private GameObject maze;

    private void Awake()
    {
        Singleton = this;
        agent = cubeAgent ? agentCube : agentSphere;
        agent.SetActive(true);
        var otherAgent = cubeAgent ? agentSphere : agentCube;
        otherAgent.SetActive(false);
    }

    public void GenerateGrid()
    {
        // Create a graph with walls in horizontal and vertical direction
        maze = new GameObject("Maze");
        mazeGraph = new MazeGraph(xSize, zSize, maze);
        SetOuterWalls();
        SetCameraPosition();
    }

    private void SetOuterWalls()
    {
        OuterWalls = Instantiate(mazeBoundsPrefab, maze.transform);
        var size = OuterWalls.transform.localScale;
        size = new Vector3(size.x * cellSize * xSize, size.y, size.z * cellSize * zSize);
        OuterWalls.transform.localScale = size;
        var position = OuterWalls.transform.localPosition;
        position = new Vector3(xSize - cellSize / 2, position.y, zSize - cellSize / 2);
        OuterWalls.transform.localPosition = position;
    }

    private void SetCameraPosition()
    {
        camera.transform.localPosition = new Vector3(xSize - cellSize / 2, (xSize - cellSize / 2) * 2, -cellSize / 2);
    }

    public void PlaceAgent()
    {
        var randomX = Random.Range(0, xSize);
        var randomZ = Random.Range(0, zSize);

        agent.transform.localPosition =
            new Vector3( randomX * cellSize, 0.6f, randomZ * cellSize);
        mazeGraph.MarkCellVisited(randomX, randomZ);
        mazeGraph.PlaceStart(agent.transform.localPosition);
    }

    public void ClearMaze()
    {
        if (maze != null)
        {
            Destroy(maze);
        }
        mazeGraph = null!;
    }
    
    public int GetCellSize()
    {
        return (int) cellSize;
    }
    
    public int GetCellCount()
    {
        return xSize * zSize;
    }
}
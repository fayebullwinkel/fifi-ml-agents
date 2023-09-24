using MazeDatatype;
using Unity.MLAgents.Policies;
using UnityEngine;
using Random = UnityEngine.Random;

public class MazeManager : MonoBehaviour
{
    [Header("Maze Settings")]
    
    [SerializeField]
    [Tooltip("If true, the agent can be played manually to generate the maze.")]
    private bool manualGeneration;
    
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
    private GameObject agentPrefab;

    public static MazeManager Singleton { get; private set; } = null!;
    
    public MazeGraph mazeGraph { get; private set; }
    
    public GameObject OuterWalls { get; private set; } = null!;
    
    private GameObject agent;

    private GameObject maze;

    private void Awake()
    {
        Singleton = this;
        agent = GameObject.FindGameObjectWithTag("MazeGenerationAgent");
    }
    
    private void Start()
    {
        if (manualGeneration)
        {
            GenerateGrid();
            PlaceAgent();
            DeactivateAgentProperties();
        }
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
        if (agent == null)
        {
            agent = Instantiate(agentPrefab);
        }
        agent.transform.localPosition =
            new Vector3( randomX * cellSize, 0.6f, randomZ * cellSize);
        mazeGraph.MarkCellVisited(randomX, randomZ);
        mazeGraph.PlaceStart(agent.transform.localPosition);
    }
    
    private void DeactivateAgentProperties()
    {
        agent.GetComponent<MazeGenerationAgent>().enabled = false;
        agent.GetComponent<BehaviorParameters>().enabled = false;
        agent.AddComponent<ManualMovement>();
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
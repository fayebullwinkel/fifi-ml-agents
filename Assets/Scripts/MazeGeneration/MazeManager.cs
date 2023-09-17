using MazeDatatype;
using UnityEngine;
using Random = UnityEngine.Random;

public class MazeManager : MonoBehaviour
{
    [Header("Maze Settings")]
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

    public MazeGraph mazeGraph { get; private set; }
    
    public static MazeManager Singleton { get; private set; } = null!;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        // Create a graph with walls in horizontal and vertical direction
        mazeGraph = new MazeGraph(xSize, zSize);
        SetOuterWalls();
        SetCameraPosition();
        PlaceAgent();
    }

    private void SetOuterWalls()
    {
        var mazeBounds = Instantiate(mazeBoundsPrefab, transform);
        var size = mazeBounds.transform.localScale;
        size = new Vector3(size.x * cellSize * xSize, size.y, size.z * cellSize * zSize);
        mazeBounds.transform.localScale = size;
        var position = mazeBounds.transform.localPosition;
        position = new Vector3(xSize - cellSize / 2, position.y, zSize - cellSize / 2);
        mazeBounds.transform.localPosition = position;
    }

    private void SetCameraPosition()
    {
        camera.transform.localPosition = new Vector3(xSize - cellSize / 2, (xSize - cellSize / 2) * 2, -cellSize / 2);
    }

    private void PlaceAgent()
    {
        var randomX = Random.Range(0, xSize);
        var randomZ = Random.Range(0, zSize);
        var agent = Instantiate(agentPrefab);
        agent.transform.localPosition =
            new Vector3( randomX * cellSize, 0.6f, randomZ * cellSize);
        mazeGraph.MarkCellVisited(randomX, randomZ);
        mazeGraph.PlaceStart(agent.transform.localPosition);
    }
    
    public int GetCellSize()
    {
        return (int) cellSize;
    }
}
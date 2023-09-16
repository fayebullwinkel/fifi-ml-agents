using System.Collections.Generic;
using MazeDatatype;
using UnityEngine;

public class MazeManager : MonoBehaviour
{
    [Header("Maze Settings")]
    [SerializeField]
    private int x_Size;
    [SerializeField]
    private int z_Size;

    [SerializeField]
    private float cellSize;

    [Header("Resources")]
    [SerializeField]
    private GameObject wallPrefab;

    [SerializeField]
    private GameObject mazeBoundsPrefab;

    [SerializeField]
    private GameObject camera;
    
    [SerializeField]
    private GameObject agentPrefab;

    private MazeGraph _mazeGraph;
    private List<GameObject> walls = new();

    private void Start()
    {
        // Create a graph with walls in horizontal and vertical direction
        _mazeGraph = new MazeGraph(x_Size, z_Size, withWalls: true);
        GenerateGrid();
        SetOuterWalls();
        SetCameraPosition();
        PlaceAgent();
    }

    private void GenerateGrid()
    {
        var wallParent = new GameObject("wallParent");
        wallParent.transform.parent = transform;
        wallParent.transform.localPosition = new Vector3(0, 0.5f, 0);

        foreach (var cell in _mazeGraph.GetMazeCells())
        {
            if (cell.GetWall(Wall.Top))
            {
                var position = new Vector3(cell.GetX() * cellSize, 0, cell.GetZ() * cellSize + cellSize / 2);
                if (!walls.Exists(x => x.transform.localPosition == position))
                {
                    var wall = Instantiate(wallPrefab, wallParent.transform);
                    wall.transform.localPosition = position;
                    wall.transform.localScale = new Vector3(cellSize, 1, 0.1f);
                    walls.Add(wall);
                }
            }

            // Place Bottom Wall if there is one
            if (cell.GetWall(Wall.Bottom))
            {
                var position = new Vector3(cell.GetX() * cellSize, 0, cell.GetZ() * cellSize - cellSize / 2);
                if (!walls.Exists(x => x.transform.localPosition == position))
                {
                    var wall = Instantiate(wallPrefab, wallParent.transform);
                    wall.transform.localPosition = position;
                    wall.transform.localScale = new Vector3(cellSize, 1, 0.1f);
                    walls.Add(wall);
                }
            }

            // Place Left Wall if there is one
            if (cell.GetWall(Wall.Left))
            {
                var position = new Vector3(cell.GetX() * cellSize - cellSize / 2, 0, cell.GetZ() * cellSize);
                if (!walls.Exists(x => x.transform.localPosition == position))
                {
                    var wall = Instantiate(wallPrefab, wallParent.transform);
                    wall.transform.localPosition = position;
                    wall.transform.localScale = new Vector3(0.1f, 1, cellSize);
                    walls.Add(wall);
                }
            }

            // Place Right Wall if there is one
            if (cell.GetWall(Wall.Right))
            {
                var position = new Vector3(cell.GetX() * cellSize + cellSize / 2, 0, cell.GetZ() * cellSize);
                if (!walls.Exists(x => x.transform.localPosition == position))
                {
                    var wall = Instantiate(wallPrefab, wallParent.transform);
                    wall.transform.localPosition = position;
                    wall.transform.localScale = new Vector3(0.1f, 1, cellSize);
                    walls.Add(wall);
                }
            }
        }
    }

    private void SetOuterWalls()
    {
        var mazeBounds = Instantiate(mazeBoundsPrefab, transform);
        var size = mazeBounds.transform.localScale;
        size = new Vector3(size.x * cellSize * x_Size, size.y, size.z * cellSize * z_Size);
        mazeBounds.transform.localScale = size;
        var position = mazeBounds.transform.localPosition;
        position = new Vector3(x_Size - cellSize / 2, position.y, z_Size - cellSize / 2);
        mazeBounds.transform.localPosition = position;
    }

    private void SetCameraPosition()
    {
        camera.transform.localPosition = new Vector3(x_Size - cellSize / 2, (x_Size - cellSize / 2) * 2, -cellSize / 2);
    }
    
    private void PlaceAgent()
    {
        var agent = Instantiate(agentPrefab);
        agent.transform.localPosition = new Vector3(Random.Range(0, x_Size) * cellSize, 0.6f, Random.Range(0, z_Size) * cellSize);
    }

    public MazeGraph GetMazeGraph()
    {
        return _mazeGraph;
    }
}

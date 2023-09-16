using System;
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

    [Header("Resources")]
    [SerializeField]
    private GameObject wallPrefab;

    [SerializeField]
    private GameObject mazeBoundsPrefab;

    [SerializeField]
    private GameObject camera;

    [SerializeField]
    private GameObject agentPrefab;

    public MazeGraph mazeGraph { get; private set; }

    private void Start()
    {
        // Create a graph with walls in horizontal and vertical direction
        mazeGraph = new MazeGraph(xSize, zSize, true);
        GenerateGrid();
        SetOuterWalls();
        SetCameraPosition();
        PlaceAgent();
    }

    private void GenerateGrid()
    {
        var wallParent = CreateWallParent();

        foreach (var cell in mazeGraph.Cells)
        {
            PlaceWall(cell, Wall.Top, wallParent, MazeWall.WallType.Horizontal);
            PlaceWall(cell, Wall.Bottom, wallParent, MazeWall.WallType.Horizontal);
            PlaceWall(cell, Wall.Left, wallParent, MazeWall.WallType.Vertical);
            PlaceWall(cell, Wall.Right, wallParent, MazeWall.WallType.Vertical);
        }
    }

    private GameObject CreateWallParent()
    {
        var wallParent = new GameObject("wallParent");
        wallParent.transform.parent = transform;
        wallParent.transform.localPosition = new Vector3(0, 0.5f, 0);
        return wallParent;
    }

    private void PlaceWall(MazeCell cell, Wall wall, GameObject wallParent, MazeWall.WallType wallType)
    {
        if (!cell.GetWall(wall))
        {
            return;
        }
        var position = GetWallPosition(cell, wall);
        var existingWall = mazeGraph.GetExistingWall(position);

        if (existingWall == null)
        {
            CreateNewWall(cell, position, wallParent, wallType);
        }
        else
        {
            SetSecondCell(existingWall, cell);
        }
    }

    private Vector3 GetWallPosition(MazeCell cell, Wall wall)
    {
        float xOffset = 0;
        float zOffset = 0;

        switch (wall)
        {
            case Wall.Top:
                zOffset = cellSize / 2;
                break;
            case Wall.Bottom:
                zOffset = -cellSize / 2;
                break;
            case Wall.Left:
                xOffset = -cellSize / 2;
                break;
            case Wall.Right:
                xOffset = cellSize / 2;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(wall), wall, null);
        }

        return new Vector3(cell.GetX() * cellSize + xOffset, 0, cell.GetZ() * cellSize + zOffset);
    }

    private void CreateNewWall(MazeCell cell, Vector3 position, GameObject wallParent, MazeWall.WallType wallType)
    {
        var wall = Instantiate(wallPrefab, wallParent.transform);
        wall.transform.localPosition = position;
        wall.transform.localScale = GetWallScale(wallType);

        var mazeWall = wall.GetComponent<MazeWall>();
        mazeWall.Type = wallType;
        mazeWall.Cell1 = cell;
        mazeGraph.Walls.Add(mazeWall);
    }

    private void SetSecondCell(MazeWall wall, MazeCell cell)
    {
        wall.Cell2 = cell;
        // Add neighbour to both cells
        var cell1 = mazeGraph.GetCell(cell.GetX(), cell.GetZ());
        var cell2 = mazeGraph.GetCell(wall.Cell1.GetX(), wall.Cell1.GetZ());
        cell1.AddNeighbour(cell2);
        cell2.AddNeighbour(cell1);
    }

    private Vector3 GetWallScale(MazeWall.WallType wallType)
    {
        return wallType == MazeWall.WallType.Horizontal ? new Vector3(cellSize, 1, 0.1f) : new Vector3(0.1f, 1, cellSize);
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
        var agent = Instantiate(agentPrefab);
        agent.transform.localPosition =
            new Vector3(Random.Range(0, xSize) * cellSize, 0.6f, Random.Range(0, zSize) * cellSize);
    }
}
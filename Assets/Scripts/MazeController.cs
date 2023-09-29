using System.Collections.Generic;
using UnityEngine;

public class MazeController : MonoBehaviour
{
    public MazeGeneration mazeGenerator;
    public int mazeCountX = 1;
    public int mazeCountZ = 1;
    public int spacing = 4;

    private MazeBuilder _mazeBuilder;

    private GameObject _mazeObj;
    private GameObject _agentPrefab;
    private GameObject _agentObj;
    private GameObject _endCubeObj;
    private Maze _maze;

    private Vector3 _mazePosition;
    private bool _first;

    private void Start()
    {
        _agentPrefab = (GameObject)Resources.Load("Prefabs/MazeAgent", typeof(GameObject));
        var localScale = transform.localScale;

        for (int i = 0; i < mazeCountX; i++)
        {
            for (int j = 0; j < mazeCountZ; j++)
            {
                Maze maze = null;
                _mazeBuilder = gameObject.AddComponent<MazeBuilder>();

                _endCubeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _endCubeObj.tag = "EndCube";

                _agentObj = Instantiate(_agentPrefab);
                _agentObj.transform.localScale = _endCubeObj.transform.localScale;
                _agentObj.GetComponent<Rigidbody>().isKinematic = true;

                float xOffset = (i * (localScale.x + spacing)) - ((mazeCountX - 1) * 0.5f * (localScale.x + spacing));
                float zOffset = (j * (localScale.z + spacing)) - ((mazeCountZ - 1) * 0.5f * (localScale.z + spacing));

                _mazePosition = transform.position + new Vector3(xOffset, 0f, zOffset);
                _first = true;
                
                ResetArea(_mazeBuilder, maze, _endCubeObj, _agentObj, _mazePosition);
            }
        }
    }

    private void ResetArea(MazeBuilder mazeBuilder, Maze maze, GameObject endCubeObj, GameObject agentObj, Vector3 mazePosition)
    {
        if (!_first)
        {
            _first = false;
            mazeBuilder.mazeObj.transform.Find("MazeAgent").SetParent(null); 
        }
        Destroy(mazeBuilder.mazeObj);

        maze = GenerateMaze();
        BuildMaze(mazeBuilder, maze, mazePosition);
        agentObj.GetComponent<MazeAgent>().SetStartPosition(maze.GetStartCube().GetPos());
        agentObj.GetComponent<MazeAgent>().SetMaze(maze);
        MoveCube(mazeBuilder.mazeObj, agentObj, maze.GetStartCube(), Color.green);
        MoveCube(mazeBuilder.mazeObj, endCubeObj, maze.GetEndCube(), Color.red);
        RotateMazeToFaceCamera(mazeBuilder.mazeObj, agentObj);
    }

    private Maze GenerateMaze()
    {
        return mazeGenerator.Generate(transform.localScale);
    }

    private void BuildMaze(MazeBuilder mazeBuilder, Maze maze, Vector3 mazePosition)
    {
        mazeBuilder.Initialize(maze);
        mazeBuilder.BuildMaze(mazePosition, transform.localScale);
    }

    private void RotateMazeToFaceCamera(GameObject mazeObj, GameObject agentObj)
    {
        var startCubeCenter = agentObj.transform.position;
        var mazeCubeCenter = mazeObj.transform.position;

        // Direction from the startCube to the mazeCube
        var directionToStartCube = startCubeCenter - mazeCubeCenter;
        var x = Mathf.Abs(directionToStartCube.x);
        var y = Mathf.Abs(directionToStartCube.y);
        var z = Mathf.Abs(directionToStartCube.z);

        // Rotate cube so that face with startCube is facing the camera
        if (x >= y && x >= z)
        {
            if (directionToStartCube.x > 0)
            {
                // right
                mazeObj.transform.Rotate(0.0f, 90.0f, 0.0f, Space.World);
            }
            else
            {
                // left
                mazeObj.transform.Rotate(0.0f, -90.0f, 0.0f, Space.World);
            }
        }
        else if (y >= x && y >= z)
        {
            if (directionToStartCube.y > 0)
            {
                // top
                mazeObj.transform.Rotate(-90.0f, 0.0f, 0.0f, Space.World);
            }
            else
            {
                // bottom
                mazeObj.transform.Rotate(90.0f, 0.0f, 0.0f, Space.World);
            }
        }
        else
        {
            if (directionToStartCube.z > 0)
            {
                // back
                mazeObj.transform.Rotate(0.0f, 180.0f, 0.0f, Space.World);
            }
        }
    }

    private void MoveCube(GameObject mazeObj, GameObject cubeObj, Cube cube, Color color)
    {
        cubeObj.transform.SetParent(mazeObj.transform);
        cubeObj.transform.localPosition = cube.GetRelativePosition(mazeObj.transform.localScale);
        cubeObj.GetComponent<Renderer>().material.color = color;
    }

    public List<Vector3Int> GetSurfaceCubePositions()
    {
        return mazeGenerator.GetSurfaceCubePositions();
    }

    public void Reset()
    {
        ResetArea(_mazeBuilder, _maze, _endCubeObj, _agentObj, _mazePosition);
    }
}
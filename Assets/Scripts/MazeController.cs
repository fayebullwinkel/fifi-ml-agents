using System.Collections.Generic;
using UnityEngine;

public class MazeController : MonoBehaviour
{
    public MazeGeneration mazeGenerator;

    private MazeBuilder _mazeBuilder;
    private MazeAgent _mazeAgent;
    private Maze _maze;
    
    // for presenting the cube
    public bool rotateCube;
    private readonly float _rotationSpeed = 30f;
    private Quaternion _currentRotation;
    private GameObject _mazeObj;

    private void Start()
    {
        _mazeBuilder = gameObject.AddComponent<MazeBuilder>();
        ResetArea();
    }

    public void ResetArea()
    {
        _mazeBuilder.Delete();
        _maze = GenerateMaze();
        BuildMaze(_maze);
        _mazeBuilder.PlaceMazeAgent();
        RotateMazeToFaceCamera();
    }

    private Maze GenerateMaze()
    {
        return mazeGenerator.Generate(transform.localScale);
    }
    
    private void BuildMaze(Maze maze)
    {
        _mazeBuilder.Initialize(maze);
        
        var localScale = transform.localScale;
        _mazeObj = _mazeBuilder.BuildMaze(transform.position, localScale);
    }
    
    private void RotateMazeToFaceCamera()
    {
        var startCubeCenter = _mazeBuilder.GetStartCube().transform.position;
        var mazeCubeCenter = _mazeObj.transform.position;

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
                _mazeObj.transform.Rotate(0.0f, 90.0f, 0.0f, Space.World);
            }
            else
            {
                // left
                _mazeObj.transform.Rotate(0.0f, -90.0f, 0.0f, Space.World);
            }
        }
        else if (y >= x && y >= z)
        {
            if (directionToStartCube.y > 0)
            {
                // top
                _mazeObj.transform.Rotate(-90.0f, 0.0f, 0.0f, Space.World);
            }
            else
            {
                // bottom
                _mazeObj.transform.Rotate(90.0f, 0.0f, 0.0f, Space.World);
            }
        }
        else
        {
            if (directionToStartCube.z > 0)
            {
                // back
                _mazeObj.transform.Rotate(0.0f, 180.0f, 0.0f, Space.World);
            }
        }
    }

    void Update()
    {
        if (!rotateCube) return;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _currentRotation = _mazeObj.transform.rotation;
            ResetArea();
            _mazeBuilder.GetMazeObj().transform.rotation = _currentRotation;
        }

        // rotate the maze, just for fun
        _mazeObj = _mazeBuilder.GetMazeObj();
        if (_mazeObj)
        {
            _mazeObj.transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    public Vector3Int GetStartPosition()
    {
        return _mazeBuilder.GetStartPosition();
    }

    public Maze GetMaze()
    {
        return _mazeBuilder.GetMaze();
    }

    public List<Vector3Int> GetSurfaceCubePositions()
    {
        return _mazeBuilder.GetSurfaceCubePositions();
    }
}
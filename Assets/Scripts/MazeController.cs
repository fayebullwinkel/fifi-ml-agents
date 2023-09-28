using System.Collections.Generic;
using UnityEngine;

public class MazeController : MonoBehaviour
{
    public MazeGeneration mazeGenerator;
    private MazeBuilder _mazeBuilder;
    private Maze _maze;
    
    private GameObject _endCubeObj;
    private GameObject _agentPrefab;
    private GameObject _agentObj;
    
    // for presenting the cube
    public bool rotateCube;
    private readonly float _rotationSpeed = 30f;
    private Quaternion _currentRotation;
    private GameObject _mazeObj;
    
    private void Start()
    {
        _mazeBuilder = gameObject.AddComponent<MazeBuilder>();
        
        _endCubeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _endCubeObj.tag = "EndCube";
        
        _agentPrefab = (GameObject)Resources.Load("Prefabs/MazeAgent", typeof(GameObject));
        _agentObj = Instantiate(_agentPrefab);  
        _agentObj.transform.localScale = _endCubeObj.transform.localScale;
        _agentObj.GetComponent<Rigidbody>().isKinematic = true;
    }

    public void ResetArea()
    {
        _maze = GenerateMaze();
        BuildMaze(_maze);
        MoveCube(_agentObj, _maze.GetStartCube(), Color.green);
        MoveCube(_endCubeObj, _maze.GetEndCube(), Color.red);
        RotateMazeToFaceCamera();
    }

    private Maze GenerateMaze() 
    {
        return mazeGenerator.Generate(transform.localScale);
    }
    
    private void BuildMaze(Maze maze)
    {
        _mazeBuilder.Initialize(maze, _endCubeObj);
        
        var localScale = transform.localScale;
        _mazeObj = _mazeBuilder.BuildMaze(transform.position, localScale);
    }
    
    private void RotateMazeToFaceCamera()
    {
        var startCubeCenter = _agentObj.transform.position;
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
            _mazeObj.transform.rotation = _currentRotation;
        }

        // rotate the maze, just for fun
        if (_mazeObj)
        {
            _mazeObj.transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime, Space.World);
        }
    }
    
    private void MoveCube(GameObject cubeObj, Cube cube, Color color)
    {
        cubeObj.transform.parent = _mazeObj.transform;
        cubeObj.transform.localPosition = cube.GetCubePosition(_mazeObj.transform.localScale);
        cubeObj.GetComponent<Renderer>().material.color = color;
    }

    public Vector3Int GetStartPosition()
    {
        return _maze.GetStartCube().GetPos();
    }

    public Maze GetMaze()
    {
        return _maze;
    }

    public List<Vector3Int> GetSurfaceCubePositions()
    {
        return mazeGenerator.GetSurfaceCubePositions();
    }
}
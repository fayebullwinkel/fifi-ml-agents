using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MazeController : MonoBehaviour
{
    [FormerlySerializedAs("MazeGenerator")] [FormerlySerializedAs("TestCube")]
    public MazeGeneration mazeGenerator; // TODO: implement new scriptable object

    // for presenting the cube
    public bool rotateCube;
    private readonly float _rotationSpeed = 30f;
    private Quaternion _currentRotation;
    private GameObject _maze;

    public GameObject agentPrefab;

    private MazeAgent _mazeAgent;
    private GameObject _mazeAgentObj;
    private GameObject _startCubeObj;
    private Vector3Int _startPosition;
    private Cube[,,] _mazeArray;
    private List<Vector3Int> _surfaceCubePositions;

    private void Start()
    {
        ResetArea();
    }

    public void ResetArea()
    {
        mazeGenerator.Delete();
        Destroy(_startCubeObj);
        GenerateMaze();
        PlaceMazeAgent();
        RotateMazeToFaceCamera();
    }

    private void GenerateMaze()
    {
        var transform1 = transform;
        mazeGenerator.Generate(transform1.position, transform1.localScale);
        _maze = mazeGenerator.GetMazeObj();
    }

    private void RotateMazeToFaceCamera()
    {
        var startCubeCenter = _startCubeObj.transform.position;
        var mazeCubeCenter = _maze.transform.position;

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
                _maze.transform.Rotate(0.0f, 90.0f, 0.0f, Space.World);
            }
            else
            {
                // left
                _maze.transform.Rotate(0.0f, -90.0f, 0.0f, Space.World);
            }
        }
        else if (y >= x && y >= z)
        {
            if (directionToStartCube.y > 0)
            {
                // top
                _maze.transform.Rotate(-90.0f, 0.0f, 0.0f, Space.World);
            }
            else
            {
                // bottom
                _maze.transform.Rotate(90.0f, 0.0f, 0.0f, Space.World);
            }
        }
        else
        {
            if (directionToStartCube.z > 0)
            {
                // back
                _maze.transform.Rotate(0.0f, 180.0f, 0.0f, Space.World);
            }
        }
    }

    private List<Vector3Int> FindSurfaceCubePositions(Cube[,,] mazeArray)
    {
        // Get the dimensions of the 3D array
        var size = mazeArray.GetLength(0);

        var positions = new List<Vector3Int>();

        // Loop through the surface cubes
        for (int d = 0; d < size; d++)
        {
            for (int h = 0; h < size; h++)
            {
                for (int w = 0; w < size; w++)
                {
                    // Check if the cube is on the surface (i.e., on the outermost layer)
                    if (d == 0 || d == size - 1 || h == 0 || h == size - 1 || w == 0 || w == size - 1)
                    {
                        // Check if surfaceCube is not a wall
                        if (!mazeArray[w, h, d].GetIsWall())
                        {
                            positions.Add(new Vector3Int(w, h, d));
                        }
                    }
                }
            }
        }

        return positions;
    }

    private void PlaceMazeAgent()
    {
        _mazeArray = mazeGenerator.GetMazeArray();

        _surfaceCubePositions = FindSurfaceCubePositions(_mazeArray);

        // Check if there are surface cubes.
        if (_surfaceCubePositions.Count > 0)
        {
            // Select a random surface cube.
            var random = Random.Range(0, _surfaceCubePositions.Count);
            _startPosition = _surfaceCubePositions[random];

            var referenceCube = mazeGenerator.GetEndCube();

            _startCubeObj = Instantiate(agentPrefab);
            _startCubeObj.transform.parent = _maze.transform;
            _startCubeObj.transform.localScale = referenceCube.transform.localScale;
            _startCubeObj.transform.localPosition = _mazeArray[_startPosition.x, _startPosition.y, _startPosition.z]
                .GetCubePosition(_maze.transform.localScale);
            _startCubeObj.GetComponent<Rigidbody>().isKinematic = true;
            _startCubeObj.GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            Debug.Log("No surface cubes found.");
        }
    }

    void Update()
    {
        if (!rotateCube) return;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _currentRotation = _maze.transform.rotation;
            ResetArea();
            mazeGenerator.GetMazeObj().transform.rotation = _currentRotation;
        }

        // rotate the maze, just for fun
        _maze = mazeGenerator.GetMazeObj();
        if (_maze)
        {
            _maze.transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    public float GetReferenceCubeSize()
    {
        return _startCubeObj.transform.localScale.x;
    }

    public Vector3Int GetStartPosition()
    {
        return _startPosition;
    }

    public Cube[,,] GetMazeArray()
    {
        return _mazeArray;
    }

    public List<Vector3Int> GetSurfaceCubePositions()
    {
        return _surfaceCubePositions;
    }
}
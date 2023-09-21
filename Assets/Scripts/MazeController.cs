using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MazeController : MonoBehaviour
{
    [FormerlySerializedAs("MazeGenerator")] [FormerlySerializedAs("TestCube")]
    public MazeGeneration mazeGenerator; // TODO: implement new scriptable object

    public bool rotateCube;
    private readonly float _rotationSpeed = 30f;
    private Quaternion _currentRotation;
    private GameObject _maze;

    private MazeAgent _mazeAgent;
    private GameObject _mazeAgentObj;

    public GameObject agentPrefab;
    private GameObject _startCubeObj;

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
    }

    private void GenerateMaze()
    {
        var transform1 = transform;
        mazeGenerator.Generate(transform1.position, transform1.localScale);

        _maze = mazeGenerator.GetMazeObj();
    }

    private bool IsPointInWall(Vector3 point, float edgeLength)
    {
        Collider[] colliders = new Collider[10];
        Vector3 halfExtents = new Vector3(edgeLength / 2f, edgeLength / 2f, edgeLength / 2f);

        // Check for collisions within the box
        int colliderCount = Physics.OverlapBoxNonAlloc(point, halfExtents, colliders, Quaternion.identity,
            LayerMask.GetMask("Wall"));

        return colliderCount > 0;
    }

    private List<Vector3Int> FindSurfaceCubePositions(Cube[,,] mazeArray)
    {
        // Get the dimensions of the 3D array
        int size = mazeArray.GetLength(0);
        
        List<Vector3Int> positions = new List<Vector3Int>();

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
        Cube[,,] mazeArray = mazeGenerator.GetMazeArray();

        List<Vector3Int> positions = FindSurfaceCubePositions(mazeArray);
        
        // Check if there are surface cubes.
        if (positions.Count > 0)
        {
            // Select a random surface cube.
            int random = Random.Range(0, positions.Count);
            Vector3Int position = positions[random];

            GameObject referenceCube = mazeGenerator.GetEndCube();

            _startCubeObj = Instantiate(agentPrefab);
            _startCubeObj.transform.parent = _maze.transform;
            _startCubeObj.transform.localScale = referenceCube.transform.localScale;
            _startCubeObj.transform.localPosition = mazeArray[position.x, position.y, position.z]
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
}
using UnityEngine;
using UnityEngine.Serialization;

public class MazeController : MonoBehaviour
{
    [FormerlySerializedAs("MazeGenerator")] [FormerlySerializedAs("TestCube")] public MazeGeneration mazeGenerator; // TODO: implement new scriptable object
    //private readonly float _rotationSpeed = 30f;
    private Quaternion _currentRotation;
    private GameObject _maze;

    private MazeAgent _mazeAgent;
    private GameObject _mazeAgentObj;

    private void Start()
    {
        ResetArea();
    }

    public void ResetArea()
    {
        mazeGenerator.Delete();
        GenerateMaze();
        PlaceMazeAgent();
    }

    private void GenerateMaze()
    {
        var transform1 = transform;
        mazeGenerator.Generate(transform1.position, transform1.localScale);

        _maze = mazeGenerator.GetMazeObj();
    }

    private void PlaceMazeAgent()
    {
        _mazeAgentObj = GameObject.FindGameObjectWithTag("MazeAgent");
        _mazeAgent = _mazeAgentObj.GetComponent<MazeAgent>();
        _mazeAgentObj.GetComponent<Renderer>().material.color = Color.green;
    }

    /*void Update()
    {
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
    }*/

}

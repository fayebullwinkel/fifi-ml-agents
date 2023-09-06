using UnityEngine;
using UnityEngine.Serialization;

public class MazeController : MonoBehaviour
{
    [FormerlySerializedAs("MazeGenerator")] [FormerlySerializedAs("TestCube")] public MazeGeneration mazeGenerator; // TODO: implement new scriptable object
    private readonly float _rotationSpeed = 30f;
    private Quaternion _currentRotation;
    private GameObject _maze;

    public MazeAgent mazeAgent;

    private void Start()
    {
        ResetArea();
    }

    private void ResetArea()
    {
        mazeGenerator.Delete();
        GenerateMaze();
        PlaceMazeAgent();
    }

    private void GenerateMaze()
    {
        mazeGenerator.Generate(transform.position, transform.localScale); 
        
        _maze = mazeGenerator.GetMazeObj();
    }

    private void PlaceMazeAgent()
    {
        GameObject start = mazeGenerator.GetStartCube();
        start.GetComponent<Renderer>().material.color = Color.green;
        start.AddComponent<MazeAgent>();
    }

    void Update()
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
    }

}

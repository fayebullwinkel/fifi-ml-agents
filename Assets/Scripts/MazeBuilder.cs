using System.Collections.Generic;
using UnityEngine;

public class MazeBuilder: MonoBehaviour
{
    private GameObject _mazeObj;
    private GameObject _mazeBase;
    private GameObject _startCubeObj;
    private GameObject _wallPrefab;
    private GameObject _endCubeObj;
    private GameObject _agentPrefab;
    
    private int _x;
    private int _y;
    private int _z;
    
    private Vector3Int _startPosition;
    private Cube[,,] _cubes;
    private Maze _maze;
    
    private List<Cube> _path;
    private List<Vector3Int> _surfaceCubePositions;
    
    public void Initialize(Maze maze)
    {
        _maze = maze;
        _cubes = _maze.GetCubes();
        _x = _cubes.GetLength(0);
        _y = _cubes.GetLength(1);
        _z = _cubes.GetLength(2);
        
        _path = new List<Cube>();
        
        _wallPrefab = (GameObject)Resources.Load("Prefabs/Wall", typeof(GameObject));
        _agentPrefab = (GameObject)Resources.Load("Prefabs/MazeAgent", typeof(GameObject));
        _endCubeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _endCubeObj.tag = "EndCube";
    }
    
    public void Delete()
    {
        Destroy(_mazeObj);
        
        // TODO: test what needs to be destroyed
        /*Destroy(_startCubeObj);
        Destroy(_endCubeObj);*/
    }

    public GameObject BuildMaze(Vector3 position, Vector3 scale)
    {
        _mazeObj = new GameObject();
        _mazeObj.transform.position = position;
        _mazeObj.transform.localScale = scale;

        CreateWalls();
        SetUpMazeBase();

        return _mazeObj;
    }
    
    private void SetUpMazeBase()
    {
        _mazeBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _mazeBase.transform.parent = _mazeObj.transform;
        _mazeBase.transform.localPosition = new Vector3(0, 0, 0);
        _mazeBase.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
        _mazeBase.tag = "Floor";
        _mazeBase.GetComponent<BoxCollider>().isTrigger = true;
        _mazeBase.GetComponent<Renderer>().material.color = new Color32(168, 119, 90, 255);
    }

    private void CreateWalls()
    {
        for (int x = 0; x < _x; x++)
        {
            for (int y = 0; y < _y; y++)
            {
                for (int z = 0; z < _z; z++)
                {
                    if (_cubes[x, y, z].GetIsWall())
                    {
                        var wall = Instantiate(_wallPrefab);
                        wall.transform.localScale = _endCubeObj.transform.localScale;
                        wall.transform.parent = _mazeObj.transform;
                        wall.transform.localPosition = _cubes[x, y, z]
                            .GetCubePosition(_mazeObj.transform.localScale);
                    }
                    else
                    {
                        _path.Add(_cubes[x, y, z]);
                    }
                }
            }
        }
        
        // place end cube
        var endCube = _path[_path.Count - 1];
        endCube.SetIsGoal(true);
        PlaceCube(_endCubeObj, endCube, Color.red);
        
    }
    
    private void PlaceCube(GameObject cubeObj, Cube cube, Color color)
    {
        cubeObj.transform.parent = _mazeObj.transform;
        cubeObj.transform.localPosition = cube.GetCubePosition(_mazeObj.transform.localScale);
        cubeObj.GetComponent<Renderer>().material.color = color;
    }
    
    public void PlaceMazeAgent()
    {
        _surfaceCubePositions = FindSurfaceCubePositions(_maze);

        // Check if there are surface cubes.
        if (_surfaceCubePositions.Count > 0)
        {
            // Select a random surface cube.
            var random = Random.Range(0, _surfaceCubePositions.Count);
            _startPosition = _surfaceCubePositions[random];
            
            _startCubeObj = Instantiate(_agentPrefab);
            _startCubeObj.transform.parent = _mazeObj.transform;
            _startCubeObj.transform.localScale = _endCubeObj.transform.localScale;
            _startCubeObj.transform.localPosition = _maze.GetCube(new Vector3Int(_startPosition.x, _startPosition.y, _startPosition.z))
                .GetCubePosition(_mazeObj.transform.localScale);
            _startCubeObj.GetComponent<Rigidbody>().isKinematic = true;
            _startCubeObj.GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            Debug.Log("No surface cubes found.");
        }
    }
    
    private List<Vector3Int> FindSurfaceCubePositions(Maze maze)
    {
        // Get the dimensions of the 3D array
        var size = maze.GetCubes().GetLength(0);

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
                        if (!maze.GetCube(new Vector3Int(w, h, d)).GetIsWall())
                        {
                            positions.Add(new Vector3Int(w, h, d));
                        }
                    }
                }
            }
        }

        return positions;
    }
    
    public Vector3Int GetStartPosition()
    {
        return _startPosition;
    }

    public GameObject GetStartCube()
    {
        return _startCubeObj;
    }

    public List<Vector3Int> GetSurfaceCubePositions()
    {
        return _surfaceCubePositions;
    }

    public Maze GetMaze()
    {
        return _maze;
    }

    public GameObject GetMazeObj()
    {
        return _mazeObj;
    }
}
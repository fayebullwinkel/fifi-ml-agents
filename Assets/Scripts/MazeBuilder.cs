using System.Collections.Generic;
using UnityEngine;

public class MazeBuilder: MonoBehaviour
{
    private GameObject _mazeObj;
    private GameObject _mazeBase;
    private GameObject _agentObj;
    private GameObject _wallPrefab;
    private GameObject _endCubeObj;
    private GameObject _agentPrefab;
    
    private int _x;
    private int _y;
    private int _z;
    
    private Cube[,,] _cubes;
    private Maze _maze;
    
    public void Initialize(Maze maze)
    {
        _maze = maze;
        _cubes = _maze.GetCubes();
        _x = _cubes.GetLength(0);
        _y = _cubes.GetLength(1);
        _z = _cubes.GetLength(2);
        
        _wallPrefab = (GameObject)Resources.Load("Prefabs/Wall", typeof(GameObject));
        _agentPrefab = (GameObject)Resources.Load("Prefabs/MazeAgent", typeof(GameObject));
        _endCubeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _endCubeObj.tag = "EndCube";
    }
    
    public void Delete()
    {
        Destroy(_mazeObj);
    }

    public GameObject BuildMaze(Vector3 position, Vector3 scale)
    {
        _mazeObj = new GameObject();
        _mazeObj.transform.position = position;
        _mazeObj.transform.localScale = scale;
        _mazeObj.gameObject.name = "Maze";

        CreateWalls();
        CreateMazeBase();
        PlaceEndCube();

        return _mazeObj;
    }
    
    private void CreateMazeBase()
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
                }
            }
        }
    }

    private void PlaceEndCube()
    {
        var endCube = _maze.GetEndCube();
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
        _agentObj = Instantiate(_agentPrefab);
        PlaceCube(_agentObj, _maze.GetStartCube(), Color.green);
        _agentObj.transform.localScale = _endCubeObj.transform.localScale;
        _agentObj.GetComponent<Rigidbody>().isKinematic = true;
    }

    public GameObject GetAgentObject()
    {
        return _agentObj;
    }

    public GameObject GetMazeObj()
    {
        return _mazeObj;
    }
}
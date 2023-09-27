using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MazeGeneration : ScriptableObject
{
    private Cube[,,] _mazeArray;
    private int _x;
    private int _y;
    private int _z;
    private List<Cube> _cubeList;
    private GameObject _mazeObj;
    private List<Cube> _possibleNextCubes;
    private List<Cube> _path;
    private GameObject _startCubeObj;
    private GameObject _endCubeObj;
    private GameObject _mazeBase;

    public GameObject wallPrefab;

    public void Generate(Vector3 position, Vector3 scale)
    {
        _x = Mathf.RoundToInt(scale.x);
        _y = Mathf.RoundToInt(scale.y);
        _z = Mathf.RoundToInt(scale.z);
        _mazeArray = new Cube[_x, _y, _z];
        _possibleNextCubes = new List<Cube>();
        _cubeList = new List<Cube>();
        _path = new List<Cube>();

        _endCubeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _endCubeObj.tag = "EndCube";

        SearchArray(0);
        PrimsAlgorithm();
        BuildMaze(position, scale);
    }

    public void BuildMaze(Vector3 position, Vector3 scale)
    {
        _mazeObj = new GameObject();
        _mazeObj.transform.position = position;
        _mazeObj.transform.localScale = scale;

        SearchArray(2);
        SetUpMazeBase();
    }
    
    public void Delete()
    {
        Destroy(_mazeObj);
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

    private bool IsBoundaryCube(int width, int height, int depth)
    {
        // Check if a cube is located on boundary
        return depth == 0 || height == 0 || width == 0 || depth == _z - 1 || height == _y - 1 || width == _x - 1;
    }

    private Cube SearchArray(int mode)
    {
        // Count valid cubes in mode 1
        int count = 0;

        for (int width = 0; width < _x; width++)
        {
            for (int height = 0; height < _y; height++)
            {
                for (int depth = 0; depth < _z; depth++)
                {
                    switch (mode)
                    {
                        case 0:
                            // Mode 0: Initiating all walls and setting cube properties
                            if (IsBoundaryCube(width, height, depth))
                            {
                                // Set cube properties for boundary cubes
                                _mazeArray[width, height, depth].SetIsWall(true);
                                _mazeArray[width, height, depth].SetIsDeletable(false);
                                _mazeArray[width, height, depth].SetPos(width, height, depth);
                                _mazeArray[width, height, depth].SetWeight(Random.Range(1, 199));
                            }
                            else
                            {
                                // Set cube properties for non-boundary cubes
                                _mazeArray[width, height, depth].SetIsWall(false);
                                _mazeArray[width, height, depth].SetIsDeletable(true);
                            }

                            break;
                        case 1:
                            // Mode 1: Collect valid wall cubes
                            if (_mazeArray[width, height, depth].GetIsWall())
                            {
                                _possibleNextCubes.Add(_mazeArray[width, height, depth]);
                                count++;
                            }

                            break;
                        case 2:
                            // Mode 2: Create walls, cubes with isWall == false make up the _path
                            if (_mazeArray[width, height, depth].GetIsWall())
                            {
                                GameObject wall = Instantiate(wallPrefab);
                                wall.transform.localScale = _endCubeObj.transform.localScale;
                                wall.transform.parent = _mazeObj.transform;
                                wall.transform.localPosition = _mazeArray[width, height, depth]
                                    .GetCubePosition(_mazeObj.transform.localScale);
                            }
                            else
                            {
                                _path.Add(_mazeArray[width, height, depth]);
                            }

                            break;
                        default:
                            Debug.Log("Mode Input Error");
                            break;
                    }
                }
            }
        }

        if (mode == 1)
        {
            // Return a random valid next cube
            int random = Random.Range(0, count);
            return _possibleNextCubes[random];
        }

        if (mode == 2)
        {
            // Place the end cube at the end of the path
            PlaceCube(_endCubeObj, _path[_path.Count - 1], Color.red);
        }

        // Return a placeholder cube for invalid cases
        Cube badSearch = new Cube();
        badSearch.SetIsWall(false);
        return badSearch;
    }

    private void PlaceCube(GameObject cubeObj, Cube cube, Color color)
    {
        cubeObj.transform.parent = _mazeObj.transform;
        cubeObj.transform.localPosition = cube.GetCubePosition(_mazeObj.transform.localScale);
        cubeObj.GetComponent<Renderer>().material.color = color;
    }

    private void Cutout(Cube cube)
    {
        if (cube.GetIsWall())
        {
            _cubeList.Add(_mazeArray[cube.GetX(), cube.GetY(), cube.GetZ()]);
            _mazeArray[cube.GetX(), cube.GetY(), cube.GetZ()].SetIsWall(false);
        }
    }

    private Cube GetNeighborCubes(int pointX, int pointY, int pointZ, int mode)
    {
        for (int xi = -1; xi <= 1; xi++)
        {
            for (int yi = -1; yi <= 1; yi++)
            {
                for (int zi = -1; zi <= 1; zi++)
                {
                    switch (mode)
                    {
                        // Initial check for adjacent neighbor squares
                        case 0:
                            if (IsValidNeighbor(xi, yi, zi, pointX, pointY, pointZ))
                            {
                                if (_mazeArray[pointX + xi, pointY + yi, pointZ + zi].GetIsWall())
                                {
                                    _possibleNextCubes.Clear();
                                    GetNeighborCubes(pointX + xi, pointY + yi, pointZ + zi, 1);
                                    if (_possibleNextCubes.Count == 1)
                                    {
                                        _possibleNextCubes.Clear();
                                        return _mazeArray[pointX + xi, pointY + yi, pointZ + zi];
                                    }
                                }
                            }

                            break;
                        // Check for possible cubes that only one active neighbor adjacent
                        case 1:
                            if (IsValidNonDiagonalNeighbor(xi, yi, zi) &&
                                IsValidNeighbor(xi, yi, zi, pointX, pointY, pointZ))
                            {
                                if (!_mazeArray[pointX + xi, pointY + yi, pointZ + zi].GetIsWall() &&
                                    !_mazeArray[pointX + xi, pointY + yi, pointZ + zi].GetIsDeletable())
                                {
                                    _possibleNextCubes.Add(_mazeArray[pointX, pointY, pointZ]);
                                }
                            }

                            break;
                        default:
                            Debug.Log("Mode Input Error");
                            break;
                    }
                }
            }
        }

        // If no valid cube found, return a placeholder
        Cube badSearch = new Cube();
        badSearch.SetWeight(999);
        return badSearch;
    }

    // Check if a neighboring cube is valid
    private bool IsValidNeighbor(int xi, int yi, int zi, int pointX, int pointY, int pointZ)
    {
        return pointX + xi >= 0 && pointX + xi < _x && pointY + yi >= 0 && pointY + yi < _y && pointZ + zi >= 0 &&
               pointZ + zi < _z && !(pointX == 0 && pointY == 0 && pointZ == 0);
    }

    private bool IsValidNonDiagonalNeighbor(int xi, int yi, int zi)
    {
        return !(Mathf.Pow(xi, 2) == 1 && Mathf.Pow(yi, 2) == 1 || Mathf.Pow(xi, 2) == 1 && Mathf.Pow(zi, 2) == 1 ||
                 Mathf.Pow(zi, 2) == 1 && Mathf.Pow(yi, 2) == 1)
               && !(xi == 0 && yi == 0 && zi == 0);
    }

    private Nullable<Cube> FindBestCube()
    {
        Nullable<Cube> targetCube = null;
        float bestWeight = float.MaxValue;

        foreach (Cube cube in _cubeList)
        {
            Cube temp = GetNeighborCubes(cube.GetX(), cube.GetY(), cube.GetZ(), 0);
            float tempWeight = _mazeArray[temp.GetX(), temp.GetY(), temp.GetZ()].GetWeight();

            if (temp.GetIsWall() && tempWeight < bestWeight)
            {
                targetCube = temp;
                bestWeight = tempWeight;
            }
        }

        return targetCube;
    }

    private void PrimsAlgorithm()
    {
        // Cut start cube
        Cutout(SearchArray(1));

        while (true)
        {
            Nullable<Cube> bestCube = FindBestCube();
            if (!bestCube.HasValue)
                break;

            Cutout(bestCube.Value);
        }
    }

    public GameObject GetMazeObj()
    {
        return _mazeObj;
    }

    public GameObject GetEndCube()
    {
        return _endCubeObj;
    }

    public Cube[,,] GetMazeArray()
    {
        return _mazeArray;
    }
}
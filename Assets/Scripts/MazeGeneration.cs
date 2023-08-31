using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MazeGeneration : ScriptableObject
{
    private Cube[,,] mazeArray;
    private List<Cube> cubeList;
    private int x;
    private int y;
    private int z;
    private GameObject MazeObj;
    private List<Cube> possibleNextCubes;

    public void Generate(Vector3 position, Vector3 scale)
    {
        x = Mathf.RoundToInt(scale.x);
        y = Mathf.RoundToInt(scale.y);
        z = Mathf.RoundToInt(scale.z);
        mazeArray = new Cube[x, y, z];
        possibleNextCubes = new List<Cube>();
        cubeList = new List<Cube>();

        SearchArray(0);
        PrimsAlgorithm();
        BuildMaze(position, scale);
    }

    public void BuildMaze(Vector3 position, Vector3 scale)
    {
        MazeObj = new GameObject();
        MazeObj.transform.position = position;
        MazeObj.transform.localScale = scale;

        SearchArray(2);
        GameObject MazeBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        MazeBase.transform.parent = MazeObj.transform;
        MazeBase.transform.localPosition = new Vector3(0, 0, 0);
        MazeBase.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
    }

    public void Delete()
    {
        Destroy(MazeObj);
    }

    private bool IsBoundaryCube(int width, int height, int depth)
    {
        // Check if a cube is located on boundary
        return depth == 0 || height == 0 || width == 0 || depth == z - 1 || height == y - 1 || width == x - 1;
    }

    private Cube SearchArray(int mode)
    {
        // Count valid cubes in mode 1
        int count = 0;

        for (int width = 0; width < x; width++)
        {
            for (int height = 0; height < y; height++)
            {
                for (int depth = 0; depth < z; depth++)
                {
                    switch (mode)
                    {
                        case 0:
                            // Mode 0: Initiating all walls and setting cube properties
                            if (IsBoundaryCube(width, height, depth))
                            {
                                // Set cube properties for boundary cubes
                                mazeArray[width, height, depth].SetIsWall(true);
                                mazeArray[width, height, depth].SetIsDeletable(false);
                                mazeArray[width, height, depth].SetPos(width, height, depth);
                                mazeArray[width, height, depth].SetWeight(Random.Range(1, 199));
                            }
                            else
                            {
                                // Set cube properties for non-boundary cubes
                                mazeArray[width, height, depth].SetIsWall(false);
                                mazeArray[width, height, depth].SetIsDeletable(true);
                            }
                            break;
                        case 1:
                            // Mode 1: Collect valid wall cubes
                            if (mazeArray[width, height, depth].GetIsWall())
                            {
                                possibleNextCubes.Add(mazeArray[width, height, depth]);
                                count++;
                            }
                            break;
                        case 2:
                            // Mode 2: Create walls, cubes with isWall == false make up the path
                            if (mazeArray[width, height, depth].GetIsWall())
                            {
                                mazeArray[width, height, depth].Generate(MazeObj);
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
            // Mode 1: Return a random valid next cube
            int random = Random.Range(0, count);
            return possibleNextCubes[random];
        }

        // Return a placeholder cube for invalid cases
        Cube badSearch = new Cube();
        badSearch.SetIsWall(false);
        return badSearch;
    }

    private void Cutout(Cube cube)
    {
        if (cube.GetIsWall() == true)
        {
            cubeList.Add(mazeArray[cube.GetX(), cube.GetY(), cube.GetZ()]);
            mazeArray[cube.GetX(), cube.GetY(), cube.GetZ()].SetIsWall(false); 
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
                                if (mazeArray[pointX + xi, pointY + yi, pointZ + zi].GetIsWall())
                                {
                                    possibleNextCubes.Clear();
                                    GetNeighborCubes(pointX + xi, pointY + yi, pointZ + zi, 1);
                                    if (possibleNextCubes.Count == 1)
                                    {
                                        possibleNextCubes.Clear();
                                        return mazeArray[pointX + xi, pointY + yi, pointZ + zi];
                                    }
                                }
                            }
                            break;
                        // Check for possible cubes that only one active neighbor adjacent
                        case 1:
                            if (IsValidNonDiagonalNeighbor(xi, yi, zi) && IsValidNeighbor(xi, yi, zi, pointX, pointY, pointZ))
                            {
                                if (!mazeArray[pointX + xi, pointY + yi, pointZ + zi].GetIsWall() && !mazeArray[pointX + xi, pointY + yi, pointZ + zi].GetIsDeletable())
                                {
                                    possibleNextCubes.Add(mazeArray[pointX, pointY, pointZ]);
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
        return pointX + xi >= 0 && pointX + xi < x && pointY + yi >= 0 && pointY + yi < y && pointZ + zi >= 0 &&
               pointZ + zi < z && !(pointX == 0 && pointY == 0 && pointZ == 0);
    }

    private bool IsValidNonDiagonalNeighbor(int xi, int yi, int zi)
    {
        return !(Mathf.Pow(xi, 2) == 1 && Mathf.Pow(yi, 2) == 1 || Mathf.Pow(xi, 2) == 1 && Mathf.Pow(zi, 2) == 1 || Mathf.Pow(zi, 2) == 1 && Mathf.Pow(yi, 2) == 1)
               && !(xi == 0 && yi == 0 && zi == 0);
    }

    private Nullable<Cube> FindBestCube()
    {
        Nullable<Cube> targetCube = null;
        float bestWeight = float.MaxValue;

        foreach (Cube cube in cubeList)
        {
            Cube temp = GetNeighborCubes(cube.GetX(), cube.GetY(), cube.GetZ(), 0);
            float tempWeight = mazeArray[temp.GetX(), temp.GetY(), temp.GetZ()].GetWeight();

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
        return MazeObj;
    }
}

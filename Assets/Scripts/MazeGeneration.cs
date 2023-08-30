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
        Build(position, scale);
    }

    public void Build(Vector3 position, Vector3 scale)
    {
        Debug.Log("hier kommen wir durch");
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
                            // Mode 0: Initiating carveability and setting cube properties
                            mazeArray[width, height, depth] = new Cube();
                            if (IsBoundaryCube(width, height, depth))
                            {
                                // Set cube properties for boundary cubes
                                mazeArray[width, height, depth].SetIsCarveable(true);
                                mazeArray[width, height, depth].SetIsDeletable(false);
                                mazeArray[width, height, depth].SetPos(width, height, depth);
                                mazeArray[width, height, depth].SetWeight(Random.Range(1, 199));
                            }
                            else
                            {
                                // Set cube properties for non-boundary cubes
                                mazeArray[width, height, depth].SetIsCarveable(false);
                                mazeArray[width, height, depth].SetIsDeletable(true);
                            }
                            break;
                        case 1:
                            // Mode 1: Collect valid carveable cubes
                            if (mazeArray[width, height, depth].GetIsCarveable())
                            {
                                possibleNextCubes.Add(mazeArray[width, height, depth]);
                                count++;
                            }
                            break;
                        case 2:
                            // Mode 2: Generate cubes for carveable positions
                            if (mazeArray[width, height, depth].GetIsCarveable())
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
            Debug.Log(count);
            Debug.Log(random);
            return possibleNextCubes[random];
        }

        // Return a placeholder cube for invalid cases
        Cube badSearch = new Cube();
        badSearch.SetIsCarveable(false);
        return badSearch;
    }

    private void Cutout(Cube cube)
    {
        if (cube.GetIsCarveable() == true)
        {
            cubeList.Add(mazeArray[cube.GetX(), cube.GetY(), cube.GetZ()]);
            mazeArray[cube.GetX(), cube.GetY(), cube.GetZ()].SetIsCarveable(false); // TODO: why not just cube.SetIsCarveable(false)?
        }
    }

    private bool IsDiagonalOrSelf(int x, int y, int z)
    {
        return (x * x == 1 && y * y == 1) || (x * x == 1 && z * z == 1) || (z * z == 1 && y * y == 1);
    }

    private bool IsOutOfBounds(int x, int y, int z)
    {
        return this.x < 0 || this.x >= x || this.y < 0 || this.y >= y || this.z < 0 || this.z >= z;
    }

    private Cube FindPossibleCube(int pointX, int pointY, int pointZ, int xi, int yi, int zi)
    {
        if (IsDiagonalOrSelf(xi, yi, zi) || IsOutOfBounds(pointX + xi, pointY + yi, pointZ + zi))
            return null;

        Cube cube = mazeArray[pointX + xi, pointY + yi, pointZ + zi];

        if (cube.GetIsCarveable())
        {
            possibleNextCubes.Clear();
            GetNeighborCubes(pointX + xi, pointY + yi, pointZ + zi, 1);
            if (possibleNextCubes.Count == 1)
            {
                possibleNextCubes.Clear();
                return cube;
            }
        }

        return null;
    }

    private Cube GetNeighborCubes(int pointX, int pointY, int pointZ, int mode)
    {
        for (int xi = -1; xi <= 1; xi++)
        {
            for (int yi = -1; yi <= 1; yi++)
            {
                for (int zi = -1; zi <= 1; zi++)
                {
                    if (mode == 0)
                    {
                        Cube possibleCube = FindPossibleCube(pointX, pointY, pointZ, xi, yi, zi);
                        if (possibleCube != null)
                            return possibleCube;
                    }
                }
            }
        }

        Cube badSearch = new Cube();
        badSearch.SetWeight(999);
        return badSearch;
    }

    private Cube FindBestCube()
    {
        Cube targetCube = null;
        float bestWeight = float.MaxValue;

        foreach (Cube cube in cubeList)
        {
            Cube temp = GetNeighborCubes(cube.GetX(), cube.GetY(), cube.GetZ(), 0);
            float tempWeight = mazeArray[temp.GetX(), temp.GetY(), temp.GetZ()].GetWeight();

            if (temp.GetIsCarveable() && tempWeight < bestWeight)
            {
                targetCube = temp;
                bestWeight = tempWeight;
            }
        }

        return targetCube;
    }
    
    private void PrimsAlgorithm()
    {
        // Set starting block
        Cutout(SearchArray(1));

        while (true)
        {
            Cube bestCube = FindBestCube();
            if (bestCube == null)
                break;

            Cutout(bestCube);
        }
    }
}

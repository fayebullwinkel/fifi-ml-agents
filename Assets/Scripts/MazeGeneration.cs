using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MazeGeneration : ScriptableObject
{
    private int _x;
    private int _y;
    private int _z;
    private Cube[,,] _cubes;
    private List<Cube> _cutOutCubes;
    private List<Cube> _possibleNextCubes;
    private List<Vector3Int> _surfaceCubePositions;

    public Maze Generate(Vector3 scale)
    {
        _x = Mathf.RoundToInt(scale.x);
        _y = Mathf.RoundToInt(scale.y);
        _z = Mathf.RoundToInt(scale.z);
        _possibleNextCubes = new List<Cube>();
        _cutOutCubes = new List<Cube>();
        _cubes = new Cube[_x, _y, _x];

        SearchArray(0);
        PrimsAlgorithm();

        var randomStartPos = ChooseRandomPosition();
        Debug.Log("startPos: " + randomStartPos);
        _cubes[randomStartPos.x, randomStartPos.y, randomStartPos.z].SetIsStartCube(true);

        var randomEndPos = ChooseRandomPosition();
        Debug.Log("endPos: " + randomEndPos);
        _cubes[randomEndPos.x, randomEndPos.y, randomEndPos.z].SetIsGoalCube(true);

        return new Maze(_cubes, _cubes[randomStartPos.x, randomStartPos.y, randomStartPos.z],
            _cubes[randomEndPos.x, randomEndPos.y, randomEndPos.z]);
    }

    private bool IsBoundaryCube(int width, int height, int depth)
    {
        return depth == 0 || height == 0 || width == 0 || depth == _z - 1 || height == _y - 1 || width == _x - 1;
    }

    private Cube SearchArray(int mode)
    {
        // Count valid cubes in mode 1
        var count = 0;

        for (var x = 0; x < _x; x++)
        {
            for (var y = 0; y < _y; y++)
            {
                for (var z = 0; z < _z; z++)
                {
                    _cubes[x, y, z] ??= new Cube();

                    switch (mode)
                    {
                        case 0:
                            // Mode 0: Initiating all walls and setting cube properties
                            if (IsBoundaryCube(x, y, z))
                            {
                                // Set cube properties for boundary cubes
                                _cubes[x, y, z].SetIsWall(true);
                                _cubes[x, y, z].SetPos(x, y, z);
                                _cubes[x, y, z].SetWeight(Random.Range(1, 199));
                            }
                            else
                            {
                                // Set cube properties for non-boundary cubes
                                _cubes[x, y, z].SetIsWall(false);
                                _cubes[x, y, z].SetIsDeletable(true);
                            }

                            break;
                        case 1:
                            // Mode 1: Collect valid wall cubes
                            if (_cubes[x, y, z].GetIsWall())
                            {
                                _possibleNextCubes.Add(_cubes[x, y, z]);
                                count++;
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
            var random = Random.Range(0, count);
            return _possibleNextCubes[random];
        }

        // Return a placeholder cube for invalid cases
        var badSearch = new Cube();
        badSearch.SetIsWall(false);
        return badSearch;
    }

    private void Cutout(Cube cube)
    {
        if (cube.GetIsWall())
        {
            _cutOutCubes.Add(_cubes[cube.GetX(), cube.GetY(), cube.GetZ()]);
            _cubes[cube.GetX(), cube.GetY(), cube.GetZ()].SetIsWall(false);
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
                                if (_cubes[pointX + xi, pointY + yi, pointZ + zi].GetIsWall())
                                {
                                    _possibleNextCubes.Clear();
                                    GetNeighborCubes(pointX + xi, pointY + yi, pointZ + zi, 1);
                                    if (_possibleNextCubes.Count == 1)
                                    {
                                        _possibleNextCubes.Clear();
                                        return _cubes[pointX + xi, pointY + yi, pointZ + zi];
                                    }
                                }
                            }

                            break;
                        // Check for possible cubes that only one active neighbor adjacent
                        case 1:
                            if (IsValidNonDiagonalNeighbor(xi, yi, zi) &&
                                IsValidNeighbor(xi, yi, zi, pointX, pointY, pointZ))
                            {
                                if (!_cubes[pointX + xi, pointY + yi, pointZ + zi].GetIsWall() &&
                                    !_cubes[pointX + xi, pointY + yi, pointZ + zi].GetIsDeletable())
                                {
                                    _possibleNextCubes.Add(_cubes[pointX, pointY, pointZ]);
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
        var badSearch = new Cube();
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

    private Cube FindBestCube()
    {
        Cube targetCube = null;
        var bestWeight = float.MaxValue;

        foreach (Cube cube in _cutOutCubes)
        {
            Cube temp = GetNeighborCubes(cube.GetX(), cube.GetY(), cube.GetZ(), 0);
            var tempWeight = _cubes[temp.GetX(), temp.GetY(), temp.GetZ()].GetWeight();

            if (!temp.GetIsWall() || !(tempWeight < bestWeight)) continue;
            targetCube = temp;
            bestWeight = tempWeight;
        }

        return targetCube;
    }

    private void PrimsAlgorithm()
    {
        // Cut start cube
        Cutout(SearchArray(1));

        while (true)
        {
            Cube bestCube = FindBestCube();
            if (bestCube == null)
                break;

            Cutout(bestCube);
        }
    }

    private Vector3Int ChooseRandomPosition()
    {
        _surfaceCubePositions = FindValidSurfaceCubePositions(_cubes);

        // Check if there are surface cubes.
        if (_surfaceCubePositions.Count > 0)
        {
            // Select a random surface cube.
            var random = Random.Range(0, _surfaceCubePositions.Count);
            return _surfaceCubePositions[random];
        }

        Debug.Log("no surface cubes found. this should not happen.");
        return Vector3Int.zero;
    }

    private List<Vector3Int> FindValidSurfaceCubePositions(Cube[,,] cubes)
    {
        // Get the dimensions of the 3D array
        var size = cubes.GetLength(0);

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
                        if (!cubes[w, h, d].GetIsWall() && !cubes[w, h, d].GetIsStartCube())
                        {
                            positions.Add(new Vector3Int(w, h, d));
                        }
                    }
                }
            }
        }

        return positions;
    }

    public List<Vector3Int> GetSurfaceCubePositions()
    {
        return _surfaceCubePositions;
    }
}
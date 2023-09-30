using System;
using MazeGeneration_vivi.MazeDatatype.Enums;
using UnityEngine;

public class SharedMaze
{
    public Cube [,,] Cubes { get; set; }
    public Cube StartCube { get; set; }
    public Cube GoalCube { get; set; }
    
    public SharedMaze(int size)
    {
        Cubes = new Cube[size, size, size];
    }

    public void FillCubes(MazeGeneration_vivi.MazeDatatype.Maze maze)
    {
        foreach (var grid in maze.Grids.Values)
        {
            var face = grid.Face;
            foreach (var cell in grid.Cells)
            {
                var position = Vector3Int.zero;
                switch (face)
                {
                    case ECubeFace.None:
                        break;
                    case ECubeFace.Front:
                        position.z = 0;
                        position.x = cell.X * 2 + 1;
                        position.y = cell.Z * 2 + 1;
                        break;
                    case ECubeFace.Back:
                        break;
                    case ECubeFace.Left:
                        break;
                    case ECubeFace.Right:
                        break;
                    case ECubeFace.Top:
                        break;
                    case ECubeFace.Bottom:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                var cube = new Cube();
                cube.SetPos(position.x, position.y, position.z);
                cube.SetIsWall(false);
                Cubes[position.x, position.y, position.z] = cube;

                // Add Wall Cubes
                for (var i = 0; i < 4; i++)
                {
                    var wallPoition = position;
                    var isWall = true;
                    switch (face)
                    {
                        case ECubeFace.None:
                            break;
                        case ECubeFace.Front:
                            switch (i)
                            {
                                case 0:
                                    wallPoition.x -= 1;
                                    isWall = cell.HasWall(EDirection.Left);
                                    break;
                                case 1:
                                    wallPoition.x += 1;
                                    isWall = cell.HasWall(EDirection.Right);
                                    break;
                                case 2:
                                    wallPoition.y -= 1;
                                    isWall = cell.HasWall(EDirection.Bottom);
                                    break;
                                case 3:
                                    wallPoition.y += 1;
                                    isWall = cell.HasWall(EDirection.Top);
                                    break;
                            }
                            break;
                        case ECubeFace.Back:
                            break;
                        case ECubeFace.Left:
                            break;
                        case ECubeFace.Right:
                            break;
                        case ECubeFace.Top:
                            break;
                        case ECubeFace.Bottom:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    // Set wall cube
                    if (CubeExists(wallPoition))
                    {
                        continue;
                    }
                    var wallCube = new Cube();
                    wallCube.SetPos(wallPoition.x, wallPoition.y, wallPoition.z);
                    wallCube.SetIsWall(isWall);
                    Cubes[wallPoition.x, wallPoition.y, wallPoition.z] = wallCube;
                }
                // Add Corner Cubes
                for (var i = 0; i < 4; i++)
                {
                    var cornerPoition = position;
                    switch (face)
                    {
                        case ECubeFace.None:
                            break;
                        case ECubeFace.Front:
                            switch (i)
                            {
                                case 0:
                                    cornerPoition.x -= 1;
                                    cornerPoition.y -= 1;
                                    break;
                                case 1:
                                    cornerPoition.x += 1;
                                    cornerPoition.y -= 1;
                                    break;
                                case 2:
                                    cornerPoition.x -= 1;
                                    cornerPoition.y += 1;
                                    break;
                                case 3:
                                    cornerPoition.x += 1;
                                    cornerPoition.y += 1;
                                    break;
                            }
                            break;
                        case ECubeFace.Back:
                            break;
                        case ECubeFace.Left:
                            break;
                        case ECubeFace.Right:
                            break;
                        case ECubeFace.Top:
                            break;
                        case ECubeFace.Bottom:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    // Set corner cube
                    if (CubeExists(cornerPoition))
                    {
                        continue;
                    }
                    var cornerCube = new Cube();
                    cornerCube.SetPos(cornerPoition.x, cornerPoition.y, cornerPoition.z);
                    cornerCube.SetIsWall(true);
                    Cubes[cornerPoition.x, cornerPoition.y, cornerPoition.z] = cornerCube;
                }
            }
        }
    }
    
    private bool CubeExists(Vector3Int position)
    {
        return Cubes[position.x, position.y, position.z] != null;
    }
}
using System;
using MazeGeneration_vivi.MazeDatatype.Enums;
using UnityEngine;
using MazeSolving_Faye;

public static class SharedMaze
{
    public static Cube [,,] Cubes { get; set; }
    public static int Size {get; set;}
    public static Cube EndCube { get; set; }
    public static Cube StartCube { get; set; }

    public static void FillCubes(MazeGeneration_vivi.MazeDatatype.Maze maze)
    {
        Cubes = new Cube[Size, Size, Size];
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
                        position.z = Size - 1;
                        position.x = (Size - 1) - (cell.X * 2 + 1);
                        position.y = cell.Z * 2 + 1;
                        break;
                    case ECubeFace.Left:
                        position.x = 0;
                        position.z = (Size - 1) - (cell.X * 2 + 1);
                        position.y = cell.Z * 2 + 1;
                        break;
                    case ECubeFace.Right:
                        position.x = Size - 1;
                        position.z = cell.X * 2 + 1;
                        position.y = cell.Z * 2 + 1;
                        break;
                    case ECubeFace.Top:
                        position.y = Size - 1;
                        position.x = cell.X * 2 + 1;
                        position.z = cell.Z * 2 + 1;
                        break;
                    case ECubeFace.Bottom:
                        position.y = 0;
                        position.x = cell.X * 2 + 1;
                        position.z = (Size - 1) - (cell.Z * 2 + 1);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                var cube = new Cube();
                cube.SetPos(position.x, position.y, position.z);
                cube.SetIsWall(false);
                if (cell == maze.StartCell)
                {
                    cube.SetIsStart(true);
                    StartCube = cube;
                }

                if (cell == maze.EndCell)
                {
                    cube.SetIsGoal(cell == maze.EndCell);
                    EndCube = cube;
                }
                
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
                            switch (i)
                            {
                                case 0:
                                    wallPoition.x -= 1;
                                    isWall = cell.HasWall(EDirection.Right);
                                    break;
                                case 1:
                                    wallPoition.x += 1;
                                    isWall = cell.HasWall(EDirection.Left);
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
                        case ECubeFace.Left:
                            switch (i)
                            {
                                case 0:
                                    wallPoition.z -= 1;
                                    isWall = cell.HasWall(EDirection.Right);
                                    break;
                                case 1:
                                    wallPoition.z += 1;
                                    isWall = cell.HasWall(EDirection.Left);
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
                        case ECubeFace.Right:
                            switch (i)
                            {
                                case 0:
                                    wallPoition.z -= 1;
                                    isWall = cell.HasWall(EDirection.Left);
                                    break;
                                case 1:
                                    wallPoition.z += 1;
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
                        case ECubeFace.Top:
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
                                wallPoition.z -= 1;
                                isWall = cell.HasWall(EDirection.Bottom);
                                break;
                                case 3:
                                wallPoition.z += 1;
                                isWall = cell.HasWall(EDirection.Top);
                                break; 
                            }
                            break;
                        case ECubeFace.Bottom:
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
                                    wallPoition.z -= 1;
                                    isWall = cell.HasWall(EDirection.Top);
                                    break;
                                case 3:
                                    wallPoition.z += 1;
                                    isWall = cell.HasWall(EDirection.Bottom);
                                    break; 
                            }
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
                        case ECubeFace.Back:
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
                        case ECubeFace.Left:
                        case ECubeFace.Right:
                            switch (i)
                            {
                                case 0: 
                                    cornerPoition.z -= 1;
                                    cornerPoition.y -= 1;
                                    break;
                                case 1:
                                    cornerPoition.z += 1;
                                    cornerPoition.y -= 1;
                                    break;
                                case 2:
                                    cornerPoition.z -= 1;
                                    cornerPoition.y += 1;
                                    break;
                                case 3: 
                                    cornerPoition.z += 1;
                                    cornerPoition.y += 1;
                                    break;
                            }
                            break;
                        case ECubeFace.Top:
                        case ECubeFace.Bottom:
                            switch (i)
                            {
                                case 0: 
                                    cornerPoition.x -= 1;
                                    cornerPoition.z -= 1;
                                    break;
                                case 1:
                                    cornerPoition.x += 1;
                                    cornerPoition.z -= 1;
                                    break;
                                case 2:
                                    cornerPoition.x -= 1;
                                    cornerPoition.z += 1;
                                    break;
                                case 3:
                                    cornerPoition.x += 1;
                                    cornerPoition.z += 1;
                                    break;
                            }
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
    
    private static bool CubeExists(Vector3Int position)
    {
        return Cubes[position.x, position.y, position.z] != null;
    }
}
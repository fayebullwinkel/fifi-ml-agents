using UnityEngine;

public class Maze
{
    private Cube[,,] _cubes;
    
    public Cube[,,] GetCubes()
    {
        return _cubes;
    }

    public void SetCubes(Cube[,,] cubes)
    {
        _cubes = cubes;
    }

    public Cube GetCube(Vector3Int position)
    {
        return _cubes[position.x, position.y, position.z];
    }
}
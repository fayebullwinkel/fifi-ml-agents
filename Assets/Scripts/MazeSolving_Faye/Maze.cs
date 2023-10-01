using UnityEngine;

namespace MazeSolving_Faye
{
    public class Maze
    {
        private readonly Cube[,,] _cubes;
        private readonly Cube _startCube;
        private readonly Cube _endCube;

        public Maze(Cube[,,] cubes, Cube startCube, Cube endCube)
        {
            _cubes = cubes;
            _startCube = startCube;
            _endCube = endCube;
        }

        public Cube[,,] GetCubes()
        {
            return _cubes;
        }

        public Cube GetCube(Vector3Int position)
        {
            return _cubes[position.x, position.y, position.z];
        }

        public Cube GetStartCube()
        {
            return _startCube;
        }

        public Cube GetEndCube()
        {
            return _endCube;
        }
    }
}
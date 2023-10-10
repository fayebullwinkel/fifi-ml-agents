using UnityEngine;

namespace MazeSolving_Faye
{
    public class Cube
    {
        private float _weight;
        private bool _isWall;
        private bool _isCoreCube;
        private bool _isGoalCube;
        private bool _isStartCube;
        private GameObject _cubeObj;
        private int _x;
        private int _y;
        private int _z;

        private int _visitedCounter;

        public Vector3 GetRelativePosition(Vector3 localScale)
        {
            return new Vector3(
                (_x + 0.5f - (localScale.x / 2)) / localScale.x,
                (_y + 0.5f - (localScale.y / 2)) / localScale.y,
                (_z + 0.5f - (localScale.z / 2)) / localScale.z
            );
        }
        
        public void IncreaseVisitedCounter()
        {
            _visitedCounter++;
        }

        public int GetVisitedCounter()
        {
            return _visitedCounter;
        }

        public float GetWeight()
        {
            return _weight;
        }

        public void SetWeight(float weight)
        {
            _weight = weight;
        }

        public bool GetIsWall()
        {
            return _isWall;
        }

        public void SetIsWall(bool isWall)
        {
            _isWall = isWall;
        }

        public bool GetIsCoreCube()
        {
            return _isCoreCube;
        }

        public void IsCoreCube(bool isCoreCube)
        {
            _isCoreCube = isCoreCube;
        }

        public int GetX()
        {
            return _x;
        }

        public int GetY()
        {
            return _y;
        }

        public int GetZ()
        {
            return _z;
        }

        public void SetPos(int x, int y, int z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public bool GetIsStartCube()
        {
            return _isStartCube;
        }

        public void SetIsStart(bool isStartCube)
        {
            _isStartCube = isStartCube;
        }

        public Vector3Int GetPos()
        {
            return new Vector3Int(_x, _y, _z);
        }

        public void SetIsGoal(bool isGoal)
        {
            _isGoalCube = isGoal;
        }

        public bool GetIsGoalCube()
        {
            return _isGoalCube;
        }

        public void SetIsGoalCube(bool isGoalCube)
        {
            _isGoalCube = isGoalCube;
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace MazeDatatype
{
    public enum WallType
    {
        Horizontal,
        Vertical
    }
    
    public enum WallOrientation
    {
        Left,
        Right,
        Top,
        Bottom
    }
    
    public class MazeWall : MonoBehaviour
    {
        public WallType Type { get; set; }
        public List<MazeCell> Cells { get; private set; }

        public void InitMazeWall(WallType type, List<MazeCell> cells)
        {
            Type = type;
            Cells = cells;
        }
        
        public void AddCell(MazeCell cell)
        {
            Cells.Add(cell);
        }

        private void DestroyWall()
        {
            var mazeManager = MazeManager.Singleton;
            mazeManager.mazeGraph.RemoveWall(this);
            Destroy(gameObject);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("MazeGenerationAgent"))
            {
                DestroyWall();
            }
        }
    }
}
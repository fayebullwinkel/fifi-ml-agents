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
        public MazeCell Cell1 { get; set; }
        public MazeCell Cell2 { get; set; }

        public void InitMazeWall(WallType type, MazeCell cell1, MazeCell cell2)
        {
            Type = type;
            Cell1 = cell1;
            Cell2 = cell2;
        }
        
        public List<MazeCell> GetCells()
        {
            var cells = new List<MazeCell>();
            if (Cell1 != null) cells.Add(Cell1);
            if (Cell2 != null) cells.Add(Cell2);
            return cells;
        }

        private void DestroyWall()
        {
            Destroy(gameObject);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("MazeGenerationAgent"))
            {
                DestroyWall();
                // TODO: Remove wall from graph
                // TODO: Set Cell Corner Count
            }
        }
    }
}
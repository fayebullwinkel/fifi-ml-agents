using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MazeGeneration_vivi.MazeDatatype
{
    public enum WallType
    {
        Horizontal,
        Vertical
    }

    public class MazeWall : MonoBehaviour
    {
        public WallType Type { get; set; }
        public List<MazeCell> Cells { get; private set; }
        public Grid Grid { get; set; }
        public Maze Maze { get; set; }

        public void InitMazeWall(Maze maze, Grid grid, WallType type, List<MazeCell> cells)
        {
            Maze = maze;
            Grid = grid;
            Type = type;
            Cells = cells;
        }
        
        public void AddCell(MazeCell cell)
        {
            Cells.Add(cell);
        }

        public void DestroyWall()
        {
            Grid.RemoveWall(this);

            // Mark all cells that this wall was connected to as visited
            foreach (var cell in Cells.Where(cell => !cell.Visited))
            {
                Maze.MarkCellVisited(cell);
            }
            
            Debug.Log("Maze Meets Requirements: " + Maze.MeetsRequirements());
            
            Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("MazeGenerationAgent"))
            {
                DestroyWall();
            }
        }
    }
}
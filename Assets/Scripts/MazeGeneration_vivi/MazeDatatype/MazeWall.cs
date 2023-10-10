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

        public void DestroyWall()
        {
            Grid.RemoveWall(this);
            foreach (var corner in Grid.Corners)
            {
                if (corner.Walls.Contains(this))
                {
                    corner.Walls.Remove(this);
                }
            }

            // Mark all cells that this wall was connected to as visited
            foreach (var cell in Cells.Where(cell => !cell.Visited))
            {
                Maze.MarkCellVisited(cell);
            }

            if (!Maze.MeetsRequirements())
            {
                // Debug.Log("Maze Meets Requirements: " + Maze.MeetsRequirements());
            }
            
            Destroy(gameObject);
        }
    }
}
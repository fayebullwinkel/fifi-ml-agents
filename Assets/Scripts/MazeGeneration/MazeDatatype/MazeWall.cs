﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MazeDatatype
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
            var mazeManager = Maze.Singleton;
            mazeManager.Grid.RemoveWall(this);

            // Mark all cells that this wall was connected to as visited
            foreach (var cell in Cells.Where(cell => !cell.Visited))
            {
                mazeManager.Grid.MarkCellVisited(cell.X, cell.Z);
            }
            
            // Check if the maze still meets the requirements
            var meetsRequirements = mazeManager.Grid.MazeMeetsRequirements();
            Debug.Log("Maze meets requirements: " + meetsRequirements);
            
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
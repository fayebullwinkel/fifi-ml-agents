﻿using MazeGeneration_vivi.MazeDatatype;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Grid = MazeGeneration_vivi.MazeDatatype.Grid;

namespace MazeGeneration_vivi
{
    public class MazeGenerationAgent : Agent
    {
        [SerializeField] 
        protected float speed = 10f;

        [SerializeField]
        protected Maze Maze = null!;
        
        public Grid CurrentGrid {get; set;} = null!;
        public MazeCell CurrentCell {get; set;} = null!;

        protected void SetupMaze()
        {
            // generate new maze grid and place agent
            if (!Maze.GetIsMazeEmpty())
            {
                Maze.ClearMaze();
            }

            Maze.GenerateMaze();
            Maze.PlaceAgent();
        }

        protected void AddMazeObservations(VectorSensor sensor)
        {
            if (Maze == null)
            {
                return;
            }
            
            // Position of the Start Cell
            var startCell = Maze.StartCell;
            var grid = startCell.Grid;
            var startCellPosition = grid.GetPositionFromCell(startCell);
            sensor.AddObservation(startCellPosition);
            // The number of walls on each corner of the current cell
            foreach (var corner in CurrentCell.Corners)
            {
                sensor.AddObservation(corner.Walls.Count);
            }
            // Maze percentage of visited cells: float
            sensor.AddObservation(Maze.GetPercentageOfVisitedCells());
        }

        protected void GrantReward()
        {
            if (Maze.IsFinished())
            {
                // calculate the reward based on the percentage of visited cells, should be between 0 and 1
                var percentageOfVisitedCells = Maze.GetPercentageOfVisitedCells();
                var reward = Maze.IsValid() ? percentageOfVisitedCells : -1.0f;
                SetReward(reward);
                EndEpisode();
            }

            if (!Maze.MeetsRequirements())
            {
                SetReward(-1.0f);
                EndEpisode();
            }
        }
    }
}

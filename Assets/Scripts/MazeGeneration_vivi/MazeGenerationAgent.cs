using System.Linq;
using MazeGeneration_vivi.MazeDatatype;
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
        
        public Grid Grid {get; set;} = null!;
    
        protected int visitedCells;
        protected int notReachedNewCellCounter;
        protected bool reachedNewCell;

        protected void SetupMaze()
        {
            // reset visited cells
            visitedCells = 0;
        
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
            // Walls on each Corner in each Grid
            foreach (var wall in grid.Corners.SelectMany(corner => corner.Walls))
            {
                sensor.AddObservation(wall);
            }
            // Maze percentage of visited cells: float
            sensor.AddObservation(Maze.GetPercentageOfVisitedCells());

            // // Visited Cells
            // sensor.AddObservation(Maze.GetVisitedCells());
            // // // Walls: for each cell in each grid from Grids, how many walls are there
            // // foreach (var grid in Maze.Grids)
            // // {
            // //     foreach (var cell in grid.Value.Cells)
            // //     {
            // //         sensor.AddObservation(cell.Walls.Count);
            // //     }
            // // }
            // // Corners: for each corner in each grid from Grids, how many walls are there
            // foreach (var grid in Maze.Grids)
            // {
            //     foreach (var corner in grid.Value.Corners)
            //     {
            //         sensor.AddObservation(corner.Walls.Count);
            //     }
            // }
            // // Maze Start Cell: position
            // if (Maze.StartCell != null)
            // {
            //     var start = Maze.StartCell;
            //     sensor.AddObservation(start.X);
            //     sensor.AddObservation(start.Z);
            // }
            // // Maze longest path: length
            // sensor.AddObservation(Maze.Grid.FindLongestPath(Maze.Grid.StartCell).Count);
            // // Maze meets requirements: bool
            // sensor.AddObservation(Maze.Grid.MeetsRequirements());
            // // Maze is solvable: bool
            // sensor.AddObservation(Maze.Grid.IsValid());
            // // Maze percentage of visited cells: float
            // sensor.AddObservation(Maze.Grid.GetPercentageOfVisitedCells());
            // // Maze percentage of longest path: float
            // sensor.AddObservation(Maze.Grid.GetPercentageOfLongestPath());
            // // reached a new cell: bool
            // sensor.AddObservation(reachedNewCell);
        }

        protected void GrantReward()
        {
            if (Maze.IsFinished())
            {
                // TODO: calculate the reward based on the length of the path, should be between 0 and 1
                var reward = Maze.IsValid() ? 1.0f : -1.0f;
                SetReward(reward);
                EndEpisode();
            }

            if (!Maze.MeetsRequirements())
            {
                EndEpisode();
            }
            
            // var mazeIsFinished = Maze.EndCell != null;
            // if (mazeIsFinished)
            // {
            //     var longestPathCount = Maze.FindLongestPath(Maze.StartCell).Count;
            //     Debug.Log("Longest Path: " + longestPathCount);
            //     var isSolvable = Maze.IsValid();
            //     if(isSolvable)
            //     {
            //         SetReward(10.0f);
            //         EndEpisode();
            //     }
            //     else
            //     {
            //         // calculate the reward based on the length of the longest path, should be between 0 and -1 
            //         var cellCount = Maze.GetCellCount();
            //         var reward = -1.0f + (longestPathCount - 1) / (cellCount - 1);
            //         SetReward(reward);
            //         EndEpisode();
            //     }
            // }
            // var mazeMeetsRequirements = Maze.MeetsRequirements();
            // var percentageOfVisitedCells = Maze.GetPercentageOfVisitedCells();
            // if (mazeMeetsRequirements)
            // {
            //     // Existentially penalize the agent for taking too long to solve the maze
            //     AddReward(-0.001f);
            //
            //     // +0.1 reward for each visited cell
            //     var newVisitedCells = Maze.GetVisitedCells();
            //     var visitedCellsDelta = newVisitedCells - visitedCells;
            //     visitedCells = newVisitedCells;
            //     if (visitedCellsDelta > 0)
            //     {
            //         AddReward(visitedCellsDelta * 1.0f);
            //         notReachedNewCellCounter = 0;
            //         reachedNewCell = true;
            //     }
            //     // -0.01 reward for not reaching a new cell
            //     else
            //     {
            //         AddReward(-0.01f);
            //         notReachedNewCellCounter++;
            //         reachedNewCell = false;
            //     }
            //     if(notReachedNewCellCounter > 500)
            //     {
            //         // Penalize the agent for not reaching a new cell for too long
            //         var reward = -1.0f + percentageOfVisitedCells;
            //         SetReward(reward);
            //         EndEpisode();
            //     }
            // }
            // else
            // {
            //     // Penalize the agent for not meeting the requirements
            //     var reward = -5.0f + percentageOfVisitedCells;
            //     SetReward(reward);
            //     EndEpisode();
            // }
        }
    }
}

using MazeGeneration_vivi.MazeDatatype;
using MazeGeneration_vivi.MazeDatatype.Enums;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Grid = MazeGeneration_vivi.MazeDatatype.Grid;

namespace MazeGeneration_vivi
{
    public class MazeGenerationAgent : Agent
    {
        public MazeDatatype.Maze Maze = null!;
        public Grid CurrentGrid {get; set;} = null!;
        public MazeCell CurrentCell {get; set;} = null!;
        private int oldVisitedCellsCount;
        private int oldMovingDirection;
        private int newMovingDirection;
        
        // Initialize the agent and set the maze grid every new episode
        public override void OnEpisodeBegin()
        {
            // reset moving direction to 5 (no movement)
            oldMovingDirection = 5;
            newMovingDirection = 5;
            // reset visited cells count
            oldVisitedCellsCount = 0;
            
            // clear old maze, generate new maze grid and place agent
            if (!Maze.GetIsMazeEmpty())
            {
                Maze.ClearMaze();
            }

            Maze.GenerateMaze();
            Maze.PlaceAgent();
        }

        // Collect observations used by the neural network to make decisions
        public override void CollectObservations(VectorSensor sensor)
        {
            if (Maze == null)
            {
                return;
            }
            
            // Agent position
            sensor.AddObservation(transform.localPosition);
            // TODO: current cell position
            // oldMovingDirection
            sensor.AddObservation(oldMovingDirection);
            // newMovingDirection
            sensor.AddObservation(newMovingDirection);
            // TODO: path length from start to current cell
            // -> reward based on length of path
            // Position of the Start Cell
            var startCell = Maze.StartCell;
            var grid = startCell.Grid;
            var startCellPosition = grid.GetPositionFromCell(startCell);
            sensor.AddObservation(startCellPosition);
            // TODO: Start cell in maze
            // For each Neighbour of the current cell: 1 if visited, 0 if not visited
            foreach (var neighbour in CurrentCell.Neighbours)
            {
                sensor.AddObservation(neighbour.Visited ? 1 : 0);
            }
            // TODO: current cell visited (1) or not visited (0)
            // The number of walls on each corner of the current cell
            foreach (var corner in CurrentCell.Corners)
            {
                sensor.AddObservation(corner.Walls.Count);
            }
            // TODO: more!
            // Maze percentage of visited cells: float
            sensor.AddObservation(Maze.GetPercentageOfVisitedCells());
            // oldVisitedCellsCount: int
            sensor.AddObservation(oldVisitedCellsCount);
            // newVisitedCellsCount: int
            sensor.AddObservation(Maze.GetVisitedCells());
        }

        // Perform actions based on the decision of the neural network and reward the agent
        public override void OnActionReceived(ActionBuffers actions)
        {
            if(Maze.AgentIsMoving)
            {
                return;
            }

            // Perform action
            newMovingDirection = 5;
            switch (actions.DiscreteActions[0])
            {
                case 0:
                    Maze.MoveAgent(EDirection.Left);
                    newMovingDirection = 0;
                    break;
                case 1:
                    Maze.MoveAgent(EDirection.Right);
                    newMovingDirection = 1;
                    break;
                case 2:
                    Maze.MoveAgent(EDirection.Top);
                    newMovingDirection = 2;
                    break;
                case 3:
                    Maze.MoveAgent(EDirection.Bottom);
                    newMovingDirection = 3;
                    break;
                case 4:
                    Maze.PlaceGoal(transform.localPosition);
                    break;
                case 5:
                    break;
            }

            // Reward for action
            // termination condition: agent created a valid maze -> Task completed
            if (Maze.IsFinished())
            {
                // calculate the reward based on the percentage of visited cells, should be between 0 and 1
                var percentageOfVisitedCells = Maze.GetPercentageOfVisitedCells();
                var reward = Maze.IsValid() ? percentageOfVisitedCells : -1.0f;
                SetReward(reward);
                EndEpisode();
            }

            // termination condition: agent has removed too many walls -> Task failed
            if (!Maze.MeetsRequirements())
            {
                SetReward(-1.0f);
                EndEpisode();
            }
            
            // Reward for finding new cells // TODO: bool for rewarding small steps
            var visitedCellsCount = Maze.GetVisitedCells();
            if (visitedCellsCount > oldVisitedCellsCount)
            {
                AddReward(0.005f);
                oldVisitedCellsCount = visitedCellsCount;
                if(newMovingDirection != oldMovingDirection)
                {
                    AddReward(0.0025f);
                    oldMovingDirection = newMovingDirection;
                }
            }
            
            // Existential penalty
            AddReward(-0.00025f);
        }
    
        // Manual control of the agent
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var discreteActionsOut = actionsOut.DiscreteActions;
        
            // Map the keyboard input to the discrete action number
            // // 0: left, 1: right, 2: top, 3: bottom, 4: place goal
            var value = 5;
            if (Input.GetKey(KeyCode.A))
            {
                value = 0;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                value = 1;
            }
            else if (Input.GetKey(KeyCode.W))
            {
                value = 2;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                value = 3;
            }
            else if (Input.GetKey(KeyCode.Space))
            {
                value = 4;
            }
            discreteActionsOut[0] = value;
        }
    }
}

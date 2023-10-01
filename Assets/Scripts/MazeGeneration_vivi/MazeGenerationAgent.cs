using MazeGeneration_vivi.MazeDatatype;
using MazeGeneration_vivi.MazeDatatype.Enums;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Grid = MazeGeneration_vivi.MazeDatatype.Grid;

namespace MazeGeneration_vivi
{
    public class MazeGenerationAgent : Agent
    {
        public MazeDatatype.Maze Maze = null!;
        public Grid CurrentGrid {get; set;} = null!;
        public MazeCell CurrentCell {get; set;} = null!;
        public EManualInput ManualInput {get; set;} = EManualInput.None;
        private int oldVisitedCellsCount;
        
        // Initialize the agent and set the maze grid every new episode
        public override void OnEpisodeBegin()
        {
            ManualInput = EManualInput.None;
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

            // Observe the current cell
            AddCellObservation(sensor, CurrentCell);
            
            // Observe each neighbour of the current cell
            foreach (var neighbour in CurrentCell.Neighbours)
            {
                AddCellObservation(sensor, neighbour);
                // Observe each neighbour of the neighbour cell
                foreach (var neighbourOfNeighbour in neighbour.Neighbours)
                {
                    AddCellObservation(sensor, neighbourOfNeighbour);
                }
            }

            // Information about the start cell
            var startCell = Maze.StartCell;
            sensor.AddObservation((int)startCell.Grid.Face);
            sensor.AddObservation(startCell.X);
            sensor.AddObservation(startCell.Z);
            sensor.AddObservation(startCell.Visited);

            // Current Path Length in the Maze from start to current cell
            sensor.AddObservation(Maze.CurrentPath.Count);
            // Current Percentage of the Path Length in relation to the total number of cells in the maze
            sensor.AddObservation(Maze.CurrentPath.Count/Maze.GetCellCount());
            // Visited Cells in the Maze
            sensor.AddObservation(Maze.GetVisitedCells());
        }

        // Perform actions based on the decision of the neural network and reward the agent
        public override void OnActionReceived(ActionBuffers actions)
        {
            if(Maze.AgentIsMoving)
            {
                ManualInput = EManualInput.None;
                return;
            }

            // Perform action
            switch (actions.DiscreteActions[0])
            {
                case 0:
                    Maze.MoveAgent(EDirection.Left);
                    break;
                case 1:
                    Maze.MoveAgent(EDirection.Right);
                    break;
                case 2:
                    Maze.MoveAgent(EDirection.Top);
                    break;
                case 3:
                    Maze.MoveAgent(EDirection.Bottom);
                    break;
                case 4:
                    Maze.PlaceGoal(CurrentCell);
                    break;
                case 5:
                    break;
            }
            ManualInput = EManualInput.None;

            // Reward for action
            // termination condition: agent created a valid maze -> Task completed
            if (Maze.IsFinished())
            {
                // calculate the reward based on the percentage of visited cells, should be between 0 and 1
                var percentageOfPathLength = Maze.GetPercentageOfPathLength();
                var reward = Maze.IsValid() ? percentageOfPathLength * 10.0f : -1.0f;
                SetReward(reward);
                EndEpisode();
            }

            // termination condition: agent has removed too many walls -> Task failed
            if (!Maze.MeetsRequirements())
            {
                SetReward(-1.0f);
                EndEpisode();
            }
            
            // // Reward for finding new cells // TODO: bool for rewarding small steps
            // var visitedCellsCount = Maze.GetVisitedCells();
            // if (visitedCellsCount > oldVisitedCellsCount)
            // {
            //     AddReward(0.005f);
            //     oldVisitedCellsCount = visitedCellsCount;
            // }
            //
            // // Existential penalty
            // AddReward(-0.00025f);
        }
    
        // Manual control of the agent
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var discreteActionsOut = actionsOut.DiscreteActions;
        
            // Map the keyboard input to the discrete action number
            // // 0: left, 1: right, 2: top, 3: bottom, 4: place goal
            if (ManualInput == EManualInput.A)
            {
                discreteActionsOut[0] = 0;
            }
            else if (ManualInput == EManualInput.D)
            {
                discreteActionsOut[0] = 1;
            }
            else if (ManualInput == EManualInput.W)
            {
                discreteActionsOut[0] = 2;
            }
            else if (ManualInput == EManualInput.S)
            {
                discreteActionsOut[0] = 3;
            }
            else if (ManualInput == EManualInput.Space)
            {
                discreteActionsOut[0] = 4;
            }
            else if (ManualInput == EManualInput.None)
            {
                discreteActionsOut[0] = 5;
            }
        }

        private void AddCellObservation(VectorSensor sensor, MazeCell cell)
        {
            // Cube Face and Cell Position and Visited Flag of the Neighbour Cell
            sensor.AddObservation((int)cell.Grid.Face);
            sensor.AddObservation(cell.X);
            sensor.AddObservation(cell.Z);
            sensor.AddObservation(cell.Visited);
            // The number of walls on each corner of the neighbour cell
            foreach (var corner in cell.Corners)
            {
                sensor.AddObservation(corner.Walls.Count);
            }
        }
    }
}

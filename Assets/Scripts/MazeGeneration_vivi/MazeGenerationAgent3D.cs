using MazeGeneration_vivi.MazeDatatype.Enums;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace MazeGeneration_vivi
{
    public sealed class MazeGenerationAgent3D: MazeGenerationAgent
    {
        private int oldMovingDirection;
        private int newMovingDirection;
        
        public override void OnEpisodeBegin()
        {
            // reset moving direction to 5 (no movement)
            oldMovingDirection = 5;
            newMovingDirection = 5;
            SetupMaze();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            // Agent position
            sensor.AddObservation(transform.localPosition);
            // oldMovingDirection
            sensor.AddObservation(oldMovingDirection);
            // newMovingDirection
            sensor.AddObservation(newMovingDirection);
        
            AddMazeObservations(sensor);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            if(Maze.AgentIsMoving)
            {
                return;
            }

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

            GrantReward();
            if(newMovingDirection != oldMovingDirection)
            {
                AddReward(0.0025f);
                oldMovingDirection = newMovingDirection;
            }
        }
    
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var discreteActionsOut = actionsOut.DiscreteActions;
        
            // Manual control of the agent
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
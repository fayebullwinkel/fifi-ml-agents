using MazeGeneration_vivi.MazeDatatype.Enums;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace MazeGeneration_vivi
{
    public sealed class MazeGenerationAgent3D: MazeGenerationAgent
    {
        public override void OnEpisodeBegin()
        {
            SetupMaze();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            // Agent position
            sensor.AddObservation(transform.localPosition);
        
            AddMazeObservations(sensor);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            if(Maze.AgentIsMoving)
            {
                return;
            }
            // get the first action that is not 0 from the discrete actions from 0 to 3 -> Moving Actions
            var action = -1;
            for (var i = 0; i < 4; i++)
            {
                if (actions.DiscreteActions[i] > 0)
                {
                    action = i;
                    break;
                }
            }
            if (action != -1)
            {
                var direction = (EDirection)action;
                Maze.MoveAgent(direction);
            }
            else
            {
                // Place End Cell
                var placeGoal = actions.DiscreteActions[4] > 0;
                if (placeGoal)
                {
                    Maze.PlaceGoal(transform.localPosition);
                }
            }

            GrantReward();
        }
    
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var discreteActionsOut = actionsOut.DiscreteActions;
        
            // Manual control of the agent
            // 0: left, 1: right, 2: top, 3: bottom, 4: place goal
            discreteActionsOut[0] = Input.GetKey(KeyCode.A) ? 1 : 0;
            discreteActionsOut[1] = Input.GetKey(KeyCode.D) ? 1 : 0;
            discreteActionsOut[2] = Input.GetKey(KeyCode.W) ? 1 : 0;
            discreteActionsOut[3] = Input.GetKey(KeyCode.S) ? 1 : 0;
            discreteActionsOut[4] = Input.GetKey(KeyCode.Space) ? 1 : 0;
        }
    }
}
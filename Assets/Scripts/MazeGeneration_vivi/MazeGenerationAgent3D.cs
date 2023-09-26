using MazeGeneration_vivi.MazeDatatype;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace MazeGeneration_vivi
{
    public sealed class MazeGenerationAgent3D: MazeGenerationAgent
    {
        private void Start()
        {
        }
    
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
            var controlSignal = Vector3.zero;
            controlSignal.x = actions.ContinuousActions[0];
            controlSignal.z = actions.ContinuousActions[1];
        
            var movement = controlSignal * (speed * Time.deltaTime);
            transform.Translate(movement);
            // // look in movement direction
            // if (controlSignal != Vector3.zero) {
            //     var targetRotation = Quaternion.LookRotation(controlSignal);
            //     transform.localRotation = Quaternion.Slerp(transform.rotation, targetRotation, 1 * Time.deltaTime);
            // }
        
            // // Place End Cell
            // var placeGoal = actions.DiscreteActions[0] > 0;
            // if (placeGoal)
            // {
            //     Maze.PlaceGoal(transform.localPosition);
            // }
        
            GrantReward();
        }
    
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;
            var discreteActionsOut = actionsOut.DiscreteActions;
        
            // Manual control of the agent
            continuousActionsOut[0] = Input.GetAxis("Horizontal");
            continuousActionsOut[1] = Input.GetAxis("Vertical");
            discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
        }
    }
}
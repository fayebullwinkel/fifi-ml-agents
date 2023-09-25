﻿using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MazeGenerationAgent_Sphere : MazeGenerationAgent
{
    private Rigidbody rBody;

    private void Awake()
    {
        mazeManager = MazeManager.Singleton;
        rBody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // reset angel velocity
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        
        SetupMaze();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent position
        sensor.AddObservation(transform.localPosition);
        // Agent velocity
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
        
        AddMazeObservations(sensor);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Perform Action to solve the task - Perform movement in x and z
        var controlSignal = Vector3.zero;
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        rBody.AddForce(controlSignal * speed);

        // Place End Cell
        var placeGoal = actions.DiscreteActions[0] > 0;
        if (placeGoal)
        {
            mazeManager.mazeGraph.PlaceGoal(transform.localPosition);
        }

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
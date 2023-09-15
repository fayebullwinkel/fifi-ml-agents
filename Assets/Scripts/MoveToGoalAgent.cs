using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class MoveToGoalAgent : Agent
{
    [SerializeField] 
    private Transform goal;
    [SerializeField]
    private float speed = 10f;

    private Rigidbody rBody;
    private Vector3 startPos;

    private void Start()
    {
        rBody = GetComponent<Rigidbody>();
        startPos = transform.localPosition;
    }

    // On Begin of each episode: Reset the agent and goal positions
    // One Episode lasts until the agent reaches the goal, falls of the platform or times out
    public override void OnEpisodeBegin()
    {
        // Task failed: Agent fell of the platform - Reset agent position and velocity
        if (transform.localPosition.y < 0)
        {
            transform.localPosition = startPos;
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
        }

        // Move the goal to a new spot
        goal.localPosition = new Vector3(
            UnityEngine.Random.value * 8 - 4,
            1,
            UnityEngine.Random.value * 8 - 4);
    }
    
    // Collect observations from the environment
    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(goal.localPosition);
        sensor.AddObservation(transform.localPosition);

        // Agent velocity
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
    }

    // Receive actions and assign the reward
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Perform Action to solve the task - Perform movement in x and z direction to reach the goal
        var controlSignal = Vector3.zero;
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        rBody.AddForce(controlSignal * speed);
        
        // Reward for solving the task
        var distanceToGoal = Vector3.Distance(transform.localPosition, goal.localPosition);
        // Agent reached the goal
        if(distanceToGoal < 1.42f)
        {
            SetReward(1.0f);
            EndEpisode();
        }
        
        // Agent fell of the platform
        if(transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }
    
    // For manual control of the agent 
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        
        // Manual control of the agent
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}
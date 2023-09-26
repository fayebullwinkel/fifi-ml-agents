using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MazeAgent : Agent
{
    private MazeController _mazeController;

    public override void Initialize()
    {
        base.Initialize();
        _mazeController = GameObject.Find("MazeController").GetComponent<MazeController>();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int discreteAction = actionBuffers.DiscreteActions[0];
        
        float cubeSize = _mazeController.GetReferenceCubeSize();

        switch (discreteAction)
        {
            case 0:
                // agent stays in place
                break;
            case 1:
                // forward in world space
                MoveInGlobalDirection(Vector3.up, cubeSize);
                break;
            case 2:
                // backward in world space
                MoveInGlobalDirection(Vector3.down, cubeSize);
                break;
            case 3:
                // left in world space
                MoveInGlobalDirection(Vector3.left, cubeSize);
                break;
            case 4:
                // right in world space
                MoveInGlobalDirection(Vector3.right, cubeSize);
                break;
        }

        // Apply a tiny negative reward every step to encourage action
        if (MaxStep > 0)
        {
            AddReward(-1f / MaxStep);
        }
    }

    void MoveInGlobalDirection(Vector3 globalDirection, float cubeSize)
    {
        // Move the agent in the global direction
        transform.position += globalDirection * cubeSize;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        // Initialize action values
        discreteActionsOut.Array[0] = 0;

        // Determine the agent's current orientation (relative to the maze faces)
        var transform1 = transform;
        Vector3 upDir = transform1.up;
        Vector3 rightDir = transform1.right;

        // Check keyboard input to move along the surface
        if (Input.GetKey(KeyCode.W))
        {
            // Move forward along the surface
            discreteActionsOut.Array[0] = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            // Move backward along the surface
            discreteActionsOut.Array[0] = 2;
        }

        if (Input.GetKey(KeyCode.A))
        {
            // Move left along the surface
            discreteActionsOut.Array[0] = 3;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            // Move right along the surface
            discreteActionsOut.Array[0] = 4;
        }

        // Apply orientation adjustments based on the movement
        var transform2 = transform;
        transform2.up = upDir;
        transform2.right = rightDir;
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("Epsiode begins");
        _mazeController.ResetArea();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // number of observations corresponds to mazeAgentPrefab > Behavior Parameters > Vector Observation Space Size
        sensor.AddObservation(transform.position);
        
        GameObject goal = GameObject.FindWithTag("EndCube");
        if (goal)
        {
            sensor.AddObservation(goal.transform.position);
        }
        
        // 2 x Vector3 = 6 values
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("EndCube"))
        {
            SetReward(10.0f);
            EndEpisode(); 
        }
        else if (other.gameObject.CompareTag("Wall")) // won't work if we are not moving into walls! 
        {
            AddReward(-0.1f);
        }
    }
}
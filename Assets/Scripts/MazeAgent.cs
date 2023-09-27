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

    private bool IsAgentOnEdge()
    {
        GameObject floorCube = GameObject.FindWithTag("Floor");
        Bounds agentBounds = transform.GetComponent<Collider>().bounds;
        Bounds largerCubeBounds = floorCube.GetComponent<Collider>().bounds;

        Vector3 agentCenter = agentBounds.center;
        Vector3 largerCubeCenter = largerCubeBounds.center;

        float agentRadius = Mathf.Max(Mathf.Max(agentBounds.extents.x, agentBounds.extents.y), agentBounds.extents.z);
        float largerCubeRadius = Mathf.Max(Mathf.Max(largerCubeBounds.extents.x, largerCubeBounds.extents.y),
            largerCubeBounds.extents.z);

        float distance = Vector3.Distance(agentCenter, largerCubeCenter);

        if (distance > (agentRadius + largerCubeRadius + 0.2f))
        {
            return true;
        }

        return false;
    }

    private void MoveInGlobalDirection(Vector3 globalDirection, float cubeSize)
    {
        // Move the agent in the global direction
        transform.position += globalDirection * cubeSize;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        // Initialize action values
        discreteActionsOut.Array[0] = 0;

        // agent's current orientation (relative to the maze faces)
        var transform1 = transform;
        Vector3 upDir = transform1.up;
        Vector3 rightDir = transform1.right;

        // Check keyboard input to move along the surface
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut.Array[0] = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut.Array[0] = 2;
        }

        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut.Array[0] = 3;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut.Array[0] = 4;
        }

        // orientation adjustments based on the movement
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("EndCube"))
        {
            SetReward(10.0f);
            EndEpisode();
        }

        if (collision.gameObject.CompareTag("Wall")) // TODO: won't work if we are not moving into walls! 
        {
            AddReward(-0.1f);
        }
    }
}
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MazeAgent : Agent
{
    private MazeController _mazeController;
    private Rigidbody _rb;
    private readonly float _moveSpeed = 5f;

    public override void Initialize()
    {
        base.Initialize();
        _mazeController = GameObject.Find("MazeController").GetComponent<MazeController>();
        _rb = GetComponent<Rigidbody>();
        
        if (_mazeController == null)
        {
            Debug.LogError("MazeController not found. Make sure the MazeController object is in the scene.");
        }

        if (_rb == null)
        {
            Debug.LogError("Rigidbody component not found on the MazeAgent.");
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int moveAction = actionBuffers.DiscreteActions[0];

        // Define movement directions based on actions
        Vector3 moveDirection = Vector3.zero;

        // Apply movement based on moveAction
        switch (moveAction)
        {
            case 1:
                moveDirection = transform.forward;
                break;
            case 2: 
                moveDirection = -transform.forward;
                break;
            case 3: 
                moveDirection = transform.up;
                break;
            case 4: 
                moveDirection = -transform.up;
                break;
        }

        // Move the agent
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, transform.forward, out hit, 0.9f, LayerMask.GetMask("Wall")))
        {
            transform.position += moveDirection * (_moveSpeed * Time.fixedDeltaTime);
        }
        
        // Apply a tiny negative reward every step to encourage action
        if (MaxStep > 0)
        {
            AddReward(-1f / MaxStep);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        // Initialize action values
        discreteActionsOut.Array[0] = 0;

        // Determine the agent's current orientation (relative to the larger cube's faces)
        var transform1 = transform;
        Vector3 forwardDir = transform1.forward;
        Vector3 rightDir = transform1.right;

        // Check keyboard input to move along the surface
        if (Input.GetKey(KeyCode.A))
        {
            // Move forward along the surface
            discreteActionsOut.Array[0] = 1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            // Move backward along the surface
            discreteActionsOut.Array[0] = 2;
        }

        if (Input.GetKey(KeyCode.W))
        {
            // Move left along the surface
            discreteActionsOut.Array[0] = 3;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            // Move right along the surface
            discreteActionsOut.Array[0] = 4;
        }

        // Apply orientation adjustments based on the movement
        var transform2 = transform;
        transform2.forward = forwardDir;
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
        else if (other.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.1f);
        }
    }
}
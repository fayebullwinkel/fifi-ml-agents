using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MazeAgent : Agent
{
    private MazeController _mazeController;
    private Rigidbody _rigidbody;
    private readonly float _moveSpeed = 10f;
    
    public override void Initialize() {
        base.Initialize();
        _mazeController = GameObject.Find("MazeController").GetComponent<MazeController>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Use discrete actions to move the agent
        int movementAction = actionBuffers.DiscreteActions[0];
        
        // Map discrete actions to movement directions
        Vector3 moveDirection = Vector3.zero;

        switch (movementAction)
        {
            case 0:
                moveDirection = transform.forward; // Move forward
                break;
            case 1:
                moveDirection = -transform.forward; // Move backward
                break;
            case 2:
                moveDirection = -transform.right; // Move left
                break;
            case 3:
                moveDirection = transform.right; // Move right
                break;
        }
        
        // Apply a force to the Rigidbody to move the agent
        _rigidbody.AddForce(moveDirection * _moveSpeed);
    }

    public override void OnEpisodeBegin()
    {
        // TODO: implement
        //_mazeController.ResetArea();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // TODO: implement
    }
    
}

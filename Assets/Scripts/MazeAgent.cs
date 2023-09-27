using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MazeAgent : Agent
{
    private MazeController _mazeController;
    private Vector3Int _currPos;
    private Cube[,,] _mazeArray;
    
    // Heuristic
    private MovementDirection _myNextMove = MovementDirection.Nothing;

    private enum MovementDirection
    {
        Nothing,
        XPos,
        XNeg,
        YPos,
        YNeg,
        ZPos,
        ZNeg
    }

    public override void Initialize()
    {
        base.Initialize();
        _mazeController = GameObject.Find("MazeController").GetComponent<MazeController>();
        _currPos = _mazeController.GetStartPosition();
        _mazeArray = _mazeController.GetMazeArray();
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("Epsiode begins");
        _mazeController.ResetArea();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var discreteAction = (MovementDirection)actionBuffers.DiscreteActions[0];

        if (IsActionLegal(discreteAction))
        {
            _currPos += GetDirectionVector(discreteAction);
            // convert to world coordinates
            var newPosUnity = _mazeArray[_currPos.x, _currPos.y, _currPos.z]
                .GetCubePosition(_mazeController.transform.localScale);
            transform.localPosition = newPosUnity;
        }
        else
        {
            AddReward(-0.1f);
        }

        // Apply a tiny negative reward every step to encourage action
        if (MaxStep > 0)
        {
            AddReward(-1f / MaxStep);
        }
        
        actionBuffers.DiscreteActions.Array[0] = (int)MovementDirection.Nothing;
    }

    public void Update()
    {
        if (_myNextMove != MovementDirection.Nothing) return;
        
        var moveAction = MovementDirection.Nothing;
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            moveAction = MovementDirection.XPos;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            moveAction = MovementDirection.XNeg;
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            moveAction = MovementDirection.YPos;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            moveAction = MovementDirection.YNeg;
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            moveAction = MovementDirection.ZPos;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            moveAction = MovementDirection.ZNeg;
        }

        _myNextMove = moveAction;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        actionsOut.DiscreteActions.Array[0] = (int)_myNextMove;
        _myNextMove = MovementDirection.Nothing;
    }

    //checks in all dimensions for possible next steps
    private List<MovementDirection> GetPossibleActions(Cube[,,] mazeArray, Vector3Int currPos)
    {
        var possibleActions = new List<MovementDirection>();
        
        foreach (MovementDirection dir in Enum.GetValues(typeof(MovementDirection)))
        {
            if (IsActionLegal(dir))
            {
                possibleActions.Add(dir);
            }
        }

        return possibleActions;
    }

    private bool IsActionLegal(MovementDirection direction)
    {
        var newPosition = _currPos + GetDirectionVector(direction);
        return IsInRange(newPosition) && IsSurfaceCube(newPosition) && !GetCube(newPosition).GetIsWall();
    }

    private Cube GetCube(Vector3Int coords)
    {
        return _mazeArray[coords.x, coords.y, coords.z];
    }

    // checks if cube outside of the maze bounds
    private bool IsInRange(Vector3 newPosition)
    {
        return newPosition.x >= 0 && newPosition.x < _mazeController.GetMazeArray().GetLength(0) &&
               newPosition.y >= 0 && newPosition.y < _mazeController.GetMazeArray().GetLength(1) &&
               newPosition.z >= 0 && newPosition.z < _mazeController.GetMazeArray().GetLength(2);
    }

    // check if cube is on the surface
    private bool IsSurfaceCube(Vector3Int newPosition)
    {
        var surfaceCubePositions = _mazeController.GetSurfaceCubePositions();
        return surfaceCubePositions.Contains(newPosition);
    }

    // converts enums to vectors
    private static Vector3Int GetDirectionVector(MovementDirection val)
    {
        return val switch
        {
            MovementDirection.XPos => new Vector3Int(1, 0, 0),
            MovementDirection.XNeg => new Vector3Int(-1, 0, 0),
            MovementDirection.YPos => new Vector3Int(0, 1, 0),
            MovementDirection.YNeg => new Vector3Int(0, -1, 0),
            MovementDirection.ZPos => new Vector3Int(0, 0, 1),
            MovementDirection.ZNeg => new Vector3Int(0, 0, -1),
            _ => new Vector3Int(0, 0, 0)
        };
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // number of observations corresponds to mazeAgentPrefab > Behavior Parameters > Vector Observation Space Size
        sensor.AddObservation(transform.position);

        var goal = GameObject.FindWithTag("EndCube");
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
            Debug.Log("End reached");
            SetReward(10.0f);
            EndEpisode();
        }
    }
}
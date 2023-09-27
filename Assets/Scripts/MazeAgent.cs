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

        var possibleActions = GetPossibleActions(_mazeArray, _currPos);

        if (possibleActions.Contains(discreteAction))
        {
            _currPos += GetDirectionVector(discreteAction);
            // convert to world coordinates
            var newPosUnity = _mazeArray[_currPos.x, _currPos.y, _currPos.z]
                .GetCubePosition(_mazeController.transform.localScale);
            transform.localPosition = newPosUnity;
        }
        else
        {
            {
                AddReward(-0.1f);
            }
        }

        // Apply a tiny negative reward every step to encourage action
        if (MaxStep > 0)
        {
            AddReward(-1f / MaxStep);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        MovementDirection moveAction = MovementDirection.Nothing;

        if (Input.GetKey(KeyCode.Q))
        {
            moveAction = MovementDirection.XPos;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            moveAction = MovementDirection.XNeg;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            moveAction = MovementDirection.YPos;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            moveAction = MovementDirection.YNeg;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            moveAction = MovementDirection.ZPos;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveAction = MovementDirection.ZNeg;
        }

        actionsOut.DiscreteActions.Array[0] = (int)moveAction;
    }

    //checks in all dimensions for possible next steps
    private List<MovementDirection> GetPossibleActions(Cube[,,] mazeArray, Vector3Int currPos)
    {
        var possibleActions = new List<MovementDirection>();

        // iterate enum values
        foreach (MovementDirection dir in Enum.GetValues(typeof(MovementDirection)))
        {
            var direction = GetDirectionVector(dir);
            var newPosition = currPos + direction;

            if (IsInRange(newPosition) && IsSurfaceCube(newPosition) &&
                !mazeArray[newPosition.x, newPosition.y, newPosition.z].GetIsWall())
            {
                // TODO: what to do with already visited cubes?
                possibleActions.Add(dir);
            }
        }

        return possibleActions;
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
        switch (val)
        {
            case MovementDirection.XPos:
                return new Vector3Int(1, 0, 0);
            case MovementDirection.XNeg:
                return new Vector3Int(-1, 0, 0);
            case MovementDirection.YPos:
                return new Vector3Int(0, 1, 0);
            case MovementDirection.YNeg:
                return new Vector3Int(0, -1, 0);
            case MovementDirection.ZPos:
                return new Vector3Int(0, 0, 1);
            case MovementDirection.ZNeg:
                return new Vector3Int(0, 0, -1);
            default:
                return new Vector3Int(0, 0, 0);
        }
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
            SetReward(10.0f);
            EndEpisode();
        }
    }
}
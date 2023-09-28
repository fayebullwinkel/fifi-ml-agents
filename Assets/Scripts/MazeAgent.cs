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
    private Maze _maze;

    // Heuristic
    private MovementDirection _myNextMove = MovementDirection.Nothing;

    private enum MovementDirection
    {
        XPos,
        XNeg,
        YPos,
        YNeg,
        ZPos,
        ZNeg,
        Nothing
    }

    public override void Initialize()
    {
        base.Initialize();
        _mazeController = GameObject.Find("MazeController").GetComponent<MazeController>();
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("Epsiode begins");
        _mazeController.ResetArea();
        _currPos = _mazeController.GetStartPosition();
        _maze = _mazeController.GetMaze();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var discreteAction = (MovementDirection)actionBuffers.DiscreteActions[0];

        if (IsActionLegal(discreteAction))
        {
            _currPos += GetVectorToEnum(discreteAction);
            // convert to world coordinates
            var newPosUnity = _maze.GetCube(_currPos)
                .GetRelativePosition(_mazeController.transform.localScale);
            transform.localPosition = newPosUnity;
            
            if (GetPossibleActions(_currPos).Count == 1)
            {
                Debug.Log("Dead end reached");
                SetReward(-10.0f);
            }

            if (_maze.GetCube(_currPos).GetIsGoalCube())
            {
                Debug.Log("End reached");
                SetReward(10.0f);
                EndEpisode();
            }
        }
        else
        {
            AddReward(-20.0f);
        }

        // apply a tiny negative reward every step to encourage action
        if (MaxStep > 0)
        {
            AddReward(-1f / MaxStep);
        }

        actionBuffers.DiscreteActions.Array[0] = (int)MovementDirection.Nothing;
    }
    
    private List<MovementDirection> GetPossibleActions(Vector3Int currPos)
    {
        var possibleActions = new List<MovementDirection>();

        // iterate enum values
        foreach (MovementDirection dir in Enum.GetValues(typeof(MovementDirection)))
        {
            var direction = GetVectorToEnum(dir);
            var newPosition = currPos + direction;

            if (IsInRange(newPosition) && IsCubeOnSurface(newPosition) && !_maze.GetCube(newPosition).GetIsWall())
            {
                possibleActions.Add(dir);
            }
        }

        return possibleActions;
    }
    
    // checks if cube outside of the maze bounds
    private bool IsInRange(Vector3 newPosition)
    {
        return newPosition.x >= 0 && newPosition.x < _maze.GetCubes().GetLength(0) &&
               newPosition.y >= 0 && newPosition.y < _maze.GetCubes().GetLength(1) &&
               newPosition.z >= 0 && newPosition.z < _maze.GetCubes().GetLength(2);
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

    private bool IsActionLegal(MovementDirection direction)
    {
        var newPosition = _currPos + GetVectorToEnum(direction);
        return IsCubeInsideMaze(newPosition) && IsCubeOnSurface(newPosition) && !_maze.GetCube(newPosition).GetIsWall();
    }

    private bool IsCubeInsideMaze(Vector3 newPosition)
    {
        return newPosition.x >= 0 && newPosition.x < _mazeController.GetMaze().GetCubes().GetLength(0) &&
               newPosition.y >= 0 && newPosition.y < _mazeController.GetMaze().GetCubes().GetLength(1) &&
               newPosition.z >= 0 && newPosition.z < _mazeController.GetMaze().GetCubes().GetLength(2);
    }

    private bool IsCubeOnSurface(Vector3Int newPosition)
    {
        var surfaceCubePositions = _mazeController.GetSurfaceCubePositions();
        return surfaceCubePositions.Contains(newPosition);
    }

    private static Vector3Int GetVectorToEnum(MovementDirection val)
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
        // number of observations corresponds to mazeAgentPrefab > behavior parameters > vector observation space size
        sensor.AddObservation(_currPos);

        sensor.AddObservation(_maze.GetEndCube().GetPos());

        var cubes = _maze.GetCubes();
        for (var z = 0; z < cubes.GetLength(2); z++)
        {
            for (var y = 0; y < cubes.GetLength(1); y++)
            {
                for (var x = 0; x < cubes.GetLength(0); x++)
                {
                    // Check if the cube is on the surface (i.e., on the outermost layer)
                    if (z == 0 || z == cubes.GetLength(2) - 1 || y == 0 || y == cubes.GetLength(1) - 1 || x == 0 ||
                        x == cubes.GetLength(0) - 1)
                    {
                        sensor.AddObservation(_maze.GetCube(new Vector3Int(x, y, z)).GetIsWall()); // depends on dimensions of maze, right now 5x5x5 - 3x3x3 = 98
                    }
                    else
                    {
                        sensor.AddObservation(true); // observe inner cubes for consistent data
                    }
                }
            }
        }

        // 2 x Vector3 + 98 = 104
    }
}
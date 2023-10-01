using System;
using System.Collections.Generic;
using MazeSolving_Faye;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


public class MazeAgent : Agent
{
    private MazeController _mazeController;
    private Vector3Int _currPos;
    private Maze _maze;
    private Vector3Int _startPos;
    private MazeBuilder _mazeBuilder;
    private GameObject _endCubeObj;
    private GameObject _agentObj;
    private Vector3 _mazePosition;
    private GameObject _mazeObj;
    private bool _propertiesSet;
    private int _visitedCellCount;

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
        _currPos = _startPos;
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (_propertiesSet)
        {
            _propertiesSet = false;
            _currPos = _startPos;
        }
        var discreteAction = (MovementDirection)actionBuffers.DiscreteActions[0];

        if (IsActionLegal(discreteAction))
        {
            _currPos += GetVectorToEnum(discreteAction);
            
            PenalizeAmountVisited(_currPos);
            // convert to world coordinates
            var newPosUnity = _maze.GetCube(_currPos)
                .GetRelativePosition(_mazeController.transform.localScale);
            transform.localPosition = newPosUnity;
            
            if (GetPossibleActions(_currPos).Count == 1)
            {
                Debug.Log("Dead end reached");
                AddReward(-1.0f);
            }

            if (_maze.GetCube(_currPos).GetIsGoalCube())
            {
                Debug.Log("Goal reached");
                AddReward(1.0f);
                _mazeController.ResetArea(_mazeObj, _mazeBuilder, _endCubeObj, _agentObj, _mazePosition);
                EndEpisode();
            }
        }
        else
        {
            AddReward(-1.0f);
        }

        // negative reward for each step
        if (MaxStep > 0)
        {
            AddReward(-0.1f);
        }
        
        actionBuffers.DiscreteActions.Array[0] = (int)MovementDirection.Nothing;
    }
    
    private void PenalizeAmountVisited(Vector3Int currPos)
    {
        _maze.GetCube(currPos).IncreaseVisitedCounter();
        var visitedCounter = _maze.GetCube(currPos).GetVisitedCounter();
        
        switch (visitedCounter)
        {
            case 1:
                _visitedCellCount++;
                break;
        }

        var penalty = -(1.0f - 1.0f / visitedCounter);
        AddReward(penalty);
    }
    
    private List<MovementDirection> GetPossibleActions(Vector3Int currPos)
    {
        var possibleActions = new List<MovementDirection>();

        // iterate enum values
        foreach (MovementDirection dir in Enum.GetValues(typeof(MovementDirection)))
        {
            if (dir == MovementDirection.Nothing) continue;
            var direction = GetVectorToEnum(dir);
            var newPosition = currPos + direction;

            if (IsCubeInMazeBounds(newPosition) && IsCubeOnSurface(newPosition, false) && !_maze.GetCube(newPosition).GetIsWall())
            {
                possibleActions.Add(dir);
            }
        }

        return possibleActions;
    }
    
    private bool IsActionLegal(MovementDirection direction)
    {
        var newPosition = _currPos + GetVectorToEnum(direction);
        
        return IsCubeInMazeBounds(newPosition) && IsCubeOnSurface(newPosition, true) && !_maze.GetCube(newPosition).GetIsWall();
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

    private bool IsCubeInMazeBounds(Vector3 newPosition)
    {
        return newPosition.x >= 0 && newPosition.x < _maze.GetCubes().GetLength(0) &&
               newPosition.y >= 0 && newPosition.y < _maze.GetCubes().GetLength(1) &&
               newPosition.z >= 0 && newPosition.z < _maze.GetCubes().GetLength(2);
    }

    private bool IsCubeOnSurface(Vector3Int newPosition, bool checkForWalls)
    {
        var surfaceCubePositions = GetSurfaceCubePositions(checkForWalls);
        return surfaceCubePositions.Contains(newPosition);
    }

    private List<Vector3Int> GetSurfaceCubePositions(bool checkForWalls)
    {
        var positions = new List<Vector3Int>();
        var cubes = _maze.GetCubes();

        for (var d = 0; d < cubes.GetLength(2); d++)
        {
            for (var h = 0; h < cubes.GetLength(1); h++)
            {
                for (var w = 0; w < cubes.GetLength(0); w++)
                {
                    // Check if the cube is on the surface
                    if (d == 0 || d == cubes.GetLength(2) - 1 || h == 0 || h == cubes.GetLength(1) - 1 || w == 0 ||
                        w == cubes.GetLength(0) - 1)
                    {
                        if (!checkForWalls || !cubes[w, h, d].GetIsWall())
                        {
                            positions.Add(new Vector3Int(w, h, d));
                        }
                    }
                }
            }
        }

        return positions;
    }
    
    static Vector3Int GetVectorToEnum(MovementDirection val)
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
                        sensor.AddObservation(_maze.GetCube(new Vector3Int(x, y, z)).GetIsWall()); // depends on dimensions of maze, right now 7x7x7 - 5x5x5 = 218
                    }
                    else
                    {
                        sensor.AddObservation(true); // observe inner cubes for consistent data
                    }
                }
            }
        }
        
        sensor.AddObservation(_visitedCellCount);

        // 2 x Vector3 + 343 + 1 = 350
        
        // VectorSpace dynamisch anpassen im Code? 
        // TODO: Ausprobieren, schau so und so viel in eine bestimmte Richtung -> auch dynamisch, weil nur innerhalb der Maze bounds
        // Überlegung: Seite auf der der Würfel ist und Seite, auf der man selbst gerade ist?
        // Überlegung: Sind die Nachbarn schon besucht 
    }

    public void SetStartPosition(Vector3Int startPos)
    {
        _startPos = startPos;
    }
    public void SetMaze(Maze maze)
    {
        _maze = maze;
    }
    public void SetMazeBuilder(MazeBuilder mazeBuilder)
    {
        _mazeBuilder = mazeBuilder;
    }
    public void SetEndCubeObj(GameObject endCubeObj)
    {
        _endCubeObj = endCubeObj;
    }
    public void SetAgentObj(GameObject agentObj)
    {
        _agentObj = agentObj;
    }
    public void SetMazePosition(Vector3 mazePosition)
    {
        _mazePosition = mazePosition;
    }
    public void SetMazeObj(GameObject mazeObj)
    {
        _mazeObj = mazeObj;
    }

    public void SetPropertiesSet(bool propertiesSet)
    {
        _propertiesSet = propertiesSet;
    }
}
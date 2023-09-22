using MazeDatatype;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MazeGenerationAgent : Agent
{
    [SerializeField]
    private float speed = 10f;

    private Rigidbody rBody;
    private MazeManager mazeManager;
    
    private void Start()
    {
        rBody = GetComponent<Rigidbody>();
        mazeManager = MazeManager.Singleton;
    }
    
    public override void OnEpisodeBegin()
    {
        // reset angel velocity
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        
        // generate new maze grid and place agent
        if (mazeManager.mazeGraph != null)
        {
            mazeManager.ClearMaze();
        }
        mazeManager.GenerateGrid();
        mazeManager.PlaceAgent();
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent position
        sensor.AddObservation(transform.localPosition);
        // Agent velocity
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
        // TODO: Add observations for the maze
        // Maze Cells: 1 for visited, 0 for not visited
        foreach (var cell in mazeManager.mazeGraph.Cells)
        {
            sensor.AddObservation(cell.Visited ? 1 : 0);
        }
        // Maze Walls: 1 for wall, 0 for no wall
        foreach (var wall in mazeManager.mazeGraph.Walls)
        {
            sensor.AddObservation(wall.Type == WallType.Horizontal ? 1 : 0);
            sensor.AddObservation(wall.Type == WallType.Vertical ? 1 : 0);
        }
        // Maze Corners: int for number of walls
        foreach (var corner in mazeManager.mazeGraph.Corners)
        {
            sensor.AddObservation(corner.Walls.Count);
        }
        // Maze Start Cell: position
        sensor.AddObservation(mazeManager.mazeGraph.StartCell.X);
        sensor.AddObservation(mazeManager.mazeGraph.StartCell.Z);
        // Maze End Cell: placed or not, position
        sensor.AddObservation(mazeManager.mazeGraph.EndCell == null ? 1 : 0);
        if (mazeManager.mazeGraph.EndCell != null)
        {
            sensor.AddObservation(mazeManager.mazeGraph.EndCell.X);
            sensor.AddObservation(mazeManager.mazeGraph.EndCell.Z); 
        }
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
        
        // Reward for solving the task: solvable maze
        var mazeMeetsRequirements = mazeManager.mazeGraph.MazeMeetsRequirements();
        // Agent created a maze that does not meet the requirements
        if (!mazeMeetsRequirements)
        {
            EndEpisode();
        }
        var mazeIsFinished = mazeManager.mazeGraph.EndCell != null;
        if (mazeIsFinished)
        {
            // TODO: Add reward for every cell in the path from start to end
            var mazeIsValid = mazeManager.mazeGraph.MazeIsValid();
            if (mazeIsValid)
            {
                SetReward(1.0f);
                EndEpisode();
            }
            else
            {
                SetReward(-1.0f);
                EndEpisode();
            }
        }
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

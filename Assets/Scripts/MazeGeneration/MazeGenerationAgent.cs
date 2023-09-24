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
    
    private int visitedCells;
    private int notVisitingNewCellsCounter;
    private bool reachedNewCell;
    
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
        // reset visited cells
        visitedCells = 0;
        
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
        // Maze Cells: 1 for visited, 0 for not visited
        foreach (var cell in mazeManager.mazeGraph.Cells)
        {
            sensor.AddObservation(cell.Visited ? 1 : 0);
        }
        // Maze visited Cells Count
        sensor.AddObservation(mazeManager.mazeGraph.GetVisitedCells());
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
        // Maze Outer Walls: position, size
        sensor.AddObservation(mazeManager.OuterWalls.transform.localPosition);
        sensor.AddObservation(mazeManager.OuterWalls.transform.localScale);
        // Maze longest path: length
        sensor.AddObservation(mazeManager.mazeGraph.FindLongestPath(mazeManager.mazeGraph.StartCell).Count);
        // Maze meets requirements: bool
        sensor.AddObservation(mazeManager.mazeGraph.MazeMeetsRequirements());
        // Maze is solvable: bool
        sensor.AddObservation(mazeManager.mazeGraph.MazeIsValid());
        // Maze percentage of visited cells: float
        sensor.AddObservation(mazeManager.mazeGraph.GetPercentageOfVisitedCells());
        // Maze percentage of longest path: float
        sensor.AddObservation(mazeManager.mazeGraph.GetPercentageOfLongestPath());
        // reached a new cell: bool
        sensor.AddObservation(reachedNewCell);
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
        
        var mazeIsFinished = mazeManager.mazeGraph.EndCell != null;
        if (mazeIsFinished)
        {
            var longestPathCount = mazeManager.mazeGraph.FindLongestPath(mazeManager.mazeGraph.StartCell).Count;
            Debug.Log("Longest Path: " + longestPathCount);
            var isSolvable = mazeManager.mazeGraph.MazeIsValid();
            if(isSolvable)
            {
                SetReward(10.0f);
                EndEpisode();
            }
            else
            {
                // calculate the reward based on the length of the longest path, should be between 0 and -1 
                var cellCount = mazeManager.GetCellCount();
                var reward = -1.0f + (longestPathCount - 1) / (cellCount - 1);
                SetReward(reward);
                EndEpisode();
            }
        }
        var mazeMeetsRequirements = mazeManager.mazeGraph.MazeMeetsRequirements();
        var percentageOfVisitedCells = mazeManager.mazeGraph.GetPercentageOfVisitedCells();
        if (mazeMeetsRequirements)
        {
            // Existentially penalize the agent for taking too long to solve the maze
            AddReward(-0.001f);
            
            // +0.1 reward for each visited cell
            var newVisitedCells = mazeManager.mazeGraph.GetVisitedCells();
            var visitedCellsDelta = newVisitedCells - visitedCells;
            visitedCells = newVisitedCells;
            if (visitedCellsDelta > 0)
            {
                AddReward(visitedCellsDelta * 1.0f);
                notVisitingNewCellsCounter = 0;
                reachedNewCell = true;
            }
            // -0.01 reward for not reaching a new cell
            else
            {
                AddReward(-0.01f);
                notVisitingNewCellsCounter++;
                reachedNewCell = false;
            }
            if(notVisitingNewCellsCounter > 1000)
            {
                // Penalize the agent for not reaching a new cell for too long
                var reward = -1.0f + percentageOfVisitedCells;
                SetReward(reward);
                EndEpisode();
            }
        }
        else
        {
            // Penalize the agent for not meeting the requirements
            var reward = -5.0f + percentageOfVisitedCells;
            SetReward(reward);
            EndEpisode();
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

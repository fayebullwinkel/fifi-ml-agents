using MazeDatatype;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MazeGenerationAgent : Agent
{
    [SerializeField] 
    protected float speed = 10f;
    
    protected Maze Maze;
    
    protected int visitedCells;
    protected int notReachedNewCellCounter;
    protected bool reachedNewCell;

    protected void SetupMaze()
    {
        // reset visited cells
        visitedCells = 0;
        
        // generate new maze grid and place agent
        if (Maze.Grid != null)
        {
            Maze.ClearMaze();
        }
        // mazeManager.GenerateGrid();
        // mazeManager.PlaceAgent();
    }

    protected void AddMazeObservations(VectorSensor sensor)
    {
        if (Maze == null)
        {
            return;
        }
        // Maze Cells: 1 for visited, 0 for not visited
        foreach (var cell in Maze.Grid.Cells)
        {
            sensor.AddObservation(cell.Visited ? 1 : 0);
        }
        // Maze visited Cells Count
        sensor.AddObservation(Maze.Grid.GetVisitedCells());
        // Maze Walls: 1 for wall, 0 for no wall
        foreach (var wall in Maze.Grid.Walls)
        {
            sensor.AddObservation(wall.Type == WallType.Horizontal ? 1 : 0);
            sensor.AddObservation(wall.Type == WallType.Vertical ? 1 : 0);
        }
        // Maze Corners: int for number of walls
        foreach (var corner in Maze.Grid.Corners)
        {
            sensor.AddObservation(corner.Walls.Count);
        }
        // Maze Start Cell: position
        sensor.AddObservation(Maze.Grid.StartCell.X);
        sensor.AddObservation(Maze.Grid.StartCell.Z);
        // Maze End Cell: placed or not, position
        sensor.AddObservation(Maze.Grid.EndCell == null ? 1 : 0);
        if (Maze.Grid.EndCell != null)
        {
            sensor.AddObservation(Maze.Grid.EndCell.X);
            sensor.AddObservation(Maze.Grid.EndCell.Z); 
        }
        // Maze Outer Walls: position, size
        sensor.AddObservation(Maze.OuterWalls.transform.localPosition);
        sensor.AddObservation(Maze.OuterWalls.transform.localScale);
        // Maze longest path: length
        sensor.AddObservation(Maze.Grid.FindLongestPath(Maze.Grid.StartCell).Count);
        // Maze meets requirements: bool
        sensor.AddObservation(Maze.Grid.MazeMeetsRequirements());
        // Maze is solvable: bool
        sensor.AddObservation(Maze.Grid.MazeIsValid());
        // Maze percentage of visited cells: float
        sensor.AddObservation(Maze.Grid.GetPercentageOfVisitedCells());
        // Maze percentage of longest path: float
        sensor.AddObservation(Maze.Grid.GetPercentageOfLongestPath());
        // reached a new cell: bool
        sensor.AddObservation(reachedNewCell);
    }

    protected void GrantReward()
    {
        var mazeIsFinished = Maze.Grid.EndCell != null;
        if (mazeIsFinished)
        {
            var longestPathCount = Maze.Grid.FindLongestPath(Maze.Grid.StartCell).Count;
            Debug.Log("Longest Path: " + longestPathCount);
            var isSolvable = Maze.Grid.MazeIsValid();
            if(isSolvable)
            {
                SetReward(10.0f);
                EndEpisode();
            }
            else
            {
                // calculate the reward based on the length of the longest path, should be between 0 and -1 
                var cellCount = Maze.GetCellCount();
                var reward = -1.0f + (longestPathCount - 1) / (cellCount - 1);
                SetReward(reward);
                EndEpisode();
            }
        }
        var mazeMeetsRequirements = Maze.Grid.MazeMeetsRequirements();
        var percentageOfVisitedCells = Maze.Grid.GetPercentageOfVisitedCells();
        if (mazeMeetsRequirements)
        {
            // Existentially penalize the agent for taking too long to solve the maze
            AddReward(-0.001f);
            
            // +0.1 reward for each visited cell
            var newVisitedCells = Maze.Grid.GetVisitedCells();
            var visitedCellsDelta = newVisitedCells - visitedCells;
            visitedCells = newVisitedCells;
            if (visitedCellsDelta > 0)
            {
                AddReward(visitedCellsDelta * 1.0f);
                notReachedNewCellCounter = 0;
                reachedNewCell = true;
            }
            // -0.01 reward for not reaching a new cell
            else
            {
                AddReward(-0.01f);
                notReachedNewCellCounter++;
                reachedNewCell = false;
            }
            if(notReachedNewCellCounter > 1000)
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
}

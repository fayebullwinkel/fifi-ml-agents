using System.Collections.Generic;
using UnityEngine;

public class MazeController : MonoBehaviour
{
    public MazeGeneration mazeGenerator;
    public int mazeCountX = 1;
    public int mazeCountZ = 1;
    public int spacing = 4;

    private bool _firstRound;
    private void Start()
    {
        var agentPrefab = (GameObject)Resources.Load("Prefabs/MazeAgent", typeof(GameObject));
        var localScale = transform.localScale;

        for (var i = 0; i < mazeCountX; i++)
        {
            for (var j = 0; j < mazeCountZ; j++)
            {
                GameObject mazeObj = new GameObject();
                var mazeBuilder = gameObject.AddComponent<MazeBuilder>();

                var endCubeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                endCubeObj.tag = "EndCube";
                endCubeObj.name = "EndCube";

                var agentObj = Instantiate(agentPrefab);
                agentObj.transform.localScale = endCubeObj.transform.localScale;
                agentObj.GetComponent<Rigidbody>().isKinematic = true;

                var xOffset = (i * (localScale.x + spacing)) - ((mazeCountX - 1) * 0.5f * (localScale.x + spacing));
                var zOffset = (j * (localScale.z + spacing)) - ((mazeCountZ - 1) * 0.5f * (localScale.z + spacing));

                var mazePosition = transform.position + new Vector3(xOffset, 0f, zOffset);

                ResetArea(mazeObj, mazeBuilder, endCubeObj, agentObj, mazePosition);
            }
        }
    }

    public void ResetArea(GameObject mazeObj, MazeBuilder mazeBuilder, GameObject endCubeObj, GameObject agentObj, Vector3 mazePosition)
    {
        var agent = mazeObj.transform.Find("MazeAgent(Clone)");
        if (agent != null)
        {
            agent.SetParent(null);
            mazeObj.transform.Find("EndCube").SetParent(null);
            int childCount = mazeObj.transform.childCount;

            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = mazeObj.transform.GetChild(i);
                Destroy(child.gameObject);
            }
        }
        
        var maze = GenerateMaze();
        BuildMaze(mazeObj, mazeBuilder, maze, mazePosition);
        SetAgentProperties(agentObj, maze, mazeBuilder, endCubeObj, mazePosition, mazeObj);
        MoveCube(mazeObj, agentObj, maze.GetStartCube(), Color.green);
        MoveCube(mazeObj, endCubeObj, maze.GetEndCube(), Color.red);
        RotateMazeToFaceCamera(mazeObj, agentObj);
    }

    private Maze GenerateMaze()
    {
        return mazeGenerator.Generate(transform.localScale);
    }

    private void BuildMaze(GameObject mazeObj, MazeBuilder mazeBuilder, Maze maze, Vector3 mazePosition)
    {
        mazeBuilder.Initialize(maze);
        mazeBuilder.BuildMaze(mazePosition, transform.localScale, mazeObj);
    }

    private void SetAgentProperties(GameObject agentObj, Maze maze, MazeBuilder mazeBuilder, GameObject endCubeObj,
        Vector3 mazePosition, GameObject mazeObj)
    {
        var agent = agentObj.GetComponent<MazeAgent>();
        agent.SetStartPosition(maze.GetStartCube().GetPos());
        agent.SetMaze(maze);
        agent.SetMazeBuilder(mazeBuilder);
        agent.SetEndCubeObj(endCubeObj);
        agent.SetAgentObj(agentObj);
        agent.SetMazePosition(mazePosition);
        agent.SetMazeObj(mazeObj);
    }

    private void RotateMazeToFaceCamera(GameObject mazeObj, GameObject agentObj)
    {
        var startCubeCenter = agentObj.transform.position;
        var mazeCubeCenter = mazeObj.transform.position;

        // Direction from the startCube to the mazeCube
        var directionToStartCube = startCubeCenter - mazeCubeCenter;
        var x = Mathf.Abs(directionToStartCube.x);
        var y = Mathf.Abs(directionToStartCube.y);
        var z = Mathf.Abs(directionToStartCube.z);

        // Rotate cube so that face with startCube is facing the camera
        if (x >= y && x >= z)
        {
            if (directionToStartCube.x > 0)
            {
                // right
                mazeObj.transform.Rotate(0.0f, 90.0f, 0.0f, Space.World);
            }
            else
            {
                // left
                mazeObj.transform.Rotate(0.0f, -90.0f, 0.0f, Space.World);
            }
        }
        else if (y >= x && y >= z)
        {
            if (directionToStartCube.y > 0)
            {
                // top
                mazeObj.transform.Rotate(-90.0f, 0.0f, 0.0f, Space.World);
            }
            else
            {
                // bottom
                mazeObj.transform.Rotate(90.0f, 0.0f, 0.0f, Space.World);
            }
        }
        else
        {
            if (directionToStartCube.z > 0)
            {
                // back
                mazeObj.transform.Rotate(0.0f, 180.0f, 0.0f, Space.World);
            }
        }
    }

    private void MoveCube(GameObject mazeObj, GameObject cubeObj, Cube cube, Color color)
    {
        cubeObj.transform.SetParent(mazeObj.transform);
        cubeObj.transform.localPosition = cube.GetRelativePosition(mazeObj.transform.localScale);
        cubeObj.GetComponent<Renderer>().material.color = color;
    }

    public List<Vector3Int> GetSurfaceCubePositions()
    {
        return mazeGenerator.GetSurfaceCubePositions();
    }
}
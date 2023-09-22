using MazeDatatype;
using Unity.MLAgents;
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
        // check if Task is failed
        if (mazeManager.mazeGraph == null || !mazeManager.mazeGraph.IsMazeValid())
        {
            // reset angel velocity
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
            // generate new maze grid and place agent
            mazeManager.GenerateGrid();
            mazeManager.PlaceAgent();
        }
    }
}

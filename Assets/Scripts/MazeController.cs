using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class MazeController : MonoBehaviour
{
    [FormerlySerializedAs("TestCube")] public MazeGeneration MazeGenerator;
    private bool isFirst = true;
    private float rotationSpeed = 30f;
    private Quaternion currentRotation;
    private GameObject Maze;
    private bool isNew = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isFirst)
            {

                isFirst = false;
                MazeGenerator.Generate(transform.position, transform.localScale);

            }
            else
            {
                currentRotation = Maze.transform.rotation;
                
                MazeGenerator.Delete();
                MazeGenerator.Generate(transform.position, transform.localScale);
                
                MazeGenerator.GetMazeObj().transform.rotation = currentRotation;
            }
        }
        
        Maze = MazeGenerator.GetMazeObj();
        if (Maze)
        {
            Maze.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public MazeGeneration TestCube;
    private int i = 0;

    bool switcher = true;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (i == 0)
            {

                i++;
                TestCube.Generate(transform.position, transform.localScale);

            }
            else
            {
                TestCube.Delete();
                TestCube.Generate(transform.position, transform.localScale);
            }
        }
    }
}

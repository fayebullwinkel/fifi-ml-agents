using MazeGeneration_vivi.MazeDatatype.Enums;
using UnityEngine;

namespace MazeGeneration_vivi
{
    public class AgentManualInput : MonoBehaviour
    {
        private MazeGenerationAgent agent;
        
        private void Start()
        {
            agent = transform.GetComponent<MazeGenerationAgent>();
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                agent.ManualInput = EManualInput.A;
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                agent.ManualInput = EManualInput.D;
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                agent.ManualInput = EManualInput.W;
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                agent.ManualInput = EManualInput.S;
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                agent.ManualInput = EManualInput.Space;
            }
        }
    }
}
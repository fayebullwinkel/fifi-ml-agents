using UnityEngine;

namespace MazeGeneration_vivi
{
    public class CameraRotator : MonoBehaviour
    {
        [SerializeField]
        private float speed = 10f;
        
        private void Update()
        {
            transform.Rotate(0, speed * Time.deltaTime, 0);
        }
    }
}
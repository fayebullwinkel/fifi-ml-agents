using UnityEngine;

namespace MazeDatatype
{
    public class MazeWall : MonoBehaviour
    {
        public enum WallType
        {
            Horizontal,
            Vertical
        }

        public WallType Type { get; set; }
        public MazeCell Cell1 { get; set; }
        public MazeCell Cell2 { get; set; }

        public MazeWall(WallType type, MazeCell cell1, MazeCell cell2)
        {
            Type = type;
            Cell1 = cell1;
            Cell2 = cell2;
        }

        public void DestroyWall()
        {
            Destroy(gameObject);
        }
    }
}
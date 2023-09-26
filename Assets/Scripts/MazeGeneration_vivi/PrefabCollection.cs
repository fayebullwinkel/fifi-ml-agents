using UnityEngine;

namespace MazeGeneration_vivi
{
    [CreateAssetMenu(fileName = "PrefabCollection", menuName = "Configs/Prefab Collection", order = 0)]
    public class PrefabCollection : ScriptableObject
    {
        public GameObject wallPrefab = null!;

        public GameObject mazeBoundsPrefab = null!;

        public GameObject startCellPrefab = null!;

        public GameObject goalCellPrefab = null!;

        public GameObject pathCellPrefab = null!;
    }
}
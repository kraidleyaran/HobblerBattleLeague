using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Adventure
{
    [CreateAssetMenu(fileName = "Adventure Map", menuName = "Ancible Tools/Adventure Map")]
    public class AdventureMap : ScriptableObject
    {
        public AdventureMapController MapController = null;
        public Vector2Int DefaultTile = Vector2Int.zero;
    }
}
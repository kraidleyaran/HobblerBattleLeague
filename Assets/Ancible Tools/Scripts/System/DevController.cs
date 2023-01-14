using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class DevController : MonoBehaviour
    {
        [SerializeField] private Trait[] _devHobblerTraits = new Trait[0];
        [SerializeField] private Vector2Int _startingTile = Vector2Int.zero;

        void Start()
        {
            //var startingTile = WorldController.Pathing.GetTileByPosition(_startingTile);
            //if (startingTile != null)
            //{
            //    var unitController = Instantiate(FactoryController.UNIT_CONTROLLER, startingTile.World, Quaternion.identity);

            //    var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            //    for (var i = 0; i < _devHobblerTraits.Length; i++)
            //    {
            //        addTraitToUnitMsg.Trait = _devHobblerTraits[i];
            //        gameObject.SendMessageTo(addTraitToUnitMsg, unitController.gameObject);
            //    }

            //    MessageFactory.CacheMessage(addTraitToUnitMsg);

            //    var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
            //    setMapTileMsg.Tile = startingTile;
            //    gameObject.SendMessageTo(setMapTileMsg, unitController.gameObject);
            //    MessageFactory.CacheMessage(setMapTileMsg);
            //}
            
        }
    }
}
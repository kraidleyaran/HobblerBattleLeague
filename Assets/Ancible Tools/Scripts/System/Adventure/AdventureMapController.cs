using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Adventure
{
    public class AdventureMapController : MonoBehaviour
    {
        public PathingGridController PlayerPathing;
        public PathingGridController MonsterPathing;

        public void Setup(Vector2Int playerPos)
        {
            PlayerPathing.Setup();
            MonsterPathing.Setup();
            var playerTile = PlayerPathing.GetTileByPosition(playerPos);

            var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
            setMapTileMsg.Tile = playerTile;
            gameObject.SendMessageTo(setMapTileMsg, WorldAdventureController.Player);
            MessageFactory.CacheMessage(setMapTileMsg);

            gameObject.SendMessage(SpawnAdventureUnitsMessage.INSTANCE);
        }

        public void SetBlockingTile(GameObject block, Vector2Int tile)
        {
            PlayerPathing.SetTileBlock(block, tile, false);
            MonsterPathing.SetTileBlock(block,tile, false);
        }

        public void RemoveBlockingTile(GameObject block, Vector2Int tile)
        {
            PlayerPathing.RemoveTileBlock(block, tile, false);
            MonsterPathing.RemoveTileBlock(block, tile, false);
        }

        public MapTile GetPlayerMapTileFromMonster(Vector2Int pos)
        {
            return PlayerPathing.GetTileByPosition(pos);
        }
    }
}
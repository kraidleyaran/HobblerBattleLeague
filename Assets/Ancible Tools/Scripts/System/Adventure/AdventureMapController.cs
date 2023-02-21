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

            WorldAdventureController.SetPlayerTile(playerTile);
            AdventureCameraController.SetCameraPosition(playerTile.World);

            gameObject.SendMessage(SpawnAdventureUnitsMessage.INSTANCE);

            if (WorldController.State == WorldState.Adventure)
            {
                var setUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
                setUnitStateMsg.State = AdventureUnitState.Idle;
                gameObject.SendMessageTo(setUnitStateMsg, WorldAdventureController.Player);
                MessageFactory.CacheMessage(setUnitStateMsg);
            }
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
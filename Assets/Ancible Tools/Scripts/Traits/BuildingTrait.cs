using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Building;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.UnitCommands;
using MessageBusLib;
using UnityEditor;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Building Trait", menuName = "Ancible Tools/Traits/Building/Building")]
    public class BuildingTrait : Trait
    {
        [SerializeField] private UnitCommand _moveBuilding = null;
        [SerializeField] private UnitCommand _upgradeCommand = null;
        [SerializeField] private UnitCommand _sellBuildingTemplate = null;

        private MapTile _currentTile = null;
        private MapTile[] _blockedTiles = new MapTile[0];
        private Vector2Int[] _passiveTiles = new Vector2Int[0];
        private WorldBuilding _building = null;
        private string _id = string.Empty;

        private TraitController[] _activeSprites = new TraitController[0];

        private CommandInstance _moveCommandInstance = null;
        private CommandInstance _sellBuildingInstance = null;
        private CommandInstance _upgradeInstance = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _moveCommandInstance = _moveBuilding.GenerateInstance();
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetupBuildingMessage>(SetupBuilding, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryUnitNameMessage>(QueryUnitName, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryBuildingMessge>(QueryBuilding, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryCommandsMessage>(QueryCommands, _instanceId);
        }

        private void UpdateMapTile(UpdateMapTileMessage msg)
        {
            if (_blockedTiles.Length > 0)
            {
                
                for (var i = 0; i < _blockedTiles.Length; i++)
                {
                    WorldController.Pathing.RemoveTileBlock(_controller.transform.parent.gameObject, _blockedTiles[i].Position);
                }
                
            }
            WorldBuildingManager.RemovePassivePositions(_passiveTiles);
            _currentTile = msg.Tile;
            var blockingPositions = _building.BlockingTiles.Select(p => p + _currentTile.Position).ToList();

            _blockedTiles = blockingPositions.Select(WorldController.Pathing.GetTileByPosition).Where(t => t != null).ToArray();
            for (var i = 0; i < _blockedTiles.Length; i++)
            {
                var tile = _blockedTiles[i];
                WorldController.Pathing.SetTileBlock(_controller.transform.parent.gameObject, tile.Position);
            }

            var passiveTiles = _building.GetRequiredPositions(_currentTile.Position).Where(p => !blockingPositions.Contains(p)).ToList();
            if (!_currentTile.Block)
            {
                passiveTiles.Add(_currentTile.Position);
            }

            _passiveTiles = passiveTiles.ToArray();
            WorldBuildingManager.AddPassivePositions(_passiveTiles);
        }

        private void SetupBuilding(SetupBuildingMessage msg)
        {
            _building = msg.Building;
            _id = msg.Id;
            if (_sellBuildingInstance == null)
            {
                var sellBuildingObj = Instantiate(_sellBuildingTemplate, _controller.transform);
                
                _sellBuildingInstance = sellBuildingObj.GenerateInstance();
            }

            _sellBuildingInstance.Command.GoldValue = WorldBuildingManager.CalculateSellbackValue(_building.Cost);

            if (_building.Upgrades.Buildings.Length > 0)
            {
                if (_upgradeInstance != null)
                {
                    if (_upgradeInstance.Tree.SubCommands.Count > 0)
                    {
                        _upgradeInstance.Tree.Destroy(true);
                        _upgradeInstance.Tree = new InstanceSubCommandTree();
                    }
                }
                else
                {
                    _upgradeInstance = _upgradeCommand.GenerateInstance();
                }

                foreach (var upgrade in _building.Upgrades.Buildings)
                {
                    var commandObj = Instantiate(FactoryController.COMMAND_TEMPLATE, _controller.transform);
                    commandObj.DoAfter = () => { UpgradeBuilding(upgrade); };
                    commandObj.Icons = new []{new CommandIcon{Sprite = _building.Sprites[0].Sprite, ColorMask = Color.white}};
                    commandObj.Description = upgrade.Description;
                    commandObj.GoldValue = upgrade.Cost;
                    commandObj.Command = $"Upgrade to {upgrade.DisplayName}";
                    var commandInstance = commandObj.GenerateInstance();
                    _upgradeInstance.Tree.SubCommands.Add(commandInstance);
                }
            }
            else
            {
                _upgradeInstance?.Destroy();
            }

            var updateBuildingIdMsg = MessageFactory.GenerateUpdateBuildingIdMsg();
            updateBuildingIdMsg.Id = _id;
            _controller.gameObject.SendMessageTo(updateBuildingIdMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateBuildingIdMsg);

            if (_activeSprites.Length > 0)
            {
                var removeTraitFromUnitByControllerMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
                for (var i = 0; i < _activeSprites.Length; i++)
                {
                    removeTraitFromUnitByControllerMsg.Controller = _activeSprites[i];
                    _controller.gameObject.SendMessageTo(removeTraitFromUnitByControllerMsg, _controller.transform.parent.gameObject);
                }
                MessageFactory.CacheMessage(removeTraitFromUnitByControllerMsg);
            }

            var sprites = new List<TraitController>();
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            addTraitToUnitMsg.DoAfter = controller => { sprites.Add(controller); };
            for (var i = 0; i < _building.Sprites.Length; i++)
            {
                addTraitToUnitMsg.Trait = _building.Sprites[i];
                _controller.gameObject.SendMessageTo(addTraitToUnitMsg, _controller.transform.parent.gameObject);
            }
            MessageFactory.CacheMessage(addTraitToUnitMsg);
            _activeSprites = sprites.ToArray();
            _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
        }

        private void UpgradeBuilding(WorldBuilding upgrade)
        {
            WorldBuildingManager.UpgradeBuilding(_controller.transform.parent.gameObject, _currentTile, _building, upgrade);
        }

        private void QueryUnitName(QueryUnitNameMessage msg)
        {
            msg.DoAfter.Invoke(_building.DisplayName);
        }

        private void QueryBuilding(QueryBuildingMessge msg)
        {
            msg.DoAfter.Invoke(_building, _currentTile, _id);
        }

        private void QueryCommands(QueryCommandsMessage msg)
        {
            var commands = new List<CommandInstance>{_moveCommandInstance, _sellBuildingInstance};
            if (_upgradeInstance != null)
            {
                commands.Add(_upgradeInstance);
            }
            msg.DoAfter.Invoke(commands.ToArray());
        }

        public override void Destroy()
        {
            _moveCommandInstance.Destroy();
            _sellBuildingInstance.Command.Destroy();
            Destroy(_sellBuildingInstance.Command);
            _sellBuildingInstance.Destroy();
            WorldBuildingManager.RemovePassivePositions(_passiveTiles);
            if (_blockedTiles.Length > 0)
            {
                for (var i = 0; i < _blockedTiles.Length; i++)
                {
                    WorldController.Pathing.RemoveTileBlock(_controller.transform.parent.gameObject, _blockedTiles[i].Position);
                }
            }
            base.Destroy();
        }
    }
}
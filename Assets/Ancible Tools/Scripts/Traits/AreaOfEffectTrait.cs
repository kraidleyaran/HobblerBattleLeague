using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Area of Effect Trait", menuName = "Ancible Tools/Traits/Combat/Area of Effect")]
    public class AreaOfEffectTrait : Trait
    {
        [SerializeField] private int _area = 1;
        [SerializeField] private AbilityTargetAlignment _targetAlignment = AbilityTargetAlignment.Ally;
        [SerializeField] private Trait[] _applyToObj = new Trait[0];
        [SerializeField] private bool _applyToPositionTile = true;
        

        private MapTile[] _affectedTiles = new MapTile[0];
        private GameObject _owner = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var queryMapTileMsg = MessageFactory.GenerateQueryMapTileMsg();
            queryMapTileMsg.DoAfter = UpdateTiles;
            _controller.gameObject.SendMessageTo(queryMapTileMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(queryMapTileMsg);
            SubscribeToMessages();
        }

        private void UpdateTiles(MapTile tile)
        {
            foreach (var affected in _affectedTiles)
            {
                affected.OnObjectEnteringTile -= ApplyEffects;
            }
            var pathingGrid = WorldController.State == WorldState.Battle ? BattleLeagueController.PathingGrid : MinigameController.Pathing;
            var tiles = pathingGrid.GetMapTilesInArea(tile.Position, _area).ToList();
            if (_applyToPositionTile && !tiles.Contains(tile))
            {
                tiles.Add(tile);
            }
            else if (!_applyToPositionTile && tiles.Contains(tile))
            {
                tiles.Remove(tile);
            }

            for (var i = 0; i < tiles.Count; i++)
            {
                var affectedTile = tiles[i];
                affectedTile.OnObjectEnteringTile += ApplyEffects;
                if (affectedTile.Block)
                {
                    ApplyEffects(affectedTile.Block);
                }
            }

            _affectedTiles = tiles.ToArray();
        }

        private void ApplyEffects(GameObject obj)
        {
            var apply = false;
            switch (WorldController.State)
            {
                case WorldState.Battle:
                    apply = IsValidBattleAlignment(obj);
                    break;
                case WorldState.Minigame:
                    apply = IsValidMinigameAlignment(obj);
                    break;
            }

            if (apply)
            {
                var parent = _owner ? _owner : _controller.transform.parent.gameObject;
                var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                foreach (var trait in _applyToObj)
                {
                    addTraitToUnitMsg.Trait = trait;
                    parent.SendMessageTo(addTraitToUnitMsg, obj);
                }
                MessageFactory.CacheMessage(addTraitToUnitMsg);

            }
        }

        private bool IsValidBattleAlignment(GameObject obj)
        {
            if (_targetAlignment == AbilityTargetAlignment.Both)
            {
                return true;
            }

            var parentAligment = BattleAlignment.None;
            var parent = _owner ? _owner : _controller.transform.parent.gameObject;
            var queryBattleAlignmentMsg = MessageFactory.GenerateQueryBattleAlignmentMsg();
            queryBattleAlignmentMsg.DoAfter = alignment => parentAligment = alignment;
            _controller.gameObject.SendMessageTo(queryBattleAlignmentMsg, parent);

            var targetAlignment = BattleAlignment.None;
            queryBattleAlignmentMsg.DoAfter = alignment => targetAlignment = alignment;
            _controller.gameObject.SendMessageTo(queryBattleAlignmentMsg, obj);

            MessageFactory.CacheMessage(queryBattleAlignmentMsg);

            switch (_targetAlignment)
            {
                case AbilityTargetAlignment.Ally:
                    return parentAligment == targetAlignment;
                case AbilityTargetAlignment.Enemy:
                    return parentAligment != targetAlignment;
                default:
                    return true;
            }
        }

        private bool IsValidMinigameAlignment(GameObject obj)
        {
            if (_targetAlignment == AbilityTargetAlignment.Both)
            {
                return true;
            }
            CombatAlignment parentAlignment = CombatAlignment.Neutral;
            var parent = _owner ? _owner : _controller.transform.parent.gameObject;
            var queryCombatAlignmentMsg = MessageFactory.GenerateQueryCombatAlignmentMsg();
            queryCombatAlignmentMsg.DoAfter = alignment => parentAlignment = alignment;
            _controller.gameObject.SendMessageTo(queryCombatAlignmentMsg, parent);

            CombatAlignment targetAlignment = CombatAlignment.Neutral;
            queryCombatAlignmentMsg.DoAfter = alignment => targetAlignment = alignment;
            _controller.gameObject.SendMessageTo(queryCombatAlignmentMsg, obj);

            MessageFactory.CacheMessage(queryCombatAlignmentMsg);

            switch (_targetAlignment)
            {
                case AbilityTargetAlignment.Ally:
                    return parentAlignment == targetAlignment;
                case AbilityTargetAlignment.Enemy:
                    return parentAlignment != targetAlignment;
                default:
                    return true;
            }
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateOwnerMessage>(UpdateOwner, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
        }

        private void UpdateOwner(UpdateOwnerMessage msg)
        {
            _owner = msg.Owner;
        }

        private void UpdateMapTile(UpdateMapTileMessage msg)
        {
            UpdateTiles(msg.Tile);
        }

    }
}
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Adventure Monster Encounter Trait", menuName = "Ancible Tools/Traits/Adventure/Interaction/Adventure Monster Encounter")]
    public class AdventureMonsterEncounterTrait : Trait
    {
        [SerializeField] private BattleEncounter[] _encounters = new BattleEncounter[0];
        [SerializeField] private Color _exclamationColor = Color.red;
        [SerializeField] private Vector2Int _exclamationOffset = Vector2Int.zero;
        [SerializeField] private int _exclamationTicks = 30;

        private GameObject _spawner = null;
        private AdventureBattleExclamationController _exclamationController = null;
        

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void StartBump(GameObject player)
        {
            if (WorldController.State == WorldState.Adventure && _encounters.Length > 0)
            {

                var diff = player.transform.position.ToVector2() - _controller.transform.parent.position.ToVector2();
                var doBumpPixelsMsg = MessageFactory.GenerateDoBumpOverPixelsPerSecondMsg();
                doBumpPixelsMsg.PixelsPerSecond = WorldAdventureController.BattleBumpSpeed;
                doBumpPixelsMsg.Direction = diff.normalized;
                doBumpPixelsMsg.Distance = WorldAdventureController.BattleBumpDistance;
                doBumpPixelsMsg.DoAfter = StartEncounter;
                doBumpPixelsMsg.OnBump = () => { };
                _controller.gameObject.SendMessageTo(doBumpPixelsMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(doBumpPixelsMsg);
            }
        }

        private void StartEncounter()
        {
            var encounter = _encounters.GetRandom();
            BattleLeagueManager.SetupEncounter(encounter, _controller.transform.parent.gameObject);
        }

        private void StartExclamation(GameObject player)
        {
            var playerState = AdventureUnitState.Idle;
            var queryUnitStateMsg = MessageFactory.GenerateQueryAdventureUnitStateMsg();
            queryUnitStateMsg.DoAfter = state =>
            {
                playerState = state;
            };
            _controller.gameObject.SendMessageTo(queryUnitStateMsg, WorldAdventureController.Player);
            MessageFactory.CacheMessage(queryUnitStateMsg);

            if (playerState != AdventureUnitState.Interaction)
            {
                var setUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
                setUnitStateMsg.State = AdventureUnitState.Interaction;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, player);
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);

                _exclamationController = Instantiate(FactoryController.BATTLE_EXCLAMATION, _controller.transform);
                var offset = new Vector2(_exclamationOffset.x * DataController.Interpolation, _exclamationOffset.y * DataController.Interpolation);
                _exclamationController.transform.SetLocalPosition(offset);
                _exclamationController.Setup(_exclamationTicks, () => { FinishExclamation(player); }, _exclamationColor);
            }
        }

        private void FinishExclamation(GameObject player)
        {
            Destroy(_exclamationController.gameObject);
            StartBump(player);
        }

        private void ProcessObstacle(GameObject obj, AdventureUnitType type)
        {
            switch (type)
            {
                case AdventureUnitType.Player:
                    StartExclamation(obj);
                    break;
                case AdventureUnitType.NPC:
                    break;
                case AdventureUnitType.Monster:
                    break;
                case AdventureUnitType.Interaction:
                    break;
            }
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateAdventureUnitStateMessage>(UpdateUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetUnitSpawnerMessage>(SetUnitSpawner, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ObstacleMessage>(Obstacle, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<InteractMessage>(Interact, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<EncounterFinishedMessage>(EncounterFinished, _instanceId);
        }

        private void EncounterFinished(EncounterFinishedMessage msg)
        {
            if (msg.Result == BattleResult.Victory)
            {
                var setUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
                setUnitStateMsg.State = AdventureUnitState.Disabled;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);

                setUnitStateMsg.State = AdventureUnitState.Idle;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, WorldAdventureController.Player);
                MessageFactory.CacheMessage(setUnitStateMsg);
            }
            else if (msg.Result == BattleResult.Defeat)
            {
                var setUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
                setUnitStateMsg.State = AdventureUnitState.Idle;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);
                
                _controller.gameObject.SendMessageTo(RespawnPlayerMessage.INSTANCE, WorldAdventureController.Player);
            }
        }

        private void SetUnitSpawner(SetUnitSpawnerMessage msg)
        {
            _spawner = msg.Spawner;
            _controller.transform.parent.gameObject.UnsubscribeFromFilter<SetUnitSpawnerMessage>(_instanceId);
        }

        private void UpdateUnitState(UpdateAdventureUnitStateMessage msg)
        {
            if (msg.State == AdventureUnitState.Disabled)
            {
                _controller.gameObject.SendMessageTo(DespawnUnitMessage.INSTANCE, _spawner);
            }
        }

        private void Obstacle(ObstacleMessage msg)
        {
            var queryAdventureUnitTypeMsg = MessageFactory.GenerateQueryAdventureUnitTypeMsg();
            queryAdventureUnitTypeMsg.DoAfter = unitType => ProcessObstacle(msg.Obstacle, unitType);
            _controller.gameObject.SendMessageTo(queryAdventureUnitTypeMsg,msg.Obstacle);
            MessageFactory.CacheMessage(queryAdventureUnitTypeMsg);
        }

        private void Interact(InteractMessage msg)
        {
            var setAdventureUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
            setAdventureUnitStateMsg.State = AdventureUnitState.Interaction;
            _controller.gameObject.SendMessageTo(setAdventureUnitStateMsg, _controller.transform.parent.gameObject);
            _controller.gameObject.SendMessageTo(setAdventureUnitStateMsg, msg.Owner);
            MessageFactory.CacheMessage(setAdventureUnitStateMsg);
            

            var diff = _controller.transform.parent.position.ToVector2() - msg.Owner.transform.position.ToVector2();
            var doBumpPixelsMsg = MessageFactory.GenerateDoBumpOverPixelsPerSecondMsg();
            doBumpPixelsMsg.PixelsPerSecond = WorldAdventureController.BattleBumpSpeed;
            doBumpPixelsMsg.Direction = diff.normalized;
            doBumpPixelsMsg.Distance = WorldAdventureController.BattleBumpDistance;
            doBumpPixelsMsg.DoAfter = StartEncounter;
            doBumpPixelsMsg.OnBump = () => { };
            _controller.gameObject.SendMessageTo(doBumpPixelsMsg, msg.Owner);
            MessageFactory.CacheMessage(doBumpPixelsMsg);
        }
    }
}
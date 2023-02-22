using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.Dialogue;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Adventure Monster Encounter Trait", menuName = "Ancible Tools/Traits/Adventure/Interaction/Adventure Monster Encounter")]
    public class AdventureMonsterEncounterTrait : Trait
    {
        [SerializeField] private BattleEncounter[] _encounters = new BattleEncounter[0];
        [SerializeField] private Color _exclamationColor = Color.red;
        [SerializeField] private Vector2Int _exclamationOffset = Vector2Int.zero;
        [SerializeField] private int _exclamationTicks = 30;
        [SerializeField] private Vector2Int[] _encounterPositions = new []{Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right};

        private GameObject _spawner = null;
        private AdventureBattleExclamationController _exclamationController = null;
        private MapTile _mapTile = null;
        private AdventureAiState _aiState = AdventureAiState.Wander;
        private AdventureUnitState _unitState = AdventureUnitState.Idle;
        private Vector2Int[] _encounterTiles = new Vector2Int[0];
        private bool _activeDialogue = false;
        private Coroutine _dialogueRoutine = null;
        private BattleEncounter _currentEncounter = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private Vector2Int[] GetEncounterTiles(Vector2Int pos)
        {
            var returnPositions = new List<Vector2Int>();
            foreach (var position in _encounterPositions)
            {
                var tile = position + pos;
                if (WorldAdventureController.MapController.MonsterPathing.DoesTileExist(tile))
                {
                    returnPositions.Add(tile);
                }
            }

            return returnPositions.ToArray();
        }

        private bool IsPlayerEncounterable(AdventureUnitState unitState)
        {
            if (_aiState == AdventureAiState.Aggro && unitState != AdventureUnitState.Interaction)
            {
                return _mapTile != null && Array.IndexOf(_encounterTiles, WorldAdventureController.PlayerTile.Position) >= 0;
            }
            return false;
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
            if (WorldHobblerManager.GetAvailableRoster().Length > 0)
            {
                _currentEncounter = _encounters.GetRandom();
                BattleLeagueManager.SetupEncounter(_currentEncounter, _controller.transform.parent.gameObject);
            }
            else
            {
                _controller.gameObject.SubscribeWithFilter<DialogueClosedMessage>(PreBattleDialogueClosed, _instanceId);
                var showCustomDialogueMsg = MessageFactory.GenerateShowCustomDialogueMsg();
                showCustomDialogueMsg.Dialogue = DialogueFactory.MonsterUnpreparedBattle;
                showCustomDialogueMsg.DoAfter = UnpreparedDialogueFinished;
                showCustomDialogueMsg.Owner = _controller.gameObject;
                _controller.gameObject.SendMessage(showCustomDialogueMsg);
                MessageFactory.CacheMessage(showCustomDialogueMsg);
            }
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
                var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                setDirectionMsg.Direction = Vector2.zero;
                _controller.gameObject.SendMessageTo(setDirectionMsg, WorldAdventureController.Player);
                MessageFactory.CacheMessage(setDirectionMsg);

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
            //Destroy(_exclamationController.gameObject);
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

        private void UnpreparedDialogueFinished()
        {
            var setUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
            setUnitStateMsg.State = AdventureUnitState.Idle;
            _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setUnitStateMsg);

            WorldController.SetWorldState(WorldState.World);
            _controller.gameObject.SendMessageTo(RespawnPlayerMessage.INSTANCE, WorldAdventureController.Player);
            WorldAdventureController.SetAdventureState(AdventureState.Overworld);
        }

        private void Defeat()
        {
            _controller.gameObject.SubscribeWithFilter<DialogueClosedMessage>(DefeatDialogueClosed, _instanceId);
            var showDialogueMsg = MessageFactory.GenerateShowCustomDialogueMsg();
            var dialogue = DialogueFactory.DefaultDefeat.ToList();
            if (_currentEncounter.GoldRemoveOnDefeat > 0)
            {
                dialogue.Add($"You lose {_currentEncounter.GoldRemoveOnDefeat}g");
            }
            showDialogueMsg.Dialogue = DialogueFactory.DefaultDefeat;
            showDialogueMsg.Owner = _controller.gameObject;
            _controller.gameObject.SendMessage(showDialogueMsg);
            MessageFactory.CacheMessage(showDialogueMsg);
            _currentEncounter = null;
        }

        private void DefeatDialogueFinished()
        {
            var setUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
            setUnitStateMsg.State = AdventureUnitState.Idle;
            _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);

            _controller.gameObject.SendMessageTo(RespawnPlayerMessage.INSTANCE, WorldAdventureController.Player);

            _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setUnitStateMsg);
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateAdventureUnitStateMessage>(UpdateUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetUnitSpawnerMessage>(SetUnitSpawner, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ObstacleMessage>(Obstacle, _instanceId);
            //_controller.transform.parent.gameObject.SubscribeWithFilter<InteractMessage>(Interact, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<EncounterFinishedMessage>(EncounterFinished, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateAdventureAiStateMessage>(UpdateAdventureAiState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateAdventureUnitStateMessage>(UpdateAdventureUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<MovementCompletedMessage>(MovementCompleted, _instanceId);
        }

        private void EncounterFinished(EncounterFinishedMessage msg)
        {
            if (msg.Result == BattleResult.Victory)
            {
                _currentEncounter = null;
                var setUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
                setUnitStateMsg.State = AdventureUnitState.Disabled;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);

                setUnitStateMsg.State = AdventureUnitState.Idle;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, WorldAdventureController.Player);
                MessageFactory.CacheMessage(setUnitStateMsg);
            }
            else if (msg.Result == BattleResult.Defeat)
            {
                Defeat();
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
                WorldAdventureController.Player.UnsubscribeFromAllMessagesWithFilter(_instanceId);
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
            var faceDirection = diff.normalized.ToVector2Int();
            var setFacingDirectionMsg = MessageFactory.GenerateSetFaceDirectionMsg();
            setFacingDirectionMsg.Direction = faceDirection;
            _controller.gameObject.SendMessageTo(setFacingDirectionMsg, WorldAdventureController.Player);
            MessageFactory.CacheMessage(setFacingDirectionMsg);

            Debug.Log($"Face Direction on Monster Encounter: {faceDirection}");

            var doBumpPixelsMsg = MessageFactory.GenerateDoBumpOverPixelsPerSecondMsg();
            doBumpPixelsMsg.PixelsPerSecond = WorldAdventureController.BattleBumpSpeed;
            doBumpPixelsMsg.Direction = faceDirection;
            doBumpPixelsMsg.Distance = WorldAdventureController.BattleBumpDistance;
            doBumpPixelsMsg.DoAfter = StartEncounter;
            doBumpPixelsMsg.OnBump = () => { };
            _controller.gameObject.SendMessageTo(doBumpPixelsMsg, msg.Owner);
            MessageFactory.CacheMessage(doBumpPixelsMsg);
        }

        private void UpdateMapTile(UpdateMapTileMessage msg)
        {
            _mapTile = msg.Tile;
            _encounterTiles = GetEncounterTiles(_mapTile.Position);


        }

        private void UpdatePlayerTile(UpdateMapTileMessage msg)
        {
            var playerUnitState = AdventureUnitState.Idle;
            var queryPlayerUnitStateMsg = MessageFactory.GenerateQueryAdventureUnitStateMsg();
            queryPlayerUnitStateMsg.DoAfter = unitState => playerUnitState = unitState;
            _controller.gameObject.SendMessageTo(queryPlayerUnitStateMsg, WorldAdventureController.Player);
            MessageFactory.CacheMessage(queryPlayerUnitStateMsg);

            if (_unitState == AdventureUnitState.Idle && IsPlayerEncounterable(playerUnitState))
            {
                if (playerUnitState == AdventureUnitState.Move)
                {
                    var setAdventureUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
                    setAdventureUnitStateMsg.State = AdventureUnitState.Interaction;
                    _controller.gameObject.SendMessageTo(setAdventureUnitStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setAdventureUnitStateMsg);

                    WorldAdventureController.Player.SubscribeWithFilter<MovementCompletedMessage>(PlayerMovementCompleted, _instanceId);
                }
                else
                {
                    StartExclamation(WorldAdventureController.Player);
                }
            }
        }

        private void UpdateAdventureAiState(UpdateAdventureAiStateMessage msg)
        {
            _aiState = msg.State;
            if (_aiState != AdventureAiState.Aggro)
            {
                WorldAdventureController.Player.UnsubscribeFromAllMessagesWithFilter(_instanceId);
            }
            else if (_aiState == AdventureAiState.Aggro)
            {
                WorldAdventureController.Player.SubscribeWithFilter<UpdateMapTileMessage>(UpdatePlayerTile, _instanceId);
            }
        }

        private void UpdateAdventureUnitState(UpdateAdventureUnitStateMessage msg)
        {
            _unitState = msg.State;
            if (_unitState == AdventureUnitState.Disabled)
            {
                WorldAdventureController.Player.UnsubscribeFromAllMessagesWithFilter(_instanceId);
            }
        }

        private void PlayerMovementCompleted(MovementCompletedMessage msg)
        {
            WorldAdventureController.Player.UnsubscribeFromFilter<MovementCompletedMessage>(_instanceId);
            StartExclamation(WorldAdventureController.Player);
        }

        private void MovementCompleted(MovementCompletedMessage msg)
        {
            var playerUnitState = AdventureUnitState.Idle;
            var queryPlayerUnitStateMsg = MessageFactory.GenerateQueryAdventureUnitStateMsg();
            queryPlayerUnitStateMsg.DoAfter = unitState => playerUnitState = unitState;
            _controller.gameObject.SendMessageTo(queryPlayerUnitStateMsg, WorldAdventureController.Player);

            MessageFactory.CacheMessage(queryPlayerUnitStateMsg);

            if (IsPlayerEncounterable(playerUnitState))
            {
                if (playerUnitState == AdventureUnitState.Move)
                {
                    var setAdventureUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
                    setAdventureUnitStateMsg.State = AdventureUnitState.Interaction;
                    _controller.gameObject.SendMessageTo(setAdventureUnitStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setAdventureUnitStateMsg);

                    WorldAdventureController.Player.SubscribeWithFilter<MovementCompletedMessage>(PlayerMovementCompleted, _instanceId);
                }
                else
                {
                    StartExclamation(WorldAdventureController.Player);
                }
            }
        }

        private void PreBattleDialogueClosed(DialogueClosedMessage msg)
        {
            if (_activeDialogue)
            {
                _activeDialogue = false;
                _controller.gameObject.Unsubscribe<DialogueClosedMessage>();
                _dialogueRoutine = _controller.StartCoroutine(StaticMethods.WaitForFrames(1, UnpreparedDialogueFinished));
            }
        }

        private void DefeatDialogueClosed(DialogueClosedMessage msg)
        {
            if (_activeDialogue)
            {
                _activeDialogue = false;
                _controller.gameObject.Unsubscribe<DialogueClosedMessage>();
                _dialogueRoutine = _controller.StartCoroutine(StaticMethods.WaitForFrames(1, DefeatDialogueFinished));
            }
        }

        public override void Destroy()
        {
            if (_dialogueRoutine != null)
            {
                _controller.StopCoroutine(_dialogueRoutine);
                _dialogueRoutine = null;
            }

            if (_activeDialogue)
            {
                _controller.gameObject.SendMessage(CloseDialogueMessage.INSTANCE);
            }

            _encounters = null;
            _encounterTiles = null;
            base.Destroy();
        }
    }
}
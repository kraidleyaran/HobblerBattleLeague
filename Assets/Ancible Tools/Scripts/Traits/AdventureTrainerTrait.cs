using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Adventure Trainer Trait", menuName = "Ancible Tools/Traits/Adventure/Interaction/Adventure Trainer")]
    public class AdventureTrainerTrait : Trait
    {
        [SerializeField] private BattleEncounter _encounter = null;
        [SerializeField] private int _engagePlayerDistance = 1;
        [SerializeField] private DialogueData _preBattleDialogue = null;
        [SerializeField] private DialogueData _defeatedDialogue = null;

        private MapTile[] _subscribedTiles = new MapTile[0];
        private Vector2Int _faceDirection = Vector2Int.zero;
        private MapTile _currentTile = null;

        private bool _defeated = false;

        private Coroutine _dialogueRoutine = null;

        private bool _isBattling = false;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void PreBattleDialogueClosed()
        {
            _dialogueRoutine = null;
            BattleLeagueManager.SetupEncounter(_encounter, _controller.transform.parent.gameObject);
        }

        private void DefeatDialogueClosed()
        {
            _dialogueRoutine = null;

            var setUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
            setUnitStateMsg.State = AdventureUnitState.Idle;
            _controller.gameObject.SendMessageTo(setUnitStateMsg, WorldAdventureController.Player);
            _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setUnitStateMsg);
        }

        private void ForceEncounter(GameObject obj)
        {
            MapTile playerTile = null;
            var queryMapTileMsg = MessageFactory.GenerateQueryMapTileMsg();
            queryMapTileMsg.DoAfter = tile => playerTile = tile;
            _controller.gameObject.SendMessageTo(queryMapTileMsg, obj);
            MessageFactory.CacheMessage(queryMapTileMsg);

            if (playerTile != null)
            {
                var setUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
                setUnitStateMsg.State = AdventureUnitState.Interaction;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, obj);
                MessageFactory.CacheMessage(setUnitStateMsg);
                var distance = _currentTile.Position.DistanceTo(playerTile.Position);
                if (distance > 1)
                {
                    var moveTile = WorldAdventureController.MapController.PlayerPathing.GetTileByPosition(playerTile.Position + _faceDirection * -1);
                    if (moveTile != null)
                    {
                        var path = WorldAdventureController.MapController.PlayerPathing.GetPath(_currentTile.Position, moveTile.Position, false);
                        if (path.Length > 0)
                        {
                            var setPathMsg = MessageFactory.GenerateSetPathMsg();
                            setPathMsg.Path = path;
                            setPathMsg.DoAfter = StartEncounter;
                            _controller.gameObject.SendMessageTo(setPathMsg, _controller.transform.parent.gameObject);
                            MessageFactory.CacheMessage(setPathMsg);
                        }
                        else
                        {
                            StartEncounter();
                        }
                    }
                    else
                    {
                        StartEncounter();
                    }
                }
                else
                {
                    StartEncounter();
                }

            }
        }

        private void StartEncounter()
        {
            var setFacingDirectionMsg = MessageFactory.GenerateSetFaceDirectionMsg();
            setFacingDirectionMsg.Direction = (WorldAdventureController.Player.transform.position.ToVector2() - _controller.transform.parent.position.ToVector2()).normalized.ToVector2Int();
            _controller.gameObject.SendMessageTo(setFacingDirectionMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setFacingDirectionMsg);

            var setUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
            setUnitStateMsg.State = AdventureUnitState.Interaction;
            _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setUnitStateMsg);

            var setplayerInteractionObjMsg = MessageFactory.GenerateSetPlayerInteractionObjectMsg();
            setplayerInteractionObjMsg.Interact = _controller.transform.parent.gameObject;
            _controller.gameObject.SendMessageTo(setplayerInteractionObjMsg, WorldAdventureController.Player);
            MessageFactory.CacheMessage(setplayerInteractionObjMsg);

            _isBattling = true;
            if (_preBattleDialogue)
            {
                var showDialogueMsg = MessageFactory.GenerateShowDialogueMsg();
                showDialogueMsg.Owner = _controller.gameObject;
                showDialogueMsg.Dialogue = _preBattleDialogue;
                _controller.gameObject.SendMessage(showDialogueMsg);
                MessageFactory.CacheMessage(showDialogueMsg);
            }
            else
            {
                PreBattleDialogueClosed();
            }
            
        }

        private Vector2Int[] GenerateRelativePositions()
        {
            if (_faceDirection != Vector2Int.zero)
            {
                var positions = new List<Vector2Int>();
                for (var i = 0; i < _engagePlayerDistance; i++)
                {
                    positions.Add(_currentTile.Position + _faceDirection * (i + 1));
                }

                return positions.ToArray();
            }
            return new Vector2Int[0];
        }

        private void UpdateSubscribedTiles()
        {
            foreach (var tile in _subscribedTiles)
            {
                tile.OnObjectEnteringTile -= ForceEncounter;
            }
            _subscribedTiles = GenerateRelativePositions().Select(WorldAdventureController.MapController.PlayerPathing.GetTileByPosition).Where(t => t != null).ToArray();
            foreach (var tile in _subscribedTiles)
            {
                tile.OnObjectEnteringTile += ForceEncounter;
            }
        }

        private void Victory()
        {
            _defeated = true;
            foreach (var tile in _subscribedTiles)
            {
                tile.OnObjectEnteringTile -= ForceEncounter;
            }

            if (_defeatedDialogue)
            {
                var showDialogueMsg = MessageFactory.GenerateShowDialogueMsg();
                showDialogueMsg.Dialogue = _defeatedDialogue;
                showDialogueMsg.Owner = _controller.gameObject;
                _controller.gameObject.SendMessage(showDialogueMsg);
                MessageFactory.CacheMessage(showDialogueMsg);
            }
            else
            {
                DefeatDialogueClosed();
            }

        }

        private void Defeat()
        {
            _controller.gameObject.SendMessageTo(RespawnPlayerMessage.INSTANCE, WorldAdventureController.Player);
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.SubscribeWithFilter<DialogueClosedMessage>(DialogueClosed, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateFacingDirectionMessage>(UpdateFaceDirection, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<EncounterFinishedMessage>(EncounterFinished, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<PlayerInteractMessage>(PlayerInteract, _instanceId);
        }

        private void UpdateFaceDirection(UpdateFacingDirectionMessage msg)
        {
            _faceDirection = msg.Direction;
            if (!_defeated)
            {
                UpdateSubscribedTiles();
            }

        }

        private void UpdateMapTile(UpdateMapTileMessage msg)
        {
            _currentTile = msg.Tile;
            if (!_defeated)
            {
                UpdateSubscribedTiles();
            }
        }

        private void EncounterFinished(EncounterFinishedMessage msg)
        {
            _isBattling = false;
            var setUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
            setUnitStateMsg.State = AdventureUnitState.Idle;
            _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setUnitStateMsg);
            switch (msg.Result)
            {
                case BattleResult.Victory:
                    Victory();
                    break;
                case BattleResult.Defeat:
                    Defeat();
                    break;
            }
        }

        private void DialogueClosed(DialogueClosedMessage msg)
        {
            _dialogueRoutine = _controller.StartCoroutine(_isBattling ? StaticMethods.WaitForFrames(1, PreBattleDialogueClosed) : StaticMethods.WaitForFrames(1, DefeatDialogueClosed));
        }

        private void PlayerInteract(PlayerInteractMessage msg)
        {
            if (WorldController.State == WorldState.Adventure && WorldAdventureController.State == AdventureState.Overworld)
            {
                if (_defeated)
                {
                    _isBattling = false;
                    var setFacingDirectionMsg = MessageFactory.GenerateSetFaceDirectionMsg();
                    setFacingDirectionMsg.Direction = (WorldAdventureController.Player.transform.position.ToVector2() - _controller.transform.parent.position.ToVector2()).normalized.ToVector2Int();
                    _controller.gameObject.SendMessageTo(setFacingDirectionMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setFacingDirectionMsg);

                    var setUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
                    setUnitStateMsg.State = AdventureUnitState.Interaction;
                    _controller.gameObject.SendMessageTo(setUnitStateMsg, WorldAdventureController.Player);
                    _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setUnitStateMsg);

                    var showDialogueMsg = MessageFactory.GenerateShowDialogueMsg();
                    showDialogueMsg.Dialogue = _defeatedDialogue;
                    showDialogueMsg.Owner = _controller.gameObject;
                    _controller.gameObject.SendMessage(showDialogueMsg);
                    MessageFactory.CacheMessage(showDialogueMsg);
                }
                else
                {
                    StartEncounter();
                }
            }
            
        }
    }
}
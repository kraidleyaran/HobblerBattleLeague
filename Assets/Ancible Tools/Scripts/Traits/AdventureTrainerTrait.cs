using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.Dialogue;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.SaveData;
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
        [SerializeField] private string[] _victoryDialogue = new string[0];
        [SerializeField] private Color _exclamationColor = Color.red;
        [SerializeField] private Vector2Int _exclamationOffset = Vector2Int.zero;
        [SerializeField] private int _exclamationTicks = 30;
        public string SaveId = string.Empty;

        private MapTile[] _subscribedTiles = new MapTile[0];
        private Vector2Int _faceDirection = Vector2Int.zero;
        private MapTile _currentTile = null;
        private AdventureBattleExclamationController _exclamationController = null;
        private MapTile _originTile = null;

        private bool _defeated = false;

        private Coroutine _dialogueRoutine = null;

        private bool _isBattling = false;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            if (!string.IsNullOrEmpty(SaveId))
            {
                _defeated = PlayerDataController.GetTrainerDataById(SaveId) != null;
            }
            SubscribeToMessages();
        }

        private void PreBattleDialogueClosed()
        {
            _dialogueRoutine = null;
            BattleLeagueManager.SetupEncounter(_encounter, _controller.transform.parent.gameObject);
        }

        private void VictoryDialogueClosed()
        {
            _dialogueRoutine = null;
            
            _controller.gameObject.SubscribeWithFilter<DialogueClosedMessage>(VictoryDefaultDialogueClosed, _instanceId);
            var showDialogueMsg = MessageFactory.GenerateShowCustomDialogueMsg();
            var defaultVictory = DialogueFactory.DefaultDefeat.ToList();
            if (_encounter.GoldRemoveOnDefeat > 0)
            {
                defaultVictory.Add($"You lose {_encounter.GoldRemoveOnDefeat}g");
            }
            showDialogueMsg.Owner = _controller.gameObject;
            showDialogueMsg.Dialogue = defaultVictory.ToArray();
            _controller.gameObject.SendMessage(showDialogueMsg);
            MessageFactory.CacheMessage(showDialogueMsg);
        }

        private void DefaultVictoryDialogueClosed()
        {
            _dialogueRoutine = null;

            _controller.gameObject.SendMessageTo(RespawnPlayerMessage.INSTANCE, WorldAdventureController.Player);
            var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
            setMapTileMsg.Tile = _originTile;
            _controller.gameObject.SendMessageTo(setMapTileMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setMapTileMsg);
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

        private void UnpreparedDialogueClosed()
        {
            _dialogueRoutine = null;
            var setUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
            setUnitStateMsg.State = AdventureUnitState.Idle;
            _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setUnitStateMsg);

            WorldController.SetWorldState(WorldState.World);
            _controller.gameObject.SendMessageTo(RespawnPlayerMessage.INSTANCE, WorldAdventureController.Player);
            WorldAdventureController.SetAdventureState(AdventureState.Overworld);
        }

        private void ForceEncounter(GameObject obj)
        {
            var playerState = AdventureUnitState.Idle;
            var queryAdventureUnitStateMsg = MessageFactory.GenerateQueryAdventureUnitStateMsg();
            queryAdventureUnitStateMsg.DoAfter = state => { playerState = state; };
            _controller.gameObject.SendMessageTo(queryAdventureUnitStateMsg, WorldAdventureController.Player);
            MessageFactory.CacheMessage(queryAdventureUnitStateMsg);

            if (playerState != AdventureUnitState.Interaction)
            {
                MapTile playerTile = null;
                var queryMapTileMsg = MessageFactory.GenerateQueryMapTileMsg();
                queryMapTileMsg.DoAfter = tile => playerTile = tile;
                _controller.gameObject.SendMessageTo(queryMapTileMsg, obj);
                MessageFactory.CacheMessage(queryMapTileMsg);

                if (playerTile != null)
                {

                    var distance = _currentTile.Position.DistanceTo(playerTile.Position);
                    if (distance > 1)
                    {
                        var moveTile = WorldAdventureController.MapController.PlayerPathing.GetTileByPosition(playerTile.Position + _faceDirection * -1);
                        if (moveTile != null)
                        {
                            var path = WorldAdventureController.MapController.PlayerPathing.GetPath(_currentTile.Position, moveTile.Position);
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
            if (WorldHobblerManager.GetAvailableRoster().Length > 0)
            {
                _isBattling = true;
                if (_preBattleDialogue)
                {
                    _controller.gameObject.SubscribeWithFilter<DialogueClosedMessage>(BattleDialogueClosed, _instanceId);
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
            else
            {
                Unprepared();
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
                tile.OnObjectEnteringTile -= StartExclamation;
            }

            if (_currentTile != null)
            {
                _subscribedTiles = GenerateRelativePositions().Select(WorldAdventureController.MapController.PlayerPathing.GetTileByPosition).Where(t => t != null).ToArray();
                foreach (var tile in _subscribedTiles)
                {
                    tile.OnObjectEnteringTile += StartExclamation;
                }
            }
            else
            {
                _subscribedTiles = new MapTile[0];
            }
        }

        private void Victory()
        {
            //TODO: Repeatable rewards - trainers will have better rewards so they will only be fightable once a day?
            if (!_defeated)
            {
                _defeated = true;
                _controller.gameObject.Subscribe<QueryTrainerDataMessage>(QueryTrainerData);
            }
            
            foreach (var tile in _subscribedTiles)
            {
                tile.OnObjectEnteringTile -= StartExclamation;
            }

            if (_defeatedDialogue)
            {
                _controller.gameObject.SubscribeWithFilter<DialogueClosedMessage>(BattleDialogueClosed, _instanceId);
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
            _controller.gameObject.SubscribeWithFilter<DialogueClosedMessage>(VictoryDialogueClosed, _instanceId);
            var showDialogueMsg = MessageFactory.GenerateShowCustomDialogueMsg();
            showDialogueMsg.Dialogue = _victoryDialogue;
            showDialogueMsg.Owner = _controller.gameObject;
            _controller.gameObject.SendMessage(showDialogueMsg);
            MessageFactory.CacheMessage(showDialogueMsg);
        }

        private void Unprepared()
        {
            _controller.gameObject.SubscribeWithFilter<DialogueClosedMessage>(UnpreparedDialogueClosed, _instanceId);
            var showDialogueMsg = MessageFactory.GenerateShowCustomDialogueMsg();
            showDialogueMsg.Dialogue = DialogueFactory.TrainerUnpreparedBattle;
            showDialogueMsg.Owner = _controller.gameObject;
            _controller.gameObject.SendMessage(showDialogueMsg);
            MessageFactory.CacheMessage(showDialogueMsg);
        }

        private void StartExclamation(GameObject obj)
        {
            var setUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
            setUnitStateMsg.State = AdventureUnitState.Interaction;
            _controller.gameObject.SendMessageTo(setUnitStateMsg, obj);
            MessageFactory.CacheMessage(setUnitStateMsg);

            _exclamationController = Instantiate(FactoryController.BATTLE_EXCLAMATION, _controller.transform);
            var offset = new Vector2(_exclamationOffset.x * DataController.Interpolation, _exclamationOffset.y * DataController.Interpolation);
            _exclamationController.transform.SetLocalPosition(offset);
            _exclamationController.Setup(_exclamationTicks, FinishExclamation, _exclamationColor);
        }

        private void FinishExclamation()
        {
            Destroy(_exclamationController.gameObject);
            ForceEncounter(WorldAdventureController.Player);
        }

        private void SubscribeToMessages()
        {
            

            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateFacingDirectionMessage>(UpdateFaceDirection, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<EncounterFinishedMessage>(EncounterFinished, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<PlayerInteractMessage>(PlayerInteract, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetupTrainerMessage>(SetupTrainer, _instanceId);
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
            if (_originTile == null)
            {
                _originTile = _currentTile;
            }
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

        private void BattleDialogueClosed(DialogueClosedMessage msg)
        {
            _controller.gameObject.Unsubscribe<DialogueClosedMessage>();
            _dialogueRoutine = _controller.StartCoroutine(_isBattling ? StaticMethods.WaitForFrames(1, PreBattleDialogueClosed) : StaticMethods.WaitForFrames(1, DefeatDialogueClosed));
        }

        private void UnpreparedDialogueClosed(DialogueClosedMessage msg)
        {
            _controller.gameObject.Unsubscribe<DialogueClosedMessage>();
            _dialogueRoutine = _controller.StartCoroutine(StaticMethods.WaitForFrames(1, UnpreparedDialogueClosed));
        }

        private void VictoryDialogueClosed(DialogueClosedMessage msg)
        {
            _controller.gameObject.Unsubscribe<DialogueClosedMessage>();
            _dialogueRoutine = _controller.StartCoroutine(StaticMethods.WaitForFrames(1, VictoryDialogueClosed));
        }

        private void VictoryDefaultDialogueClosed(DialogueClosedMessage msg)
        {
            _controller.gameObject.Unsubscribe<DialogueClosedMessage>();
            _dialogueRoutine = _controller.StartCoroutine(StaticMethods.WaitForFrames(1, DefaultVictoryDialogueClosed));
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

        private void QueryTrainerData(QueryTrainerDataMessage msg)
        {
            if (_defeated)
            {
                msg.DoAfter.Invoke(SaveId);
            }
        }

        private void SetupTrainer(SetupTrainerMessage msg)
        {
            _encounter = msg.Encounter;
            _preBattleDialogue = msg.PreEncounterDialogue;
            _defeatedDialogue = msg.DefeatedDialogue;
            SaveId = msg.Id;
            _defeated = PlayerDataController.GetTrainerDataById(SaveId) != null;
            if (_defeated && _subscribedTiles.Length > 0)
            {
                foreach (var tile in _subscribedTiles)
                {
                    tile.OnObjectEnteringTile -= StartExclamation;
                }
                _subscribedTiles = new MapTile[0];
            }
        }

        public override void Destroy()
        {
            if (_dialogueRoutine != null)
            {
                _controller.StopCoroutine(_dialogueRoutine);
            }
            base.Destroy();
        }
    }

}
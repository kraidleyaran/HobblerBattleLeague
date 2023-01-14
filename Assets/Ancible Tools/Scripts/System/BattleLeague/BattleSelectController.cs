using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague
{
    public class BattleSelectController : MonoBehaviour
    {
        private static BattleSelectController _instance = null;

        private ContactFilter2D _contactFilter = new ContactFilter2D();

        [SerializeField] private UnitSelectorController _hoverController;
        [SerializeField] private UnitSelectorController _selectController;
        [SerializeField] private GameObject _placeSelector = null;

        private QueryBattleGamePiecePlacementMessage _queryBattleGamePiecePlacementMsg = new QueryBattleGamePiecePlacementMessage();
        private UpdateSelectedBattleUnitMessage _updateSelectedBattleUnitMsg = new UpdateSelectedBattleUnitMessage();
        private SetSelectStateMesage _setSelectStateMsg = new SetSelectStateMesage();
        private SetHoveredStateMessage _setHoverStateMsg = new SetHoveredStateMessage();

        private GameObject _hoveredGamePiece = null;
        private GameObject _selectedGamePiece = null;
        private GameObject _movableGamePiece = null;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _selectController.ResetSelector(transform);
            _hoverController.ResetSelector(transform);
            _selectController.gameObject.SetActive(false);
            _hoverController.gameObject.SetActive(false);
            _placeSelector.gameObject.SetActive(false);
        }

        void Start()
        {
            _contactFilter = new ContactFilter2D { useLayerMask = true, layerMask = CollisionLayerFactory.BattleSelect.ToMask(), useTriggers = true};
            SubscribeToMessages();
        }

        public static void ReturnHover(GameObject owner)
        {
            if (_instance)
            {
                if (_instance._hoveredGamePiece && _instance._hoveredGamePiece == owner)
                {
                    _instance._hoveredGamePiece = null;
                    _instance._hoverController.ResetSelector(_instance.transform);
                    _instance._hoverController.gameObject.SetActive(false);
                }
            }

        }

        public static void ReturnSelect(GameObject owner)
        {
            if (_instance)
            {
                if (_instance._selectedGamePiece && _instance._selectedGamePiece == owner)
                {
                    _instance._selectedGamePiece = null;
                    _instance._selectController.ResetSelector(_instance.transform);
                    _instance._selectController.gameObject.SetActive(false);
                }
            }

        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
            gameObject.Subscribe<UpdateBattleStateMessage>(UpdateBattleState);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            var pos = BattleLeagueCameraController.Camera.ScreenToWorldPoint(msg.Current.MousePos).ToVector2();
            if (_movableGamePiece)
            {
                if (msg.Previous.LeftClick && msg.Current.LeftClick)
                {
                    if (msg.Previous.MousePos != msg.Current.MousePos)
                    {
                        _movableGamePiece.transform.SetTransformPosition(pos);
                    }

                    if (BattleLeagueController.State == BattleState.Prep)
                    {
                        var availableTile = BattleLeagueController.GetMapTileByWorldPositionAlignment(pos, BattleAlignment.Left);
                        if (availableTile != null)
                        {
                            _placeSelector.transform.SetTransformPosition(availableTile.World);
                            _placeSelector.gameObject.SetActive(true);
                        }
                        else
                        {
                            BattleBenchSlotController closestSlot = null;
                            _queryBattleGamePiecePlacementMsg.DoAfter = (current, bench, tile) =>
                            {
                                closestSlot = bench;
                            };
                            gameObject.SendMessageTo(_queryBattleGamePiecePlacementMsg, _movableGamePiece);
                            if (closestSlot)
                            {
                                _placeSelector.transform.SetTransformPosition(closestSlot.transform.position.ToVector2());
                                _placeSelector.gameObject.SetActive(true);
                            }
                            else
                            {
                                _placeSelector.gameObject.SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        BattleBenchSlotController closestSlot = null;
                        _queryBattleGamePiecePlacementMsg.DoAfter = (current, bench, tile) =>
                        {
                            closestSlot = bench;
                        };
                        gameObject.SendMessageTo(_queryBattleGamePiecePlacementMsg, _movableGamePiece);
                        if (closestSlot)
                        {
                            _placeSelector.transform.SetTransformPosition(closestSlot.transform.position.ToVector2());
                            _placeSelector.gameObject.SetActive(true);
                        }
                        else
                        {
                            _placeSelector.gameObject.SetActive(false);
                        }
                    }
                }
                else if (msg.Previous.LeftClick && !msg.Current.LeftClick)
                {   
                    BattleBenchSlotController currentSlot = null;
                    BattleBenchSlotController closestSlot = null;
                    MapTile mapTile = null;
                    _queryBattleGamePiecePlacementMsg.DoAfter = (current, bench, tile) =>
                    {
                        currentSlot = current;
                        closestSlot = bench;
                        mapTile = tile;
                    };
                    gameObject.SendMessageTo(_queryBattleGamePiecePlacementMsg, _movableGamePiece);
                    if (closestSlot)
                    {
                        if (currentSlot)
                        {
                            BattleLeagueController.LeftBench.ChangeGamePieceBenchSlot(_movableGamePiece, closestSlot, currentSlot);
                        }
                        else if (mapTile != null && BattleLeagueController.State == BattleState.Prep)
                        {
                            BattleLeagueController.LeftBench.BenchPieceFromPlay(_movableGamePiece, closestSlot, mapTile);
                        }
                    }
                    else
                    {
                        if (BattleLeagueController.State == BattleState.Prep)
                        {
                            var availableTile = BattleLeagueController.GetMapTileByWorldPositionAlignment(pos, BattleAlignment.Left);
                            if (availableTile != null)
                            {
                                if (mapTile != null)
                                {
                                    BattleLeagueController.LeftBench.PlacePieceAtTileFromPlay(_movableGamePiece, availableTile, mapTile);
                                }
                                else if (currentSlot)
                                {
                                    BattleLeagueController.LeftBench.PlacePieceAtTileFromBench(_movableGamePiece, availableTile, currentSlot);
                                }
                            }
                            else if (mapTile != null)
                            {
                                var setGamePieceMapTileMsg = MessageFactory.GenerateSetGamePieceMapTileMsg();
                                setGamePieceMapTileMsg.Tile = mapTile;
                                gameObject.SendMessageTo(setGamePieceMapTileMsg, _movableGamePiece);
                                MessageFactory.CacheMessage(setGamePieceMapTileMsg);
                            }
                            else
                            {
                                BattleLeagueController.LeftBench.BenchPieceFromPlay(_movableGamePiece, null, mapTile);
                            }
                        }
                        else if (currentSlot)
                        {
                            currentSlot.SetCurrentPiece(_movableGamePiece);
                        }

                    }

                    if (_selectedGamePiece)
                    {
                        _selectController.gameObject.SetActive(true);
                    }
                    
                    if (_hoveredGamePiece)
                    {
                        _hoverController.gameObject.SetActive(true);
                    }
                    _placeSelector.gameObject.SetActive(false);
                    _movableGamePiece = null;
                }
            }
            else
            {
                
                var results = new RaycastHit2D[5];
                var hitCount = Physics2D.Raycast(pos, Vector2.zero, _contactFilter, results, 2f);
                if (hitCount > 0)
                {
                    var hovered = false;
                    var isPiece = false;
                    var queryBattlePieceMsg = MessageFactory.GenerateQueryBattlePieceMsg();
                    queryBattlePieceMsg.DoAfter = (data, alignment) => { isPiece = true; };
                    for (var i = 0; i < hitCount; i++)
                    {
                        var obj = results[i].transform.gameObject;
                        gameObject.SendMessageTo(queryBattlePieceMsg, obj);
                        if (isPiece)
                        {
                            hovered = true;
                            if (!_hoveredGamePiece || _hoveredGamePiece != obj)
                            {
                                if (_hoveredGamePiece)
                                {
                                    _setHoverStateMsg.Selector = null;
                                    gameObject.SendMessageTo(_setHoverStateMsg, _hoveredGamePiece);
                                }
                                _hoveredGamePiece = obj;
                                _setHoverStateMsg.Selector = _hoverController;
                                gameObject.SendMessageTo(_setHoverStateMsg, _hoveredGamePiece);

                            }
                            break;
                        }

                    }
                    MessageFactory.CacheMessage(queryBattlePieceMsg);

                    if (!hovered)
                    {
                        if (_hoveredGamePiece)
                        {
                            _setHoverStateMsg.Selector = null;
                            gameObject.SendMessageTo(_setHoverStateMsg, _hoveredGamePiece);
                            _hoveredGamePiece = null;
                            _hoverController.ResetSelector(transform);
                            _hoverController.gameObject.SetActive(false);
                        }

                    }
                }
                else if (_hoveredGamePiece)
                {
                    _setHoverStateMsg.Selector = null;
                    gameObject.SendMessageTo(_setHoverStateMsg, _hoveredGamePiece);
                    _hoveredGamePiece = null;
                    _hoverController.ResetSelector(transform);
                    _hoverController.gameObject.SetActive(false);
                }

                var change = false;
                if (_hoveredGamePiece)
                {
                    if (!msg.Previous.LeftClick && msg.Current.LeftClick)
                    {

                        if (!_selectedGamePiece || _selectedGamePiece != _hoveredGamePiece)
                        {
                            change = true;
                            if (_selectedGamePiece)
                            {
                                _setSelectStateMsg.Selector = null;
                                gameObject.SendMessageTo(_setSelectStateMsg, _selectedGamePiece);
                            }

                            _selectedGamePiece = _hoveredGamePiece;
                            _setSelectStateMsg.Selector = _selectController;
                            gameObject.SendMessageTo(_setSelectStateMsg, _selectedGamePiece);

                        }

                        if (_selectedGamePiece && BattleLeagueController.State == BattleState.Prep)
                        {
                            var queryBattlePieceMsg = MessageFactory.GenerateQueryBattlePieceMsg();
                            queryBattlePieceMsg.DoAfter = (data, alignment) =>
                            {
                                _movableGamePiece = alignment == BattleAlignment.Left ? _selectedGamePiece : null;
                            };
                            gameObject.SendMessageTo(queryBattlePieceMsg, _selectedGamePiece);
                            MessageFactory.CacheMessage(queryBattlePieceMsg);

                            if (_movableGamePiece)
                            {
                                _selectController.gameObject.SetActive(false);
                                _hoverController.gameObject.SetActive(false);
                            }

                        }
                    }
                }
                else if (!msg.Previous.LeftClick && msg.Current.LeftClick && _selectedGamePiece)
                {
                    _setSelectStateMsg.Selector = null;
                    gameObject.SendMessageTo(_setSelectStateMsg, _selectedGamePiece);
                    change = true;
                    _selectedGamePiece = null;
                    _selectController.ResetSelector(transform);
                    _selectController.gameObject.SetActive(false);
                }

                if (change)
                {
                    _updateSelectedBattleUnitMsg.Unit = _selectedGamePiece;
                    gameObject.SendMessage(_updateSelectedBattleUnitMsg);
                }
            }
        }

        private void UpdateBattleState(UpdateBattleStateMessage msg)
        {
            if (msg.State == BattleState.Countdown)
            {
                if (_movableGamePiece)
                {
                    BattleBenchSlotController currentSlot = null;
                    MapTile mapTile = null;
                    _queryBattleGamePiecePlacementMsg.DoAfter = (current, bench, tile) =>
                    {
                        currentSlot = current;
                        mapTile = tile;
                    };
                    gameObject.SendMessageTo(_queryBattleGamePiecePlacementMsg, _movableGamePiece);
                    if (mapTile != null)
                    {
                        var setGamePieceMapTileMsg = MessageFactory.GenerateSetGamePieceMapTileMsg();
                        setGamePieceMapTileMsg.Tile = mapTile;
                        gameObject.SendMessageTo(setGamePieceMapTileMsg, _movableGamePiece);
                        MessageFactory.CacheMessage(setGamePieceMapTileMsg);
                    }
                    else if (currentSlot)
                    {
                        currentSlot.SetCurrentPiece(_movableGamePiece);
                    }
                    _placeSelector.gameObject.SetActive(false);
                    _movableGamePiece = null;

                }


                //if (_selectedGamePiece)
                //{
                //    _selectedGamePiece = null;
                //    _updateSelectedBattleUnitMsg.Unit = _selectedGamePiece;
                //    gameObject.SendMessage(_updateSelectedBattleUnitMsg);
                //    _selectController.ResetSelector(transform);
                //    _selectController.gameObject.SetActive(false);
                //}
            }
            //else if (msg.State == BattleState.Prep && _selectedGamePiece)
            //{
            //    _selectedGamePiece = null;
            //    _updateSelectedBattleUnitMsg.Unit = _selectedGamePiece;
            //    gameObject.SendMessage(_updateSelectedBattleUnitMsg);
            //}

        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}
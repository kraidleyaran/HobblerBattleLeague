using Assets.Resources.Ancible_Tools.Scripts.System.Building;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class UnitSelectController : MonoBehaviour
    {
        public static GameObject Hovered => _instance._hoveredUnit;

        private static UnitSelectController _instance = null;

        private GameObject _hoveredUnit = null;
        private GameObject _selectedUnit = null;

        [SerializeField] private int _maxHitCount = 5;
        [SerializeField] private float _depthCheck = 1f;
        
        [Header("Child References")]
        [SerializeField] private UnitSelectorController _hoveredSelector;
        [SerializeField] private UnitSelectorController _selectedSelector;

        private SetHoveredStateMessage _setHoveredStateMsg = new SetHoveredStateMessage();
        private SetSelectStateMesage _setSelectStateMsg = new SetSelectStateMesage();
        private UpdateSelectedUnitMessage _updateSelectedUnitMsg = new UpdateSelectedUnitMessage();

        private ContactFilter2D _contactFilter = new ContactFilter2D();

        void Start()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            _contactFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = CollisionLayerFactory.UnitSelect.ToMask(),
                useTriggers = true,
            };
            _hoveredSelector.gameObject.SetActive(false);
            _selectedSelector.gameObject.SetActive(false);
            SubscribeToMessages();
        }

        public static void SelectUnit(GameObject unit)
        {
            _instance.SelectUnitOverride(unit);
        }

        private void SelectUnitOverride(GameObject unit)
        {
            if (_selectedUnit)
            {
                _setSelectStateMsg.Selector = null;
                gameObject.SendMessageTo(_setHoveredStateMsg, _selectedUnit);
            }

            _selectedUnit = unit;
            _setSelectStateMsg.Selector = _selectedSelector;
            gameObject.SendMessageTo(_setSelectStateMsg, _selectedUnit);
            _updateSelectedUnitMsg.Unit = _selectedUnit;
            gameObject.SendMessage(_updateSelectedUnitMsg);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
            gameObject.Subscribe<RemoveHoveredUnitMessage>(RemoveHoveredUnit);
            gameObject.Subscribe<RemoveSelectedUnitMessage>(RemoveSelectedUnit);
            gameObject.Subscribe<WorldBuildingActiveMessage>(WorldBuildingActive);
            gameObject.Subscribe<UpdateWorldStateMessage>(UpdateWorldState);
            gameObject.Subscribe<ClearWorldMessage>(ClearWorld);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (WorldController.State == WorldState.World && !WorldBuildingManager.Active && !UiWindowManager.Hovered && WorldCameraController.Camera)
            {
                var results = new RaycastHit2D[_maxHitCount];
                var hitCount = Physics2D.Raycast(WorldCameraController.Camera.ScreenToWorldPoint(msg.Current.MousePos), Vector2.zero, _contactFilter, results, _depthCheck);
                if (hitCount > 0)
                {
                    if (!_hoveredUnit || _hoveredUnit != results[0].transform.gameObject)
                    {
                        if (_hoveredUnit)
                        {
                            _setHoveredStateMsg.Selector = null;
                            gameObject.SendMessageTo(_setHoveredStateMsg, _hoveredUnit);
                        }
                        _hoveredSelector.ResetSelector(transform);
                        _hoveredUnit = results[0].transform.gameObject;
                        _setHoveredStateMsg.Selector = _hoveredSelector;
                        gameObject.SendMessageTo(_setHoveredStateMsg, _hoveredUnit);
                    }
                }
                else if (_hoveredUnit)
                {
                    _setHoveredStateMsg.Selector = null;
                    gameObject.SendMessageTo(_setHoveredStateMsg, _hoveredUnit);
                    _hoveredUnit = null;
                    _hoveredSelector.ResetSelector(transform);
                    _hoveredSelector.gameObject.SetActive(false);
                }

                if (!msg.Previous.LeftClick && msg.Current.LeftClick)
                {
                    if (_hoveredUnit && (!_selectedUnit || _hoveredUnit != _selectedUnit))
                    {
                        if (_selectedUnit)
                        {
                            _setSelectStateMsg.Selector = null;
                            gameObject.SendMessageTo(_setHoveredStateMsg, _selectedUnit);
                        }

                        _selectedUnit = _hoveredUnit;
                        _setSelectStateMsg.Selector = _selectedSelector;
                        gameObject.SendMessageTo(_setSelectStateMsg, _selectedUnit);
                        _updateSelectedUnitMsg.Unit = _selectedUnit;
                        gameObject.SendMessage(_updateSelectedUnitMsg);
                    }
                    //else if (!_hoveredUnit && _selectedUnit)
                    //{
                    //    if (_selectedUnit)
                    //    {
                    //        _setSelectStateMsg.Selector = null;
                    //        gameObject.SendMessageTo(_setHoveredStateMsg, _selectedUnit);
                    //        _selectedUnit = null;
                    //        _selectedSelector.ResetSelector(transform);
                    //        _selectedSelector.gameObject.SetActive(false);
                    //        _updateSelectedUnitMsg.Unit = null;
                    //        gameObject.SendMessage(_updateSelectedUnitMsg);
                    //    }
                    //}
                }
                else if (!msg.Previous.RightClick && msg.Current.RightClick && _selectedUnit)
                {

                    _setSelectStateMsg.Selector = null;
                    gameObject.SendMessageTo(_setHoveredStateMsg, _selectedUnit);
                    _selectedUnit = null;
                    _selectedSelector.ResetSelector(transform);
                    _selectedSelector.gameObject.SetActive(false);
                    _updateSelectedUnitMsg.Unit = null;
                    gameObject.SendMessage(_updateSelectedUnitMsg);
                }
            }

        }

        private void RemoveHoveredUnit(RemoveHoveredUnitMessage msg)
        {
            if (_hoveredUnit && _hoveredUnit == msg.Unit)
            {
                _hoveredUnit = null;
                _hoveredSelector.ResetSelector(transform);
                _hoveredSelector.gameObject.SetActive(false);
            }
        }

        private void RemoveSelectedUnit(RemoveSelectedUnitMessage msg)
        {
            if (_selectedUnit && _selectedUnit == msg.Unit)
            {
                _selectedUnit = null;
                _selectedSelector.ResetSelector(transform);
                _selectedSelector.gameObject.SetActive(false);
            }
        }

        private void WorldBuildingActive(WorldBuildingActiveMessage msg)
        {
            if (_hoveredUnit)
            {
                _setHoveredStateMsg.Selector = null;
                gameObject.SendMessageTo(_setHoveredStateMsg, _hoveredUnit);
                _hoveredUnit = null;
                _hoveredSelector.ResetSelector(transform);
                _hoveredSelector.gameObject.SetActive(false);
            }

            if (_selectedUnit)
            {
                _setSelectStateMsg.Selector = null;
                gameObject.SendMessageTo(_setHoveredStateMsg, _selectedUnit);
                _selectedUnit = null;
                _selectedSelector.ResetSelector(transform);
                _selectedSelector.gameObject.SetActive(false);
                _updateSelectedUnitMsg.Unit = null;
                gameObject.SendMessage(_updateSelectedUnitMsg);
            }

        }

        private void UpdateWorldState(UpdateWorldStateMessage msg)
        {
            if (msg.State != WorldState.World)
            {
                if (_hoveredUnit)
                {
                    _setHoveredStateMsg.Selector = null;
                    gameObject.SendMessageTo(_setHoveredStateMsg, _hoveredUnit);
                    _hoveredUnit = null;
                    _hoveredSelector.ResetSelector(transform);
                    _hoveredSelector.gameObject.SetActive(false);
                }

                if (_selectedUnit)
                {
                    _setSelectStateMsg.Selector = null;
                    gameObject.SendMessageTo(_setHoveredStateMsg, _selectedUnit);
                    _selectedUnit = null;
                    _selectedSelector.ResetSelector(transform);
                    _selectedSelector.gameObject.SetActive(false);
                    _updateSelectedUnitMsg.Unit = null;
                    gameObject.SendMessage(_updateSelectedUnitMsg);
                }
            }
        }

        private void ClearWorld(ClearWorldMessage msg)
        {
            if (_hoveredUnit)
            {
                _setHoveredStateMsg.Selector = null;
                gameObject.SendMessageTo(_setHoveredStateMsg, _hoveredUnit);
                _hoveredUnit = null;
                _hoveredSelector.ResetSelector(transform);
                _hoveredSelector.gameObject.SetActive(false);
            }

            if (_selectedUnit)
            {
                _setSelectStateMsg.Selector = null;
                gameObject.SendMessageTo(_setHoveredStateMsg, _selectedUnit);
                _selectedUnit = null;
                _selectedSelector.ResetSelector(transform);
                _selectedSelector.gameObject.SetActive(false);
                _updateSelectedUnitMsg.Unit = null;
                gameObject.SendMessage(_updateSelectedUnitMsg);
            }
        }

        void OnDestroy()
        {
            if (_selectedSelector)
            {
                Destroy(_selectedSelector.gameObject);
            }
            if (_hoveredSelector)
            {
                Destroy(_hoveredSelector.gameObject);
            }
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}
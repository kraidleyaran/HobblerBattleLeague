using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Minigame
{
    public class MinigameUnitSelectController : MonoBehaviour
    {
        private static MinigameUnitSelectController _instance = null;

        [SerializeField] private UnitSelectorController _selectedController = null;
        [SerializeField] private UnitSelectorController _hoveredController = null;

        private ContactFilter2D _contactFilter = new ContactFilter2D();

        private GameObject _hoveredUnit = null;
        private GameObject _selectedUnit = null;

        private SetHoveredStateMessage _setHoveredStateMsg = new SetHoveredStateMessage();
        private SetSelectStateMesage _setSelectStateMsg = new SetSelectStateMesage();
        private UpdateSelectedMinigameUnitMessage _updateSelectedMinigameUnitMsg = new UpdateSelectedMinigameUnitMessage();

        void Awake()
        {
            if (_instance)
            {
                Destroy(_instance.gameObject);
            }

            _instance = this;
        }

        void Start()
        {
            _contactFilter = new ContactFilter2D { useTriggers = true, useLayerMask = true, layerMask = CollisionLayerFactory.MinigameSelect.ToMask() };
            SubscribeToMessages();
        }

        public static void RemoveSelectedUnit(GameObject unit)
        {
            if (_instance)
            {
                if (_instance._selectedUnit && _instance._selectedUnit == unit)
                {
                    _instance._selectedUnit = null;
                    _instance._selectedController.SetParent(_instance.transform, Vector2.zero);
                    _instance._selectedController.gameObject.SetActive(false);

                    _instance._updateSelectedMinigameUnitMsg.Unit = null;
                    _instance.gameObject.SendMessage(_instance._updateSelectedMinigameUnitMsg);
                }
            }

        }

        public static void RemoveHoveredUnit(GameObject unit)
        {
            if (_instance)
            {
                if (_instance._hoveredController && _instance._hoveredUnit == unit)
                {
                    _instance._hoveredUnit = null;
                    _instance._hoveredController.SetParent(_instance.transform, Vector2.zero);
                    _instance._hoveredController.gameObject.SetActive(false);
                }
            }
        }



        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            var pos = MinigameCameraController.Camera.ScreenToWorldPoint(msg.Current.MousePos).ToVector2();
            var results = new RaycastHit2D[5];

            var hitCount = Physics2D.Raycast(pos, Vector2.zero, _contactFilter, results, 2f);
            var hoveredUnit = hitCount > 0 ? results[0].transform.gameObject : null;
            if (hoveredUnit && (!_hoveredUnit || _hoveredUnit != hoveredUnit))
            {
                if (_hoveredUnit)
                {
                    _setHoveredStateMsg.Selector = null;
                    gameObject.SendMessageTo(_setHoveredStateMsg, _hoveredUnit);
                }

                _hoveredUnit = hoveredUnit;
                Debug.Log("Hovering Unit");
                _setHoveredStateMsg.Selector = _hoveredController;
                gameObject.SendMessageTo(_setHoveredStateMsg, _hoveredUnit);
            }
            else if (_hoveredUnit && !hoveredUnit)
            {
                Debug.Log("Stopped Hovering Unit");
                _setHoveredStateMsg.Selector = null;
                gameObject.SendMessageTo(_setHoveredStateMsg, _hoveredUnit);
                _hoveredUnit = null;
                _hoveredController.SetParent(transform, Vector2.zero);
                _hoveredController.gameObject.SetActive(false);
            }

            if (!UiWindowManager.Hovered)
            {
                if (_hoveredUnit)
                {
                    if (!msg.Previous.LeftClick && msg.Current.LeftClick && (!_selectedUnit || _selectedUnit != _hoveredUnit))
                    {
                        if (_selectedUnit)
                        {
                            _setSelectStateMsg.Selector = null;
                            gameObject.SendMessageTo(_setSelectStateMsg, _selectedUnit);
                        }
                        _selectedUnit = _hoveredUnit;
                        if (_selectedUnit)
                        {
                            _setSelectStateMsg.Selector = _selectedController;
                            gameObject.SendMessageTo(_setSelectStateMsg, _selectedUnit);
                        }
                        else
                        {
                            _selectedController.SetParent(transform, Vector2.zero);
                            _selectedController.gameObject.SetActive(false);
                        }

                        _updateSelectedMinigameUnitMsg.Unit = _selectedUnit;
                        gameObject.SendMessage(_updateSelectedMinigameUnitMsg);

                    }
                }
                else if (!msg.Previous.LeftClick && msg.Current.LeftClick && _selectedUnit)
                {
                    _selectedController.SetParent(transform, Vector2.zero);
                    _selectedController.gameObject.SetActive(false);
                    _setSelectStateMsg.Selector = null;
                    gameObject.SendMessageTo(_setSelectStateMsg, _selectedUnit);
                    _selectedUnit = null;
                    _updateSelectedMinigameUnitMsg.Unit = null;
                    gameObject.SendMessage(_updateSelectedMinigameUnitMsg);
                }
            }
            
        }

        

        void OnDestroy()
        {
            if (_instance && _instance == this)
            {
                _instance = null;
            }
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Roster
{ 
    public class UiRosterBenchController : MonoBehaviour
    {
        [SerializeField] private RosterType _type;
        [SerializeField] private UiRosterSlotController _rosterSlotTemplate;
        [SerializeField] private RectTransform _content;
        [SerializeField] private VerticalLayoutGroup _grid;

        private List<UiRosterSlotController> _rosterSlots = new List<UiRosterSlotController>();

        void Awake()
        {
            gameObject.Subscribe<WorldPopulationUpdatedMessage>(WorldPopulationUpdated);
            RefreshInfo();
        }

        private void RefreshInfo()
        {
            for (var i = 0; i < _rosterSlots.Count; i++)
            {
                _rosterSlots[i].Destroy();
                Destroy(_rosterSlots[i].gameObject);
            }

            _rosterSlots.Clear();
            var units = new GameObject[0];
            switch (_type)
            {
                case RosterType.Bench:
                    units = WorldHobblerManager.All.Where(o => !WorldHobblerManager.Roster.Contains(o)).ToArray();
                    break;
                case RosterType.Roster:
                    units = WorldHobblerManager.Roster.ToArray();
                    break;
            }

            for (var i = 0; i < units.Length; i++)
            {
                var controller = Instantiate(_rosterSlotTemplate, _grid.transform);
                controller.Setup(units[i], _type);
                _rosterSlots.Add(controller);
            }

            var height = _rosterSlots.Count * (_rosterSlotTemplate.RectTransform.rect.height + _grid.spacing) + _grid.padding.top;
            _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        private void WorldPopulationUpdated(WorldPopulationUpdatedMessage msg)
        {
            RefreshInfo();
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}
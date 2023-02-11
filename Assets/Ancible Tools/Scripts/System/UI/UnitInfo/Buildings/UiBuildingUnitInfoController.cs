using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Building;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Ancible_Tools.Scripts.System.UI.UnitInfo.Buildings
{
    public class UiBuildingUnitInfoController : MonoBehaviour
    {
        [SerializeField] protected internal Text _unitNameText = null;

        protected internal GameObject _owner = null;
        protected internal virtual string _filter => $"{GetInstanceID()}";

        public virtual void Setup(GameObject owner, WorldBuilding building)
        {
            _owner = owner;
            _unitNameText.text = building.DisplayName;
            RefreshOwner();
            SubscribeToMessages();
        }

        protected internal virtual void RefreshOwner()
        {

        }

        protected internal virtual void SubscribeToMessages()
        {
            _owner.SubscribeWithFilter<RefreshUnitMessage>(RefreshUnit, _filter);
        }

        protected internal virtual void RefreshUnit(RefreshUnitMessage msg)
        {
            RefreshOwner();
        }

        public virtual void Destroy()
        {
            _owner.UnsubscribeFromAllMessagesWithFilter(_filter);
            gameObject.UnsubscribeFromAllMessages();
        }

        protected internal virtual void OnDestroy()
        {
            Destroy();
        }


    }
}
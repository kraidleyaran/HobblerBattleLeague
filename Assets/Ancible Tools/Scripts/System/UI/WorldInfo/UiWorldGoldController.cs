using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Ancible_Tools.Scripts.System.UI.WorldInfo
{
    public class UiWorldGoldController : MonoBehaviour
    {
        [SerializeField] private Text _worldGoldText = null;

        void Awake()
        {
            SubscribeToMessages();
        }

        void Start()
        {
            _worldGoldText.text = $"{WorldStashController.Gold:N0}";
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<GoldUpdatedMessage>(GoldUpdated);
        }

        private void GoldUpdated(GoldUpdatedMessage msg)
        {
            _worldGoldText.text = $"{WorldStashController.Gold:N0}";
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}
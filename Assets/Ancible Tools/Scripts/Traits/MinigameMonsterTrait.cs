using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.MInigame;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Minigame Monster Trait", menuName = "Ancible Tools/Traits/Minigame/Minigame Monster")]
    public class MinigameMonsterTrait : Trait
    {
        [SerializeField] private Vector2 _barOffset = Vector2.zero;
        [SerializeField] private Color _healthBarColor = Color.red;

        private string _name = string.Empty;

        private List<GameObject> _killClaims = new List<GameObject>();

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            UiMinigameStatusBarManager.RegisterMinigameUnit(_controller.transform.parent.gameObject, _controller.transform, _healthBarColor, _barOffset);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<ReportDamageMessage>(ReportDamage, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UnitDiedMessage>(UnitDied, _instanceId);
        }

        private void ReportDamage(ReportDamageMessage msg)
        {
            if (msg.Owner != _controller.transform.parent.gameObject && !_killClaims.Contains(msg.Owner))
            {
                _killClaims.Add(msg.Owner);
            }
        }

        private void UnitDied(UnitDiedMessage msg)
        {
            var claimKillMsg = MessageFactory.GenerateClaimKillMsg();
            claimKillMsg.Kill = _controller.transform.parent.gameObject;
            for (var i = 0; i < _killClaims.Count; i++)
            {
                _controller.gameObject.SendMessageTo(claimKillMsg, _killClaims[i]);
            }
            MessageFactory.CacheMessage(claimKillMsg);
            _killClaims.Clear();
            _controller.transform.parent.gameObject.UnsubscribeFromFilter<ReportDamageMessage>(_instanceId);
        }

        public override void Destroy()
        {
            UiMinigameStatusBarManager.RemoveMinigameUnit(_controller.transform.parent.gameObject);
            base.Destroy();
        }
    }
}
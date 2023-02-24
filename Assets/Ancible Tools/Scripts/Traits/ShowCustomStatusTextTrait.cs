using System;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.BattleLeague.Status;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Show Custom Status Text Trait", menuName = "Ancible Tools/Traits/Ui/Show Custom Status Text")]
    public class ShowCustomStatusTextTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] private string _text = string.Empty;
        [SerializeField] private Color _color = Color.white;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var showFloatingTextMsg = MessageFactory.GenerateShowFloatingTextMsg();
            showFloatingTextMsg.Color = _color;
            showFloatingTextMsg.Text = _text;
            showFloatingTextMsg.World = _controller.transform.parent.position.ToVector2();
            _controller.gameObject.SendMessage(showFloatingTextMsg);
            MessageFactory.CacheMessage(showFloatingTextMsg);
        }
    }
}
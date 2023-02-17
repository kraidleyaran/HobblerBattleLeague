using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.SaveData;
using Assets.Resources.Ancible_Tools.Scripts.System.UI;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Exile Hobbler Trait", menuName = "Ancible Tools/Traits/Hobbler/Exile Hobbler")]
    public class ExileHobblerTrait : Trait
    {
        public override bool Instant => true;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var parent = _controller.transform.parent.gameObject;
            HobblerData hobblerData = null;
            var queryHobblerDataMsg = MessageFactory.GenerateQueryHobblerDataMsg();
            queryHobblerDataMsg.DoAfter = data => hobblerData = data;
            _controller.gameObject.SendMessageTo(queryHobblerDataMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(queryHobblerDataMsg);

            if (hobblerData != null)
            {
                var template = WorldHobblerManager.GetTemplateByName(hobblerData.Template);
                if (template)
                {
                    UiController.ShowConfirmationAlert($"Are you sure you want to exile {hobblerData.Name} for {WorldHobblerManager.GetExileGold(template.Cost):N0}g?", template.Sprite.Sprite,
                        () =>
                        {
                            WorldHobblerManager.ExileHobbler(parent);
                        }, Color.white);
                }
            }
            
        }
    }
}
using System;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "On Chance Trait", menuName = "Ancible Tools/Traits/General/On Chance")]
    public class OnChanceTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] [Range(0f, 1f)] private float _chance;
        [SerializeField] private Trait[] _applyOnSuccess;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var success = _chance >= Random.Range(0f, 1f);
            if (success)
            {
                var owner = _controller.transform.parent.gameObject;
                var queryOwnerMsg = MessageFactory.GenerateQueryOwnerMsg();
                queryOwnerMsg.DoAfter = obj => owner = obj;
                _controller.gameObject.SendMessageTo(queryOwnerMsg, owner);
                MessageFactory.CacheMessage(queryOwnerMsg);

                var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                foreach (var trait in _applyOnSuccess)
                {
                    addTraitToUnitMsg.Trait = trait;
                    owner.SendMessageTo(addTraitToUnitMsg, _controller.transform.parent.gameObject);
                }
                MessageFactory.CacheMessage(addTraitToUnitMsg);
            }
        }

        public override string GetDescription(bool equipment = false)
        {
            var description = StaticMethods.ApplyColorToText($"{_chance * 100f:N}% Chance to Apply", ColorFactoryController.BonusStat);
            var traitDescriptions = _applyOnSuccess.OrderByDescending(t => t.DescriptionPriority).Select(t => t.GetDescription()).Where(d => !string.IsNullOrEmpty(d)).ToArray();
            if (traitDescriptions.Length > 0)
            {
                for (var i = 0; i < traitDescriptions.Length; i++)
                {
                    if (i == 0)
                    {
                        description = $"{description} {traitDescriptions[i]}";
                    }
                    else if (i < traitDescriptions.Length - 1)
                    {
                        description = $"{description}, {traitDescriptions[i]}";
                    }
                    else
                    {
                        description = $"{description} and {traitDescriptions[i]}";
                    }

                }
            }
            else
            {
                description = string.Empty;
            }



            return description;
        }
    }
}
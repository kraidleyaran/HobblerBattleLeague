using System;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.UI;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Show Confirmation Alert Trait", menuName = "Ancible Tools/Traits/Ui/Show Confirmation Alert")]
    public class ShowConfirmationAlertTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] [TextArea(3, 5)] private string _promptText = string.Empty;
        [SerializeField] private Trait[] _applyOnConfirm = null;
        [SerializeField] private Sprite _icon = null;
        [SerializeField] private Color _colorMask = Color.white;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var parent = _controller.transform.parent.gameObject;
            var icon = _icon;
            var traits = _applyOnConfirm.ToArray();
            UiController.ShowConfirmationAlert(_promptText,icon, () =>
            {
                parent.AddTraitsToUnit(traits, parent);
            }, _colorMask);
        }
    }
}
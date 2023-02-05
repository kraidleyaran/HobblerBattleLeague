using Assets.Ancible_Tools.Scripts.System.UI.BattleLeague.Roster;
using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Templates;
using Assets.Resources.Ancible_Tools.Scripts.System.UI;
using Assets.Resources.Ancible_Tools.Scripts.System.UnitCommands;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class FactoryController : MonoBehaviour
    {
        public static UnitController UNIT_CONTROLLER => _instance._unitControllerTemplate;
        public static TraitController TRAIT_CONTROLLER => _instance._traitControllerTemplate;
        public static SpriteController SPRITE_CONTROLLER => _instance._spriteControllerTemplate;
        public static ProjectileController PROJECTILE_CONTROLLER => _instance._projectileTemplate;
        public static BattleLeagueAlignmentController ALIGNMENT_CONTROLLER => _instance._alignmentTemplate;
        public static VisualFxController VISUAL_FX_CONTROLLER => _instance._visualFxTemplate;
        public static ParticleFxController PARTICLE_FX_CONTROLLER => _instance._particleFxTemplate;
        public static UiAbilityController ABILITY_CONTROLLER => _instance._abilityControllerTemplate;
        public static UiRosterUnitController ROSTER_UNIT_CONTROLLER => _instance._rosterUnitTemplate;
        public static UnitCommand COMMAND_TEMPLATE => _instance._commandTemplate;
        public static UnitTemplate HOBBLER_TEMPLATE => _instance._hobblerTemplate;
        public static GameObject INVISIBLE => _instance._invisible;
        public static AdventureBattleExclamationController BATTLE_EXCLAMATION => _instance._battleExclamationTemplate;

        private static FactoryController _instance;

        [Header("Controller Templates")]
        [SerializeField] private UnitController _unitControllerTemplate;
        [SerializeField] private TraitController _traitControllerTemplate;
        [SerializeField] private SpriteController _spriteControllerTemplate;
        [SerializeField] private ProjectileController _projectileTemplate;
        [SerializeField] private BattleLeagueAlignmentController _alignmentTemplate;
        [SerializeField] private VisualFxController _visualFxTemplate;
        [SerializeField] private ParticleFxController _particleFxTemplate;
        [SerializeField] private GameObject _invisible;
        [SerializeField] private AdventureBattleExclamationController _battleExclamationTemplate;

        [Header("Ui Templates")]
        [SerializeField] private UiAbilityController _abilityControllerTemplate;
        [SerializeField] private UiRosterUnitController _rosterUnitTemplate;
        [Space]
        [SerializeField] private UnitCommand _commandTemplate;
        [Header("Unit Templates")]
        [SerializeField] private UnitTemplate _hobblerTemplate = null;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }
    }
}
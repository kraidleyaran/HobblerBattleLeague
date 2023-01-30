using System.Linq;
using Assets.Ancible_Tools.Scripts.Traits;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Templates
{
    [CreateAssetMenu(fileName = "Unit Template", menuName = "Ancible Tools/Templates/Unit Template")]
    public class UnitTemplate : ScriptableObject
    {
        [SerializeField] protected internal Trait[] _traits = new Trait[0];

        public virtual UnitController GenerateUnit(Vector2 position)
        {
            var unitController = Instantiate(FactoryController.UNIT_CONTROLLER, position, Quaternion.identity);
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            for (var i = 0; i < _traits.Length; i++)
            {
                addTraitToUnitMsg.Trait = _traits[i];
                this.SendMessageTo(addTraitToUnitMsg, unitController.gameObject);
            }
            MessageFactory.CacheMessage(addTraitToUnitMsg);
            return unitController;
        }

        public virtual UnitController GenerateUnit(Transform parent, Vector2 worldPos)
        {
            
            var unitController = Instantiate(FactoryController.UNIT_CONTROLLER, parent);
            unitController.transform.SetLocalPosition(worldPos);
            unitController.gameObject.layer = parent.gameObject.layer;
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            for (var i = 0; i < _traits.Length; i++)
            {
                addTraitToUnitMsg.Trait = _traits[i];
                this.SendMessageTo(addTraitToUnitMsg, unitController.gameObject);
            }
            MessageFactory.CacheMessage(addTraitToUnitMsg);
            return unitController;

        }

        public virtual UnitController GenerateUnitWithRotation(Vector2 position, float rotation)
        {
            var unitController = Instantiate(FactoryController.UNIT_CONTROLLER, position, Quaternion.Euler(0,0,rotation));
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            for (var i = 0; i < _traits.Length; i++)
            {
                addTraitToUnitMsg.Trait = _traits[i];
                this.SendMessageTo(addTraitToUnitMsg, unitController.gameObject);
            }
            MessageFactory.CacheMessage(addTraitToUnitMsg);
            return unitController;
        }

        public virtual Trait[] GetTraits()
        {
            return _traits.ToArray();
        }
    }
}
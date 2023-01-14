using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    public class TraitController : MonoBehaviour
    {
        public Trait Trait { get; private set; }
        public Trait Prefab { get; private set; }
        public object Sender { get; private set; }

        public void Setup(Trait trait, object sender = null)
        {
            Prefab = trait;
            Sender = sender;
            if (trait.Instant)
            {
                trait.SetupController(this);
            }
            else
            {
                Trait = Instantiate(trait, transform);
                Trait.name = trait.name;
                name = $"{Trait.name} Controller";
                Trait.SetupController(this);
            }
        }

        public void Destroy()
        {
            if (Trait)
            {
                Trait.Destroy();
                Destroy(Trait);
            }

            Prefab = null;
        }

        public void ResetController()
        {
            if (Trait)
            {
                Trait.ResetTrait();
            }
        }
    }
}

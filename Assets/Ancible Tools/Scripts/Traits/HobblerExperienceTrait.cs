using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Hobbler Experience Trait", menuName = "Ancible Tools/Traits/Hobbler/Leveling/Hobbler Experience")]
    public class HobblerExperienceTrait : Trait
    {
        [SerializeField] private float _xpRate = 1;
        [SerializeField] private int _baseExperience = 1;

        private int _experience = 0;
        private int _level = 0;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<AddExperienceMessage>(AddExperience, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryExperienceMessage>(QueryExperience, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetHobblerExperienceMessage>(SetHobblerExperience, _instanceId);
        }

        private void AddExperience(AddExperienceMessage msg)
        {
            _experience += msg.Amount;
            var requiredExperience = StaticMethods.CalculateNextLevel(_level, _baseExperience, _xpRate);
            var leveldUp = false;
            while (_experience >= requiredExperience)
            {
                _experience -= requiredExperience;
                _level++;
                leveldUp = true;
                requiredExperience = StaticMethods.CalculateNextLevel(_level, _baseExperience, _xpRate);
                //TODO: Show one level up effect thingy instead of one for each - hence the leveldUp boolean;
                _controller.gameObject.SendMessageTo(LevelUpMessage.INSTANCE, _controller.transform.parent.gameObject);
            }

            var updateHobblerExperienceMsg = MessageFactory.GenerateUpdateHobblerExperienceMsg();
            updateHobblerExperienceMsg.Experience = _experience;
            updateHobblerExperienceMsg.ExperienceToNextLevel = requiredExperience;
            updateHobblerExperienceMsg.Level = _level;
            _controller.gameObject.SendMessageTo(updateHobblerExperienceMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateHobblerExperienceMsg);
            _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
        }

        private void QueryExperience(QueryExperienceMessage msg)
        {
            msg.DoAfter.Invoke(_experience, _level, StaticMethods.CalculateNextLevel(_level, _baseExperience, _xpRate));
        }

        private void SetHobblerExperience(SetHobblerExperienceMessage msg)
        {
            _level = msg.Level;
            _experience = msg.Experience;
        }
    }
}
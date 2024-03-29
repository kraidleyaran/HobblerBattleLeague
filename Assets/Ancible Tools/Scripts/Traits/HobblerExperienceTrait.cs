﻿using Assets.Ancible_Tools.Scripts.System.Wellbeing;
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
        private int _experiencePool = 0;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void AddExperience(int amount, bool addToPool = true)
        {
            _experience += amount;
            if (addToPool)
            {
                _experiencePool += amount;
            }
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

            var ignorance = Mathf.RoundToInt(amount * WellBeingController.IgnorancePerExperience);
            var applyWellbeingStatsMsg = MessageFactory.GenerateApplyWellbeingStatsMsg();
            applyWellbeingStatsMsg.Stats = new WellbeingStats { Ignorance = ignorance };
            _controller.gameObject.SendMessageTo(applyWellbeingStatsMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(applyWellbeingStatsMsg);
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.Subscribe<ApplyGlobalExperienceMessage>(ApplyGlobalExperience);
            _controller.transform.parent.gameObject.SubscribeWithFilter<AddExperienceMessage>(AddExperience, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryExperienceMessage>(QueryExperience, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetHobblerExperienceMessage>(SetHobblerExperience, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryRequiredLevelExperienceMessage>(QueryRequiredLevelExeperience, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryExperiencePoolMessage>(QueryExperiencePool, _instanceId);
        }

        private void AddExperience(AddExperienceMessage msg)
        {
            AddExperience(msg.Amount);
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

        private void QueryRequiredLevelExeperience(QueryRequiredLevelExperienceMessage msg)
        {
            msg.DoAfter.Invoke(StaticMethods.CalculateNextLevel(msg.Level - 1, _baseExperience, _xpRate));
        }

        private void ApplyGlobalExperience(ApplyGlobalExperienceMessage msg)
        {
            if (msg.Owner == _controller.transform.parent.gameObject)
            {
                _experiencePool = Mathf.Max(0, _experiencePool - msg.Amount);
            }
            else
            {
                AddExperience(msg.Amount, false);
            }
        }

        private void QueryExperiencePool(QueryExperiencePoolMessage msg)
        {
            msg.DoAfter.Invoke(_experiencePool);
        }
    }
}
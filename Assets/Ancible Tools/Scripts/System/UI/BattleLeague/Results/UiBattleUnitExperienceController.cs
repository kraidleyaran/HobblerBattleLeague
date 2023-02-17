using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using DG.Tweening;
using MessageBusLib;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.BattleLeague
{
    public class UiBattleUnitExperienceController : MonoBehaviour
    {
        [SerializeField] private Image _hobblerIconImage;
        [SerializeField] private Text _hobblerNameText;
        [SerializeField] private UiFillBarController _experienceBarController = null;
        [SerializeField] private int _fillTimePerPercent = 5;

        private GameObject _hobbler = null;

        private int _currentXp = 0;
        private int _gainedXp = 0;
        private int _currentLevel = 0;
        private int _nextLevelXp = 0;
        private float _startingPercent = 0f;
        private Sequence _fillSequence = null;

        public void Setup(GameObject hobbler, int experienceGained)
        {
            _hobbler = hobbler;
            _gainedXp = experienceGained;
            var queryNameMsg = MessageFactory.GenerateQueryUnitNameMsg();
            queryNameMsg.DoAfter = UpdateName;
            gameObject.SendMessageTo(queryNameMsg, _hobbler);
            MessageFactory.CacheMessage(queryNameMsg);

            var queryExperienceMsg = MessageFactory.GenerateQueryExperienceMsg();
            queryExperienceMsg.DoAfter = UpdateExperience;
            gameObject.SendMessageTo(queryExperienceMsg, _hobbler );
            MessageFactory.CacheMessage(queryExperienceMsg);

            var querySpriteMsg = MessageFactory.GenerateQuerySpriteMsg();
            querySpriteMsg.DoAfter = UpdateSprite;
            gameObject.SendMessageTo(querySpriteMsg, _hobbler);
            MessageFactory.CacheMessage(querySpriteMsg);
        }

        public void ActivateBar()
        {
            var startingExperience = _currentXp;
            var endExperience = startingExperience + _gainedXp;
            if (endExperience >= _nextLevelXp)
            {
                var leftoverExperience = endExperience - _nextLevelXp;
                _fillSequence = DoExperienceBarFill(1f).OnComplete(() => ProcessLargeExperience(leftoverExperience, _currentLevel + 1));
            }
            else
            {
                if (endExperience > startingExperience)
                {
                    _fillSequence = DoExperienceBarFill((float)endExperience / _nextLevelXp).OnComplete(() =>
                    {
                        _fillSequence = null;
                    });
                }

            }
        }

        private Sequence DoExperienceBarFill(float percent)
        {
            var fillPercent = percent - _startingPercent;
            var fillTime = (int) (_fillTimePerPercent * fillPercent);
            if (fillTime < _fillTimePerPercent)
            {
                fillTime = _fillTimePerPercent;
            }

            var time = fillTime * TickController.TickRate;
            return DOTween.Sequence().AppendInterval(time).OnUpdate(() =>
            {

                var setPercent = (_fillSequence.position / time) * (fillPercent);
                _experienceBarController.Setup(setPercent + _startingPercent, string.Empty, ColorFactoryController.Experience);
            });
        }

        private void UpdateName(string hobblerName)
        {
            _hobblerNameText.text = hobblerName;
        }

        private void UpdateSprite(SpriteTrait sprite)
        {
            _hobblerIconImage.sprite = sprite.Sprite;
        }

        private void UpdateExperience(int experience, int level, int experienceToNextLevel)
        {
            _startingPercent = (float) experience / experienceToNextLevel;
            _experienceBarController.Setup(_startingPercent, string.Empty, ColorFactoryController.Experience);
            _nextLevelXp = experienceToNextLevel;
            _currentLevel = level;
            _currentXp = experience;

        }

        private void ProcessLargeExperience(int experienceAmount, int level)
        {
            _fillSequence = null;
            _startingPercent = 0f;
            var requiredExperience = 0;
            var queryRequiredExperienceForLevelMsg = MessageFactory.GenerateQueryRequiredLevelExperienceMsg();
            queryRequiredExperienceForLevelMsg.Level = level + 1;
            queryRequiredExperienceForLevelMsg.DoAfter = experience => requiredExperience = experience;
            gameObject.SendMessageTo(queryRequiredExperienceForLevelMsg, _hobbler);
            MessageFactory.CacheMessage(queryRequiredExperienceForLevelMsg);
            if (experienceAmount >= requiredExperience)
            {
                var leftoverExperience = experienceAmount - requiredExperience;
                _fillSequence = DoExperienceBarFill(1f).OnComplete(() => { ProcessLargeExperience(leftoverExperience, level + 1);});
            }
            else
            {
                var percent = 0f;
                if (experienceAmount > 0)
                {
                    percent = (float) experienceAmount / requiredExperience;
                }

                if (percent > 1f)
                {
                    percent = 1f;
                }
                _experienceBarController.Setup(0f, string.Empty, ColorFactoryController.Experience);
                _fillSequence = DoExperienceBarFill(percent).OnComplete(() => { _fillSequence = null; });
            }
        }

        void OnDestroy()
        {
            if (_fillSequence != null)
            {
                if (_fillSequence.IsActive())
                {
                    _fillSequence.Kill();
                }

                _fillSequence = null;
            }

            _hobbler = null;
        }
    }
}
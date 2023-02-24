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
        [SerializeField] private Text _levelUpText;
        [SerializeField] private UiFillBarController _experienceBarController = null;
        [SerializeField] private int _fillTimePerPercent = 5;
        [SerializeField] private float _levelUpJumpPower = 1f;
        [SerializeField] private Vector2 _levelUpJumpOffset = Vector2.up * 5;
        [SerializeField] private int _levelUpJumpTicks = 30;

        private GameObject _hobbler = null;

        private int _currentXp = 0;
        private int _gainedXp = 0;
        private int _currentLevel = 0;
        private int _nextLevelXp = 0;
        private float _startingPercent = 0f;
        private Sequence _fillSequence = null;
        private Tween _levelUpJumpTween = null;

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
                _fillSequence = DoExperienceBarFill(1f, true);
                _fillSequence.onComplete += () => ProcessLargeExperience(leftoverExperience, _currentLevel + 1);
            }
            else
            {
                if (endExperience > startingExperience)
                {
                    _fillSequence = DoExperienceBarFill((float) endExperience / _nextLevelXp, false);
                    _fillSequence.onComplete += () =>
                    {
                        _fillSequence = null;
                    };
                }

            }
        }

        private void ActivateLevelUp()
        {
            _levelUpText.gameObject.SetActive(true);
            if (_levelUpJumpTween != null)
            {
                if (_levelUpJumpTween.IsActive())
                {
                    _levelUpJumpTween.Complete();
                }

                _levelUpJumpTween = null;
            }

            _levelUpJumpTween = _levelUpText.transform.DOLocalJump(_levelUpJumpOffset, _levelUpJumpPower, 1,
                _levelUpJumpTicks * TickController.TickRate).OnComplete(
                () => { _levelUpJumpTween = null; });
        }

        private Sequence DoExperienceBarFill(float percent, bool levelUp)
        {
            var fillPercent = percent - _startingPercent;
            var fillTime = (int) (_fillTimePerPercent * fillPercent);
            if (fillTime < _fillTimePerPercent)
            {
                fillTime = _fillTimePerPercent;
            }

            var time = fillTime * TickController.TickRate;
            var sequence = DOTween.Sequence().AppendInterval(time).OnUpdate(() =>
            {

                var setPercent = (_fillSequence.position / time) * (fillPercent);
                _experienceBarController.Setup(setPercent + _startingPercent, string.Empty, ColorFactoryController.Experience);
            });
            if (levelUp)
            {
                ActivateLevelUp();
            }

            return sequence;

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
                _fillSequence = DoExperienceBarFill(1f, true);
                _fillSequence.onComplete += () => { ProcessLargeExperience(leftoverExperience, level + 1); };
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
                _fillSequence = DoExperienceBarFill(percent, false);
                _fillSequence.onComplete += () => { _fillSequence = null; };
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

            if (_levelUpJumpTween != null)
            {
                if (_levelUpJumpTween.IsActive())
                {
                    _levelUpJumpTween.Kill();
                }

                _levelUpJumpTween = null;
            }

            _hobbler = null;
        }
    }
}
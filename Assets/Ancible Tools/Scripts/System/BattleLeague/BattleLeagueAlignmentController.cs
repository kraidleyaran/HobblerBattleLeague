using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague
{
    public class BattleLeagueAlignmentController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer = null;

        public void Setup(BattleAlignment alignment)
        {
            switch (alignment)
            {
                case BattleAlignment.Left:
                    _renderer.sprite = BattleLeagueController.LeftSideFlagSprite;
                    break;
                case BattleAlignment.Right:
                    _renderer.sprite = BattleLeagueController.RightSideFlagSprite;
                    break;
            }
        }

        public void FlipX(bool flip)
        {
            _renderer.flipX = flip;
        }
    }
}
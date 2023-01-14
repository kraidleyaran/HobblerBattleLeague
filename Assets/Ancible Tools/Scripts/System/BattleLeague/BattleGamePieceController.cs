using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague
{
    public class BattleGamePieceController : MonoBehaviour
    {
        public BattleUnitData Data => _unitData;
        public string Id => _id;

        private BattleUnitData _unitData = null;
        private string _id = string.Empty;

        private SpriteController _spriteController = null;

        public void Setup(BattleUnitData data, string id, BattleAlignment alignment)
        {
            _id = id;
            _unitData = data;
            _spriteController = Instantiate(FactoryController.SPRITE_CONTROLLER, transform);
            _spriteController.SetScale(data.Sprite.Scaling);
            _spriteController.SetOffset(data.Sprite.Offset);
            _spriteController.SetRuntimeController(data.Sprite.RuntimeController);
        }
    }
}
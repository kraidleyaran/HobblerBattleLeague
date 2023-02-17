using Assets.Ancible_Tools.Scripts.System.Dialogue;
using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Templates;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Adventure
{
    public class AdventureTrainerSpawnController : MonoBehaviour
    {
        [SerializeField] private UnitTemplate _template = null;
        [SerializeField] private Trait[] _additionalTraits = new Trait[0];
        [SerializeField] private SpriteTrait _sprite = null;
        [SerializeField] private BattleEncounter _battleEncounter = null;
        [SerializeField] private DialogueData _preEncounterDialogue = null;
        [SerializeField] private DialogueData _defeatedDialogue = null;
        [SerializeField] private Vector2Int _faceDirection = Vector2Int.down;
        public string SaveId = string.Empty;

        [Header("Editor References")]
        [SerializeField] private SpriteController _editorSpriteController = null;
        [SerializeField] private SpriteTrait _defaultSprite = null;

        void Awake()
        {
            SubscribeToMessages();
        }

        public void RefreshEditorSprite()
        {
#if UNITY_EDITOR
            if (_editorSpriteController && (_defaultSprite || _sprite))
            {
                var sprite = _sprite ?? _defaultSprite;
                _editorSpriteController.SetFromEditor(sprite);
            }
#endif
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<SpawnAdventureUnitsMessage>(SpawnAdventureUnits);
        }

        private void SpawnAdventureUnits(SpawnAdventureUnitsMessage msg)
        {
            var mapTile = WorldAdventureController.MapController.PlayerPathing.GetMapTileByWorldPosition(transform.position.ToVector2());
            if (mapTile != null)
            {
                var unit = _template.GenerateUnit(WorldAdventureController.MapController.transform, mapTile.World);
                var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();

                addTraitToUnitMsg.Trait = _sprite;
                gameObject.SendMessageTo(addTraitToUnitMsg, unit.gameObject);

                if (_additionalTraits.Length > 0)
                {
                    foreach (var trait in _additionalTraits)
                    {
                        addTraitToUnitMsg.Trait = trait;
                        gameObject.SendMessageTo(addTraitToUnitMsg, unit.gameObject);
                    }
                }

                MessageFactory.CacheMessage(addTraitToUnitMsg);

                var setupTrainerMsg = MessageFactory.GenerateSetupTrainerMsg();
                setupTrainerMsg.Id = SaveId;
                setupTrainerMsg.Encounter = _battleEncounter;
                setupTrainerMsg.PreEncounterDialogue = _preEncounterDialogue;
                setupTrainerMsg.DefeatedDialogue = _defeatedDialogue;
                gameObject.SendMessageTo(setupTrainerMsg, unit.gameObject);
                MessageFactory.CacheMessage(setupTrainerMsg);

                var setFacingDirectionMsg = MessageFactory.GenerateSetFaceDirectionMsg();
                setFacingDirectionMsg.Direction = _faceDirection;
                gameObject.SendMessageTo(setFacingDirectionMsg, unit.gameObject);
                MessageFactory.CacheMessage(setFacingDirectionMsg);

                var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
                setMapTileMsg.Tile = mapTile;
                gameObject.SendMessageTo(setMapTileMsg, unit.gameObject);
                MessageFactory.CacheMessage(setMapTileMsg);
            }
            gameObject.Unsubscribe<SpawnAdventureUnitsMessage>();
        }

        void OnDestroy()
        {
            gameObject.Unsubscribe<SpawnAdventureUnitsMessage>();
        }
    }
}
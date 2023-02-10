using System;
using System.Collections.Generic;
using Assets.Ancible_Tools.Scripts.System.Items.Crafting;
using Assets.Ancible_Tools.Scripts.System.SaveData;
using Assets.Ancible_Tools.Scripts.System.SaveData.Building;
using Assets.Ancible_Tools.Scripts.System.UI.UnitInfo;
using Assets.Ancible_Tools.Scripts.System.WorldNodes;
using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Building;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Input;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame.AI;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame.Settings;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.SaveData;
using Assets.Resources.Ancible_Tools.Scripts.System.Skills;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.Crafting;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.DetailedInfo;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.MInigame;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.MInigame.MazeSelector;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.Stash;
using Assets.Resources.Ancible_Tools.Scripts.System.UnitCommands;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class UpdateTickMessage : EventMessage
    {
        public static UpdateTickMessage INSTANCE = new UpdateTickMessage();
    }

    public class FixedUpdateTickMessage : EventMessage
    {
        public static FixedUpdateTickMessage INSTANCE = new FixedUpdateTickMessage();
    }

    public class SetDirectionMessage : EventMessage
    {
        public Vector2 Direction;
    }

    public class UpdateDirectionMessage : EventMessage
    {
        public Vector2 Direction;
    }

    public class QueryDirectionMessage : EventMessage
    {
        public Action<Vector2> DoAfter;
    }

    public class SetMonsterStateMessage : EventMessage
    {
        public MonsterState State;
    }

    public class UpdateMonsterStateMessage : EventMessage
    {
        public MonsterState State;
    }

    public class QueryMonsterStateMessage : EventMessage
    {
        public Action<MonsterState> DoAfter;
    }

    public class SetPathMessage : EventMessage
    {
        public MapTile[] Path;
        public Action DoAfter;
    }

    public class SetMapTileMessage : EventMessage
    {
        public MapTile Tile;
    }

    public class UpdateMapTileMessage : EventMessage
    {
        public MapTile Tile;
    }

    public class QueryMapTileMessage : EventMessage
    {
        public Action<MapTile> DoAfter;
    }

    public class UpdateHappinessMessage : EventMessage
    {
        public int Happiness;
    }

    public class QueryHappinessMessage : EventMessage
    {
        public Action<int, IntNumberRange> DoAfter;
    }

    public class ApplyWellbeingStatsMessage : EventMessage
    {
        public WellbeingStats Stats;
    }

    public class QueryWellbeingStatsMessage : EventMessage
    {
        public Action<WellbeingStats, WellbeingStats, WellbeingStats> DoAfter;
    }

    public class WorldTickMessage : EventMessage
    {
        public static WorldTickMessage INSTANCE = new WorldTickMessage();
    }

    public class UpdateInputStateMessage : EventMessage
    {
        public WorldInputState Previous;
        public WorldInputState Current;
    }

    public class SetHoveredStateMessage : EventMessage
    {
        public UnitSelectorController Selector;
    }

    public class RemoveHoveredUnitMessage : EventMessage
    {
        public GameObject Unit;
    }

    public class SetSelectStateMesage : EventMessage
    {
        public UnitSelectorController Selector;
    }

    public class RemoveSelectedUnitMessage : EventMessage
    {
        public GameObject Unit;
    }

    public class UpdateSelectedUnitMessage : EventMessage
    {
        public GameObject Unit;
    }

    public class QuerySpriteMessage : EventMessage
    {
        public Action<SpriteTrait> DoAfter;
    }

    public class RefreshUnitMessage : EventMessage
    {
        public static RefreshUnitMessage INSTANCE = new RefreshUnitMessage();
    }

    public class GatherMessage : EventMessage
    {
        public int Ticks;
        public WorldNodeType NodeType;
        public bool Invisible;
        public Action<GameObject> DoAfter;
        public GameObject Node;
        public MapTile GatheringTile;
    }

    public class StopGatheringMessage : EventMessage
    {
        public GameObject Node;
    }

    public class InteractMessage : EventMessage
    {
        public GameObject Owner;
    }

    public class SearchForNodeMessage : EventMessage
    {
        public WorldNodeType Type;
        public Action DoAfter;
    }

    public class QueryCommandsMessage : EventMessage
    {
        public Action<CommandInstance[]> DoAfter;
    }

    public class DoBumpMessage : EventMessage
    {
        public Vector2 Direction;
        public Action OnBump;
        public Action DoAfter;
    }

    public class SetSpriteVisibilityMessage : EventMessage
    {
        public bool Visible;
    }

    public class SetSpriteAlphaMessage : EventMessage
    {
        public float Alpha;
    }

    public class SetMinigameUnitStateMessage : EventMessage
    {
        public MinigameUnitState State;
    }

    public class UpdateMinigameUnitStateMessage : EventMessage
    {
        public MinigameUnitState State;
    }

    public class QueryMinigameUnitStateMessage : EventMessage
    {
        public Action<MinigameUnitState> DoAfter;
    }

    public class UpdatePositionMessage : EventMessage
    {
        public Vector2 Position;
    }

    public class SetMinigameCameraPositionMessage : EventMessage
    {
        public Vector2 Position;
    }

    public class LateTickUpdateMessage : EventMessage
    {
        public static LateTickUpdateMessage INSTANCE = new LateTickUpdateMessage();
    }

    public class MinigameInteractMessage : EventMessage
    {
        public GameObject Owner;
        public Vector2 Direction;
    }

    public class ObstacleMessage : EventMessage
    {
        public GameObject Obstacle;
        public Vector2 Direction;
    }

    public class MinigameSpawnUnitsMessage : EventMessage
    {
        public static MinigameSpawnUnitsMessage INSTANCE = new MinigameSpawnUnitsMessage();
    }

    public class MinigameInteractCheckMessage : EventMessage
    {
        public Action DoAfter;
    }

    public class StartMinigameMessage : EventMessage
    {
        public MinigameSettings Settings;
        public GameObject Owner;
    }
    
    public class EndMinigameMessage : EventMessage
    {
        public MinigameResult Result;
        public GameObject Unit;
    }

    public class DespawnMinigameUnitsMessage : EventMessage
    {
        public static DespawnMinigameUnitsMessage INSTANCE = new DespawnMinigameUnitsMessage();
    }

    public class ShowNewCommandTreeMessage : EventMessage
    {
        public CommandInstance Command;
    }

    public class RemoveCommandTreeLevelMessage : EventMessage
    {
        public static RemoveCommandTreeLevelMessage INSTANCE = new RemoveCommandTreeLevelMessage();
    }

    public class UpdateWorldStateMessage : EventMessage
    {
        public WorldState State;
    }

    public class QueryWorldUnitTypeMessage : EventMessage
    {
        public Action<WorldUnitType> DoAfter;
    }

    public class RefillNodeStacksMessage : EventMessage
    {
        public int Max;
    }

    public class AddExperienceMessage : EventMessage
    {
        public int Amount;
    }

    public class QueryExperienceMessage : EventMessage
    {
        //Experience,Level, Experience to Next Level
        public Action<int, int, int> DoAfter;
    }

    public class SetExperienceMessage : EventMessage
    {
        public int Amount;
        public int Level;
    }

    public class QueryHobblerWellbeingStatusMessage : EventMessage
    {
        public Action<WellbeingStatType[]> DoAfter;
    }

    public class ApplyCombatStatsMessage : EventMessage
    {
        public CombatStats Stats;
        public bool Bonus;
    }

    public class QueryCombatStatsMessage : EventMessage
    {
        public Action<CombatStats, CombatStats, GeneticCombatStats> DoAfter;
    }

    public class UpdateCombatStatsMessage : EventMessage
    {
        public CombatStats Base;
        public CombatStats Bonus;
        public GeneticCombatStats Genetics;
    }

    public class SetCombatStatsMessage : EventMessage
    {
        public CombatStats Stats;
        public GeneticCombatStats Accumulated;
    }

    public class LevelUpMessage : EventMessage
    {
        public static LevelUpMessage INSTANCE = new LevelUpMessage();
    }

    public class DoBasicAttackMessage : EventMessage
    {
        public GameObject Target;
        public Vector2Int Direction;
    }

    public class DamageMessage : EventMessage
    {
        public GameObject Owner;
        public int Amount;
        public DamageType Type;
    }

    public class QueryBonusDamageMessage : EventMessage
    {
        public DamageType Type;
        public Action<int> DoAfter;
    }

    public class DoBumpOverPixelsPerSecondMessage : EventMessage
    {
        public Vector2 Direction;
        public Action OnBump;
        public Action DoAfter;
        public int PixelsPerSecond;
        public float Distance;
    }

    public class QueryCombatAlignmentMessage : EventMessage
    {
        public Action<CombatAlignment> DoAfter;
    }

    public class UnitDiedMessage : EventMessage
    {
        public static UnitDiedMessage INSTANCE = new UnitDiedMessage();
    }

    public class SetMinigameAiStateMessage : EventMessage
    {
        public MinigameAiState State;
    }

    public class UpdateMinigameAiStateMessage : EventMessage
    {
        public MinigameAiState State;
    }

    public class QueryMinigameAiStateMessage : EventMessage
    {
        public Action<MinigameAiState> DoAfter;
    }

    public class CullingCheckMessage : EventMessage
    {
        public static CullingCheckMessage INSTANCE = new CullingCheckMessage();
    }

    public class SetCombatAlignmentMessage : EventMessage
    {
        public CombatAlignment Alignment;
    }

    public class UpdateCombatAlignmentMessage : EventMessage
    {
        public CombatAlignment Alignment;
    }

    public class BasicAttackCheckMessage : EventMessage
    {
        public GameObject Target;
        public MapTile Origin;
        public MapTile TargetTile;
        public Action DoAfter;
        public int Distance;
    }

    public class SetProxyHobblerMessage : EventMessage
    {
        public GameObject Hobbler;
        public int MaxChests;
        public int MaxMonsters;
    }

    public class ReportDamageMessage : EventMessage
    {
        public int Amount;
        public GameObject Owner;
    }

    public class QueryOwnerMessage : EventMessage
    {
        public Action<GameObject> DoAfter;
    }

    public class SetOwnerMessage : EventMessage
    {
        public GameObject Owner;
    }

    public class UpdateOwnerMessage : EventMessage
    {
        public GameObject Owner;
    }

    public class SetBasicAttackSetupMessage : EventMessage
    {
        public BasicAttackSetup Setup;
    }

    public class ClearBasicAttackSetupMessage : EventMessage
    {
        public BasicAttackSetup Setup;
    }

    public class QueryBasicAttackSetupMessage : EventMessage
    {
        public Action<BasicAttackSetup> DoAfter;
    }

    public class EquipItemToSlotMessage : EventMessage
    {
        public int Index;
        public EquippableItem Item;
    }

    public class UnequipItemFromSlotMessage : EventMessage
    {
        public EquipSlot Slot;
        public int Index;
        public bool ReturnToStash;
    }

    public class QueryHobblerEquipmentMessage : EventMessage
    {
        //Armor, Trinkets, Weapon
        public Action<EquippableInstance[], EquippableInstance[], EquippableInstance> DoAfter;
    }

    public class StashUpdatedMessage : EventMessage
    {
        public static StashUpdatedMessage INSTANCE = new StashUpdatedMessage();
    }

    public class SetSpriteMessage : EventMessage
    {
        public SpriteTrait Sprite;
    }

    public class AddItemMessage : EventMessage
    {
        public WorldItem Item;
        public int Stack;
        public bool Alert;
    }

    public class SetLootTableMessage : EventMessage
    {
        public LootTable Table;
    }

    public class ShowDetailedHobblerInfoMessage : EventMessage
    {
        public GameObject Unit;
    }

    public class ShowHoverInfoMessage : EventMessage
    {
        public GameObject Owner;
        public Sprite Icon;
        public Color ColorMask;
        public string Title;
        public string Description;
        public Vector2 Position;
        public bool World;
        public int Gold = -1;
    }

    public class RemoveHoverInfoMessage : EventMessage
    {
        public GameObject Owner;
    }

    public class ToggleStashWindowMessage : EventMessage
    {
        public static ToggleStashWindowMessage INSTANCE = new ToggleStashWindowMessage();
    }

    public class MinigamePlayerPovUpdatedMessage : EventMessage
    {
        public static MinigamePlayerPovUpdatedMessage INSTANCE = new MinigamePlayerPovUpdatedMessage();
    }

    public class SetHoveredStashItemControllerMessage : EventMessage
    {
        public UiStashItemController Controller;
    }

    public class RemoveHoveredStashItemControllerMessage : EventMessage
    {
        public UiStashItemController Controller;
    }

    public class ReceiveDragDropItemMessage : EventMessage
    {
        public GameObject Owner;
        public WorldItem Item;
        public int Stack;
        public Action DoAfter;
    }

    public class SetHoveredEquippedItemControllerMessage : EventMessage
    {
        public UiEquippedItemController Controller;
    }

    public class RemoveHoveredEquippedItemControllerMessage : EventMessage
    {
        public UiEquippedItemController Controller;
    }

    public class RemoveItemMessage : EventMessage
    {
        public WorldItem Item;
        public int Stack;
    }

    public class SetActiveSelectableStateMessage : EventMessage
    {
        public bool Selectable;
    }

    public class UnregisterFromGatheringNodeMessage : EventMessage
    {
        public GameObject Unit;
    }

    public class SetBattleAlignmentMessage : EventMessage
    {
        public BattleAlignment Alignment;
    }

    public class UpdateBattleAlignmentMessage : EventMessage
    {
        public BattleAlignment Alignment;
    }

    public class QueryBattleAlignmentMessage : EventMessage
    {
        public Action<BattleAlignment> DoAfter;
    }

    public class SetEnemyUnitsMessage : EventMessage
    {
        public GameObject[] Units;
    }

    public class SetUnitBattleStateMessage : EventMessage
    {
        public UnitBattleState State;
    }

    public class UpdateUnitBattleStateMessage : EventMessage
    {
        public UnitBattleState State;
    }

    public class QueryUnitBattleStateMessage : EventMessage
    {
        public Action<UnitBattleState> DoAfter;
    }

    public class ActivateGlobalCooldownMessage : EventMessage
    {
        public static ActivateGlobalCooldownMessage INSTANCE = new ActivateGlobalCooldownMessage();
    }

    public class BattleAggroCheckMessage : EventMessage
    {
        public static BattleAggroCheckMessage INSTANCE = new BattleAggroCheckMessage();
    }

    public class RemoveEnemyUnitMessage : EventMessage
    {
        public GameObject Enemy;
    }

    public class BattleAbilityCheckMessage : EventMessage
    {
        public MapTile Origin;
        public GameObject Target;
        public GameObject[] Allies;
        public int Distance;
        public Action DoAfter;
    }

    public class SetAbilitiesMessage : EventMessage
    {
        public WorldAbility[] Abilities;
    }

    public class UpdateBattleLeagueScoreMessage : EventMessage
    {
        public int LeftSide;
        public int RightSide;
    }

    public class SetAlliesMessage : EventMessage
    {
        public GameObject[] Allies;
    }

    public class RemoveAllyMessage : EventMessage
    {
        public GameObject Ally;
    }

    public class SetFaceDirectionMessage : EventMessage
    {
        public Vector2 Direction;
    }

    public class QueryHealthMessage : EventMessage
    {
        //Current, Max
        public Action<int, int> DoAfter;
        public bool DirectValues;
    }

    public class HealMessage : EventMessage
    {
        public DamageType Type;
        public int Amount;
        public GameObject Owner;
    }

    public class FxAnimationFinishedMessage : EventMessage
    {
        public static FxAnimationFinishedMessage INSTANCE = new FxAnimationFinishedMessage();
    }

    public class UpdateBattleStateMessage : EventMessage
    {
        public BattleState State;
    }

    public class QueryBattlePieceMessage : EventMessage
    {
        public Action<BattleUnitData, BattleAlignment> DoAfter;
    }

    public class QueryBenchSlotMessage : EventMessage
    {
        public BattleAlignment Alignment = BattleAlignment.None;
        public Action<BattleBenchSlotController> DoAfter = null;
    }

    public class QueryClosestGamePieceBenchSlotMessage : EventMessage
    {
        public Action<BattleBenchSlotController> DoAfter;
    }

    public class SetGamePieceBenchMessage : EventMessage
    {
        public BattleBenchSlotController Bench;
    }

    public class SetGamePieceMapTileMessage : EventMessage
    {
        public MapTile Tile;
    }

    public class QueryBattlePieceMapTileMessage : EventMessage
    {
        public MapTile Tile;
    }

    public class QueryBattleGamePiecePlacementMessage : EventMessage
    {
        //Current Bench slot, Closest bench slot, current map tile
        public Action<BattleBenchSlotController, BattleBenchSlotController, MapTile> DoAfter;
    }

    public class SetGamePieceDataMessage : EventMessage
    {
        public BattleUnitData Data;
        public BattleAlignment Alignment;
    }

    public class UpdateBattlePrepPhaseTimeMessage : EventMessage
    {
        public int CurrentTicks;
        public int MaxTicks;
    }

    public class StartBattleMessage : EventMessage
    {
        public static StartBattleMessage INSTANCE = new StartBattleMessage();
    }

    public class DoVictoryAnimationMessage : EventMessage
    {
        public static DoVictoryAnimationMessage INSTANCE = new DoVictoryAnimationMessage();
    }

    public class UpdateBattlePointRequirementMessage : EventMessage
    {
        public int Requirement;
        public int Rounds;
    }

    public class UpdateBattleRoundMessage : EventMessage
    {
        public int Round;
    }

    public class UpdateSelectedBattleUnitMessage : EventMessage
    {
        public GameObject Unit;
    }

    public class QueryBattleUnitDataMessage : EventMessage
    {
        public Action<BattleUnitData> DoAfter;
    }

    public class ReportHealMessage : EventMessage
    {
        public int Amount;
        public GameObject Owner;
    }

    public class ShowBattleResultsWindowMessage : EventMessage
    {
        public BattleResult Result;
        public int LeftScore;
        public int RightScore;
        public KeyValuePair<BattleUnitData, BattleAlignment>[] Units;
        public KeyValuePair<BattleUnitData, GameObject>[] Hobblers;
        public int TotalRounds;
        public int TotalExperience;
        public ItemStack[] Items = new ItemStack[0];
    }

    public class CloseBattleMessage : EventMessage
    {
        public static CloseBattleMessage INSTANCE = new CloseBattleMessage();
    }

    public class SetHoveredAbilitySlotControllerMessage : EventMessage
    {
        public UiAbilitySlotController Controller;
    }

    public class RemoveHoveredAbilitySlotControllerMessage : EventMessage
    {
        public UiAbilitySlotController Controller;
    }

    public class QueryAbilitiesMessage : EventMessage
    {
        public Action<KeyValuePair<int, WorldAbility>[]> DoAfter;
    }

    public class ChangeAbilitySlotMessage : EventMessage
    {
        public int CurrentSlot;
        public int NewSlot;
    }

    public class LearnAbilityMessage : EventMessage
    {
        public WorldAbility Ability;
        public int Slot;
    }

    public class ForgetAbilityAtSlotMessage : EventMessage
    {
        public int Slot;
    }

    public class UpdateFogVisibilityMessage : EventMessage
    {
        public bool Visible;
    }

    public class UpdateSelectedMinigameUnitMessage : EventMessage
    {
        public GameObject Unit;
    }

    public class SetMinigameEquipmentMessage : EventMessage
    {
        public EquippableInstance[] Equipment;
    }

    public class UseActionBarButtonMessage : EventMessage
    {
        public UiActionButtonController Controller;
    }

    public class QueryMinigameAbilitiesMessage : EventMessage
    {
        public Action<KeyValuePair<int, AbilityInstance>[]> DoAfter;
    }

    public class QueryManaMessage : EventMessage
    {
        //Current, Max
        public Action<int, int> DoAfter;
    }

    public class CastAbilityMessage : EventMessage
    {
        public AbilityInstance Ability;
        public GameObject Target;
    }

    public class UpdateUnitCastTimerMessage : EventMessage
    {
        public TickTimer CastTimer;
        public Sprite Icon;
        public string Name;
    }

    public class QueryProxyRewardsMessage : EventMessage
    {
        //experience, gold, monster kills, chests found, items
        public Action<int, int, IntNumberRange, IntNumberRange, ItemStack[]> DoAfter;
    }

    public class ClaimKillMessage : EventMessage
    {
        public GameObject Kill;
    }

    public class ClaimChestMessage : EventMessage
    {
        public static ClaimChestMessage INSTANCE = new ClaimChestMessage();
    }

    public class TearDownMinigameMessage : EventMessage
    {
        public static TearDownMinigameMessage INSTANCE = new TearDownMinigameMessage();
    }

    public class UpdateWellbeingMessage : EventMessage
    {
        public WellbeingStats Stats;
        public WellbeingStats Min;
        public WellbeingStats Max;
    }

    public class SetHobblerAiStateMessage : EventMessage
    {
        public HobblerAiState State;
    }

    public class SetupBuildingMessage : EventMessage
    {
        public WorldBuilding Building;
        public string Id;
    }

    public class WorldBuildingActiveMessage : EventMessage
    {
        public static WorldBuildingActiveMessage INSTANCE = new WorldBuildingActiveMessage();
    }

    public class WorldBuildingStoppedMessage : EventMessage
    {
        public static WorldBuildingStoppedMessage INSTANCE = new WorldBuildingStoppedMessage();
    }

    public class GoldUpdatedMessage : EventMessage
    {
        public static GoldUpdatedMessage INSTANCE = new GoldUpdatedMessage();
    }

    public class GainSkillExperienceMessage : EventMessage
    {
        public WorldSkill Skill;
        public int Experience;
    }

    public class QuerySkillsByPriorityMessage : EventMessage
    {
        public Action<KeyValuePair<int, SkillInstance>[]> DoAfter;
    }

    public class SkillCheckMessage : EventMessage
    {
        public Action DoAfter;
    }

    public class SearchForResourceNodeMessage : EventMessage
    {
        public WorldItem Item;
        public Action DoAfter;
    }

    public class ChangeSkillPriorityMessage : EventMessage
    {
        public SkillInstance Skill;
        public int Origin;
        public int Priority;
    }

    public class ResetCommandCardMessage : EventMessage
    {
        public static ResetCommandCardMessage INSTANCE = new ResetCommandCardMessage();
    }

    public class RefreshHobblersMessage : EventMessage
    {
        public static RefreshHobblersMessage INSTANCE = new RefreshHobblersMessage();
    }

    public class SetEquipmentMessage : EventMessage
    {
        public EquippableItem[] Items;
    }

    public class RerollHobblersMessage : EventMessage
    {
        public static RerollHobblersMessage INSTANCE = new RerollHobblersMessage();
    }

    public class PurchaseHobblerAtSlotMessage : EventMessage
    {
        public int Slot;
    }

    public class SetUnitNameMessage : EventMessage
    {
        public string Name;
    }

    public class QueryUnitNameMessage : EventMessage
    {
        public Action<string> DoAfter;
    }

    public class SetHobblerTemplateMessage : EventMessage
    {
        public HobblerTemplate Template;
        public string Id;
    }

    public class SetupHobblerCombatStatsMessage : EventMessage
    {
        public CombatStats Stats;
        public GeneticCombatStats Genetics;
        public GeneticCombatStats Accumulated;
    }

    public class QueryHobblerIdMessage : EventMessage
    {
        public Action<string> DoAfter;
    }

    public class QueryHobGeneratorMessage : EventMessage
    {
        //Templates, Timer for generation, rerollCost
        public Action<KeyValuePair<int, HobblerTemplate>[], TickTimer, int> DoAfter;
    }

    public class ShowHobGeneratorWindowMessage : EventMessage
    {
        public GameObject Owner;
    }

    public class WorldPopulationUpdatedMessage : EventMessage
    {
        public static WorldPopulationUpdatedMessage INSTANCE = new WorldPopulationUpdatedMessage();
    }

    public class EquipmentUpdatedMessage : EventMessage
    {
        public static EquipmentUpdatedMessage INSTANCE = new EquipmentUpdatedMessage();
    }

    public class AbilitiesUpdatedMessage : EventMessage
    {
        public static AbilitiesUpdatedMessage INSTANCE = new AbilitiesUpdatedMessage();
    }

    public class ToggleRosterWindowMessage : EventMessage
    {
        public static ToggleRosterWindowMessage INSTANCE = new ToggleRosterWindowMessage();
    }

    public class ApplyHobblerBattleDataMessage : EventMessage
    {
        public BattleUnitData Data;
        public BattleResult Result;
        public string MatchId;
    }

    public class CanMoveCheckMessage : EventMessage
    {
        public Action DoAfter;
    }

    public class CanCastCheckMessage : EventMessage
    {
        public Action DoAfter;
    }

    public class CanActivateTrinketCheckMessage : EventMessage
    {
        public Action DoAfter;
    }

    public class CanAttackCheckMessage : EventMessage
    {
        public Action DoAfter;
    }

    public class StatusEffectCheckMessage : EventMessage
    {
        public StatusEffectType Type;
        public Action DoAfter;
    }

    public class StunMessage : EventMessage
    {
        public static StunMessage INSTANCE = new StunMessage();
    }

    public class SilenceMessage : EventMessage
    {
        public static SilenceMessage INSTANCE = new SilenceMessage();
    }

    public class RootMessage : EventMessage
    {
        public static RootMessage INSTANCE = new RootMessage();
    }

    public class InterruptBumpMessage : EventMessage
    {
        public static InterruptBumpMessage INSTANCE = new InterruptBumpMessage();
    }

    public class DisarmMessage : EventMessage
    {
        public static DisarmMessage INSTANCE = new DisarmMessage();
    }

    public class DispelStatusEffectsMessage : EventMessage
    {
        public StatusEffectType[] Types;
    }

    public class EnterBattleMessage : EventMessage
    {
        public static EnterBattleMessage INSTANCE = new EnterBattleMessage();
    }

    public class SetAdventureUnitStateMessage : EventMessage
    {
        public AdventureUnitState State;
    }

    public class UpdateAdventureUnitStateMessage : EventMessage
    {
        public AdventureUnitState State;
    }

    public class QueryAdventureUnitStateMessage : EventMessage
    {
        public Action<AdventureUnitState> DoAfter;
    }

    public class SetUnitAnimationStateMessage : EventMessage
    {
        public UnitAnimationState State;
    }

    public class UpdateFacingDirectionMessage : EventMessage
    {
        public Vector2Int Direction;
    }

    public class SpawnAdventureUnitsMessage : EventMessage
    {
        public static SpawnAdventureUnitsMessage INSTANCE = new SpawnAdventureUnitsMessage();
    }

    public class EncounterFinishedMessage : EventMessage
    {
        public BattleResult Result;
    }

    public class DespawnUnitMessage : EventMessage
    {
        public static DespawnUnitMessage INSTANCE = new DespawnUnitMessage();
    }

    public class SetUnitSpawnerMessage : EventMessage
    {
        public GameObject Spawner;
    }

    public class QueryAdventureUnitTypeMessage : EventMessage
    {
        public Action<AdventureUnitType> DoAfter;
    }

    public class PlayerReadyForBattleMessage : EventMessage
    {
        public static PlayerReadyForBattleMessage INSTANCE = new PlayerReadyForBattleMessage();
    }

    public class SetPlayerCheckpointMessage : EventMessage
    {
        public Vector2Int Position;
        public AdventureMap Map;
    }

    public class RespawnPlayerMessage : EventMessage
    {
        public static RespawnPlayerMessage INSTANCE = new RespawnPlayerMessage();
    }

    public class ShowDialogueMessage : EventMessage
    {
        public DialogueData Dialogue;
        public GameObject Owner;
    }

    public class ShowCurrentDialogueAnswersMessage : EventMessage
    {
        public static ShowCurrentDialogueAnswersMessage INSTANCE = new ShowCurrentDialogueAnswersMessage();
    }

    public class CloseDialogueMessage : EventMessage
    {
        public static CloseDialogueMessage INSTANCE = new CloseDialogueMessage();
    }

    public class UpdateAdventureStateMessage : EventMessage
    {
        public AdventureState State;
    }

    public class PlayerInteractMessage : EventMessage
    {
        public static PlayerInteractMessage INSTANCE = new PlayerInteractMessage();
    }

    public class DialogueClosedMessage : EventMessage
    {
        public static DialogueClosedMessage INSTANCE = new DialogueClosedMessage();
    }

    public class SetPlayerInteractionObjectMessage : EventMessage
    {
        public GameObject Interact;
    }

    public class QueryHobblerDataMessage : EventMessage
    {
        public Action<HobblerData> DoAfter;
    }

    public class QueryHobblerGeneticsMessage : EventMessage
    {
        // Rolled, Accumulated
        public Action<GeneticCombatStats, GeneticCombatStats> DoAfter;
    }

    public class QueryTrainerDataMessage : EventMessage
    {
        //Id
        public Action<string> DoAfter;
    }

    public class QueryPlayerCheckpointMessage : EventMessage
    {
        //Map, Tile
        public Action<string,Vector2Int> DoAfter;
    }

    public class QueryNodeMessage : EventMessage
    {
        //Id, stack remaining
        public Action<string, int> DoAfter;
    }

    public class QueryBuildingMessge : EventMessage
    {
        //Template, Position, Id
        public Action<WorldBuilding, MapTile, string> DoAfter;
    }

    public class ClearWorldMessage : EventMessage
    {
        public static ClearWorldMessage INSTANCE = new ClearWorldMessage();
    }

    public class SetupHobblerFromDataMessage : EventMessage
    {
        public HobblerData Data;
    }

    public class SetAbilitiesFromDataMessage : EventMessage
    {
        public AbilityData[] Abilities;
    }

    public class SetSkillsFromDataMessage : EventMessage
    {
        public SkillData[] Skills;
    }

    public class SetEquippableItemsFromDataMessage : EventMessage
    {
        public EquippableItemData[] Items;
    }

    public class LoadWorldDataMessage : EventMessage
    {
        public static LoadWorldDataMessage INSTANCE = new LoadWorldDataMessage();
    }

    public class SetBuildingIdMessage : EventMessage
    {
        public string Id;
    }

    public class UpdateBuildingIdMessage : EventMessage
    {
        public string Id;
    }

    public class SetupTrainerMessage : EventMessage
    {
        public string Id;
        public BattleEncounter Encounter;
        public DialogueData PreEncounterDialogue;
        public DialogueData DefeatedDialogue;
    }

    public class MuteMessage : EventMessage
    {
        public static MuteMessage INSTANCE = new MuteMessage();
    }

    public class StatusEffectFinishedMessage : EventMessage
    {
        public StatusEffectType Type;
    }

    public class CastInterruptedMessage : EventMessage
    {
        public static CastInterruptedMessage INSTANCE = new CastInterruptedMessage();
    }

    public class ShowFloatingTextMessage : EventMessage
    {
        public string Text;
        public Color Color;
        public Vector2 World;
    }

    public class QueryGlobalCooldownMessage : EventMessage
    {
        public Action<TickTimer> DoAfter;
    }

    public class QueryBuildingParamterDataMessage : EventMessage
    {
        public Action<BuildingParameterData> DoAfter;
    }

    public class SetSelectedMazeSettingsControllerMessage : EventMessage
    {
        public UiMazeSettingsController Controller;
    }

    public class ShowMazeSelectionWindowMessage : EventMessage
    {
        public GameObject Hobbler;
    }

    public class ApplyManaMessage : EventMessage
    {
        public int Amount;
    }

    public class ResetPositionMessage : EventMessage
    {
        public static ResetPositionMessage INSTANCE = new ResetPositionMessage();
    }

    public class UpdateHobblerExperienceMessage : EventMessage
    {
        public int Level;
        public int Experience;
        public int ExperienceToNextLevel;
    }

    public class SetHobblerExperienceMessage : EventMessage
    {
        public int Level;
        public int Experience;
    }

    public class QueryCastingMessage : EventMessage
    {
        public Action DoAfter;
    }

    public class QueryTimerMessage : EventMessage
    {
        public TimerTrait Trait;
        public Action DoAfter;
    }

    public class RefreshTimerMessage : EventMessage
    {
        public TimerTrait Trait;
    }

    public class AbsorbedDamageCheckMessage : EventMessage
    {
        public DamageType Type;
        public WorldInstance<int> Instance;
    }

    public class SetAdventureMapTransitionMessage : EventMessage
    {
        public AdventureMap Map;
        public Vector2Int Position;
        public Vector2Int Direction;
    }

    public class QueryRequiredLevelExperienceMessage : EventMessage
    {
        public int Level;
        public Action<int> DoAfter;
    }

    public class UpdateHealthMessage : EventMessage
    {
        public int Current;
        public int Max;
    }

    public class UpdateManaMessage : EventMessage
    {
        public int Current;
        public int Max;
    }

    public class SearchForCraftingNodeMessage : EventMessage
    {
        public WorldSkill Skill;
        public Action DoAfter;
    }

    public class ApplySkillBonusMessage : EventMessage
    {
        public WorldSkill Skill;
        public float Bonus;
        public bool Permanent;
    }

    public class QuerySkillBonusMessage : EventMessage
    {
        public WorldSkill Skill;
        public Action<float> DoAfter;
    }

    public class CancelCraftingQueueAtIndexMessage : EventMessage
    {
        public int Index;
    }

    public class QueueCraftingRecipeMessage : EventMessage
    {
        public CraftingRecipe Recipe;
        public int Stack;
    }

    public class QueryCraftingRecipesMessage : EventMessage
    {
        public Action<CraftingRecipe[]> DoAfter;
    }

    public class QueryCraftingQueueMessage : EventMessage
    {
        //Queue, Max Queue Slots;
        public Action<QueuedCraft[], int> DoAfter;
    }

    public class SetSelectedCraftingRecipeControllerMessage : EventMessage
    {
        public UiCraftingRecipeController Controller;
    }

    public class SetCraftingIndexMessage : EventMessage
    {
        public int Current;
        public int Target;
    }

    public class ShowCraftingWindowMessage : EventMessage
    {
        public GameObject Owner;
    }
}

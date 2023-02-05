using System;
using System.Collections.Generic;
using Assets.Ancible_Tools.Scripts.System.WorldNodes;
using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame.AI;
using MessageBusLib;
using UnityEngine;
using DamageType = Assets.Resources.Ancible_Tools.Scripts.System.Combat.DamageType;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public static class MessageFactory
    {
        private static List<AddTraitToUnitMessage> _addTraitToUnitCache = new List<AddTraitToUnitMessage>();
        private static List<RemoveTraitFromUnitMessage> _removeTraitFromUnitCache = new List<RemoveTraitFromUnitMessage>();
        private static List<RemoveTraitFromUnitByControllerMessage> _removeTraitFromUnitByControllerCache = new List<RemoveTraitFromUnitByControllerMessage>();
        private static List<TraitCheckMessage> _traitCheckCache = new List<TraitCheckMessage>();
        private static List<HitboxCheckMessage> _hitboxCheckCache = new List<HitboxCheckMessage>();
        private static List<EnterCollisionWithObjectMessage> _enterCollisiongWithObjectCache = new List<EnterCollisionWithObjectMessage>();
        private static List<ExitCollisionWithObjectMessage> _exitCollisionWithObjectCache = new List<ExitCollisionWithObjectMessage>();
        private static List<RegisterCollisionMessage> _registerCollisionCache = new List<RegisterCollisionMessage>();
        private static List<UnregisterCollisionMessage> _unregisterCollisionCache = new List<UnregisterCollisionMessage>();
        private static List<SetDirectionMessage> _setDirectionCache = new List<SetDirectionMessage>();
        private static List<UpdateDirectionMessage> _updateDirectionCache = new List<UpdateDirectionMessage>();
        private static List<QueryDirectionMessage> _queryDirectionCache = new List<QueryDirectionMessage>();
        private static List<SetMonsterStateMessage> _setMonsterStateCache = new List<SetMonsterStateMessage>();
        private static List<UpdateMonsterStateMessage> _updateMonsterCache = new List<UpdateMonsterStateMessage>();
        private static List<QueryMonsterStateMessage> _queryMonsterCache = new List<QueryMonsterStateMessage>();
        private static List<SetPathMessage> _setPathCache = new List<SetPathMessage>();
        private static List<SetMapTileMessage> _setMapTileCache = new List<SetMapTileMessage>();
        private static List<UpdateMapTileMessage> _updateMapTileCache = new List<UpdateMapTileMessage>();
        private static List<QueryMapTileMessage> _queryMapTileCache = new List<QueryMapTileMessage>();
        private static List<UpdateHappinessMessage> _updateHappinessCache = new List<UpdateHappinessMessage>();
        private static List<QueryHappinessMessage> _queryHappinessCache = new List<QueryHappinessMessage>();
        private static List<ApplyWellbeingStatsMessage> _applyWellbeingStatsCache = new List<ApplyWellbeingStatsMessage>();
        private static List<RemoveHoveredUnitMessage> _removeHoveredUnitCache = new List<RemoveHoveredUnitMessage>();
        private static List<RemoveSelectedUnitMessage> _removeSelectedUnitCache = new List<RemoveSelectedUnitMessage>();
        private static List<GatherMessage> _gatherCache = new List<GatherMessage>();
        private static List<StopGatheringMessage> _stopGatheringCache = new List<StopGatheringMessage>();
        private static List<SearchForNodeMessage> _searchForNodeCache = new List<SearchForNodeMessage>();
        private static List<QueryCommandsMessage> _queryCommandsCache = new List<QueryCommandsMessage>();
        private static List<InteractMessage> _interactCache = new List<InteractMessage>();
        private static List<DoBumpMessage> _doBumpCache = new List<DoBumpMessage>();
        private static List<SetSpriteVisibilityMessage> _setSpriteVisibilityCache = new List<SetSpriteVisibilityMessage>();
        private static List<SetMinigameUnitStateMessage> _setMinigameUnitStateCache = new List<SetMinigameUnitStateMessage>();
        private static List<UpdateMinigameUnitStateMessage> _updateMinigameUnitStateCache = new List<UpdateMinigameUnitStateMessage>();
        private static List<QueryMinigameUnitStateMessage> _queryMinigameUnitStateCache = new List<QueryMinigameUnitStateMessage>();
        private static List<UpdatePositionMessage> _updatePositionCache = new List<UpdatePositionMessage>();
        private static List<MinigameInteractMessage> _minigameInteractCache = new List<MinigameInteractMessage>();
        private static List<ObstacleMessage> _obstacleCache = new List<ObstacleMessage>();
        private static List<MinigameInteractCheckMessage> _minigameInteractCheckCache = new List<MinigameInteractCheckMessage>();
        private static List<EndMinigameMessage> _endMinigameCache = new List<EndMinigameMessage>();
        private static List<StartMinigameMessage> _startMinigameCache = new List<StartMinigameMessage>();
        private static List<ShowNewCommandTreeMessage> _showNewCommandTreeCache = new List<ShowNewCommandTreeMessage>();
        private static List<RefillNodeStacksMessage> _refillNodeStacksCache = new List<RefillNodeStacksMessage>();
        private static List<AddExperienceMessage> _addExperienceCache = new List<AddExperienceMessage>();
        private static List<QueryExperienceMessage> _queryExperienceCache = new List<QueryExperienceMessage>();
        private static List<SetExperienceMessage> _setExperienceCache = new List<SetExperienceMessage>();
        private static List<QueryHobblerWellbeingStatusMessage> _queryHobblerWellbeingStatusCache = new List<QueryHobblerWellbeingStatusMessage>();
        private static List<ApplyCombatStatsMessage> _applyCombatStatsCache = new List<ApplyCombatStatsMessage>();
        private static List<QueryCombatStatsMessage> _queryCombatStatsCache = new List<QueryCombatStatsMessage>();
        private static List<UpdateCombatStatsMessage> _updateCombatStatsCache = new List<UpdateCombatStatsMessage>();
        private static List<SetCombatStatsMessage> _setCombatStatsCache = new List<SetCombatStatsMessage>();
        private static List<DoBasicAttackMessage> _doBasicAttackCache = new List<DoBasicAttackMessage>();
        private static List<DamageMessage> _damageCache = new List<DamageMessage>();
        private static List<QueryBonusDamageMessage> _queryBonusDamageCache = new List<QueryBonusDamageMessage>();
        private static List<DoBumpOverPixelsPerSecondMessage> _doBumpOverPixelsPerSecondCache = new List<DoBumpOverPixelsPerSecondMessage>();
        private static List<QueryCombatAlignmentMessage> _queryCombatAlignmentCache = new List<QueryCombatAlignmentMessage>();
        private static List<SetMinigameAiStateMessage> _setMinigameAiStateCache = new List<SetMinigameAiStateMessage>();
        private static List<UpdateMinigameAiStateMessage> _updateMinigameAiStateCache = new List<UpdateMinigameAiStateMessage>();
        private static List<QueryMinigameAiStateMessage> _queryMinigameAiStateCache = new List<QueryMinigameAiStateMessage>();
        private static List<SetCombatAlignmentMessage> _setCombatAlignmentCache = new List<SetCombatAlignmentMessage>();
        private static List<UpdateCombatAlignmentMessage> _updateCombatAlignmentCache = new List<UpdateCombatAlignmentMessage>();
        private static List<BasicAttackCheckMessage> _basicAttackCheckCache = new List<BasicAttackCheckMessage>();
        private static List<SetProxyHobblerMessage> _setProxyHobblerCache = new List<SetProxyHobblerMessage>();
        private static List<ReportDamageMessage> _reportDamageCache = new List<ReportDamageMessage>();
        private static List<SetOwnerMessage> _setOwnerCache = new List<SetOwnerMessage>();
        private static List<QueryOwnerMessage> _queryOwnerCache = new List<QueryOwnerMessage>();
        private static List<UpdateOwnerMessage> _updateOwnerCache = new List<UpdateOwnerMessage>();
        private static List<SetBasicAttackSetupMessage> _setBasicAttackSetupCache = new List<SetBasicAttackSetupMessage>();
        private static List<ClearBasicAttackSetupMessage> _clearBasicAttackSetupCache = new List<ClearBasicAttackSetupMessage>();
        private static List<QuerySpriteMessage> _querySpriteCache = new List<QuerySpriteMessage>();
        private static List<QueryBasicAttackSetupMessage> _queryBasicAttackSetupCache = new List<QueryBasicAttackSetupMessage>();
        private static List<SetSpriteMessage> _setSpriteCache = new List<SetSpriteMessage>();
        private static List<AddItemMessage> _addItemCache = new List<AddItemMessage>();
        private static List<SetLootTableMessage> _setLootTableCache = new List<SetLootTableMessage>();
        private static List<QueryHobblerEquipmentMessage> _queryHobblerEquipmentCache = new List<QueryHobblerEquipmentMessage>();
        private static List<ShowDetailedHobblerInfoMessage> _showDetailedHobblerInfoCache = new List<ShowDetailedHobblerInfoMessage>();
        private static List<ShowHoverInfoMessage> _showHoverInfoCache = new List<ShowHoverInfoMessage>();
        private static List<RemoveHoverInfoMessage> _removeHoverInfoCache = new List<RemoveHoverInfoMessage>();
        private static List<SetHoveredStashItemControllerMessage> _setHoveredStashItemControllerCache = new List<SetHoveredStashItemControllerMessage>();
        private static List<RemoveHoveredStashItemControllerMessage> _removeHoveredStashItemControllerCache = new List<RemoveHoveredStashItemControllerMessage>();
        private static List<SetHoveredEquippedItemControllerMessage> _setHoveredequippedItemControllerCache = new List<SetHoveredEquippedItemControllerMessage>();
        private static List<RemoveHoveredEquippedItemControllerMessage> _removeHoveredEquippedItemControllerCache = new List<RemoveHoveredEquippedItemControllerMessage>();
        private static List<EquipItemToSlotMessage> _equipItemToSlotCache = new List<EquipItemToSlotMessage>();
        private static List<UnequipItemFromSlotMessage> _unequipItemFromSlotCache = new List<UnequipItemFromSlotMessage>();
        private static List<RemoveItemMessage> _removeItemCache = new List<RemoveItemMessage>();
        private static List<SetActiveSelectableStateMessage> _setActiveSelectableStateCache = new List<SetActiveSelectableStateMessage>();
        private static List<UnregisterFromGatheringNodeMessage> _unregisterFromGatheringNodeCache = new List<UnregisterFromGatheringNodeMessage>();
        private static List<SetBattleAlignmentMessage> _setBattleAlignmentCache = new List<SetBattleAlignmentMessage>();
        private static List<UpdateBattleAlignmentMessage> _updateBattleAlignmentCache = new List<UpdateBattleAlignmentMessage>();
        private static List<QueryBattleAlignmentMessage> _queryBattleAlignmentCache = new List<QueryBattleAlignmentMessage>();
        private static List<SetEnemyUnitsMessage> _setEnemyUnitsCache = new List<SetEnemyUnitsMessage>();
        private static List<SetUnitBattleStateMessage> _setUnitBattleStateCache = new List<SetUnitBattleStateMessage>();
        private static List<UpdateUnitBattleStateMessage> _updateUnitBattleStateCache = new List<UpdateUnitBattleStateMessage>();
        private static List<QueryUnitBattleStateMessage> _queryUnitBattleStateCache = new List<QueryUnitBattleStateMessage>();
        private static List<RemoveEnemyUnitMessage> _removeEnemyUnitCache = new List<RemoveEnemyUnitMessage>();
        private static List<SetAbilitiesMessage> _setAbilitiesCache = new List<SetAbilitiesMessage>();
        private static List<BattleAbilityCheckMessage> _battleAbilityCheckCache = new List<BattleAbilityCheckMessage>();
        private static List<SetAlliesMessage> _setAlliesCache = new List<SetAlliesMessage>();
        private static List<RemoveAllyMessage> _removeAllyCache = new List<RemoveAllyMessage>();
        private static List<SetFaceDirectionMessage> _setFacingDirectionCache = new List<SetFaceDirectionMessage>();
        private static List<QueryHealthMessage> _queryHealthCache = new List<QueryHealthMessage>();
        private static List<HealMessage> _healCache = new List<HealMessage>();
        private static List<UpdateBattleAlignmentMessage> _updatebattleAlignmentCache = new List<UpdateBattleAlignmentMessage>();
        private static List<QueryBenchSlotMessage> _queryBenchSlotCache = new List<QueryBenchSlotMessage>();
        private static List<QueryClosestGamePieceBenchSlotMessage> _queryClosestGamePieceBenchSlotCache = new List<QueryClosestGamePieceBenchSlotMessage>();
        private static List<SetGamePieceBenchMessage> _setGamePieceBenchCache = new List<SetGamePieceBenchMessage>();
        private static List<SetGamePieceMapTileMessage> _setGamePieceMapTileCache = new List<SetGamePieceMapTileMessage>();
        private static List<SetGamePieceDataMessage> _setGamePieceDataCache = new List<SetGamePieceDataMessage>();
        private static List<QueryBattlePieceMessage> _queryBattlePieceCache = new List<QueryBattlePieceMessage>();
        private static List<UpdateBattlePointRequirementMessage> _updateBattlePointRequirementCache = new List<UpdateBattlePointRequirementMessage>();
        private static List<ReportHealMessage> _reportHealCache = new List<ReportHealMessage>();
        private static List<SetHoveredAbilitySlotControllerMessage> _setHoveredAbilitySlotControllerCache = new List<SetHoveredAbilitySlotControllerMessage>();
        private static List<RemoveHoveredAbilitySlotControllerMessage> _removeHoveredAbilitySlotControllerCache = new List<RemoveHoveredAbilitySlotControllerMessage>();
        private static List<QueryAbilitiesMessage> _queryAbilitiesCache = new List<QueryAbilitiesMessage>();
        private static List<ChangeAbilitySlotMessage> _changeAbilitySlotCache = new List<ChangeAbilitySlotMessage>();
        private static List<LearnAbilityMessage> _learnAbilityCache = new List<LearnAbilityMessage>();
        private static List<ForgetAbilityAtSlotMessage> _forgetAbilityCache = new List<ForgetAbilityAtSlotMessage>();
        private static List<UpdateFogVisibilityMessage> _updateFogVisibilityCache = new List<UpdateFogVisibilityMessage>();
        private static List<UseActionBarButtonMessage> _useActionBarButtonCache = new List<UseActionBarButtonMessage>();
        private static List<QueryMinigameAbilitiesMessage> _queryMinigameAbilitiesCache = new List<QueryMinigameAbilitiesMessage>();
        private static List<QueryManaMessage> _queryManaCache = new List<QueryManaMessage>();
        private static List<SetMinigameEquipmentMessage> _setMinigameEquipmentCache = new List<SetMinigameEquipmentMessage>();
        private static List<CastAbilityMessage> _castAbilityCache = new List<CastAbilityMessage>();
        private static List<UpdateUnitCastTimerMessage> _updateUnitCastTimerCache = new List<UpdateUnitCastTimerMessage>();
        private static List<ClaimKillMessage> _claimKillCache = new List<ClaimKillMessage>();
        private static List<QueryProxyRewardsMessage> _queryProxyRewardsCache = new List<QueryProxyRewardsMessage>();
        private static List<UpdateWellbeingMessage> _updateWellbeingCache = new List<UpdateWellbeingMessage>();
        private static List<SetHobblerAiStateMessage> _setHobblerAiStateCache = new List<SetHobblerAiStateMessage>();
        private static List<GainSkillExperienceMessage> _gainSkillExperienceCache = new List<GainSkillExperienceMessage>();
        private static List<SearchForResourceNodeMessage> _searchForResourceNodeCache = new List<SearchForResourceNodeMessage>();
        private static List<SkillCheckMessage> _skillCheckCache = new List<SkillCheckMessage>();
        private static List<QuerySkillsByPriorityMessage> _querySkillByPriorityCache = new List<QuerySkillsByPriorityMessage>();
        private static List<ChangeSkillPriorityMessage> _changeSkillPriorityCache = new List<ChangeSkillPriorityMessage>();
        private static List<SetEquipmentMessage> _setEquipmentCache = new List<SetEquipmentMessage>();
        private static List<SetUnitNameMessage> _setUnitNameCache = new List<SetUnitNameMessage>();
        private static List<QueryUnitNameMessage> _queryUnitNameCache = new List<QueryUnitNameMessage>();
        private static List<SetHobblerTemplateMessage> _setHobblerTemplateCache = new List<SetHobblerTemplateMessage>();
        private static List<SetupHobblerCombatStatsMessage> _setupHobblerCombatStatsCache = new List<SetupHobblerCombatStatsMessage>();
        private static List<QueryHobblerIdMessage> _queryHobblerIdCache = new List<QueryHobblerIdMessage>();
        private static List<PurchaseHobblerAtSlotMessage> _purchaseHobblerAtSlotCache = new List<PurchaseHobblerAtSlotMessage>();
        private static List<QueryHobGeneratorMessage> _queryHobGeneratorCache = new List<QueryHobGeneratorMessage>();
        private static List<ShowHobGeneratorWindowMessage> _showHobGeneratorWindowCache = new List<ShowHobGeneratorWindowMessage>();
        private static List<CanMoveCheckMessage> _canMoveCheckCache = new List<CanMoveCheckMessage>();
        private static List<CanCastCheckMessage> _canCastCheckCache = new List<CanCastCheckMessage>();
        private static List<CanActivateTrinketCheckMessage> _canActivateTrinketCheckCache = new List<CanActivateTrinketCheckMessage>();
        private static List<StatusEffectCheckMessage> _statusEffectCheckCache = new List<StatusEffectCheckMessage>();
        private static List<CanAttackCheckMessage> _canAttachCheckCache = new List<CanAttackCheckMessage>();
        private static List<DispelStatusEffectsMessage> _dispelStatusEffectsCache = new List<DispelStatusEffectsMessage>();
        private static List<SetAdventureUnitStateMessage> _setAdventureStateCache = new List<SetAdventureUnitStateMessage>();
        private static List<UpdateAdventureUnitStateMessage> _updateAdventureUnitStateCache = new List<UpdateAdventureUnitStateMessage>();
        private static List<QueryAdventureUnitStateMessage> _queryAdventureUnitStateCache = new List<QueryAdventureUnitStateMessage>();
        private static List<SetUnitAnimationStateMessage> _setUnitAnimationStateCache = new List<SetUnitAnimationStateMessage>();
        private static List<UpdateFacingDirectionMessage> _updateFacingDirectionCache = new List<UpdateFacingDirectionMessage>();
        private static List<SetUnitSpawnerMessage> _setUnitSpawnerCache = new List<SetUnitSpawnerMessage>();
        private static List<QueryAdventureUnitTypeMessage> _queryAdventureUnitTypeCache = new List<QueryAdventureUnitTypeMessage>();
        private static List<SetPlayerCheckpointMessage> _setPlayerCheckpointCache = new List<SetPlayerCheckpointMessage>();
        private static List<ShowDialogueMessage> _showDialogueCache = new List<ShowDialogueMessage>();
        private static List<SetPlayerInteractionObjectMessage> _setPlayerInteractionObjectCache = new List<SetPlayerInteractionObjectMessage>();
        private static List<QueryHobblerDataMessage> _queryHobblerDataCache = new List<QueryHobblerDataMessage>();
        private static List<QueryHobblerGeneticsMessage> _queryHobblerGeneticsCache = new List<QueryHobblerGeneticsMessage>();
        private static List<QueryWellbeingStatsMessage> _queryWellbeingStatsCache = new List<QueryWellbeingStatsMessage>();
        private static List<QueryPlayerCheckpointMessage> _queryPlayerCheckpointCache = new List<QueryPlayerCheckpointMessage>();
        private static List<QueryTrainerDataMessage> _queryTrainerDataCache = new List<QueryTrainerDataMessage>();
        private static List<QueryNodeMessage> _queryNodeCache = new List<QueryNodeMessage>();
        private static List<QueryBuildingMessge> _queryBuildingCache = new List<QueryBuildingMessge>();
        private static List<SetAbilitiesFromDataMessage> _setAbilitiesFromDataCache = new List<SetAbilitiesFromDataMessage>();
        private static List<SetSkillsFromDataMessage> _setSkillsFromDataCache = new List<SetSkillsFromDataMessage>();
        private static List<SetEquippableItemsFromDataMessage> _setEquippableItemsFromDataCache = new List<SetEquippableItemsFromDataMessage>();
        private static List<UpdateBuildingIdMessage> _updateBuildingIdCache = new List<UpdateBuildingIdMessage>();
        private static List<SetupTrainerMessage> _setupTrainerCache = new List<SetupTrainerMessage>();
        private static List<StatusEffectFinishedMessage> _statusEffectFinishedCache = new List<StatusEffectFinishedMessage>();
        private static List<ShowFloatingTextMessage> _showFloatingTextCache = new List<ShowFloatingTextMessage>();
        private static List<QueryGlobalCooldownMessage> _queryGlobalCooldownCache = new List<QueryGlobalCooldownMessage>();
        private static List<QueryBuildingParamterDataMessage> _queryBuildingParameterCache = new List<QueryBuildingParamterDataMessage>();
        private static List<SetSelectedMazeSettingsControllerMessage> _setSelectedMazeSettingsControllerCache = new List<SetSelectedMazeSettingsControllerMessage>();
        private static List<ShowMazeSelectionWindowMessage> _showMazeSelectionWindowCache = new List<ShowMazeSelectionWindowMessage>();
        private static List<ApplyManaMessage> _applyManaCache = new List<ApplyManaMessage>();
        private static List<SetSpriteAlphaMessage> _setSpriteAlphaCache = new List<SetSpriteAlphaMessage>();
        private static List<UpdateHobblerExperienceMessage> _updateHobblerExperienceCache = new List<UpdateHobblerExperienceMessage>();
        private static List<SetHobblerExperienceMessage> _setHobblerExperienceCache = new List<SetHobblerExperienceMessage>();
        private static List<QueryCastingMessage> _queryCastingCache = new List<QueryCastingMessage>();
        private static List<QueryTimerMessage> _queryTimerCache = new List<QueryTimerMessage>();
        private static List<RefreshTimerMessage> _refreshTimerCache = new List<RefreshTimerMessage>();
        private static List<AbsorbedDamageCheckMessage> _absorbedDamageCheckCache = new List<AbsorbedDamageCheckMessage>();

        public static AddTraitToUnitMessage GenerateAddTraitToUnitMsg()
        {
            if (_addTraitToUnitCache.Count > 0)
            {
                var message = _addTraitToUnitCache[0];
                _addTraitToUnitCache.Remove(message);
                return message;
            }

            return new AddTraitToUnitMessage();
        }

        public static RemoveTraitFromUnitMessage GenerateRemoveTraitFromUnitMsg()
        {
            if (_removeTraitFromUnitCache.Count > 0)
            {
                var message = _removeTraitFromUnitCache[0];
                _removeTraitFromUnitCache.Remove(message);
                return message;
            }

            return new RemoveTraitFromUnitMessage();
        }

        public static RemoveTraitFromUnitByControllerMessage GenerateRemoveTraitFromUnitByControllerMsg()
        {
            if (_removeTraitFromUnitByControllerCache.Count > 0)
            {
                var message = _removeTraitFromUnitByControllerCache[0];
                _removeTraitFromUnitByControllerCache.Remove(message);
                return message;
            }

            return new RemoveTraitFromUnitByControllerMessage();
        }

        public static HitboxCheckMessage GenerateHitboxCheckMsg()
        {
            if (_hitboxCheckCache.Count > 0)
            {
                var message = _hitboxCheckCache[0];
                _hitboxCheckCache.Remove(message);
                return message;
            }

            return new HitboxCheckMessage();
        }

        public static EnterCollisionWithObjectMessage GenerateEnterCollisionWithObjectMsg()
        {
            if (_enterCollisiongWithObjectCache.Count > 0)
            {
                var message = _enterCollisiongWithObjectCache[0];
                _enterCollisiongWithObjectCache.Remove(message);
                return message;
            }

            return new EnterCollisionWithObjectMessage();
        }

        public static ExitCollisionWithObjectMessage GenerateExitCollisionWithObjectMsg()
        {
            if (_exitCollisionWithObjectCache.Count > 0)
            {
                var message = _exitCollisionWithObjectCache[0];
                _exitCollisionWithObjectCache.Remove(message);
                return message;
            }

            return new ExitCollisionWithObjectMessage();
        }

        public static TraitCheckMessage GenerateTraitCheckMsg()
        {
            if (_traitCheckCache.Count > 0)
            {
                var message = _traitCheckCache[0];
                _traitCheckCache.Remove(message);
                return message;
            }

            return new TraitCheckMessage();
        }

        public static RegisterCollisionMessage GenerateRegisterCollisionMsg()
        {
            if (_registerCollisionCache.Count > 0)
            {
                var message = _registerCollisionCache[0];
                _registerCollisionCache.Remove(message);
                return message;
            }

            return new RegisterCollisionMessage();
        }

        public static UnregisterCollisionMessage GenerateUnregisterCollisionMsg()
        {
            if (_unregisterCollisionCache.Count > 0)
            {
                var message = _unregisterCollisionCache[0];
                _unregisterCollisionCache.Remove(message);
                return message;
            }

            return new UnregisterCollisionMessage();
        }

        public static SetDirectionMessage GenerateSetDirectionMsg()
        {
            if (_setDirectionCache.Count > 0)
            {
                var message = _setDirectionCache[0];
                _setDirectionCache.Remove(message);
                return message;
            }

            return new SetDirectionMessage();
        }

        public static UpdateDirectionMessage GenerateUpdateDirectionMsg()
        {
            if (_updateDirectionCache.Count > 0)
            {
                var message = _updateDirectionCache[0];
                _updateDirectionCache.Remove(message);
                return message;
            }

            return new UpdateDirectionMessage();
        }

        public static QueryDirectionMessage GenerateQueryDirectionMsg()
        {
            if (_queryDirectionCache.Count > 0)
            {
                var message = _queryDirectionCache[0];
                _queryDirectionCache.Remove(message);
                return message;
            }

            return new QueryDirectionMessage();
        }

        public static QueryMonsterStateMessage GenerateQueryMonsterStateMsg()
        {
            if (_queryMonsterCache.Count > 0)
            {
                var message = _queryMonsterCache[0];
                _queryMonsterCache.Remove(message);
                return message;
            }

            return new QueryMonsterStateMessage();
        }

        public static UpdateMonsterStateMessage GenerateUpdateMonsterStateMsg()
        {
            if (_updateMonsterCache.Count > 0)
            {
                var message = _updateMonsterCache[0];
                _updateMonsterCache.Remove(message);
                return message;
            }

            return new UpdateMonsterStateMessage();
        }

        public static SetMonsterStateMessage GenerateSetMonsterStateMsg()
        {
            if (_setMonsterStateCache.Count > 0)
            {
                var message = _setMonsterStateCache[0];
                _setMonsterStateCache.Remove(message);
                return message;
            }

            return new SetMonsterStateMessage();
        }

        public static SetPathMessage GenerateSetPathMsg()
        {
            if (_setPathCache.Count > 0)
            {
                var message = _setPathCache[0];
                _setPathCache.Remove(message);
                return message;
            }

            return new SetPathMessage();
        }

        public static SetMapTileMessage GenerateSetMapTileMsg()
        {
            if (_setMapTileCache.Count > 0)
            {
                var message = _setMapTileCache[0];
                _setMapTileCache.Remove(message);
                return message;
            }

            return new SetMapTileMessage();
        }

        public static UpdateMapTileMessage GenerateUpdateMapTileMsg()
        {
            if (_updateMapTileCache.Count > 0)
            {
                var message = _updateMapTileCache[0];
                _updateMapTileCache.Remove(message);
                return message;
            }

            return new UpdateMapTileMessage();
        }

        public static QueryMapTileMessage GenerateQueryMapTileMsg()
        {
            if (_queryMapTileCache.Count > 0)
            {
                var message = _queryMapTileCache[0];
                _queryMapTileCache.Remove(message);
                return message;
            }

            return new QueryMapTileMessage();
        }

        public static UpdateHappinessMessage GenerateUpdateHappinessMsg()
        {
            if (_updateHappinessCache.Count > 0)
            {
                var message = _updateHappinessCache[0];
                _updateHappinessCache.Remove(message);
                return message;
            }

            return new UpdateHappinessMessage();
        }

        public static QueryHappinessMessage GenerateQueryHappinessMsg()
        {
            if (_queryHappinessCache.Count > 0)
            {
                var message = _queryHappinessCache[0];
                _queryHappinessCache.Remove(message);
                return message;
            }

            return new QueryHappinessMessage();
        }

        public static ApplyWellbeingStatsMessage GenerateApplyWellbeingStatsMsg()
        {
            if (_applyWellbeingStatsCache.Count > 0)
            {
                var message = _applyWellbeingStatsCache[0];
                _applyWellbeingStatsCache.Remove(message);
                return message;
            }

            return new ApplyWellbeingStatsMessage();
        }

        public static RemoveHoveredUnitMessage GenerateRemoveHoveredUnitMsg()
        {
            if (_removeHoveredUnitCache.Count > 0)
            {
                var message = _removeHoveredUnitCache[0];
                _removeHoveredUnitCache.Remove(message);
                return message;
            }

            return new RemoveHoveredUnitMessage();
        }

        public static RemoveSelectedUnitMessage GenerateRemoveSelectedUnitMsg()
        {
            if (_removeSelectedUnitCache.Count > 0)
            {
                var message = _removeSelectedUnitCache[0];
                _removeSelectedUnitCache.Remove(message);
                return message;
            }

            return new RemoveSelectedUnitMessage();
        }

        public static GatherMessage GenerateGatherMsg()
        {
            if (_gatherCache.Count > 0)
            {
                var message = _gatherCache[0];
                _gatherCache.Remove(message);
                return message;
            }

            return new GatherMessage();
        }

        public static StopGatheringMessage GenerateStopGatheringMsg()
        {
            if (_stopGatheringCache.Count > 0)
            {
                var message = _stopGatheringCache[0];
                _stopGatheringCache.Remove(message);
                return message;
            }

            return new StopGatheringMessage();
        }

        public static SearchForNodeMessage GenerateSearchForNodeMsg()
        {
            if (_searchForNodeCache.Count > 0)
            {
                var message = _searchForNodeCache[0];
                _searchForNodeCache.Remove(message);
                return message;
            }

            return new SearchForNodeMessage();
        }

        public static QueryCommandsMessage GenerateQueryCommandsMsg()
        {
            if (_queryCommandsCache.Count > 0)
            {
                var message = _queryCommandsCache[0];
                _queryCommandsCache.Remove(message);
                return message;
            }

            return new QueryCommandsMessage();
        }

        public static InteractMessage GenerateInteractMsg()
        {
            if (_interactCache.Count > 0)
            {
                var message = _interactCache[0];
                _interactCache.Remove(message);
                return message;
            }

            return new InteractMessage();
        }

        public static DoBumpMessage GenerateDoBumpMsg()
        {
            if (_doBumpCache.Count > 0)
            {
                var message = _doBumpCache[0];
                _doBumpCache.Remove(message);
                return message;
            }

            return new DoBumpMessage();
        }

        public static SetSpriteVisibilityMessage GenerateSetSpriteVisibilityMsg()
        {
            if (_setSpriteVisibilityCache.Count > 0)
            {
                var message = _setSpriteVisibilityCache[0];
                _setSpriteVisibilityCache.Remove(message);
                return message;
            }

            return new SetSpriteVisibilityMessage();
        }

        public static SetMinigameUnitStateMessage GenerateSetMinigameUnitStateMsg()
        {
            if (_setMinigameUnitStateCache.Count > 0)
            {
                var message = _setMinigameUnitStateCache[0];
                _setMinigameUnitStateCache.Remove(message);
                return message;
            }

            return new SetMinigameUnitStateMessage();
        }

        public static UpdateMinigameUnitStateMessage GenerateUpdateMinigameUnitStateMsg()
        {
            if (_updateMinigameUnitStateCache.Count > 0)
            {
                var message = _updateMinigameUnitStateCache[0];
                _updateMinigameUnitStateCache.Remove(message);
                return message;
            }

            return new UpdateMinigameUnitStateMessage();
        }

        public static QueryMinigameUnitStateMessage GenerateQueryMinigameUnitStateMsg()
        {
            if (_queryMinigameUnitStateCache.Count > 0)
            {
                var message = _queryMinigameUnitStateCache[0];
                _queryMinigameUnitStateCache.Remove(message);
                return message;
            }

            return new QueryMinigameUnitStateMessage();
        }

        public static UpdatePositionMessage GenerateUpdatePositionMsg()
        {
            if (_updatePositionCache.Count > 0)
            {
                var message = _updatePositionCache[0];
                _updatePositionCache.Remove(message);
                return message;
            }

            return new UpdatePositionMessage();
        }

        public static MinigameInteractMessage GenerateMinigameInteractMsg()
        {
            if (_minigameInteractCache.Count > 0)
            {
                var message = _minigameInteractCache[0];
                _minigameInteractCache.Remove(message);
                return message;
            }

            return new MinigameInteractMessage();
        }

        public static ObstacleMessage GenerateObstacleMsg()
        {
            if (_obstacleCache.Count > 0)
            {
                var message = _obstacleCache[0];
                _obstacleCache.Remove(message);
                return message;
            }

            return new ObstacleMessage();
        }

        public static MinigameInteractCheckMessage GenerateMinigameInteractCheckMsg()
        {
            if (_minigameInteractCheckCache.Count > 0)
            {
                var message = _minigameInteractCheckCache[0];
                _minigameInteractCheckCache.Remove(message);
                return message;
            }

            return new MinigameInteractCheckMessage();
        }

        public static EndMinigameMessage GenerateEndMinigameMsg()
        {
            if (_endMinigameCache.Count > 0)
            {
                var message = _endMinigameCache[0];
                _endMinigameCache.Remove(message);
                return message;
            }

            return new EndMinigameMessage();
        }

        public static StartMinigameMessage GenerateStartMinigameMsg()
        {
            if (_startMinigameCache.Count > 0)
            {
                var message = _startMinigameCache[0];
                _startMinigameCache.Remove(message);
                return message;
            }

            return new StartMinigameMessage();
        }

        public static ShowNewCommandTreeMessage GenerateShowNewCommandTreeMsg()
        {
            if (_showNewCommandTreeCache.Count > 0)
            {
                var message = _showNewCommandTreeCache[0];
                _showNewCommandTreeCache.Remove(message);
                return message;
            }

            return new ShowNewCommandTreeMessage();
        }

        public static RefillNodeStacksMessage GenerateRefillNodeStacksMsg()
        {
            if (_refillNodeStacksCache.Count > 0)
            {
                var message = _refillNodeStacksCache[0];
                _refillNodeStacksCache.Remove(message);
                return message;
            }

            return new RefillNodeStacksMessage();
        }

        public static AddExperienceMessage GenerateAddExperienceMsg()
        {
            if (_addExperienceCache.Count > 0)
            {
                var message = _addExperienceCache[0];
                _addExperienceCache.Remove(message);
                return message;
            }

            return new AddExperienceMessage();
        }

        public static QueryExperienceMessage GenerateQueryExperienceMsg()
        {
            if (_queryExperienceCache.Count > 0)
            {
                var message = _queryExperienceCache[0];
                _queryExperienceCache.Remove(message);
                return message;
            }

            return new QueryExperienceMessage();
        }

        public static SetExperienceMessage GenerateSetExperienceMsg()
        {
            if (_setExperienceCache.Count > 0)
            {
                var message = _setExperienceCache[0];
                _setExperienceCache.Remove(message);
                return message;
            }

            return new SetExperienceMessage();
        }

        public static QueryHobblerWellbeingStatusMessage GenerateQueryHobblerWellbeingStatusMsg()
        {
            if (_queryHobblerWellbeingStatusCache.Count > 0)
            {
                var message = _queryHobblerWellbeingStatusCache[0];
                _queryHobblerWellbeingStatusCache.Remove(message);
                return message;
            }

            return new QueryHobblerWellbeingStatusMessage();
        }

        public static ApplyCombatStatsMessage GenerateApplyCombatStatsMsg()
        {
            if (_applyCombatStatsCache.Count > 0)
            {
                var message = _applyCombatStatsCache[0];
                _applyCombatStatsCache.Remove(message);
                return message;
            }

            return new ApplyCombatStatsMessage();
        }

        public static SetCombatStatsMessage GenerateSetCombatStatsMsg()
        {
            if (_setCombatStatsCache.Count > 0)
            {
                var message = _setCombatStatsCache[0];
                _setCombatStatsCache.Remove(message);
                return message;
            }

            return new SetCombatStatsMessage();
        }

        public static UpdateCombatStatsMessage GenerateUpdateCombatStatsMsg()
        {
            if (_updateCombatStatsCache.Count > 0)
            {
                var message = _updateCombatStatsCache[0];
                _updateCombatStatsCache.Remove(message);
                return message;
            }

            return new UpdateCombatStatsMessage();
        }

        public static QueryCombatStatsMessage GenerateQueryCombatStatsMsg()
        {
            if (_queryCombatStatsCache.Count > 0)
            {
                var message = _queryCombatStatsCache[0];
                _queryCombatStatsCache.Remove(message);
                return message;
            }

            return new QueryCombatStatsMessage();
        }

        public static DoBasicAttackMessage GenerateDoBasicAttackMsg()
        {
            if (_doBasicAttackCache.Count > 0)
            {
                var message = _doBasicAttackCache[0];
                _doBasicAttackCache.Remove(message);
                return message;
            }

            return new DoBasicAttackMessage();
        }

        public static DamageMessage GenerateDamageMsg()
        {
            if (_damageCache.Count > 0)
            {
                var message = _damageCache[0];
                _damageCache.Remove(message);
                return message;
            }

            return new DamageMessage();
        }

        public static QueryBonusDamageMessage GenerateQueryBonusDamageMsg()
        {
            if (_queryBonusDamageCache.Count > 0)
            {
                var message = _queryBonusDamageCache[0];
                _queryBonusDamageCache.Remove(message);
                return message;
            }

            return new QueryBonusDamageMessage();
        }

        public static DoBumpOverPixelsPerSecondMessage GenerateDoBumpOverPixelsPerSecondMsg()
        {
            if (_doBumpOverPixelsPerSecondCache.Count > 0)
            {
                var message = _doBumpOverPixelsPerSecondCache[0];
                _doBumpOverPixelsPerSecondCache.Remove(message);
                return message;
            }

            return new DoBumpOverPixelsPerSecondMessage();
        }

        public static QueryCombatAlignmentMessage GenerateQueryCombatAlignmentMsg()
        {
            if (_queryCombatAlignmentCache.Count > 0)
            {
                var message = _queryCombatAlignmentCache[0];
                _queryCombatAlignmentCache.Remove(message);
                return message;
            }
            return new QueryCombatAlignmentMessage();
        }

        public static SetMinigameAiStateMessage GenerateSetMinigameAiStateMsg()
        {
            if (_setMinigameAiStateCache.Count > 0)
            {
                var message = _setMinigameAiStateCache[0];
                _setMinigameAiStateCache.Remove(message);
                return message;
            }

            return new SetMinigameAiStateMessage();
        }

        public static UpdateMinigameAiStateMessage GenerateUpdateMinigameAiStateMsg()
        {
            if (_updateMinigameAiStateCache.Count > 0)
            {
                var message = _updateMinigameAiStateCache[0];
                _updateMinigameAiStateCache.Remove(message);
                return message;
            }

            return new UpdateMinigameAiStateMessage();
        }

        public static QueryMinigameAiStateMessage GenerateQueryMinigameAiStateMsg()
        {
            if (_queryMinigameAiStateCache.Count > 0)
            {
                var message = _queryMinigameAiStateCache[0];
                _queryMinigameAiStateCache.Remove(message);
                return message;
            }

            return new QueryMinigameAiStateMessage();
        }

        public static SetCombatAlignmentMessage GenerateSetCombatAlignmentMsg()
        {
            if (_setCombatAlignmentCache.Count > 0)
            {
                var message = _setCombatAlignmentCache[0];
                _setCombatAlignmentCache.Remove(message);
                return message;
            }

            return new SetCombatAlignmentMessage();
        }

        public static UpdateCombatAlignmentMessage GenerateUpdateCombatAlignmentMsg()
        {
            if (_updateCombatAlignmentCache.Count > 0)
            {
                var message = _updateCombatAlignmentCache[0];
                _updateCombatAlignmentCache.Remove(message);
                return message;
            }

            return new UpdateCombatAlignmentMessage();
        }

        public static BasicAttackCheckMessage GenerateBasicAttackCheckMsg()
        {
            if (_basicAttackCheckCache.Count > 0)
            {
                var message = _basicAttackCheckCache[0];
                _basicAttackCheckCache.Remove(message);
                return message;
            }

            return new BasicAttackCheckMessage();
        }

        public static SetProxyHobblerMessage GenerateSetProxyHobblerMsg()
        {
            if (_setProxyHobblerCache.Count > 0)
            {
                var message = _setProxyHobblerCache[0];
                _setProxyHobblerCache.Remove(message);
                return message;
            }

            return new SetProxyHobblerMessage();
        }

        public static ReportDamageMessage GenerateReportDamageMsg()
        {
            if (_reportDamageCache.Count > 0)
            {
                var message = _reportDamageCache[0];
                _reportDamageCache.Remove(message);
                return message;
            }

            return new ReportDamageMessage();
        }

        public static SetOwnerMessage GenerateSetOwnerMsg()
        {
            if (_setOwnerCache.Count > 0)
            {
                var message = _setOwnerCache[0];
                _setOwnerCache.Remove(message);
                return message;
            }

            return new SetOwnerMessage();
        }

        public static QueryOwnerMessage GenerateQueryOwnerMsg()
        {
            if (_queryOwnerCache.Count > 0)
            {
                var message = _queryOwnerCache[0];
                _queryOwnerCache.Remove(message);
                return message;
            }

            return new QueryOwnerMessage();
        }

        public static UpdateOwnerMessage GenerateUpdateOwnerMsg()
        {
            if (_updateOwnerCache.Count > 0)
            {
                var message = _updateOwnerCache[0];
                _updateOwnerCache.Remove(message);
                return message;
            }

            return new UpdateOwnerMessage();
        }

        public static SetBasicAttackSetupMessage GenerateSetBasicAttackSetupMsg()
        {
            if (_setBasicAttackSetupCache.Count > 0)
            {
                var message = _setBasicAttackSetupCache[0];
                _setBasicAttackSetupCache.Remove(message);
                return message;
            }

            return new SetBasicAttackSetupMessage();
        }

        public static ClearBasicAttackSetupMessage GenearteClearBasicAttackSetupMsg()
        {
            if (_clearBasicAttackSetupCache.Count > 0)
            {
                var message = _clearBasicAttackSetupCache[0];
                _clearBasicAttackSetupCache.Remove(message);
                return message;
            }

            return new ClearBasicAttackSetupMessage();
        }

        public static QuerySpriteMessage GenerateQuerySpriteMsg()
        {
            if (_querySpriteCache.Count > 0)
            {
                var message = _querySpriteCache[0];
                _querySpriteCache.Remove(message);
                return message;
            }

            return new QuerySpriteMessage();
        }

        public static QueryBasicAttackSetupMessage GenerateQueryBasicAttackSetupMsg()
        {
            if (_queryBasicAttackSetupCache.Count > 0)
            {
                var message = _queryBasicAttackSetupCache[0];
                _queryBasicAttackSetupCache.Remove(message);
                return message;
            }

            return new QueryBasicAttackSetupMessage();
        }

        public static SetSpriteMessage GenerateSetSpriteMsg()
        {
            if (_setSpriteCache.Count > 0)
            {
                var message = _setSpriteCache[0];
                _setSpriteCache.Remove(message);
                return message;
            }

            return new SetSpriteMessage();
        }

        public static AddItemMessage GenerateAddItemMsg()
        {
            if (_addItemCache.Count > 0)
            {
                var message = _addItemCache[0];
                _addItemCache.Remove(message);
                return message;
            }

            return new AddItemMessage();
        }

        public static SetLootTableMessage GenerateSetLootTableMsg()
        {
            if (_setLootTableCache.Count > 0)
            {
                var message = _setLootTableCache[0];
                _setLootTableCache.Remove(message);
                return message;
            }

            return new SetLootTableMessage();
        }

        public static QueryHobblerEquipmentMessage GenerateQueryHobblerEquipmentMsg()
        {
            if (_queryHobblerEquipmentCache.Count > 0)
            {
                var message = _queryHobblerEquipmentCache[0];
                _queryHobblerEquipmentCache.Remove(message);
                return message;
            }

            return new QueryHobblerEquipmentMessage();
        }

        public static ShowDetailedHobblerInfoMessage GenerateShowDetailedHobblerInfoMsg()
        {
            if (_showDetailedHobblerInfoCache.Count > 0)
            {
                var message = _showDetailedHobblerInfoCache[0];
                _showDetailedHobblerInfoCache.Remove(message);
                return message;
            }

            return new ShowDetailedHobblerInfoMessage();
        }

        public static ShowHoverInfoMessage GenerateShowHoverInfoMsg()
        {
            if (_showHoverInfoCache.Count > 0)
            {
                var message = _showHoverInfoCache[0];
                _showHoverInfoCache.Remove(message);
                return message;
            }

            return new ShowHoverInfoMessage();
        }

        public static RemoveHoverInfoMessage GenerateRemoveHoverInfoMsg()
        {
            if (_removeHoverInfoCache.Count > 0)
            {
                var message = _removeHoverInfoCache[0];
                _removeHoverInfoCache.Remove(message);
                return message;
            }

            return new RemoveHoverInfoMessage();
        }

        public static SetHoveredStashItemControllerMessage GenerateSetHoveredStashItemControllerMsg()
        {
            if (_setHoveredStashItemControllerCache.Count > 0)
            {
                var message = _setHoveredStashItemControllerCache[0];
                _setHoveredStashItemControllerCache.Remove(message);
                return message;
            }

            return new SetHoveredStashItemControllerMessage();
        }

        public static RemoveHoveredStashItemControllerMessage GenerateRemoveHoveredStashItemControllerMsg()
        {
            if (_removeHoveredStashItemControllerCache.Count > 0)
            {
                var message = _removeHoveredStashItemControllerCache[0];
                _removeHoveredStashItemControllerCache.Remove(message);
                return message;
            }

            return new RemoveHoveredStashItemControllerMessage();
        }

        public static SetHoveredEquippedItemControllerMessage GenerateSetHoveredEquippedItemControllerMsg()
        {
            if (_setHoveredequippedItemControllerCache.Count > 0)
            {
                var message = _setHoveredequippedItemControllerCache[0];
                _setHoveredequippedItemControllerCache.Remove(message);
                return message;
            }

            return new SetHoveredEquippedItemControllerMessage();
        }

        public static RemoveHoveredEquippedItemControllerMessage GenerateRemoveHoveredEquippedItemControllerMsg()
        {
            if (_removeHoveredEquippedItemControllerCache.Count > 0)
            {
                var message = _removeHoveredEquippedItemControllerCache[0];
                _removeHoveredEquippedItemControllerCache.Remove(message);
                return message;
            }

            return new RemoveHoveredEquippedItemControllerMessage();
        }

        public static EquipItemToSlotMessage GenerateEquipItemToSlotMsg()
        {
            if (_equipItemToSlotCache.Count > 0)
            {
                var message = _equipItemToSlotCache[0];
                _equipItemToSlotCache.Remove(message);
                return message;
            }

            return new EquipItemToSlotMessage();
        }

        public static UnequipItemFromSlotMessage GenerateUnequipItemFromSlotMsg()
        {
            if (_unequipItemFromSlotCache.Count > 0)
            {
                var message = _unequipItemFromSlotCache[0];
                _unequipItemFromSlotCache.Remove(message);
                return message;
            }

            return new UnequipItemFromSlotMessage();
        }

        public static RemoveItemMessage GenerateRemoveItemMsg()
        {
            if (_removeItemCache.Count > 0)
            {
                var message = _removeItemCache[0];
                _removeItemCache.Remove(message);
                return message;
            }

            return new RemoveItemMessage();
        }

        public static SetActiveSelectableStateMessage GenerateSetActiveSelectableStateMsg()
        {
            if (_setActiveSelectableStateCache.Count > 0)
            {
                var message = _setActiveSelectableStateCache[0];
                _setActiveSelectableStateCache.Remove(message);
                return message;
            }

            return new SetActiveSelectableStateMessage();
        }

        public static UnregisterFromGatheringNodeMessage GenerateUnregisterFromGatheringNodeMsg()
        {
            if (_unregisterFromGatheringNodeCache.Count > 0)
            {
                var message = _unregisterFromGatheringNodeCache[0];
                _unregisterFromGatheringNodeCache.Remove(message);
                return message;
            }

            return new UnregisterFromGatheringNodeMessage();
        }

        public static SetBattleAlignmentMessage GenerateSetBattleAlignmentMsg()
        {
            if (_setBattleAlignmentCache.Count > 0)
            {
                var message = _setBattleAlignmentCache[0];
                _setBattleAlignmentCache.Remove(message);
                return message;
            }

            return new SetBattleAlignmentMessage();
        }

        public static UpdateBattleAlignmentMessage GenerateUpdateBattleAlignmentMsg()
        {
            if (_updateBattleAlignmentCache.Count > 0)
            {
                var message = _updateBattleAlignmentCache[0];
                _updateBattleAlignmentCache.Remove(message);
                return message;
            }

            return new UpdateBattleAlignmentMessage();
        }

        public static QueryBattleAlignmentMessage GenerateQueryBattleAlignmentMsg()
        {
            if (_queryBattleAlignmentCache.Count > 0)
            {
                var message = _queryBattleAlignmentCache[0];
                _queryBattleAlignmentCache.Remove(message);
                return message;
            }

            return new QueryBattleAlignmentMessage();
        }

        public static SetEnemyUnitsMessage GenerateSetEnemyUnitsMsg()
        {
            if (_setEnemyUnitsCache.Count > 0)
            {
                var message = _setEnemyUnitsCache[0];
                _setEnemyUnitsCache.Remove(message);
                return message;
            }

            return new SetEnemyUnitsMessage();
        }

        public static SetUnitBattleStateMessage GenerateSetUnitBattleStateMsg()
        {
            if (_setUnitBattleStateCache.Count > 0)
            {
                var message = _setUnitBattleStateCache[0];
                _setUnitBattleStateCache.Remove(message);
                return message;
            }

            return new SetUnitBattleStateMessage();
        }

        public static UpdateUnitBattleStateMessage GenerateUpdateUnitBattleStateMsg()
        {
            if (_updateUnitBattleStateCache.Count > 0)
            {
                var message = _updateUnitBattleStateCache[0];
                _updateUnitBattleStateCache.Remove(message);
                return message;
            }

            return new UpdateUnitBattleStateMessage();
        }

        public static QueryUnitBattleStateMessage GenerateQueryUnitBattleStateMsg()
        {
            if (_queryUnitBattleStateCache.Count > 0)
            {
                var message = _queryUnitBattleStateCache[0];
                _queryUnitBattleStateCache.Remove(message);
                return message;
            }

            return new QueryUnitBattleStateMessage();
        }

        public static RemoveEnemyUnitMessage GenerateRemoveEnemyUnitMsg()
        {
            if (_removeEnemyUnitCache.Count > 0)
            {
                var message = _removeEnemyUnitCache[0];
                _removeEnemyUnitCache.Remove(message);
                return message;
            }

            return new RemoveEnemyUnitMessage();
        }

        public static SetAbilitiesMessage GenerateSetAbilitiesMsg()
        {
            if (_setAbilitiesCache.Count > 0)
            {
                var message = _setAbilitiesCache[0];
                _setAbilitiesCache.Remove(message);
                return message;
            }

            return new SetAbilitiesMessage();
        }

        public static BattleAbilityCheckMessage GenerateBattleAbilityCheckMsg()
        {
            if (_battleAbilityCheckCache.Count > 0)
            {
                var message = _battleAbilityCheckCache[0];
                _battleAbilityCheckCache.Remove(message);
                return message;
            }

            return new BattleAbilityCheckMessage();
        }

        public static SetAlliesMessage GenerateSetAlliesMsg()
        {
            if (_setAlliesCache.Count > 0)
            {
                var message = _setAlliesCache[0];
                _setAlliesCache.Remove(message);
                return message;
            }

            return new SetAlliesMessage();
        }

        public static RemoveAllyMessage GenerateRemoveAllyMsg()
        {
            if (_removeAllyCache.Count > 0)
            {
                var message = _removeAllyCache[0];
                _removeAllyCache.Remove(message);
                return message;
            }

            return new RemoveAllyMessage();
        }

        public static SetFaceDirectionMessage GenerateSetFaceDirectionMsg()
        {
            if (_setFacingDirectionCache.Count > 0)
            {
                var message = _setFacingDirectionCache[0];
                _setFacingDirectionCache.Remove(message);
                return message;
            }

            return new SetFaceDirectionMessage();
        }

        public static QueryHealthMessage GenerateQueryHealthMsg()
        {
            if (_queryHealthCache.Count > 0)
            {
                var message = _queryHealthCache[0];
                _queryHealthCache.Remove(message);
                return message;
            }

            return new QueryHealthMessage();
        }

        public static HealMessage GenerateHealMsg()
        {
            if (_healCache.Count > 0)
            {
                var message = _healCache[0];
                _healCache.Remove(message);
                return message;
            }

            return new HealMessage();
        }

        public static QueryBenchSlotMessage GenerateQueryBenchSlotMsg()
        {
            if (_queryBenchSlotCache.Count > 0)
            {
                var message = _queryBenchSlotCache[0];
                _queryBenchSlotCache.Remove(message);
                return message;
            }

            return new QueryBenchSlotMessage();
        }

        public static QueryClosestGamePieceBenchSlotMessage GenerateQueryClosestGamePieceBenchSlotMsg()
        {
            if (_queryClosestGamePieceBenchSlotCache.Count > 0)
            {
                var message = _queryClosestGamePieceBenchSlotCache[0];
                _queryClosestGamePieceBenchSlotCache.Remove(message);
                return message;
            }

            return new QueryClosestGamePieceBenchSlotMessage();
        }

        public static SetGamePieceBenchMessage GenerateSetGamePieceBenchMsg()
        {
            if (_setGamePieceBenchCache.Count > 0)
            {
                var message = _setGamePieceBenchCache[0];
                _setGamePieceBenchCache.Remove(message);
                return message;
            }

            return new SetGamePieceBenchMessage();
        }

        public static SetGamePieceMapTileMessage GenerateSetGamePieceMapTileMsg()
        {
            if (_setGamePieceMapTileCache.Count > 0)
            {
                var message = _setGamePieceMapTileCache[0];
                _setGamePieceMapTileCache.Remove(message);
                return message;
            }

            return new SetGamePieceMapTileMessage();
        }

        public static SetGamePieceDataMessage GenerateSetGamePieceDataMsg()
        {
            if (_setGamePieceDataCache.Count > 0)
            {
                var message = _setGamePieceDataCache[0];
                _setGamePieceDataCache.Remove(message);
                return message;
            }

            return new SetGamePieceDataMessage();
        }

        public static QueryBattlePieceMessage GenerateQueryBattlePieceMsg()
        {
            if (_queryBattlePieceCache.Count > 0)
            {
                var message = _queryBattlePieceCache[0];
                _queryBattlePieceCache.Remove(message);
                return message;
            }

            return new QueryBattlePieceMessage();
        }

        public static UpdateBattlePointRequirementMessage GenerateUpdateBattlePointRequirementMsg()
        {
            if (_updateBattlePointRequirementCache.Count > 0)
            {
                var message = _updateBattlePointRequirementCache[0];
                _updateBattlePointRequirementCache.Remove(message);
                return message;
            }

            return new UpdateBattlePointRequirementMessage();
        }

        public static ReportHealMessage GenerateReportHealMsg()
        {
            if (_reportHealCache.Count > 0)
            {
                var message = _reportHealCache[0];
                _reportHealCache.Remove(message);
                return message;
            }

            return new ReportHealMessage();
        }

        public static SetHoveredAbilitySlotControllerMessage GenerateSetHoveredAbilitySlotControllerMsg()
        {
            if (_setHoveredAbilitySlotControllerCache.Count > 0)
            {
                var message = _setHoveredAbilitySlotControllerCache[0];
                _setHoveredAbilitySlotControllerCache.Remove(message);
                return message;
            }

            return new SetHoveredAbilitySlotControllerMessage();
        }

        public static RemoveHoveredAbilitySlotControllerMessage GenerateRemoveHoveredAbilitySlotControllerMsg()
        {
            if (_removeHoveredAbilitySlotControllerCache.Count > 0)
            {
                var message = _removeHoveredAbilitySlotControllerCache[0];
                _removeHoveredAbilitySlotControllerCache.Remove(message);
                return message;
            }

            return new RemoveHoveredAbilitySlotControllerMessage();
        }

        public static QueryAbilitiesMessage GenerateQueryAbilitiesMsg()
        {
            if (_queryAbilitiesCache.Count > 0)
            {
                var message = _queryAbilitiesCache[0];
                _queryAbilitiesCache.Remove(message);
                return message;
            }

            return new QueryAbilitiesMessage();
        }

        public static ChangeAbilitySlotMessage GenerateChangeAbilitySlotMsg()
        {
            if (_changeAbilitySlotCache.Count > 0)
            {
                var message = _changeAbilitySlotCache[0];
                _changeAbilitySlotCache.Remove(message);
                return message;
            }

            return new ChangeAbilitySlotMessage();
        }

        public static LearnAbilityMessage GenerateLearnAbilityMsg()
        {
            if (_learnAbilityCache.Count > 0)
            {
                var message = _learnAbilityCache[0];
                _learnAbilityCache.Remove(message);
                return message;
            }

            return new LearnAbilityMessage();
        }

        public static ForgetAbilityAtSlotMessage GenerateForgetAbilityAtSlotMsg()
        {
            if (_forgetAbilityCache.Count > 0)
            {
                var message = _forgetAbilityCache[0];
                _forgetAbilityCache.Remove(message);
                return message;
            }

            return new ForgetAbilityAtSlotMessage();
        }

        public static UpdateFogVisibilityMessage GenerateUpdateFogVisibilityMsg()
        {
            if (_updateFogVisibilityCache.Count > 0)
            {
                var message = _updateFogVisibilityCache[0];
                _updateFogVisibilityCache.Remove(message);
                return message;
            }

            return new UpdateFogVisibilityMessage();
        }

        public static UseActionBarButtonMessage GenerateActionBarButtonMessage()
        {
            if (_useActionBarButtonCache.Count > 0)
            {
                var message = _useActionBarButtonCache[0];
                _useActionBarButtonCache.Remove(message);
                return message;
            }

            return new UseActionBarButtonMessage();
        }

        public static QueryMinigameAbilitiesMessage GenerateQueryMinigameAbilitiesMsg()
        {
            if (_queryMinigameAbilitiesCache.Count > 0)
            {
                var message = _queryMinigameAbilitiesCache[0];
                _queryMinigameAbilitiesCache.Remove(message);
                return message;
            }

            return new QueryMinigameAbilitiesMessage();
        }

        public static QueryManaMessage GenerateQueryManaMsg()
        {
            if (_queryManaCache.Count > 0)
            {
                var message = _queryManaCache[0];
                _queryManaCache.Remove(message);
                return message;
            }

            return new QueryManaMessage();
        }

        public static SetMinigameEquipmentMessage GenerateSetMinigameEquipmentMsg()
        {
            if (_setMinigameEquipmentCache.Count > 0)
            {
                var message = _setMinigameEquipmentCache[0];
                _setMinigameEquipmentCache.Remove(message);
                return message;
            }

            return new SetMinigameEquipmentMessage();
        }

        public static UpdateUnitCastTimerMessage GenerateUpdateUnitCastTimerMsg()
        {
            if (_updateUnitCastTimerCache.Count > 0)
            {
                var message = _updateUnitCastTimerCache[0];
                _updateUnitCastTimerCache.Remove(message);
                return message;
            }

            return new UpdateUnitCastTimerMessage();
        }

        public static CastAbilityMessage GenerateCastAbilityMsg()
        {
            if (_castAbilityCache.Count > 0)
            {
                var message = _castAbilityCache[0];
                _castAbilityCache.Remove(message);
                return message;
            }

            return new CastAbilityMessage();
        }

        public static ClaimKillMessage GenerateClaimKillMsg()
        {
            if (_claimKillCache.Count > 0)
            {
                var message = _claimKillCache[0];
                _claimKillCache.Remove(message);
                return message;
            }

            return new ClaimKillMessage();
        }

        public static QueryProxyRewardsMessage GenerateQueryProxyRewardsMsg()
        {
            if (_queryProxyRewardsCache.Count > 0)
            {
                var message = _queryProxyRewardsCache[0];
                _queryProxyRewardsCache.Remove(message);
                return message;
            }

            return new QueryProxyRewardsMessage();
        }

        public static UpdateWellbeingMessage GenerateUpdateWellbeingMsg()
        {
            if (_updateWellbeingCache.Count > 0)
            {
                var message = _updateWellbeingCache[0];
                _updateWellbeingCache.Remove(message);
                return message;
            }

            return new UpdateWellbeingMessage();
        }

        public static SetHobblerAiStateMessage GenerateSetHobblerAiStateMsg()
        {
            if (_setHobblerAiStateCache.Count > 0)
            {
                var message = _setHobblerAiStateCache[0];
                _setHobblerAiStateCache.Remove(message);
                return message;
            }

            return new SetHobblerAiStateMessage();
        }

        public static GainSkillExperienceMessage GenerateGainSkillExperienceMsg()
        {
            if (_gainSkillExperienceCache.Count > 0)
            {
                var message = _gainSkillExperienceCache[0];
                _gainSkillExperienceCache.Remove(message);
                return message;
            }

            return new GainSkillExperienceMessage();
        }

        public static SearchForResourceNodeMessage GenerateSearchForResourceNodeMsg()
        {
            if (_searchForResourceNodeCache.Count > 0)
            {
                var message = _searchForResourceNodeCache[0];
                _searchForResourceNodeCache.Remove(message);
                return message;
            }

            return new SearchForResourceNodeMessage();
        }

        public static SkillCheckMessage GenerateSkillCheckMsg()
        {
            if (_skillCheckCache.Count > 0)
            {
                var message = _skillCheckCache[0];
                _skillCheckCache.Remove(message);
                return message;
            }

            return new SkillCheckMessage();
        }

        public static QuerySkillsByPriorityMessage GenerateQuerySkillsByPriorityMsg()
        {
            if (_querySkillByPriorityCache.Count > 0)
            {
                var message = _querySkillByPriorityCache[0];
                _querySkillByPriorityCache.Remove(message);
                return message;
            }

            return new QuerySkillsByPriorityMessage();
        }

        public static ChangeSkillPriorityMessage GenerateChangeSkillPriorityMsg()
        {
            if (_changeSkillPriorityCache.Count > 0)
            {
                var message = _changeSkillPriorityCache[0];
                _changeSkillPriorityCache.Remove(message);
                return message;
            }

            return new ChangeSkillPriorityMessage();
        }

        public static SetEquipmentMessage GenerateSetEquipmentMsg()
        {
            if (_setEquipmentCache.Count > 0)
            {
                var message = _setEquipmentCache[0];
                _setEquipmentCache.Remove(message);
                return message;
            }

            return new SetEquipmentMessage();
        }

        public static SetUnitNameMessage GenerateSetUnitNameMsg()
        {
            if (_setUnitNameCache.Count > 0)
            {
                var message = _setUnitNameCache[0];
                _setUnitNameCache.Remove(message);
                return message;
            }

            return new SetUnitNameMessage();
        }

        public static QueryUnitNameMessage GenerateQueryUnitNameMsg()
        {
            if (_queryUnitNameCache.Count > 0)
            {
                var message = _queryUnitNameCache[0];
                _queryUnitNameCache.Remove(message);
                return message;
            }

            return new QueryUnitNameMessage();
        }

        public static SetHobblerTemplateMessage GenerateSetHobblerTemplateMsg()
        {
            if (_setHobblerTemplateCache.Count > 0)
            {
                var message = _setHobblerTemplateCache[0];
                _setHobblerTemplateCache.Remove(message);
                return message;
            }

            return new SetHobblerTemplateMessage();
        }

        public static SetupHobblerCombatStatsMessage GenerateSetupHobblerCombatStatsMsg()
        {
            if (_setupHobblerCombatStatsCache.Count > 0)
            {
                var message = _setupHobblerCombatStatsCache[0];
                _setupHobblerCombatStatsCache.Remove(message);
                return message;
            }

            return new SetupHobblerCombatStatsMessage();
        }

        public static QueryHobblerIdMessage GenerateQueryHobblerIdMsg()
        {
            if (_queryHobblerIdCache.Count > 0)
            {
                var message = _queryHobblerIdCache[0];
                _queryHobblerIdCache.Remove(message);
                return message;
            }

            return new QueryHobblerIdMessage();
        }

        public static PurchaseHobblerAtSlotMessage GeneratePurchaseHobblerAtSlotMsg()
        {
            if (_purchaseHobblerAtSlotCache.Count > 0)
            {
                var message = _purchaseHobblerAtSlotCache[0];
                _purchaseHobblerAtSlotCache.Remove(message);
                return message;
            }

            return new PurchaseHobblerAtSlotMessage();
        }

        public static QueryHobGeneratorMessage GenerateQueryHobGeneratorMsg()
        {
            if (_queryHobGeneratorCache.Count > 0)
            {
                var message = _queryHobGeneratorCache[0];
                _queryHobGeneratorCache.Remove(message);
                return message;
            }

            return new QueryHobGeneratorMessage();
        }

        public static ShowHobGeneratorWindowMessage GenerateShowHobGeneratorWindowMsg()
        {
            if (_showHobGeneratorWindowCache.Count > 0)
            {
                var message = _showHobGeneratorWindowCache[0];
                _showHobGeneratorWindowCache.Remove(message);
                return message;
            }

            return new ShowHobGeneratorWindowMessage();
        }

        public static CanMoveCheckMessage GenerateCanMoveCheckMsg()
        {
            if (_canMoveCheckCache.Count > 0)
            {
                var message = _canMoveCheckCache[0];
                _canMoveCheckCache.Remove(message);
                return message;
            }

            return new CanMoveCheckMessage();
        }

        public static CanCastCheckMessage GenerateCanCastCheckMsg()
        {
            if (_canCastCheckCache.Count > 0)
            {
                var message = _canCastCheckCache[0];
                _canCastCheckCache.Remove(message);
                return message;
            }

            return new CanCastCheckMessage();
        }

        public static CanActivateTrinketCheckMessage GenerateCanActivateTrinketCheckMsg()
        {
            if (_canActivateTrinketCheckCache.Count > 0)
            {
                var message = _canActivateTrinketCheckCache[0];
                _canActivateTrinketCheckCache.Remove(message);
                return message;
            }

            return new CanActivateTrinketCheckMessage();
        }

        public static StatusEffectCheckMessage GenerateStatusEffectCheckMsg()
        {
            if (_statusEffectCheckCache.Count > 0)
            {
                var message = _statusEffectCheckCache[0];
                _statusEffectCheckCache.Remove(message);
                return message;
            }

            return new StatusEffectCheckMessage();
        }

        public static CanAttackCheckMessage GenerateCanAttackCheckMsg()
        {
            if (_canAttachCheckCache.Count > 0)
            {
                var message = _canAttachCheckCache[0];
                _canAttachCheckCache.Remove(message);
                return message;
            }

            return new CanAttackCheckMessage();
        }

        public static DispelStatusEffectsMessage GenerateDispelStatusEffectsMsg()
        {
            if (_dispelStatusEffectsCache.Count > 0)
            {
                var message = _dispelStatusEffectsCache[0];
                _dispelStatusEffectsCache.Remove(message);
                return message;
            }
            return new DispelStatusEffectsMessage();
        }

        public static SetAdventureUnitStateMessage GenerateSetAdventureUnitStateMsg()
        {
            if (_setAdventureStateCache.Count > 0)
            {
                var message = _setAdventureStateCache[0];
                _setAdventureStateCache.Remove(message);
                return message;
            }

            return new SetAdventureUnitStateMessage();
        }

        public static UpdateAdventureUnitStateMessage GenerateUpdateAdventureUnitStateMsg()
        {
            if (_updateAdventureUnitStateCache.Count > 0)
            {
                var message = _updateAdventureUnitStateCache[0];
                _updateAdventureUnitStateCache.Remove(message);
                return message;
            }

            return new UpdateAdventureUnitStateMessage();
        }

        public static QueryAdventureUnitStateMessage GenerateQueryAdventureUnitStateMsg()
        {
            if (_queryAdventureUnitStateCache.Count > 0)
            {
                var message = _queryAdventureUnitStateCache[0];
                _queryAdventureUnitStateCache.Remove(message);
                return message;
            }

            return new QueryAdventureUnitStateMessage();
        }

        public static SetUnitAnimationStateMessage GenerateSetUnitAnimationStateMsg()
        {
            if (_setUnitAnimationStateCache.Count > 0)
            {
                var message = _setUnitAnimationStateCache[0];
                _setUnitAnimationStateCache.Remove(message);
                return message;
            }

            return new SetUnitAnimationStateMessage();
        }

        public static UpdateFacingDirectionMessage GenerateUpdateFacingDirectionMsg()
        {
            if (_updateFacingDirectionCache.Count > 0)
            {
                var message = _updateFacingDirectionCache[0];
                _updateFacingDirectionCache.Remove(message);
                return message;
            }

            return new UpdateFacingDirectionMessage();
        }

        public static SetUnitSpawnerMessage GenerateSetUnitSpawnerMsg()
        {
            if (_setUnitSpawnerCache.Count > 0)
            {
                var message = _setUnitSpawnerCache[0];
                _setUnitSpawnerCache.Remove(message);
                return message;
            }

            return new SetUnitSpawnerMessage();
        }

        public static QueryAdventureUnitTypeMessage GenerateQueryAdventureUnitTypeMsg()
        {
            if (_queryAdventureUnitTypeCache.Count > 0)
            {
                var message = _queryAdventureUnitTypeCache[0];
                _queryAdventureUnitTypeCache.Remove(message);
                return message;
            }

            return new QueryAdventureUnitTypeMessage();
        }

        public static SetPlayerCheckpointMessage GenerateSetPlayerCheckpointMsg()
        {
            if (_setPlayerCheckpointCache.Count > 0)
            {
                var message = _setPlayerCheckpointCache[0];
                _setPlayerCheckpointCache.Remove(message);
                return message;
            }

            return new SetPlayerCheckpointMessage();
        }

        public static ShowDialogueMessage GenerateShowDialogueMsg()
        {
            if (_showDialogueCache.Count > 0)
            {
                var message = _showDialogueCache[0];
                _showDialogueCache.Remove(message);
                return message;
            }

            return new ShowDialogueMessage();
        }

        public static SetPlayerInteractionObjectMessage GenerateSetPlayerInteractionObjectMsg()
        {
            if (_setPlayerInteractionObjectCache.Count > 0)
            {
                var message = _setPlayerInteractionObjectCache[0];
                _setPlayerInteractionObjectCache.Remove(message);
                return message;
            }

            return new SetPlayerInteractionObjectMessage();
        }

        public static QueryHobblerDataMessage GenerateQueryHobblerDataMsg()
        {
            if (_queryHobblerDataCache.Count > 0)
            {
                var message = _queryHobblerDataCache[0];
                _queryHobblerDataCache.Remove(message);
                return message;
            }

            return new QueryHobblerDataMessage();
        }

        public static QueryHobblerGeneticsMessage GenerateQueryHobblerGeneticsMsg()
        {
            if (_queryHobblerGeneticsCache.Count > 0)
            {
                var message = _queryHobblerGeneticsCache[0];
                _queryHobblerGeneticsCache.Remove(message);
                return message;
            }

            return new QueryHobblerGeneticsMessage();
        }

        public static QueryWellbeingStatsMessage GenerateQueryWellbeingStatsMsg()
        {
            if (_queryWellbeingStatsCache.Count > 0)
            {
                var message = _queryWellbeingStatsCache[0];
                _queryWellbeingStatsCache.Remove(message);
                return message;
            }

            return new QueryWellbeingStatsMessage();
        }

        public static QueryPlayerCheckpointMessage GenerateQueryPlayerCheckpointMsg()
        {
            if (_queryPlayerCheckpointCache.Count > 0)
            {
                var message = _queryPlayerCheckpointCache[0];
                _queryPlayerCheckpointCache.Remove(message);
                return message;
            }

            return new QueryPlayerCheckpointMessage();
        }

        public static QueryTrainerDataMessage GenerateQueryTrainerDataMsg()
        {
            if (_queryTrainerDataCache.Count > 0)
            {
                var message = _queryTrainerDataCache[0];
                _queryTrainerDataCache.Remove(message);
                return message;
            }

            return new QueryTrainerDataMessage();
        }

        public static QueryNodeMessage GenerateQueryNodeMsg()
        {
            if (_queryNodeCache.Count > 0)
            {
                var message = _queryNodeCache[0];
                _queryNodeCache.Remove(message);
                return message;
            }

            return new QueryNodeMessage();
        }

        public static QueryBuildingMessge GenerateQueryBuildingMsg()
        {
            if (_queryBuildingCache.Count > 0)
            {
                var message = _queryBuildingCache[0];
                _queryBuildingCache.Remove(message);
                return message;
            }

            return new QueryBuildingMessge();
        }

        public static SetAbilitiesFromDataMessage GenerateSetAbilitiesFromDataMsg()
        {
            if (_setAbilitiesFromDataCache.Count > 0)
            {
                var message = _setAbilitiesFromDataCache[0];
                _setAbilitiesFromDataCache.Remove(message);
                return message;
            }

            return new SetAbilitiesFromDataMessage();
        }

        public static SetSkillsFromDataMessage GenerateSetSkillsFromDataMsg()
        {
            if (_setSkillsFromDataCache.Count > 0)
            {
                var message = _setSkillsFromDataCache[0];
                _setSkillsFromDataCache.Remove(message);
                return message;
            }

            return new SetSkillsFromDataMessage();
        }

        public static SetEquippableItemsFromDataMessage GenerateSetEquippableItemsFromDataMsg()
        {
            if (_setEquippableItemsFromDataCache.Count > 0)
            {
                var message = _setEquippableItemsFromDataCache[0];
                _setEquippableItemsFromDataCache.Remove(message);
                return message;
            }

            return new SetEquippableItemsFromDataMessage();
        }

        public static UpdateBuildingIdMessage GenerateUpdateBuildingIdMsg()
        {
            if (_updateBuildingIdCache.Count > 0)
            {
                var message = _updateBuildingIdCache[0];
                _updateBuildingIdCache.Remove(message);
                return message;
            }

            return new UpdateBuildingIdMessage();
        }

        public static SetupTrainerMessage GenerateSetupTrainerMsg()
        {
            if (_setupTrainerCache.Count > 0)
            {
                var message = _setupTrainerCache[0];
                _setupTrainerCache.Remove(message);
                return message;
            }

            return new SetupTrainerMessage();
        }

        public static StatusEffectFinishedMessage GenerateStatusEffectFinishedMsg()
        {
            if (_statusEffectFinishedCache.Count > 0)
            {
                var message = _statusEffectFinishedCache[0];
                _statusEffectFinishedCache.Remove(message);
                return message;
            }

            return new StatusEffectFinishedMessage();
        }

        public static ShowFloatingTextMessage GenerateShowFloatingTextMsg()
        {
            if (_showFloatingTextCache.Count > 0)
            {
                var message = _showFloatingTextCache[0];
                _showFloatingTextCache.Remove(message);
                return message;
            }

            return new ShowFloatingTextMessage();
        }

        public static QueryBuildingParamterDataMessage GenerateQueryBuildingParamterDataMsg()
        {
            if (_queryBuildingParameterCache.Count > 0)
            {
                var message = _queryBuildingParameterCache[0];
                _queryBuildingParameterCache.Remove(message);
                return message;
            }

            return new QueryBuildingParamterDataMessage();
        }

        public static QueryGlobalCooldownMessage GenerateQueryGlobalCooldownMsg()
        {
            if (_queryGlobalCooldownCache.Count > 0)
            {
                var message = _queryGlobalCooldownCache[0];
                _queryGlobalCooldownCache.Remove(message);
                return message;
            }

            return new QueryGlobalCooldownMessage();
        }

        public static SetSelectedMazeSettingsControllerMessage GenerateSetSelectedMazeSettingsControllerMsg()
        {
            if (_setSelectedMazeSettingsControllerCache.Count > 0)
            {
                var message = _setSelectedMazeSettingsControllerCache[0];
                _setSelectedMazeSettingsControllerCache.Remove(message);
                return message;
            }

            return new SetSelectedMazeSettingsControllerMessage();
        }

        public static ShowMazeSelectionWindowMessage GenerateShowMazeSelectionWindowMsg()
        {
            if (_showMazeSelectionWindowCache.Count > 0)
            {
                var message = _showMazeSelectionWindowCache[0];
                _showMazeSelectionWindowCache.Remove(message);
                return message;
            }

            return new ShowMazeSelectionWindowMessage();
        }

        public static ApplyManaMessage GenerateApplyManaMsg()
        {
            if (_applyManaCache.Count > 0)
            {
                var message = _applyManaCache[0];
                _applyManaCache.Remove(message);
                return message;
            }

            return new ApplyManaMessage();
        }

        public static SetSpriteAlphaMessage GenerateSetSpriteAlphaMsg()
        {
            if (_setSpriteAlphaCache.Count > 0)
            {
                var message = _setSpriteAlphaCache[0];
                _setSpriteAlphaCache.Remove(message);
                return message;
            }

            return new SetSpriteAlphaMessage();
            
        }

        public static UpdateHobblerExperienceMessage GenerateUpdateHobblerExperienceMsg()
        {
            if (_updateHobblerExperienceCache.Count > 0)
            {
                var message = _updateHobblerExperienceCache[0];
                _updateHobblerExperienceCache.Remove(message);
                return message;
            }

            return new UpdateHobblerExperienceMessage();
        }

        public static SetHobblerExperienceMessage GenerateSetHobblerExperienceMsg()
        {
            if (_setHobblerExperienceCache.Count > 0)
            {
                var message = _setHobblerExperienceCache[0];
                _setHobblerExperienceCache.Remove(message);
                return message;
            }

            return new SetHobblerExperienceMessage();
        }

        public static QueryCastingMessage GenerateQueryCastingMsg()
        {
            if (_queryCastingCache.Count > 0)
            {
                var message = _queryCastingCache[0];
                _queryCastingCache.Remove(message);
                return message;
            }

            return new QueryCastingMessage();
        }

        public static QueryTimerMessage GenerateQueryTimerMsg()
        {
            if (_queryTimerCache.Count > 0)
            {
                var message = _queryTimerCache[0];
                _queryTimerCache.Remove(message);
                return message;
            }

            return new QueryTimerMessage();
        }

        public static RefreshTimerMessage GenerateRefreshTimerMsg()
        {
            if (_refreshTimerCache.Count > 0)
            {
                var message = _refreshTimerCache[0];
                _refreshTimerCache.Remove(message);
                return message;
            }

            return new RefreshTimerMessage();
        }

        public static AbsorbedDamageCheckMessage GenerateAbsorbedDamageCheckMsg()
        {
            if (_absorbedDamageCheckCache.Count > 0)
            {
                var message = _absorbedDamageCheckCache[0];
                _absorbedDamageCheckCache.Remove(message);
                return message;
            }

            return new AbsorbedDamageCheckMessage();
        }

        //TODO: Start Cache

        public static void CacheMessage(AddTraitToUnitMessage msg)
        {
            msg.Trait = null;
            msg.DoAfter = null;
            msg.Sender = null;
            _addTraitToUnitCache.Add(msg);
        }

        public static void CacheMessage(RemoveTraitFromUnitMessage msg)
        {
            msg.Trait = null;
            msg.Sender = null;
            _removeTraitFromUnitCache.Add(msg);
        }

        public static void CacheMessage(RemoveTraitFromUnitByControllerMessage msg)
        {
            msg.Controller = null;
            msg.Sender = null;
            _removeTraitFromUnitByControllerCache.Add(msg);
        }

        public static void CacheMessage(HitboxCheckMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _hitboxCheckCache.Add(msg);
        }

        public static void CacheMessage(EnterCollisionWithObjectMessage msg)
        {
            msg.Object = null;
            msg.Sender = null;
            _enterCollisiongWithObjectCache.Add(msg);
        }

        public static void CacheMessage(ExitCollisionWithObjectMessage msg)
        {
            msg.Object = null;
            msg.Sender = null;
            _exitCollisionWithObjectCache.Add(msg);
        }

        public static void CacheMessage(TraitCheckMessage msg)
        {
            msg.DoAfter = null;
            msg.TraitsToCheck = null;
            msg.Sender = null;
            _traitCheckCache.Add(msg);
        }

        public static void CacheMessage(RegisterCollisionMessage msg)
        {
            msg.Object = null;
            msg.Sender = null;
            _registerCollisionCache.Add(msg);
        }

        public static void CacheMessage(UnregisterCollisionMessage msg)
        {
            msg.Object = null;
            msg.Sender = null;
            _unregisterCollisionCache.Add(msg);
        }

        public static void CacheMessage(SetDirectionMessage msg)
        {
            msg.Direction = Vector2.zero;
            msg.Sender = null;
            _setDirectionCache.Add(msg);
        }

        public static void CacheMessage(UpdateDirectionMessage msg)
        {
            msg.Direction = Vector2.zero;
            msg.Sender = null;
            _updateDirectionCache.Add(msg);
        }

        public static void CacheMessage(QueryDirectionMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryDirectionCache.Add(msg);
        }

        public static void CacheMessage(SetMonsterStateMessage msg)
        {
            msg.State = MonsterState.Idle;
            msg.Sender = null;
            _setMonsterStateCache.Add(msg);
        }

        public static void CacheMessage(UpdateMonsterStateMessage msg)
        {
            msg.State = MonsterState.Idle;
            msg.Sender = null;
            _updateMonsterCache.Add(msg);
        }

        public static void CacheMessage(QueryMonsterStateMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryMonsterCache.Add(msg);
        }

        public static void CacheMessage(SetPathMessage msg)
        {
            msg.Path = null;
            msg.DoAfter = null;
            msg.Sender = null;
            _setPathCache.Add(msg);
        }

        public static void CacheMessage(SetMapTileMessage msg)
        {
            msg.Tile = null;
            msg.Sender = null;
            _setMapTileCache.Add(msg);
        }

        public static void CacheMessage(UpdateMapTileMessage msg)
        {
            msg.Tile = null;
            msg.Sender = null;
            _updateMapTileCache.Add(msg);
        }

        public static void CacheMessage(QueryMapTileMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryMapTileCache.Add(msg);
        }

        public static void CacheMessage(UpdateHappinessMessage msg)
        {
            msg.Happiness = 0;
            msg.Sender = null;
            _updateHappinessCache.Add(msg);
        }

        public static void CacheMessage(QueryHappinessMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryHappinessCache.Add(msg);
        }

        public static void CacheMessage(ApplyWellbeingStatsMessage msg)
        {
            msg.Stats = new WellbeingStats();
            msg.Sender = null;
            _applyWellbeingStatsCache.Add(msg);
        }

        public static void CacheMessage(RemoveHoveredUnitMessage msg)
        {
            msg.Unit = null;
            msg.Sender = null;
            _removeHoveredUnitCache.Add(msg);
        }

        public static void CacheMessage(RemoveSelectedUnitMessage msg)
        {
            msg.Unit = null;
            msg.Sender = null;
            _removeSelectedUnitCache.Add(msg);
        }

        public static void CacheMessage(GatherMessage msg)
        {
            msg.DoAfter = null;
            msg.Node = null;
            msg.NodeType = WorldNodeType.Food;
            msg.Ticks = 0;
            msg.Invisible = false;
            msg.Sender = null;
            _gatherCache.Add(msg);
        }

        public static void CacheMessage(StopGatheringMessage msg)
        {
            msg.Node = null;
            msg.Sender = null;
            _stopGatheringCache.Add(msg);
        }

        public static void CacheMessage(SearchForNodeMessage msg)
        {
            msg.Type = WorldNodeType.Bed;
            msg.Sender = null;
            _searchForNodeCache.Add(msg);
        }

        public static void CacheMessage(QueryCommandsMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryCommandsCache.Add(msg);
        }

        public static void CacheMessage(InteractMessage msg)
        {
            msg.Owner = null;
            msg.Sender = null;
            _interactCache.Add(msg);
        }

        public static void CacheMessage(DoBumpMessage msg)
        {
            msg.Direction = Vector2.zero;
            msg.OnBump = null;
            msg.DoAfter = null;
            msg.Sender = null;
            _doBumpCache.Add(msg);
        }

        public static void CacheMessage(SetSpriteVisibilityMessage msg)
        {
            msg.Visible = false;
            msg.Sender = null;
            _setSpriteVisibilityCache.Add(msg);
        }

        public static void CacheMessage(SetMinigameUnitStateMessage msg)
        {
            msg.State = MinigameUnitState.Idle;
            msg.Sender = null;
            _setMinigameUnitStateCache.Add(msg);
        }

        public static void CacheMessage(UpdateMinigameUnitStateMessage msg)
        {
            msg.State = MinigameUnitState.Idle;
            msg.Sender = null;
            _updateMinigameUnitStateCache.Add(msg);
        }

        public static void CacheMessage(QueryMinigameUnitStateMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryMinigameUnitStateCache.Add(msg);
        }

        public static void CacheMessage(UpdatePositionMessage msg)
        {
            msg.Position = Vector2.zero;
            msg.Sender = null;
            _updatePositionCache.Add(msg);
        }

        public static void CacheMessage(MinigameInteractMessage msg)
        {
            msg.Owner = null;
            msg.Direction = Vector2.zero;
            msg.Sender = null;
            _minigameInteractCache.Add(msg);
        }

        public static void CacheMessage(ObstacleMessage msg)
        {
            msg.Obstacle = null;
            msg.Direction = Vector2.zero;
            msg.Sender = null;
            _obstacleCache.Add(msg);
        }

        public static void CacheMessage(MinigameInteractCheckMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _minigameInteractCheckCache.Add(msg);
        }

        public static void CacheMessage(EndMinigameMessage msg)
        {
            msg.Result = MinigameResult.Victory;
            msg.Sender = null;
            _endMinigameCache.Add(msg);
        }

        public static void CacheMessage(StartMinigameMessage msg)
        {
            msg.Owner = null;
            msg.Settings = null;
            msg.Sender = null;
            _startMinigameCache.Add(msg);
        }

        public static void CacheMessage(ShowNewCommandTreeMessage msg)
        {
            msg.Command = null;
            msg.Sender = null;
            _showNewCommandTreeCache.Add(msg);
        }

        public static void CacheMessage(RefillNodeStacksMessage msg)
        {
            msg.Max = 0;
            msg.Sender = null;
            _refillNodeStacksCache.Add(msg);
        }

        public static void CacheMessage(AddExperienceMessage msg)
        {
            msg.Amount = 0;
            msg.Sender = null;
            _addExperienceCache.Add(msg);
        }

        public static void CacheMessage(QueryExperienceMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryExperienceCache.Add(msg);
        }

        public static void CacheMessage(SetExperienceMessage msg)
        {
            msg.Amount = 0;
            msg.Level = 0;
            _setExperienceCache.Add(msg);
        }

        public static void CacheMessage(QueryHobblerWellbeingStatusMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryHobblerWellbeingStatusCache.Add(msg);
        }

        public static void CacheMessage(ApplyCombatStatsMessage msg)
        {
            msg.Stats = CombatStats.Zero;
            msg.Bonus = false;
            msg.Sender = null;
            _applyCombatStatsCache.Add(msg);
        }

        public static void CacheMessage(SetCombatStatsMessage msg)
        {
            msg.Stats = CombatStats.Zero;
            msg.Sender = null;
            _setCombatStatsCache.Add(msg);
        }

        public static void CacheMessage(UpdateCombatStatsMessage msg)
        {
            msg.Base = CombatStats.Zero;
            msg.Bonus = CombatStats.Zero;
            msg.Genetics = GeneticCombatStats.Zero;
            msg.Sender = null;
            _updateCombatStatsCache.Add(msg);
        }

        public static void CacheMessage(QueryCombatStatsMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryCombatStatsCache.Add(msg);
        }

        public static void CacheMessage(DoBasicAttackMessage msg)
        {
            msg.Direction = Vector2Int.zero;
            msg.Target = null;
            msg.Sender = null;
            _doBasicAttackCache.Add(msg);
        }

        public static void CacheMessage(DamageMessage msg)
        {
            msg.Amount = 0;
            msg.Type = DamageType.Physical;
            msg.Owner = null;
            msg.Sender = null;
            _damageCache.Add(msg);
        }

        public static void CacheMessage(QueryBonusDamageMessage msg)
        {
            msg.DoAfter = null;
            msg.Type = DamageType.Physical;
            msg.Sender = null;
            _queryBonusDamageCache.Add(msg);
        }

        public static void CacheMessage(DoBumpOverPixelsPerSecondMessage msg)
        {
            msg.Direction = Vector2.zero;
            msg.DoAfter = null;
            msg.OnBump = null;
            msg.PixelsPerSecond = 0;
            msg.Distance = 0f;
            msg.Sender = null;
            _doBumpOverPixelsPerSecondCache.Add(msg);
        }

        public static void CacheMessage(QueryCombatAlignmentMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryCombatAlignmentCache.Add(msg);
        }

        public static void CacheMessage(SetMinigameAiStateMessage msg)
        {
            msg.State = MinigameAiState.Wander;
            msg.Sender = null;
            _setMinigameAiStateCache.Add(msg);
        }

        public static void CacheMessage(UpdateMinigameAiStateMessage msg)
        {
            msg.State = MinigameAiState.Wander;
            msg.Sender = null;
            _updateMinigameAiStateCache.Add(msg);
        }

        public static void CacheMessage(QueryMinigameAiStateMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryMinigameAiStateCache.Add(msg);
        }

        public static void CacheMessage(SetCombatAlignmentMessage msg)
        {
            msg.Alignment = CombatAlignment.Neutral;
            msg.Sender = null;
            _setCombatAlignmentCache.Add(msg);
        }

        public static void CacheMessage(UpdateCombatAlignmentMessage msg)
        {
            msg.Alignment = CombatAlignment.Neutral;
            msg.Sender = null;
            _updateCombatAlignmentCache.Add(msg);
        }

        public static void CacheMessage(BasicAttackCheckMessage msg)
        {
            msg.Target = null;
            msg.Origin = null;
            msg.TargetTile = null;
            msg.DoAfter = null;
            msg.Distance = 0;
            msg.Sender = null;
            _basicAttackCheckCache.Add(msg);
        }

        public static void CacheMessage(SetProxyHobblerMessage msg)
        {
            msg.Hobbler = null;
            msg.Sender = null;
            _setProxyHobblerCache.Add(msg);
        }

        public static void CacheMessage(ReportDamageMessage msg)
        {
            msg.Amount = 0;
            msg.Owner = null;
            msg.Sender = null;
            _reportDamageCache.Add(msg);
        }

        public static void CacheMessage(SetOwnerMessage msg)
        {
            msg.Owner = null;
            msg.Sender = null;
            _setOwnerCache.Add(msg);
        }

        public static void CacheMessage(QueryOwnerMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryOwnerCache.Add(msg);
        }

        public static void CacheMessage(UpdateOwnerMessage msg)
        {
            msg.Owner = null;
            msg.Sender = null;
            _updateOwnerCache.Add(msg);
        }

        public static void CacheMessage(SetBasicAttackSetupMessage msg)
        {
            msg.Setup = null;
            msg.Sender = null;
            _setBasicAttackSetupCache.Add(msg);
        }

        public static void CacheMessage(ClearBasicAttackSetupMessage msg)
        {
            msg.Setup = null;
            msg.Sender = null;
            _clearBasicAttackSetupCache.Add(msg);
        }

        public static void CacheMessage(QuerySpriteMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _querySpriteCache.Add(msg);
        }

        public static void CacheMessage(QueryBasicAttackSetupMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryBasicAttackSetupCache.Add(msg);
        }

        public static void CacheMessage(SetSpriteMessage msg)
        {
            msg.Sprite = null;
            msg.Sender = null;
            _setSpriteCache.Add(msg);
        }

        public static void CacheMessage(AddItemMessage msg)
        {
            msg.Item = null;
            msg.Stack = 0;
            msg.Sender = null;
            _addItemCache.Add(msg);
        }

        public static void CacheMessage(SetLootTableMessage msg)
        {
            msg.Table = null;
            msg.Sender = null;
            _setLootTableCache.Add(msg);
        }

        public static void CacheMessage(QueryHobblerEquipmentMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryHobblerEquipmentCache.Add(msg);
        }

        public static void CacheMessage(ShowDetailedHobblerInfoMessage msg)
        {
            msg.Unit = null;
            msg.Sender = null;
            _showDetailedHobblerInfoCache.Add(msg);
        }

        public static void CacheMessage(ShowHoverInfoMessage msg)
        {
            msg.Owner = null;
            msg.Icon = null;
            msg.Title = string.Empty;
            msg.Description = string.Empty;
            msg.World = false;
            msg.Position = Vector2.zero;
            msg.ColorMask = Color.white;
            msg.Gold = -1;
            msg.Sender = null;
            _showHoverInfoCache.Add(msg);
        }

        public static void CacheMessage(RemoveHoverInfoMessage msg)
        {
            msg.Owner = null;
            msg.Sender = null;
            _removeHoverInfoCache.Add(msg);
        }

        public static void CacheMessage(SetHoveredStashItemControllerMessage msg)
        {
            msg.Controller = null;
            msg.Sender = null;
            _setHoveredStashItemControllerCache.Add(msg);
        }

        public static void CacheMessage(RemoveHoveredStashItemControllerMessage msg)
        {
            msg.Controller = null;
            msg.Sender = null;
            _removeHoveredStashItemControllerCache.Add(msg);
        }

        public static void CacheMessage(SetHoveredEquippedItemControllerMessage msg)
        {
            msg.Controller = null;
            msg.Sender = null;
            _setHoveredequippedItemControllerCache.Add(msg);
        }

        public static void CacheMessage(RemoveHoveredEquippedItemControllerMessage msg)
        {
            msg.Controller = null;
            msg.Sender = null;
            _removeHoveredEquippedItemControllerCache.Add(msg);
        }

        public static void CacheMessage(EquipItemToSlotMessage msg)
        {
            msg.Index = 0;
            msg.Item = null;
            msg.Sender = null;
            _equipItemToSlotCache.Add(msg);
        }

        public static void CacheMessage(UnequipItemFromSlotMessage msg)
        {
            msg.Index = 0;
            msg.Slot = EquipSlot.Armor;
            msg.ReturnToStash = false;
            msg.Sender = null;
            _unequipItemFromSlotCache.Add(msg);
        }

        public static void CacheMessage(RemoveItemMessage msg)
        {
            msg.Item = null;
            msg.Stack = 0;
            msg.Sender = null;
            _removeItemCache.Add(msg);
        }

        public static void CacheMessage(SetActiveSelectableStateMessage msg)
        {
            msg.Selectable = false;
            msg.Sender = null;
            _setActiveSelectableStateCache.Add(msg);
        }

        public static void CacheMessage(UnregisterFromGatheringNodeMessage msg)
        {
            msg.Unit = null;
            msg.Sender = null;
            _unregisterFromGatheringNodeCache.Add(msg);
        }

        public static void CacheMessage(SetBattleAlignmentMessage msg)
        {
            msg.Alignment = BattleAlignment.Left;
            msg.Sender = null;
            _setBattleAlignmentCache.Add(msg);
        }

        public static void CacheMessage(UpdateBattleAlignmentMessage msg)
        {
            msg.Alignment = BattleAlignment.Left;
            msg.Sender = null;
            _updateBattleAlignmentCache.Add(msg);
        }

        public static void CacheMessage(QueryBattleAlignmentMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryBattleAlignmentCache.Add(msg);
        }

        public static void CacheMessage(SetEnemyUnitsMessage msg)
        {
            msg.Units = null;
            msg.Sender = null;
            _setEnemyUnitsCache.Add(msg);
        }

        public static void CacheMessage(SetUnitBattleStateMessage msg)
        {
            msg.State = UnitBattleState.Active;
            msg.Sender = null;
            _setUnitBattleStateCache.Add(msg);
        }

        public static void CacheMessage(UpdateUnitBattleStateMessage msg)
        {
            msg.State = UnitBattleState.Active;
            msg.Sender = null;
            _updateUnitBattleStateCache.Add(msg);
        }

        public static void CacheMessage(QueryUnitBattleStateMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryUnitBattleStateCache.Add(msg);
        }

        public static void CacheMessage(RemoveEnemyUnitMessage msg)
        {
            msg.Enemy = null;
            msg.Sender = null;
            _removeEnemyUnitCache.Add(msg);
        }

        public static void CacheMessage(SetAbilitiesMessage msg)
        {
            msg.Abilities = null;
            msg.Sender = null;
            _setAbilitiesCache.Add(msg);
        }

        public static void CacheMessage(BattleAbilityCheckMessage msg)
        {
            msg.Distance = 0;
            msg.DoAfter = null;
            msg.Target = null;
            msg.Origin = null;
            msg.Allies = null;
            msg.Sender = null;
            _battleAbilityCheckCache.Add(msg);
        }

        public static void CacheMessage(SetAlliesMessage msg)
        {
            msg.Allies = null;
            msg.Sender = null;
            _setAlliesCache.Add(msg);
        }

        public static void CacheMessage(RemoveAllyMessage msg)
        {
            msg.Ally = null;
            msg.Sender = null;
            _removeAllyCache.Add(msg);
        }

        public static void CacheMessage(SetFaceDirectionMessage msg)
        {
            msg.Direction = Vector2.zero;
            msg.Sender = null;
            _setFacingDirectionCache.Add(msg);
        }

        public static void CacheMessage(QueryHealthMessage msg)
        {
            msg.DoAfter = null;
            msg.DirectValues = false;
            msg.Sender = null;
            _queryHealthCache.Add(msg);
        }

        public static void CacheMessage(HealMessage msg)
        {
            msg.Amount = 0;
            msg.Type = DamageType.Physical;
            msg.Sender = null;
            _healCache.Add(msg);
        }

        public static void CacheMessage(QueryBenchSlotMessage msg)
        {
            msg.Alignment = BattleAlignment.None;
            msg.DoAfter = null;
            msg.Sender = null;
            _queryBenchSlotCache.Add(msg);
        }

        public static void CacheMessage(QueryClosestGamePieceBenchSlotMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryClosestGamePieceBenchSlotCache.Add(msg);
        }

        public static void CacheMessage(SetGamePieceBenchMessage msg)
        {
            msg.Bench = null;
            msg.Sender = null;
            _setGamePieceBenchCache.Add(msg);
        }

        public static void CacheMessage(SetGamePieceMapTileMessage msg)
        {
            msg.Tile = null;
            msg.Sender = null;
            _setGamePieceMapTileCache.Add(msg);
        }

        public static void CacheMessage(SetGamePieceDataMessage msg)
        {
            msg.Alignment = BattleAlignment.None;
            msg.Data = null;
            msg.Sender = null;
            _setGamePieceDataCache.Add(msg);
        }

        public static void CacheMessage(QueryBattlePieceMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryBattlePieceCache.Add(msg);
        }

        public static void CacheMessage(UpdateBattlePointRequirementMessage msg)
        {
            msg.Requirement = 0;
            msg.Rounds = 0;
            msg.Sender = null;
            _updateBattlePointRequirementCache.Add(msg);
        }

        public static void CacheMessage(ReportHealMessage msg)
        {
            msg.Amount = 0;
            msg.Owner = null;
            msg.Sender = null;
            _reportHealCache.Add(msg);
        }

        public static void CacheMessage(SetHoveredAbilitySlotControllerMessage msg)
        {
            msg.Controller = null;
            msg.Sender = null;
            _setHoveredAbilitySlotControllerCache.Add(msg);
        }

        public static void CacheMessage(RemoveHoveredAbilitySlotControllerMessage msg)
        {
            msg.Controller = null;
            msg.Sender = null;
            _removeHoveredAbilitySlotControllerCache.Add(msg);
        }

        public static void CacheMessage(QueryAbilitiesMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryAbilitiesCache.Add(msg);
        }

        public static void CacheMessage(ChangeAbilitySlotMessage msg)
        {
            msg.CurrentSlot = 0;
            msg.NewSlot = 0;
            msg.Sender = null;
            _changeAbilitySlotCache.Add(msg);
        }

        public static void CacheMessage(LearnAbilityMessage msg)
        {
            msg.Ability = null;
            msg.Slot = 0;
            msg.Sender = null;
            _learnAbilityCache.Add(msg);
        }

        public static void CacheMessage(ForgetAbilityAtSlotMessage msg)
        {
            msg.Slot = 0;
            msg.Sender = null;
            _forgetAbilityCache.Add(msg);
        }

        public static void CacheMessage(UpdateFogVisibilityMessage msg)
        {
            msg.Visible = false;
            msg.Sender = null;
            _updateFogVisibilityCache.Add(msg);
        }

        public static void CacheMessage(UseActionBarButtonMessage msg)
        {
            msg.Controller = null;
            msg.Sender = null;
            _useActionBarButtonCache.Add(msg);
        }

        public static void CacheMessage(QueryMinigameAbilitiesMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryMinigameAbilitiesCache.Add(msg);
        }

        public static void CacheMessage(QueryManaMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryManaCache.Add(msg);
        }

        public static void CacheMessage(SetMinigameEquipmentMessage msg)
        {
            msg.Equipment = null;
            msg.Sender = null;
            _setMinigameEquipmentCache.Add(msg);
        }

        public static void CacheMessage(CastAbilityMessage msg)
        {
            msg.Ability = null;
            msg.Target = null;
            msg.Sender = null;
            _castAbilityCache.Add(msg);
        }

        public static void CacheMessage(UpdateUnitCastTimerMessage msg)
        {
            msg.CastTimer = null;
            msg.Name = string.Empty;
            msg.Icon = null;
            msg.Sender = null;
            _updateUnitCastTimerCache.Add(msg);
        }

        public static void CacheMessage(ClaimKillMessage msg)
        {
            msg.Kill = null;
            msg.Sender = null;
            _claimKillCache.Add(msg);
        }

        public static void CacheMessage(QueryProxyRewardsMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryProxyRewardsCache.Add(msg);
        }

        public static void CacheMessage(UpdateWellbeingMessage msg)
        {
            msg.Stats = WellbeingStats.Zero;
            msg.Min = WellbeingStats.Zero;
            msg.Max = WellbeingStats.Zero;
            msg.Sender = null;
            _updateWellbeingCache.Add(msg);
        }

        public static void CacheMessage(SetHobblerAiStateMessage msg)
        {
            msg.State = HobblerAiState.Auto;
            msg.Sender = null;
            _setHobblerAiStateCache.Add(msg);
        }

        public static void CacheMessage(GainSkillExperienceMessage msg)
        {
            msg.Experience = 0;
            msg.Skill = null;
            _gainSkillExperienceCache.Add(msg);
        }

        public static void CacheMessage(SearchForResourceNodeMessage msg)
        {
            msg.Item = null;
            msg.DoAfter = null;
            msg.Sender = null;
            _searchForResourceNodeCache.Add(msg);
        }

        public static void CacheMessage(SkillCheckMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _skillCheckCache.Add(msg);
        }

        public static void CacheMessage(QuerySkillsByPriorityMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _querySkillByPriorityCache.Add(msg);
        }

        public static void CacheMessage(ChangeSkillPriorityMessage msg)
        {
            msg.Priority = 0;
            msg.Origin = 0;
            msg.Skill = null;
            msg.Sender = null;
            _changeSkillPriorityCache.Add(msg);
        }

        public static void CacheMessage(SetEquipmentMessage msg)
        {
            msg.Items = null;
            msg.Sender = null;
            _setEquipmentCache.Add(msg);
        }

        public static void CacheMessage(SetUnitNameMessage msg)
        {
            msg.Name = null;
            msg.Sender = null;
            _setUnitNameCache.Add(msg);
        }

        public static void CacheMessage(QueryUnitNameMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryUnitNameCache.Add(msg);
        }

        public static void CacheMessage(SetHobblerTemplateMessage msg)
        {
            msg.Template = null;
            msg.Id = null;
            msg.Sender = null;
            _setHobblerTemplateCache.Add(msg);
        }

        public static void CacheMessage(SetupHobblerCombatStatsMessage msg)
        {
            msg.Stats = CombatStats.Zero;
            msg.Genetics = GeneticCombatStats.Zero;
            msg.Accumulated = GeneticCombatStats.Zero;
            msg.Sender = null;
            _setupHobblerCombatStatsCache.Add(msg);
        }

        public static void CacheMessage(QueryHobblerIdMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryHobblerIdCache.Add(msg);
        }

        public static void CacheMessage(PurchaseHobblerAtSlotMessage msg)
        {
            msg.Slot = 0;
            msg.Sender = null;
            _purchaseHobblerAtSlotCache.Add(msg);
        }

        public static void CacheMessage(QueryHobGeneratorMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryHobGeneratorCache.Add(msg);
        }

        public static void CacheMessage(ShowHobGeneratorWindowMessage msg)
        {
            msg.Owner = null;
            msg.Sender = null;
            _showHobGeneratorWindowCache.Add(msg);
        }

        public static void CacheMessage(CanMoveCheckMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _canMoveCheckCache.Add(msg);
        }

        public static void CacheMessage(CanCastCheckMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _canCastCheckCache.Add(msg);
        }

        public static void CacheMessage(CanActivateTrinketCheckMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _canActivateTrinketCheckCache.Add(msg);
        }

        public static void CacheMessage(StatusEffectCheckMessage msg)
        {
            msg.DoAfter = null;
            msg.Type = StatusEffectType.Stun;
            msg.Sender = null;
            _statusEffectCheckCache.Add(msg);
        }

        public static void CacheMessage(CanAttackCheckMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _canAttachCheckCache.Add(msg);
        }

        public static void CacheMessage(DispelStatusEffectsMessage msg)
        {
            msg.Types = null;
            msg.Sender = null;
            _dispelStatusEffectsCache.Add(msg);
        }

        public static void CacheMessage(SetAdventureUnitStateMessage msg)
        {
            msg.State = AdventureUnitState.Idle;
            msg.Sender = null;
            _setAdventureStateCache.Add(msg);
        }

        public static void CacheMessage(UpdateAdventureUnitStateMessage msg)
        {
            msg.State = AdventureUnitState.Idle;
            msg.Sender = null;
            _updateAdventureUnitStateCache.Add(msg);
        }

        public static void CacheMessage(QueryAdventureUnitStateMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryAdventureUnitStateCache.Add(msg);
        }

        public static void CacheMessage(SetUnitAnimationStateMessage msg)
        {
            msg.State = UnitAnimationState.Idle;
            msg.Sender = null;
            _setUnitAnimationStateCache.Add(msg);
        }

        public static void CacheMessage(UpdateFacingDirectionMessage msg)
        {
            msg.Direction = Vector2Int.zero;
            msg.Sender = null;
            _updateFacingDirectionCache.Add(msg);
        }

        public static void CacheMessage(SetUnitSpawnerMessage msg)
        {
            msg.Spawner = null;
            msg.Sender = null;
            _setUnitSpawnerCache.Add(msg);
        }

        public static void CacheMessage(QueryAdventureUnitTypeMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryAdventureUnitTypeCache.Add(msg);
        }

        public static void CacheMessage(SetPlayerCheckpointMessage msg)
        {
            msg.Position = Vector2Int.zero;
            msg.Map = null;
            msg.Sender = null;
            _setPlayerCheckpointCache.Add(msg);
        }

        public static void CacheMessage(ShowDialogueMessage msg)
        {
            msg.Dialogue = null;
            msg.Owner = null;
            msg.Sender = null;
            _showDialogueCache.Add(msg);
        }

        public static void CacheMessage(SetPlayerInteractionObjectMessage msg)
        {
            msg.Interact = null;
            msg.Sender = null;
            _setPlayerInteractionObjectCache.Add(msg);
        }

        public static void CacheMessage(QueryHobblerDataMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryHobblerDataCache.Add(msg);
        }

        public static void CacheMessage(QueryHobblerGeneticsMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryHobblerGeneticsCache.Add(msg);
        }

        public static void CacheMessage(QueryWellbeingStatsMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryWellbeingStatsCache.Add(msg);
        }

        public static void CacheMessage(QueryPlayerCheckpointMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryPlayerCheckpointCache.Add(msg);
        }

        public static void CacheMessage(QueryTrainerDataMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryTrainerDataCache.Add(msg);
        }

        public static void CacheMessage(QueryNodeMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryNodeCache.Add(msg);
        }

        public static void CacheMessage(QueryBuildingMessge msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryBuildingCache.Add(msg);
        }

        public static void CacheMessage(SetAbilitiesFromDataMessage msg)
        {
            msg.Abilities = null;
            msg.Sender = null;
            _setAbilitiesFromDataCache.Add(msg);
        }

        public static void CacheMessage(SetSkillsFromDataMessage msg)
        {
            msg.Skills = null;
            msg.Sender = null;
            _setSkillsFromDataCache.Add(msg);
        }

        public static void CacheMessage(SetEquippableItemsFromDataMessage msg)
        {
            msg.Items = null;
            msg.Sender = null;
            _setEquippableItemsFromDataCache.Add(msg);
        }

        public static void CacheMessage(UpdateBuildingIdMessage msg)
        {
            msg.Id = null;
            msg.Sender = null;
            _updateBuildingIdCache.Add(msg);
        }

        public static void CacheMessage(SetupTrainerMessage msg)
        {
            msg.Id = string.Empty;
            msg.Encounter = null;
            msg.PreEncounterDialogue = null;
            msg.DefeatedDialogue = null;
            msg.Sender = null;
            _setupTrainerCache.Add(msg);
        }

        public static void CacheMessage(StatusEffectFinishedMessage msg)
        {
            msg.Type = StatusEffectType.Stun;
            msg.Sender = null;
            _statusEffectFinishedCache.Add(msg);
        }

        public static void CacheMessage(ShowFloatingTextMessage msg)
        {
            msg.Color = Color.white;
            msg.Text = null;
            msg.Sender = null;
            _showFloatingTextCache.Add(msg);

        }

        public static void CacheMessage(QueryGlobalCooldownMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryGlobalCooldownCache.Add(msg);
        }

        public static void CacheMessage(QueryBuildingParamterDataMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryBuildingParameterCache.Add(msg);
        }

        public static void CacheMessage(SetSelectedMazeSettingsControllerMessage msg)
        {
            msg.Controller = null;
            msg.Sender = null;
            _setSelectedMazeSettingsControllerCache.Add(msg);
        }

        public static void CacheMessage(ShowMazeSelectionWindowMessage msg)
        {
            msg.Hobbler = null;
            msg.Sender = null;
            _showMazeSelectionWindowCache.Add(msg);
        }

        public static void CacheMessage(ApplyManaMessage msg)
        {
            msg.Amount = 0;
            msg.Sender = null;
            _applyManaCache.Add(msg);
        }

        public static void CacheMessage(SetSpriteAlphaMessage msg)
        {
            msg.Alpha = 0f;
            msg.Sender = null;
            _setSpriteAlphaCache.Add(msg);
        }

        public static void CacheMessage(UpdateHobblerExperienceMessage msg)
        {
            msg.Experience = 0;
            msg.Level = 0;
            msg.ExperienceToNextLevel = 0;
            msg.Sender = null;
            _updateHobblerExperienceCache.Add(msg);
        }

        public static void CacheMessage(SetHobblerExperienceMessage msg)
        {
            msg.Experience = 0;
            msg.Level = 0;
            msg.Sender = null;
            _setHobblerExperienceCache.Add(msg);
        }

        public static void CacheMessage(QueryCastingMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryCastingCache.Add(msg);
        }

        public static void CacheMessage(QueryTimerMessage msg)
        {
            msg.DoAfter = null;
            msg.Trait = null;
            msg.Sender = null;
            _queryTimerCache.Add(msg);
        }

        public static void CacheMessage(RefreshTimerMessage msg)
        {
            msg.Trait = null;
            msg.Sender = null;
            _refreshTimerCache.Add(msg);
        }

        public static void CacheMessage(AbsorbedDamageCheckMessage msg)
        {
            msg.Type = DamageType.Physical;
            msg.Instance?.Destroy();
            msg.Instance = null;
            msg.Sender = null;
            _absorbedDamageCheckCache.Add(msg);
        }
    }
}
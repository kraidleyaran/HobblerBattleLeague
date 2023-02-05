using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.SaveData.Buildings;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.SaveData;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using Assets.Resources.Ancible_Tools.Scripts.System.UnitCommands;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Hob Generator Trait", menuName = "Ancible Tools/Traits/Buildings/Hob Generator")]
    public class HobGeneratorTrait : Trait
    {
        [SerializeField] private HobblerTemplate[] _hobblers = new HobblerTemplate[0];
        [SerializeField] private int _maxRoll = 5;
        [SerializeField] private UnitCommand _buyCommand = null;
        [SerializeField] private int _reRollCost = 1;
        [SerializeField] private int _generationWorldTicks = 1;
        [SerializeField] private Vector2Int _spawnTileOffset = Vector2Int.down;

        private Dictionary<int, HobblerTemplate> _currentHobblers = new Dictionary<int, HobblerTemplate>();

        private TickTimer _generationTimer = null;
        private MapTile _spawnTile = null;
        private CommandInstance _buyInstance = null;
        private HobGateParameterData _data = new HobGateParameterData();

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _buyInstance = _buyCommand.GenerateInstance();
            _generationTimer = new TickTimer(_generationWorldTicks, -1, GenerateNewHobblers, null);
            GenerateNewHobblers();
            SubscribeToMessages();
        }

        private void GenerateNewHobblers()
        {
            _currentHobblers.Clear();
            for (var i = 0; i < _maxRoll; i++)
            {
                var hobbler = _hobblers.GetRandom();
                _currentHobblers.Add(i, hobbler);
            }
            
            _generationTimer?.Restart();
            RefreshData();
            _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
        }

        private void SetHobblersFromdata(string[] hobblers)
        {
            _currentHobblers.Clear();
            for (var i = 0; i < hobblers.Length; i++)
            {
                if (string.IsNullOrEmpty(hobblers[i]))
                {
                    _currentHobblers.Add(i, null);
                }
                else
                {
                    var template = WorldHobblerManager.GetTemplateByName(hobblers[i]);
                    if (template)
                    {
                        _currentHobblers.Add(i, template);
                    }
                }

            }
        }

        private void RefreshData()
        {
            var keys = _currentHobblers.Keys.ToArray();
            var hobs = new List<string>();
            foreach (var key in keys)
            {
                hobs.Add(_currentHobblers[key] ? _currentHobblers[key].name : string.Empty);
            }
            _data.Hobs = hobs.ToArray();
            _data.RemainingTicks = _generationTimer.RemainingTicks;
            _data.MaxTicks = _generationTimer.TicksPerCycle;
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<RerollHobblersMessage>(RerollHobblers, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<PurchaseHobblerAtSlotMessage>(PurchaseHobblerAtSlot, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryCommandsMessage>(QueryCommands, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryHobGeneratorMessage>(QueryHobGenerator, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryBuildingParamterDataMessage>(QueryBuidingParmeterData, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateBuildingIdMessage>(UpdateBuildingId, _instanceId);
        }

        private void RerollHobblers(RerollHobblersMessage msg)
        {
            WorldStashController.RemoveGold(_reRollCost);
            GenerateNewHobblers();
        }

        private void PurchaseHobblerAtSlot(PurchaseHobblerAtSlotMessage msg)
        {
            if (WorldHobblerManager.AvailablePopulation && _currentHobblers.TryGetValue(msg.Slot, out var template) && template)
            {
                var unitController = FactoryController.HOBBLER_TEMPLATE.GenerateUnit(WorldHobblerManager.Transform, _spawnTile.World);
                var setHobblerTemplateMsg = MessageFactory.GenerateSetHobblerTemplateMsg();
                setHobblerTemplateMsg.Template = template;
                setHobblerTemplateMsg.Id = string.Empty;
                _controller.gameObject.SendMessageTo(setHobblerTemplateMsg, unitController.gameObject);
                MessageFactory.CacheMessage(setHobblerTemplateMsg);

                var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
                setMapTileMsg.Tile = _spawnTile;
                _controller.gameObject.SendMessageTo(setMapTileMsg, unitController.gameObject);
                MessageFactory.CacheMessage(setMapTileMsg);

                _currentHobblers[msg.Slot] = null;
                WorldHobblerManager.RegisterHobbler(unitController.gameObject);

                _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
            }
        }

        private void UpdateMapTile(UpdateMapTileMessage msg)
        {
            _spawnTile = WorldController.Pathing.GetTileByPosition(msg.Tile.Position + _spawnTileOffset);
        }

        private void QueryCommands(QueryCommandsMessage msg)
        {
            msg.DoAfter.Invoke(new []{_buyInstance});
        }

        private void QueryHobGenerator(QueryHobGeneratorMessage msg)
        {
            msg.DoAfter.Invoke(_currentHobblers.ToArray(), _generationTimer, _generationWorldTicks);
        }

        private void UpdateBuildingId(UpdateBuildingIdMessage msg)
        {
            var data = PlayerDataController.GetBuildingData(msg.Id);
            if (data != null && data.Parameter is HobGateParameterData gateData)
            {
                SetHobblersFromdata(gateData.Hobs);
                _generationTimer = new TickTimer(gateData.MaxTicks, -1, GenerateNewHobblers, null);
                _generationTimer.SetCurrentTicks(gateData.MaxTicks - gateData.RemainingTicks);
                _data = gateData;
            }
            else if (_data == null)
            {
                RefreshData();
            }
        }

        private void QueryBuidingParmeterData(QueryBuildingParamterDataMessage msg)
        {
            RefreshData();
            msg.DoAfter.Invoke(_data);
        }

        public override void Destroy()
        {
            _generationTimer?.Destroy();
            _generationTimer = null;
            _data?.Dispose();
            _data = null;
            base.Destroy();
        }
    }
}
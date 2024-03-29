﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.Factories;
using Assets.Ancible_Tools.Scripts.System.SaveData;
using Assets.Ancible_Tools.Scripts.System.SaveData.Adventure;
using Assets.Ancible_Tools.Scripts.System.SaveData.Building;
using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Building;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using Assets.Resources.Ancible_Tools.Scripts.System.WorldEvents;
using FileDataLib;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.SaveData
{
    public class PlayerDataController : MonoBehaviour
    {
        private static PlayerDataController _instance = null;

        public static string DefaultPlayerName => _instance._defaultName;

        [SerializeField] private string _defaultName = "Developer";
        [SerializeField] private string _saveDataBaseFolder = string.Empty;
        [SerializeField] private string _playerFolder = string.Empty;
        [SerializeField] private string _hobblerFolder = string.Empty;

        private PlayerData _playerData = null;

        private Dictionary<string, AdventureTrainerData> _trainers = new Dictionary<string, AdventureTrainerData>();
        private Dictionary<string, AdventureDialogueData> _dialogue = new Dictionary<string, AdventureDialogueData>();
        private Dictionary<string, AdventureChestData> _chests = new Dictionary<string, AdventureChestData>();

        private string _playerFolderPath = string.Empty;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _playerFolderPath = $"{Application.persistentDataPath}{Path.DirectorySeparatorChar}{_saveDataBaseFolder}{Path.DirectorySeparatorChar}{_playerFolder}{Path.DirectorySeparatorChar}";

            if (!Directory.Exists(_playerFolderPath))
            {
                Directory.CreateDirectory(_playerFolderPath);
            }

            SubscribeToMessages();
        }

        public static void SetPlayerName(string name)
        {
            if (_instance._playerData == null)
            {
                _instance._playerData = new PlayerData {Name = name};
            }
        }

        public static void SaveData()
        {
            if (_instance._playerData == null)
            {
                _instance._playerData = new PlayerData
                {
                    Name = _instance._defaultName,
                };
            }

            var data = _instance._playerData;
            data.HobblerFolderPath = GeneratePlayerHobblerFolder(data.Name);
            data.Gold = WorldStashController.Gold;
            data.Stash = WorldStashController.GetStashData();
            data.Buildings = WorldBuildingManager.GetData();
            data.Windows = UiWindowManager.GetWindowData();
            data.BattlePositions = BattleLeagueManager.GetSavedBattlePositionData();
            data.MaxPopulation = WorldHobblerManager.PopulationLimit;
            data.MaxRoster = WorldHobblerManager.RosterLimit;
            data.WorldEvents = WorldEventManager.GetData();
            data.WorldEventReceivers = WorldEventManager.GetAllReceiverData();

            var adventureData = new List<AdventureParameterData>();
            adventureData.AddRange(_instance._trainers.Values.ToArray());
            adventureData.AddRange(_instance._dialogue.Values.ToArray());
            data.AdventureData = adventureData.ToArray();

            if (WorldAdventureController.Player)
            {
                var querySpriteMsg = MessageFactory.GenerateQuerySpriteMsg();
                querySpriteMsg.DoAfter = ApplySpriteToData;
                _instance.gameObject.SendMessageTo(querySpriteMsg, WorldAdventureController.Player);
                MessageFactory.CacheMessage(querySpriteMsg);

                data.Map = WorldAdventureController.Current.name;

                var queryMapTileMsg = MessageFactory.GenerateQueryMapTileMsg();
                queryMapTileMsg.DoAfter = ApplyMapTileToData;
                _instance.gameObject.SendMessageTo(queryMapTileMsg, WorldAdventureController.Player);
                MessageFactory.CacheMessage(queryMapTileMsg);

                var queryPlayerCheckpointMsg = MessageFactory.GenerateQueryPlayerCheckpointMsg();
                queryPlayerCheckpointMsg.DoAfter = ApplyCheckpointToData;
                _instance.gameObject.SendMessageTo(queryPlayerCheckpointMsg, WorldAdventureController.Player);
                MessageFactory.CacheMessage(queryPlayerCheckpointMsg);
            }
            else
            {
                data.Map = WorldAdventureController.Current.name;
                data.AdventureCheckpoint = WorldAdventureController.Current.DefaultTile.ToData();   
            }

            var playerFilePath = $"{GeneratePlayerFolder(data.Name)}{data.Name}.{PlayerData.EXTENSION}";
            var playerResult = FileData.SaveData(playerFilePath, data);
            if (!playerResult.Success)
            {
                Debug.LogWarning(playerResult.HasException
                    ? $"Exception while saving player data at path {playerFilePath} - {playerResult.Exception}"
                    : $"Unknown error while trying to save player data at path {playerFilePath}");
            }

            var hobblerData = WorldHobblerManager.GetData();
            var existingFiles = new DirectoryInfo(data.HobblerFolderPath).GetFiles();
            foreach (var file in existingFiles)
            {
                file.Delete();
            }

            foreach (var hobbler in hobblerData)
            {
                var hobblerPath = $"{data.HobblerFolderPath}{hobbler.Id}.{HobblerData.EXTENSION}";
                var result = FileData.SaveData(hobblerPath, hobbler);
                if (!result.Success)
                {
                    Debug.LogWarning(result.HasException
                        ? $"Exception while saving hobbler data at path {hobblerPath} - {result.Exception}"
                        : $"Unknown error while trying to save hobbler data at path {hobblerPath}");
                }
            }
            Debug.Log("Save Finished");
        }

        public static void LoadData(string playerName)
        {
            _instance.gameObject.SendMessage(ClearWorldMessage.INSTANCE);
            var playerFilePath = $"{GeneratePlayerFolder(playerName)}{playerName}.{PlayerData.EXTENSION}";
            var playerLoadResult = FileData.LoadData<PlayerData>(playerFilePath);
            if (playerLoadResult.Success)
            {
                _instance._playerData = playerLoadResult.Data;
                var data = _instance._playerData;
                WorldHobblerManager.SetMaxPopulation(data.MaxPopulation);
                WorldHobblerManager.SetMaxRoster(data.MaxRoster);
                LoadHobblers(data.HobblerFolderPath);
                _instance.ApplyTrainerData(data.AdventureData.Where(t => t is AdventureTrainerData).Select(t => t as AdventureTrainerData).ToArray());
                _instance.ApplyDialoguedata(data.AdventureData.Where(t => t is AdventureDialogueData).Select(t => t as AdventureDialogueData).ToArray());
                WorldStashController.SetStashFromData(data.Stash, data.Gold);
                WorldBuildingManager.SetFromBuildingsData(data.Buildings);
                BattleLeagueManager.SetSavedBattlePositions(data.BattlePositions);
                WorldEventManager.LoadEvents(data.WorldEvents);
                WorldEventManager.LoadReceivers(data.WorldEventReceivers);
                _instance.gameObject.SendMessage(LoadWorldDataMessage.INSTANCE);

                var map = WorldAdventureController.GetAdventuerMapByName(data.Map);
                if (map)
                {
                    WorldAdventureController.Setup(map, data.Position.ToVector(), false);
                    var spriteTrait = TraitFactory.GetSpriteByName(data.Sprite);
                    if (spriteTrait)
                    {
                        var setSpriteMsg = MessageFactory.GenerateSetSpriteMsg();
                        setSpriteMsg.Sprite = spriteTrait;
                        _instance.gameObject.SendMessageTo(setSpriteMsg, WorldAdventureController.Player);
                        MessageFactory.CacheMessage(setSpriteMsg);
                    }

                    if (!string.IsNullOrEmpty(data.CheckpointMap))
                    {
                        var checkpointMap = WorldAdventureController.GetAdventuerMapByName(data.CheckpointMap);
                        if (checkpointMap)
                        {
                            var setPlayerCheckpointMsg = MessageFactory.GenerateSetPlayerCheckpointMsg();
                            setPlayerCheckpointMsg.Map = checkpointMap;
                            setPlayerCheckpointMsg.Position = data.AdventureCheckpoint.ToVector();
                            _instance.gameObject.SendMessageTo(setPlayerCheckpointMsg, WorldAdventureController.Player);
                            MessageFactory.CacheMessage(setPlayerCheckpointMsg);
                        }
                    }
                }

                UiWindowManager.SetWindowData(data.Windows);
            }
            else
            {
                Debug.LogWarning(playerLoadResult.HasException
                    ? $"Exception while loading player data at path {playerFilePath} - {playerLoadResult.Exception}"
                    : $"Unknown error while loading player data at path {playerFilePath}");
            }
        }

        public static AdventureTrainerData GetTrainerDataById(string id)
        {
            return _instance._trainers.TryGetValue(id, out var data) ? data : null;
        }

        public static void SetTrainerData(string id)
        {
            if (!_instance._trainers.TryGetValue(id, out var data))
            {
                data = new AdventureTrainerData {Id = id};
                _instance._trainers.Add(data.Id, data);
            }
        }

        public static BuildingData GetBuildingData(string id)
        {
            return _instance._playerData?.Buildings.FirstOrDefault(t => t.Id == id);
        }

        public static AdventureDialogueData GetDialogueDataById(string id)
        {
            return _instance._dialogue.TryGetValue(id, out var data) ? data : null;

            //return _instance._playerData?.Dialogue.FirstOrDefault(d => d.Id == id);
        }

        public static void SetDialogueData(string id, string dialouge)
        {
            if (!_instance._dialogue.TryGetValue(id, out var data))
            {
                data = new AdventureDialogueData {Id = id};
                _instance._dialogue.Add(id, data);
            }
            data.Dialogue = dialouge;
        }

        public static AdventureChestData GetChestDataById(string id)
        {
            return _instance._chests.TryGetValue(id, out var data) ? data : null;
        }

        public static void SetChestData(string id)
        {
            if (!_instance._chests.ContainsKey(id))
            {
                _instance._chests.Add(id, new AdventureChestData{Id = id});
            }
        }

        private static void ApplySpriteToData(SpriteTrait trait)
        {
            _instance._playerData.Sprite = trait.name;
        }

        private static void ApplyMapTileToData(MapTile tile)
        {
            _instance._playerData.Position = tile.Position.ToData();
        }

        private static void ApplyCheckpointToData(string map, Vector2Int pos)
        {
            _instance._playerData.AdventureCheckpoint = pos.ToData();
            _instance._playerData.CheckpointMap = map;
        }

        private static string GeneratePlayerFolder(string playerName)
        {
            var playerFolderPath = $"{_instance._playerFolderPath}{playerName}{Path.DirectorySeparatorChar}";
            if (!Directory.Exists(playerFolderPath))
            {
                Directory.CreateDirectory(playerFolderPath);
            }
            return playerFolderPath;
        }

        private static string GeneratePlayerHobblerFolder(string playerName)
        {
            var playerFolder = GeneratePlayerFolder(playerName);
            var hobblerFolder = $"{playerFolder}{_instance._hobblerFolder}";
            if (!Directory.Exists(hobblerFolder))
            {
                Directory.CreateDirectory(hobblerFolder);
            }
            return $"{hobblerFolder}{Path.DirectorySeparatorChar}";
        }

        private static void LoadHobblers(string folderPath)
        {
            var hobblers = Directory.GetFiles(folderPath, $"*.{HobblerData.EXTENSION}");
            var hobblerData = new List<HobblerData>();
            foreach (var file in hobblers)
            {
                var loadResult = FileData.LoadData<HobblerData>(file);
                if (loadResult.Success)
                {
                    hobblerData.Add(loadResult.Data);
                }
                else
                {
                    Debug.LogWarning(loadResult.HasException
                        ? $"Exception while loading hobbler data from path {file} - {loadResult.Exception}"
                        : $"Unknown error while loaindg hobbler data from path {file}");
                }
            }
            WorldHobblerManager.LoadHobblersFromData(hobblerData.ToArray());

        }

        private void ApplyTrainerData(AdventureTrainerData[] trainers)
        {
            _trainers.Clear();
            foreach (var data in trainers)
            {
                _trainers.Add(data.Id, data);
            }
        }

        private void ApplyDialoguedata(AdventureDialogueData[] dialogue)
        {
            _dialogue.Clear();
            foreach (var data in dialogue)
            {
                _dialogue.Add(data.Id, data);
            }
        }

        private void ApplyChestData(AdventureChestData[] chests)
        {
            _chests.Clear();
            foreach (var chest in chests)
            {
                _chests.Add(chest.Id, chest);
            }
        }


        private void SubscribeToMessages()
        {
            gameObject.Subscribe<ClearWorldMessage>(ClearWorld);
        }

        private void ClearWorld(ClearWorldMessage msg)
        {
            _playerData?.Dispose();
            _playerData = null;
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}
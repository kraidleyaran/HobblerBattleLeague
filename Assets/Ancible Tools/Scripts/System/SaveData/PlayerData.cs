using System;
using Assets.Ancible_Tools.Scripts.System.SaveData;
using Assets.Ancible_Tools.Scripts.System.SaveData.Adventure;
using Assets.Ancible_Tools.Scripts.System.SaveData.Building;

namespace Assets.Resources.Ancible_Tools.Scripts.System.SaveData
{
    [Serializable]
    public class PlayerData : IDisposable
    {
        public static string EXTENSION = "hpd";

        public string Name = string.Empty;
        public string Map = string.Empty;
        public string Sprite = string.Empty;
        public string HobblerFolderPath = string.Empty;
        public Vector2IntData AdventureCheckpoint;
        public string CheckpointMap;
        public Vector2IntData Position;
        public int Gold;
        public ItemStackData[] Stash = new ItemStackData[0];
        //public AdventureTrainerData[] Trainers = new AdventureTrainerData[0];
        public BuildingData[] Buildings = new BuildingData[0];
        public WindowData[] Windows = new WindowData[0];
        public BattlePositionData[] BattlePositions = new BattlePositionData[0];
        //public AdventureDialogueData[] Dialogue = new AdventureDialogueData[0];
        public int MaxPopulation = 0;
        public int MaxRoster = 0;
        public WorldEventData[] WorldEvents = new WorldEventData[0];
        public WorldEventReceiverData[] WorldEventReceivers = new WorldEventReceiverData[0];
        public AdventureParameterData[] AdventureData = new AdventureParameterData[0];

        public void Dispose()
        {
            Name = null;
            Map = null;
            Sprite = null;
            HobblerFolderPath = null;
            AdventureCheckpoint = Vector2IntData.Zero;
            Position = Vector2IntData.Zero;
            Gold = 0;
            Stash = null;
            Buildings = null;
            Windows = null;
            AdventureData = null;
        }
    }
}
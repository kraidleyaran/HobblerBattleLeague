﻿using System;
using Assets.Ancible_Tools.Scripts.System.SaveData;
using Assets.Ancible_Tools.Scripts.System.SaveData.Buildings;

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
        public TrainerData[] Trainers = new TrainerData[0];
        public NodeData[] Nodes = new NodeData[0];
        public BuildingData[] Buildings = new BuildingData[0];
        public WindowData[] Windows = new WindowData[0];

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
            Trainers = null;
            Nodes = null;
            Buildings = null;
            Windows = null;
        }
    }
}
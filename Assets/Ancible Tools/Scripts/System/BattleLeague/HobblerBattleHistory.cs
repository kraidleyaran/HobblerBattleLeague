using System;
using System.Collections.Generic;

namespace Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague
{
    [Serializable]
    public class HobblerBattleHistory
    {
        public bool Victory;
        public string Id;
        public int DamageDone;
        public int HealingDone;
        public int DamageTaken;
        public int Deaths;
        public int RoundsPlayed;

        public HobblerBattleHistory FromBattleData(BattleUnitData data, bool victory, string battleId)
        {
            Victory = victory;
            Id = battleId;
            DamageDone = data.TotalDamageDone;
            HealingDone = data.TotalHeals;
            DamageTaken = data.TotalDamageTaken;
            Deaths = data.Deaths;
            RoundsPlayed = data.RoundsPlayed;
            return this;
        }
    }
}
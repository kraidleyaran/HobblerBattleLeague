using System;
using System.Collections.Generic;

namespace Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague
{
    [Serializable]
    public class HobblerBattleHistory
    {
        public int Victories;
        public int Defeats;
        public int TotalMatches;
        public List<int> DamageDone = new List<int>();
        public List<int> HealingDone = new List<int>();
        public List<int> DamageTaken = new List<int>();
        public List<int> Deaths = new List<int>();
        public List<int> RoundsPlayed = new List<int>();

        public void ApplyUnitData(BattleUnitData data, BattleResult result)
        {
            TotalMatches++;
            switch (result)
            {
                case BattleResult.Victory:
                    Victories++;
                    break;
                case BattleResult.Defeat:
                    Defeats++;
                    break;
                case BattleResult.Abandon:
                    break;
            }
            DamageDone.Add(data.TotalDamageDone);
            DamageTaken.Add(data.TotalDamageTaken);
            HealingDone.Add(data.TotalHeals);
            Deaths.Add(data.Deaths);
            RoundsPlayed.Add(data.RoundsPlayed);
        }
    }
}
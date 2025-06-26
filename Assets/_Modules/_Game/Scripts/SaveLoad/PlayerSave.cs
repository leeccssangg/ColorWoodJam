using System.Collections.Generic;

namespace Mimi.Prototypes.SaveLoad
{
    public class PlayerSave
    {
        public bool Vibration = true;
        public bool Sound = true;
        public bool Music = true;

        public int CurrentLevel = 0;
        public int LastCompleteLevel;
        public int TopLevel = 0;

        public int Coin = 0;
        public bool Rated;
        public bool IsReceiveReward = false;
        public List<int> ShownTutIds = new List<int>();
        public List<string> ShowNewBoosterIds = new List<string>();
        public int FreezeTimeAmount;
        public int HammerAmount;
        public int VacuumAmount;
    }
}
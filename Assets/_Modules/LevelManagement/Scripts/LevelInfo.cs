using System;
using LevelScrollViews;

namespace Mimi.Prototypes.LevelManagement
{
    [Serializable]
    public class LevelInfo
    {
        public string Id { get; }
        public string PrefabAddress { get; }
        public LevelDifficulty Difficulty { get; }

        public int Time { get; }
        public int Coin { get; }
        public int NewFeature { get; }
        public float CameraFov { get; }

        public LevelInfo(string id, string prefabAddress, LevelDifficulty difficulty, int time, int coin,
            int newFeature, float cameraFov)
        {
            Id = id;
            PrefabAddress = prefabAddress;
            Difficulty = difficulty;
            Time = time;
            Coin = coin;
            NewFeature = newFeature;
            CameraFov = cameraFov;
        }
    }
}
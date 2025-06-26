using LevelScrollViews;
using Mimi.DataSources.GoogleSheet;

namespace Games
{
    [SheetModel]
    public class SheetLevelModel
    {
        public string Id { private set; get; }
        public string PrefabAddress { private set; get; }
        public LevelDifficulty Difficulty { private set; get; }
        public int Time { private set; get; }
        public int Coin { private set; get; }
        public int NewFeature { private set; get; }
        public float CameraFov { private set; get; }
    }
}
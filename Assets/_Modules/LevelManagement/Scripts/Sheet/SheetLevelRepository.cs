using System.Collections.Generic;
using Games;
using Mimi.Logging;

namespace Mimi.Prototypes.LevelManagement
{
    public class SheetLevelRepository : ILevelRepository
    {
        public int NumberOfLevels => this.lookup.Count;

        private readonly Dictionary<string, LevelInfo> lookup;

        public SheetLevelRepository(List<SheetLevelModel> levels)
        {
            this.lookup = new Dictionary<string, LevelInfo>(levels.Count);

            foreach (SheetLevelModel levelData in levels)
            {
                var levelInfo = new LevelInfo(levelData.Id, levelData.PrefabAddress, levelData.Difficulty,
                    levelData.Time, levelData.Coin, levelData.NewFeature, levelData.CameraFov);
                this.lookup.Add(levelData.Id, levelInfo);
            }
        }

        public IEnumerable<LevelInfo> GetAll()
        {
            return this.lookup.Values;
        }

        public bool TryGet(string id, out LevelInfo levelInfo)
        {
            levelInfo = null;
            if (!this.lookup.ContainsKey(id))
            {
                MiLogger.Error($"Level prefab does not exist: {id}");
                return false;
            }

            levelInfo = this.lookup[id];
            return true;
        }
    }
}
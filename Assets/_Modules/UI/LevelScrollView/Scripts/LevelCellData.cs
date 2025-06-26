namespace LevelScrollViews
{
    public class LevelCellData
    {
        public bool IsLocked { private set; get; }
        public LevelDifficulty Difficulty { get; }

        public LevelCellData(LevelDifficulty difficulty, bool isLocked)
        {
            IsLocked = isLocked;
            Difficulty = difficulty;
        }

        public void SetLocked(bool isLocked)
        {
            IsLocked = isLocked;
        }
    }
}
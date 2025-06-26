using System;

namespace Levels
{
    public interface ILevel
    {
        event Action OnLevelCompleted;
    }
}
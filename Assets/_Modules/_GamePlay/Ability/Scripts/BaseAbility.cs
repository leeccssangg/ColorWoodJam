using UnityEngine;

namespace Ability
{
    public abstract class BaseAbility: MonoBehaviour
    {
        public abstract void Initialize(Block block);
        public abstract void Begin();
        public abstract void End();
    }
}
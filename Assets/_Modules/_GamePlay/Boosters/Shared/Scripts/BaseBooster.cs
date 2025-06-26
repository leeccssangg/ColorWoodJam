using UnityEngine;

namespace Boosters
{
    public abstract class BaseBooster : MonoBehaviour
    {
        public bool IsBusy { get; protected set; }

        public void Use()
        {
            if (this.IsBusy) return;
            IsBusy = true;
            OnUse();
        }

        public void Cancel()
        {
            if (!IsBusy) return;
            IsBusy = false;
            OnCancel();
        }

        protected abstract void OnUse();
        protected abstract void OnCancel();
    }
}
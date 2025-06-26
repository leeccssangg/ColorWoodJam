using UnityEngine;

namespace DepthMasks
{
    public class SetRenderQueue : MonoBehaviour
    {
        [SerializeField] private int[] queues = new int[] { 2000 };

        protected void Awake()
        {
            SetVals();
        }

        private void OnValidate()
        {
            // SetVals();
        }

        private void SetVals()
        {
            Material[] materials = GetComponent<Renderer>().materials;
            for (int i = 0; i < materials.Length; ++i)
            {
                materials[i].renderQueue = this.queues[0];
            }
        }
    }
}
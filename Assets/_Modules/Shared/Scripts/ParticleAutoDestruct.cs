using Mimi.Prototypes.Pooling;
using Mimi.ServiceLocators;
using Sirenix.OdinInspector;
using UnityEngine;

public class ParticleAutoDestruct : MonoBehaviour
{
    [SerializeField] private float duration;
    [SerializeField] private bool unscaledTime;

    private float counter = 0f;

    private void OnDisable()
    {
        this.counter = 0f;
    }

    private void Update()
    {
        this.counter += this.unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        if (this.counter >= this.duration)
        {
            this.counter = 0f;
            ServiceLocator.Global.Get<IPoolService>().Despawn(gameObject);
        }
    }

    [Button("Calculate")]
    private void CalculateDuration()
    {
        ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();

        this.duration = particleSystems[0].main.duration;

        for (int i = 0; i < particleSystems.Length; i++)
        {
            if (particleSystems[i].main.duration > this.duration)
            {
                this.duration = particleSystems[i].main.duration;
            }
        }
    }
}
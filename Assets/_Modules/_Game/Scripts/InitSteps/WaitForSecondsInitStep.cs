using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Games.InitSteps;

namespace Mimi.Prototypes
{
    public class WaitForSecondsInitStep : IGameInitStep
    {
        public float ExpectedDuration { get; }

        public WaitForSecondsInitStep(float duration)
        {
            ExpectedDuration = duration;
        }

        public async UniTask Initialize(CancellationToken cancellationToken)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(ExpectedDuration), cancellationToken: cancellationToken);
        }
    }
}
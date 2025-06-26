using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Configs;
using Mimi.Games.InitSteps;

namespace Mimi.Prototypes
{
    public class ParseLevelConfigInitStep : IGameInitStep
    {
        private readonly LevelConfig levelConfig;
        private readonly IConfigProvider configProvider;
        private readonly string configName;
        public float ExpectedDuration => 0f;

        public ParseLevelConfigInitStep(LevelConfig levelConfig, IConfigProvider configProvider, string configName)
        {
            this.levelConfig = levelConfig;
            this.configProvider = configProvider;
            this.configName = configName;
        }

        public async UniTask Initialize(CancellationToken cancellationToken)
        {
            this.levelConfig.ParseConfig(this.configProvider.GetValue(this.configName).String);
            await UniTask.CompletedTask;
        }
    }
}
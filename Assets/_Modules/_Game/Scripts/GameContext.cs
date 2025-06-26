using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using EnergyTime;
using Games;
using Mimi.Games.InitSteps;
using Mimi.Games.Plugins;
using Mimi.Games.ProjectConfigs;
using Mimi.Prototypes.LevelManagement;
using Mimi.Stats;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.Prototypes
{
    public class GameContext : BaseGameContext
    {
        public ILevelRepository LevelRepository { private set; get; }
        public ILevelOrder LevelOrder { private set; get; }
        public EnergyPool EnergyPool { private set; get; }

        protected override async UniTask OnInitializing()
        {
            await base.OnInitializing();
            PlayerResources.SetAmount(ResourceId.Coin, 300);
            PlayerResources.SetAmount(ResourceId.FreezeTimer, 1);
            PlayerResources.SetAmount(ResourceId.Hammer, 1);
            PlayerResources.SetAmount(ResourceId.Vacuum, 1);
        }

        protected override void CreateServices()
        {
            CreateLevelServices();
            CreateEnergyController();
        }

        protected override void AddInitSteps(IGameInitiator gameInitiator, ProjectConfig projectConfig)
        {
        }

        protected override void AddGlobalPlugins(CompositePlugin pluginInstaller)
        {
        }

        private void CreateEnergyController()
        {
            EnergyPool = new EnergyPool(new Stat(StrictOrderCalculator.Instance), TimeSpan.FromMinutes(30),
                TimeProvider);
            EnergyPool.SetCapacity(5);
            EnergyPool.SetEnergy(5);
            EnergyPool.StartUpdate();
        }

        private void CreateLevelServices()
        {
            LevelRepository = new SheetLevelRepository(GetDataSheet<SheetLevelModel>("LevelRepo"));
            IEnumerable<string> levelIdOrders = GetDataSheet<SheetOrderModel>().Select(x => x.Id).Distinct();
            LevelOrder = new LinearLevelOrder(LevelRepository, levelIdOrders);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            EnergyPool?.Update();
        }

#if UNITY_EDITOR
        [Button]
        private void Screenshot()
        {
            ScreenCapture.CaptureScreenshot("Screenshot.png");
        }
#endif
    }
}
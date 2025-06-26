using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using Economy.Resources;
using Mimi.Ads.Adapters;
using Mimi.Analytics.Sessions;
using Mimi.Analytics.Tracking.Trackers;
using Mimi.AntiCheat.Times;
using Mimi.Audio;
using Mimi.Audio.MasterAudios;
using Mimi.Configs;
using Mimi.DataSources.GoogleSheet;
using Mimi.Events;
using Mimi.Events.AsyncBus;
using Mimi.Games.InitSteps;
using Mimi.Games.Plugins;
using Mimi.Games.ProjectConfigs;
using Mimi.IAP;
using Mimi.Localizations;
using Mimi.Localizations.DataSources.GoogleSheet;
using Mimi.Persistence.LocalPrefs;
using Mimi.Prototypes.Pooling;
using Mimi.Prototypes.SaveLoad;
using Mimi.Prototypes.UI;
using Mimi.ServiceLocators;
using Mimi.Stats;
using Tabtale.TTPlugins;
using UnityEngine;

namespace Mimi.Prototypes
{
    public abstract class BaseGameContext : BaseContext
    {
        [SerializeField] private SheetAsset gameDataAsset;
        [SerializeField] private SheetAsset localizeAsset;
        [SerializeField] private DialogManager dialogManager;
        public RuntimeState RuntimeState { private set; get; }

        public DialogManager DialogManager => this.dialogManager;
        public ResourceCollection PlayerResources { private set; get; }
        public ISessionRecorder SessionRecorder { private set; get; }
        public ISaveManager SaveManager { private set; get; }
        public IPurchasingProvider InAppPurchaseStore { private set; get; }
        public IAsyncPublisher EventPublisher { private set; get; }
        public IAsyncSubscriber EventSubscriber { private set; get; }
        public IConfigProvider RemoteConfig { private set; get; }
        public IAdAdapter Ads { private set; get; }
        public IAnalyticTracker AnalyticTracker { private set; get; }
        public IAudioService AudioService { private set; get; }
        public ILocalizationService Localization { private set; get; }
        public ILocalPrefs LocalPrefs { private set; get; }
        public ITimeProvider TimeProvider { private set; get; }

        public LevelConfig RateConfig { get; } = new();
        public LevelConfig ShowInterstitialLevelConfig { get; } = new();

        private readonly CompositePlugin globalPluginContainer = new CompositePlugin();
        private IPluginConfigInjector projectPluginInjector;

        protected override async UniTask OnInitializing()
        {
            TTPCore.Setup();
            QualitySettings.vSyncCount = 0;
            // Application.targetFrameRate = 60;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Application.targetFrameRate = Mathf.FloorToInt((float)new Resolution().refreshRateRatio.value);
            InitSheetAssets();
            this.projectPluginInjector = new UnityResourcePluginConfigInjector();
            IProjectConfigRepository projectConfigRepository = new ResourceProjectConfigRepository();
            ProjectConfig projectConfig = projectConfigRepository.Get();
            RuntimeState = RuntimeState.Get();
            await CreateCoreServices();
            SaveManager.Load();
            IGameInitiator gameInitiator = new ReportProgressGameInitiator(EventPublisher);
            AddDefaultInitSteps(gameInitiator, projectConfig);
            AddInitSteps(gameInitiator, projectConfig);
            await gameInitiator.Initialize();
            AddGlobalPlugins(this.globalPluginContainer);
            InjectPluginConfigs(this.globalPluginContainer.Plugins);
            await this.globalPluginContainer.Install();
            await this.globalPluginContainer.Begin();
            SessionRecorder.RecordSessionStart(TimeProvider.UtcNow);
        }

        public void InjectPluginConfigs(IEnumerable<IPlugin> plugins)
        {
            foreach (IPlugin plugin in plugins)
            {
                this.projectPluginInjector.Inject(plugin);
            }
        }

        protected List<TSheet> GetDataSheet<TSheet>(string sheetName)
        {
            return this.gameDataAsset.GetSheet<TSheet>(sheetName);
        }

        protected List<TSheet> GetDataSheet<TSheet>()
        {
            return this.gameDataAsset.GetSheet<TSheet>();
        }

        private void InitSheetAssets()
        {
            ISheetSerializer sheetSerializer = new SheetSerializerDefaultMini(new ModelConverterAot());
            this.gameDataAsset.Init(sheetSerializer);
            this.localizeAsset.Init(sheetSerializer);
        }

        private void AddDefaultInitSteps(IGameInitiator gameInitiator, ProjectConfig projectConfig)
        {
            gameInitiator
                .AddStep(
                    new LocalizationInitStep(Localization));
        }

        protected abstract void CreateServices();
        protected abstract void AddInitSteps(IGameInitiator gameInitiator, ProjectConfig projectConfig);
        protected abstract void AddGlobalPlugins(CompositePlugin pluginInstaller);

        private async UniTask CreateCoreServices()
        {
            CreatePoolingService();
            CreateMessageServices();
            CreateLocalPrefsService();
            CreateTimeProvider();
            CreateGameSessionService();
            CreateDialogService();
            CreateAudioService();
            CreateAnalyticService();
            CreateRemoteConfigService();
            CreateLocalizationService();
            CreateInAppPurchaseService();
            CreatePlayerResourceService();
            CreateSaveService();
            await CreateAdService();
            CreateServices();
        }

        private void CreateTimeProvider()
        {
            TimeProvider = new AntiCheatOfflineSupportTimeProvider();
        }


        private void CreateLocalPrefsService()
        {
            LocalPrefs = new UnityPlayerPref();
        }

        private void CreateGameSessionService()
        {
            SessionRecorder = new LocalSessionRecorder(EventPublisher, LocalPrefs);
        }

        private void CreatePoolingService()
        {
            ServiceLocator.Global.Register<IPoolService>(new InternalPoolService());
        }

        private void CreateMessageServices()
        {
            EventSubscriber = AsyncMessageBus.Default;
            EventPublisher = AsyncMessageBus.Default;
            var messageServiceAdapter = new MessageServiceAdapter(EventPublisher, EventSubscriber);
            ServiceLocator.Global.Register<IEventService>(messageServiceAdapter);
        }

        private void CreateDialogService()
        {
            this.dialogManager.Initialize();
            ServiceLocator.Global.Register(this.dialogManager);
        }

        private void CreatePlayerResourceService()
        {
            var currencyRepo = new ResourceCollection();
            currencyRepo.AddResource(new Resource(ResourceId.Coin, StrictOrderCalculator.Instance));
            currencyRepo.AddResource(new Resource(ResourceId.FreezeTimer, StrictOrderCalculator.Instance));
            currencyRepo.AddResource(new Resource(ResourceId.Hammer, StrictOrderCalculator.Instance));
            currencyRepo.AddResource(new Resource(ResourceId.Vacuum, StrictOrderCalculator.Instance));
            PlayerResources = currencyRepo;
        }

        private void CreateAudioService()
        {
            IAudioPlayer audioPlayer = new MasterAudioPlayer();
            AudioService = new AudioServiceAdapter(audioPlayer);
            ServiceLocator.Global.Register(AudioService);
        }

        private void CreateRemoteConfigService()
        {
            RemoteConfig = NullConfigProvider.Instance;
        }

        private void CreateAnalyticService()
        {
            AnalyticTracker = new NullTracker();
        }

        private async UniTask CreateAdService()
        {
            Ads = await new AdAdapterFactory().CreateService();
        }

        private void CreateLocalizationService()
        {
            ILocalizer localizer = new Localizer(new GoogleSheetLanguageFactory());
            Localization = new LocalizationServiceAdapter(localizer);
            ServiceLocator.Global.Register<ILocalizationService>(Localization);
        }

        private void CreateInAppPurchaseService()
        {
            InAppPurchaseStore = NullInAppPurchaseProvider.Instance;
        }

        private void CreateSaveService()
        {
            SaveManager = new ConvertibleSaveManager(this);
            SaveManager.AddSaveLoadStrategy(new GameSaver(this), new GameLoader(this));
        }

        protected override void OnPause(bool pause)
        {
            base.OnPause(pause);
            if (pause)
            {
                Time.timeScale = 0f;
                EventPublisher.PublishAsync(new GamePaused());
            }
            else
            {
                Time.timeScale = 1f;
                EventPublisher.PublishAsync(new GameUnpaused());
            }
        }
    }
}
using System.Collections.Generic;
using Mimi.Ads.Adapters.Extensions.Throttling;
using Mimi.Rx.Variables;

namespace Mimi.Prototypes
{
    public class RuntimeState
    {
        private static readonly RuntimeState Instance = new RuntimeState();

        public IsInterstitialAdThrottled IsInterstitialAdThrottled { private set; get; }
        public RxVar<int> LastCompletedLevelOrder { get; } = new RxVar<int>();
        public RxVar<int> CurrentLevelOrder { get; } = new RxVar<int>();
        public RxVar<int> LevelTop { get; } = new RxVar<int>();
        public bool Sound = true;
        public bool Music = true;
        public bool Vibration = true;
        public List<int> ShownTutIds = new List<int>();
        public List<string> ShowNewBoosterIds = new List<string>();

        private RuntimeState()
        {
        }

        public static RuntimeState Get()
        {
            return Instance;
        }
    }
}
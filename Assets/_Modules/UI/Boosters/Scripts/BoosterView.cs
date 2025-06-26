using Mimi;
using Mimi.Prototypes.UI;
using Timers;
using UnityEngine;

namespace Boosters
{
    public class BoosterView : BaseView
    {
        [SerializeField] private BoosterButton timerBoosterButton;
        [SerializeField] private BoosterButton hammerBoosterButton;
        [SerializeField] private BoosterButton vacuumBoosterButton;
        [SerializeField] private PlayingState playingState;

        public Timer Timer => this.playingState.Timer;
        public BoosterButton TimerBoosterButton => this.timerBoosterButton;
        public BoosterButton HammerBoosterButton => this.hammerBoosterButton;
        public BoosterButton VacuumBoosterButton => this.vacuumBoosterButton;
    }
}
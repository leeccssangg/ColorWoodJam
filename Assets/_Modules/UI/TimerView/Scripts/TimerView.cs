using FrogunnerGames;
using Mimi.Prototypes.UI;
using TMPro;
using UnityEngine;

namespace Timers
{
    public class TimerView : BaseView
    {
        [SerializeField] private TextMeshProUGUI timerText;

        public void SetTimer(int remainingTime)
        {
            this.timerText.text = StringNumber.SecondsToMinuteTextColon(remainingTime);
        }
    }
}
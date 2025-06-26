using FrogunnerGames;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HeartBars
{
    public class HeartBar : MonoBehaviour
    {
        [SerializeField] private Button addButton;
        [SerializeField] private TextMeshProUGUI currentEnergyText;
        [SerializeField] private TextMeshProUGUI energyTimerText;

        public void SetCurrentEnergy(int energy)
        {
            this.currentEnergyText.text = StringNumber.IntToText(energy);
        }

        public void SetEnergyTimer(int secsToFull)
        {
            this.energyTimerText.text = StringNumber.SecondsToMinuteTextColon(secsToFull);
        }

        public void SetFullEnergyState()
        {
            this.energyTimerText.text = "Full";
        }
    }
}
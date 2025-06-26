using System;
using Economy.Resources;
using Mimi.AntiCheat.Times;
using Mimi.Prototypes.Events;
using Mimi.Stats;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EnergyTime
{
    [Serializable]
    public class EnergyPool
    {
        private readonly ResourceValue energy;
        private readonly TimeSpan rechargePeriod;
        private readonly ITimeProvider timeProvider;

        private DateTime nextRechargeTime;
        private DateTime lastRechargeTime;
        private bool update;
        private bool full;

        [ShowInInspector] public int CurrentEnergy => Mathf.CeilToInt(this.energy.Amount);

        public Stat CapacityStat => this.capacityStat;

        [ShowInInspector] private readonly Stat capacityStat;

        public int BaseCapacity => (int)this.capacityStat.BaseValue;
        public int Capacity => (int)this.capacityStat.CurrentValue;

        public bool FullCapacity => CurrentEnergy >= Capacity;

        public DateTime LastRechargeTime => this.lastRechargeTime;
        public TimeSpan RemainingTimeToNextRecharge => this.nextRechargeTime - this.timeProvider.UtcNow;

        public TimeSpan RechargePeriod => this.rechargePeriod;

        public event Action<int, int> OnValueChanged;

        public EnergyPool(Stat capacity, TimeSpan rechargePeriod, ITimeProvider timeProvider)
        {
            this.energy = new ResourceValue();
            this.capacityStat = capacity;
            this.rechargePeriod = rechargePeriod;
            this.timeProvider = timeProvider;
            this.energy.OnValueChanged += OnUpdateValue;
            this.capacityStat.ValueChanged += OnCapacityChange;
        }

        private void OnCapacityChange(object sender, StatChangedEventArgs statChangedEventArgs)
        {
            RecalculateNextRechargeTime();
        }

        public void StartUpdate()
        {
            this.update = true;
            RecalculateNextRechargeTime();
        }

        public void StopUpdate()
        {
            this.update = false;
        }

        private bool isOverCapacityLastFrame = false;

        public void Update()
        {
            if (!this.update) return;

            // Do not recharge if full capacity
            if (this.energy.Amount >= Capacity)
            {
                isOverCapacityLastFrame = true;
                return;
            }

            if (isOverCapacityLastFrame)
            {
                RecalculateNextRechargeTime();
                isOverCapacityLastFrame = false;
            }

            if (this.timeProvider.UtcNow < this.nextRechargeTime) return;

            this.energy.Add(1);
            RecalculateNextRechargeTime();
        }

        public void SetCapacity(int capacity)
        {
            if (capacity < 0)
            {
                Debug.LogError("Capacity cannot be less than zero");
                return;
            }

            this.capacityStat.SetBaseValue(capacity);
        }

        public void RecalculateNextRechargeTime()
        {
            this.lastRechargeTime = this.timeProvider.UtcNow;
            this.nextRechargeTime = this.lastRechargeTime + this.rechargePeriod;
        }

        public TimeSpan CalculateRemainingTimeToFull()
        {
            if (FullCapacity) return TimeSpan.Zero;

            int energy = Capacity - CurrentEnergy;

            // Already in the middle of a recharge cycle
            TimeSpan remainingTimeInFirstCycle = this.nextRechargeTime - this.timeProvider.UtcNow;

            int numberOfFullCycles = energy - 1;
            TimeSpan remainingTimeOfAllFullCycles = TimeSpan.Zero;

            for (int i = 0; i < numberOfFullCycles; i++)
            {
                remainingTimeOfAllFullCycles = remainingTimeOfAllFullCycles.Add(this.rechargePeriod);
            }

            TimeSpan remainingTime = remainingTimeInFirstCycle + remainingTimeOfAllFullCycles;
            return remainingTime;
        }

        public int CalculateEnergyFromLastTime(DateTime now, DateTime lastRechargeTime)
        {
            TimeSpan timePass = now - lastRechargeTime;
            return (int)(timePass.TotalSeconds / this.rechargePeriod.TotalSeconds);
        }

        public void SetEnergy(int amount)
        {
            this.energy.SetAmount(amount);
        }

        public void AddEnergy(int amount)
        {
            this.energy.Add(amount);
            Messenger.Broadcast<float>(EventKey.AddStamina, amount);
        }

        public void UseEnergy(int amount)
        {
            this.energy.Subtract(amount);
            Messenger.Broadcast<float>(EventKey.ConsumeStamina, amount);
        }

        public bool HasEnough(int amount)
        {
            return this.energy.Amount >= amount;
        }

        private void OnUpdateValue(float value)
        {
            OnValueChanged?.Invoke((int)value, Capacity);
        }
    }
}
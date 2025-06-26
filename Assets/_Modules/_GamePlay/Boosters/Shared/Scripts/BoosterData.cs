using System.Collections.Generic;
using System.Reflection;
using Mimi.Prototypes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Boosters
{
    [CreateAssetMenu(fileName = "BoosterData", menuName = "Boosters/BoosterData")]
    public class BoosterData : ScriptableObject
    {
        [SerializeField, ValueDropdown("GetResourceIds")]
        private string resourceId;

        [SerializeField] private Sprite icon;
        [SerializeField] private string boosterName;
        [SerializeField] private string description;
        [SerializeField] private int cost;
        [SerializeField] private int numberPerPack;
        [SerializeField] private int unlockLevelIndex;

        public Sprite Icon => this.icon;
        public string BoosterName => this.boosterName;
        public string Description => this.description;
        public int Cost => this.cost;
        public int NumberPerPack => this.numberPerPack;
        public string ResourceId => this.resourceId;
        public int UnlockLevelIndex => this.unlockLevelIndex;

        private IEnumerable<string> GetResourceIds()
        {
            FieldInfo[] fields = typeof(ResourceId).GetFields(BindingFlags.Static | BindingFlags.Public);
            var values = new List<string>(fields.Length);

            foreach (FieldInfo fieldInfo in fields)
            {
                string value = fieldInfo.GetValue(null) as string;
                values.Add(value);
            }

            return values;
        }
    }
}
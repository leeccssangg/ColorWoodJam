using UnityEngine;

namespace Mimi.Prototypes.SaveLoad
{
    public class PlayerSaver : ISaveStrategy<SaveRoot, BaseGameContext>
    {
        //save PlayerSaver
        public void Save(int version, SaveRoot saveRoot, BaseGameContext context)
        {
            PlayerSave playerSave = saveRoot.PlayerSave;
            //Sound
            playerSave.Music = context.RuntimeState.Music;
            playerSave.Sound = context.RuntimeState.Sound;
            playerSave.Vibration = context.RuntimeState.Vibration;
            //Level
            playerSave.CurrentLevel = context.RuntimeState.CurrentLevelOrder.Value;
            playerSave.LastCompleteLevel = context.RuntimeState.LastCompletedLevelOrder.Value;
            playerSave.TopLevel = context.RuntimeState.LevelTop.Value;
            //Games
            playerSave.Coin = Mathf.CeilToInt(context.PlayerResources.GetAmount(ResourceId.Coin));
            playerSave.ShownTutIds = context.RuntimeState.ShownTutIds;
            playerSave.ShowNewBoosterIds = context.RuntimeState.ShowNewBoosterIds;
            playerSave.FreezeTimeAmount = Mathf.CeilToInt(context.PlayerResources.GetAmount(ResourceId.FreezeTimer));
            playerSave.HammerAmount = Mathf.CeilToInt(context.PlayerResources.GetAmount(ResourceId.Hammer));
            playerSave.VacuumAmount = Mathf.CeilToInt(context.PlayerResources.GetAmount(ResourceId.Vacuum));
        }
    }
}
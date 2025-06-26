namespace Mimi.Prototypes.SaveLoad
{
    public class PlayerLoader : ILoadStrategy<SaveRoot, BaseGameContext>
    {
        public void Load(int version, SaveRoot saveRoot, BaseGameContext context, bool firstLoad)
        {
            if (saveRoot.PlayerSave == null)
            {
                saveRoot.PlayerSave = new PlayerSave();
            }

            context.RuntimeState.Music = saveRoot.PlayerSave.Music;
            context.RuntimeState.Sound = saveRoot.PlayerSave.Sound;
            context.PlayerResources.SetAmount(ResourceId.Coin, saveRoot.PlayerSave.Coin);
            context.PlayerResources.SetAmount(ResourceId.FreezeTimer, saveRoot.PlayerSave.FreezeTimeAmount);
            context.PlayerResources.SetAmount(ResourceId.Hammer, saveRoot.PlayerSave.HammerAmount);
            context.PlayerResources.SetAmount(ResourceId.Vacuum, saveRoot.PlayerSave.VacuumAmount);
            context.RuntimeState.ShowNewBoosterIds = saveRoot.PlayerSave.ShowNewBoosterIds;

            context.RuntimeState.LevelTop.Value = saveRoot.PlayerSave.TopLevel;
            context.RuntimeState.CurrentLevelOrder.Value = saveRoot.PlayerSave.CurrentLevel;
            context.RuntimeState.LastCompletedLevelOrder.Value = saveRoot.PlayerSave.LastCompleteLevel;
            context.RuntimeState.ShownTutIds = saveRoot.PlayerSave.ShownTutIds;
        }
    }
}
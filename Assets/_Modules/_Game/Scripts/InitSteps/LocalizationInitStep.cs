using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Games.InitSteps;
using Mimi.Localizations;
using Mimi.Localizations.Unity;
using UnityEngine;

namespace Mimi.Prototypes
{
    public class LocalizationInitStep : IGameInitStep
    {
        public float ExpectedDuration => 0.1f;

        private readonly ILocalizationService localizationService;

        public LocalizationInitStep(ILocalizationService localizationService)
        {
            this.localizationService = localizationService;
        }

        public async UniTask Initialize(CancellationToken cancellationToken)
        {
            string twoLetterIsoLanguageName =
                Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToLowerInvariant();
            bool isUsingHindiLanguage = twoLetterIsoLanguageName == "hi";

            Language defaultLanguage = isUsingHindiLanguage ? Language.Hindi : Application.systemLanguage.ToLanguage();
            defaultLanguage = this.localizationService.IsLanguageSupported(defaultLanguage)
                ? defaultLanguage
                : Language.English;
            this.localizationService.SetCurrentLanguage(defaultLanguage);
            await UniTask.CompletedTask;
        }
    }
}
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.Events;
using Mimi.Prototypes.SceneManagement;
using TypeReferences;
using UnityEngine;

namespace Mimi.Prototypes
{
    public class BootLoader : MonoBehaviour
    {
        [SerializeField] private BaseGameContext gameContext;
        [SerializeField] private BootView bootView;
        [SerializeField] private float fakeLoadingSecs = 15f;

        [SerializeField, ClassExtends(typeof(BaseSceneController))]
        private ClassTypeReference nextSceneType;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public async void StartLoading()
        {
            this.bootView.Show();
            await Load();
            this.gameContext.EventPublisher.PublishAsync(new BootGameCompleted());
            this.bootView.Hide();
            Destroy(gameObject);
        }

        private async UniTask Load()
        {
            float loadingSecs = Application.isEditor ? 1f : fakeLoadingSecs;
            float loadingPercentage = 0f;

            UniTask fakeLoadingBarProgress = DOTween.To(() => loadingPercentage,
                value =>
                {
                    loadingPercentage = value;
                    this.bootView.SetLoadingPercentage(loadingPercentage);
                }, 0.9f, loadingSecs).AsyncWaitForCompletion().AsUniTask();

            UniTask waitForContextInitialized = UniTask.WaitUntil(() => this.gameContext.IsInitialized);
            UniTask loadNextScene = this.gameContext.LoadSceneAsync(this.nextSceneType.Type);
            UniTask loadingProgress =
                UniTask.WhenAll(fakeLoadingBarProgress, waitForContextInitialized, loadNextScene);

            await loadingProgress;

            await DOTween.To(() => loadingPercentage,
                value =>
                {
                    loadingPercentage = value;
                    this.bootView.SetLoadingPercentage(loadingPercentage);
                }, 1f, 0.3f).AsyncWaitForCompletion().AsUniTask();
        }
    }
}
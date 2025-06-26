using UnityEngine;
using UnityEngine.UI;

namespace SettingViews
{
    public class SpriteSwapToggle : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private Image graphicImage;
        [SerializeField] private Sprite onSprite;
        [SerializeField] private Sprite offSprite;

        private void Awake()
        {
            this.toggle.onValueChanged.AddListener(ValueChangeHandler);
        }

        private void ValueChangeHandler(bool value)
        {
            this.graphicImage.sprite = value ? this.onSprite : this.offSprite;
        }
    }
}
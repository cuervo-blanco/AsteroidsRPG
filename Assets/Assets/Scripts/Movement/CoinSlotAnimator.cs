using UnityEngine;
using UnityEngine.UI;

public class CoinSlotAnimator : MonoBehaviour {
    [SerializeField] private Image iconImage;

    void Awake() {
        if (!iconImage) iconImage = GetComponentInChildren<Image>();
    }

    public void SetIcon(Sprite sprite) {
        if (iconImage != null) {
            iconImage.sprite = sprite;
            iconImage.enabled = true;
        }
    }

    public void ResetState() {
        if (iconImage != null) {
            iconImage.enabled = true;
        }

        gameObject.SetActive(true);
    }

    public void ClearSlot() {
        GetComponentInParent<CoinUI>()?.RemoveSlot(this);
        gameObject.SetActive(false);
    }
}


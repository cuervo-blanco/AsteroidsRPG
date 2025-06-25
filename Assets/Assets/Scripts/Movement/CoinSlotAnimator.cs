using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CoinSlotAnimator : MonoBehaviour {
    [SerializeField] private Image iconImage;
    [SerializeField] TMP_Text labelText;

    void Awake() {
        if (!iconImage) iconImage = GetComponentInChildren<Image>();
        if (!labelText) labelText = GetComponentInChildren<TMP_Text>();
    }

    public void SetIcon(Sprite sprite) {
        if (iconImage != null) {
            iconImage.sprite = sprite;
            iconImage.enabled = true;
        }
    }


    public void SetLabel(string text) {
        if (labelText != null) {
            labelText.text = FormatLabel(text);
        }
    }

    private string FormatLabel(string raw) {
        // E.g., FireRate → "Fire Rate"
        return System.Text.RegularExpressions.Regex.Replace(raw, "(\\B[A-Z])", " $1");
    }


    public void ResetState() {
        if (iconImage != null) {
            iconImage.enabled = true;
        }
        if (labelText != null) labelText.enabled = true;
        gameObject.SetActive(true);
    }

    public void OnDrainFinished() {
        GetComponentInParent<CoinUI>()?.RemoveSlot(this);
        gameObject.SetActive(false);
    }
}


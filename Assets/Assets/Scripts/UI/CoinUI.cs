using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class CoinUI : MonoBehaviour {
    [SerializeField] private Transform coinSlotParent = null;
    [SerializeField] private GameObject coinSlotPrefab;

    public List<Animator> coinAnimators = new();
    int activeIndex = 0;

    [SerializeField] private Image iconDisplay;

    [SerializeField] private Sprite defaultIcon;
    [SerializeField] private Sprite shieldIcon;
    [SerializeField] private Sprite fireRateIcon;
    [SerializeField] private Sprite speedBoostIcon;

    public void SetCoinIcon(MagicCoinType type) {
        iconDisplay.sprite = GetIconForType(type);
    }

    private Sprite GetIconForType(MagicCoinType type) {
        switch (type) {
            case MagicCoinType.Shield: return shieldIcon;
            case MagicCoinType.FireRate: return fireRateIcon;
            case MagicCoinType.SpeedBoost: return speedBoostIcon;
            default: return defaultIcon;
        }
    }

    public void UpdateCoinSlots(List<SuperCoin> coins) {
        foreach (Transform child in coinSlotParent) Destroy(child.gameObject);
        coinAnimators.Clear();

        for (int i = 0; i < coins.Count; i++) {
            GameObject slot = Instantiate(coinSlotPrefab, coinSlotParent);

            Transform iconTransform = slot.transform.Find("Icon");

            if (iconTransform == null) {
                Debug.LogWarning($"No 'Icon' child found in {slot.name}!");
                continue;
            }

            Image icon = iconTransform.GetComponent<Image>();
            Animator anim = iconTransform.GetComponent<Animator>();

            if (icon != null) {
                icon.sprite = GetIconForType(coins[i].type);
            } else {
                Debug.LogWarning($"No Image component found on 'Icon' in {slot.name}!");
            }

            if (anim != null) {
                coinAnimators.Add(anim); // Add to list for animation control
            } else {
                Debug.LogWarning($"No Animator found on 'Icon' in {slot.name}!");
            }
        }
    }

    public void PlayCoinDrain(int index) {
        if (index >= 0 && index < coinAnimators.Count) {
            coinAnimators[index]?.SetTrigger("Drain");
        }
    }

    public void HighlightActive(int index) {
        for (int i = 0; i < coinAnimators.Count; i++) {
            Transform slotTransform = coinAnimators[i].transform.parent;
            Transform highlight = slotTransform.Find("Highlight");

            if (highlight != null) {
                highlight.gameObject.SetActive(i == index);
            }
        }
    }

    public int GetActiveIndex() =>  activeIndex;

    public void CycleNext(int total) {
        if (total == 0) return;
        activeIndex = (activeIndex + 1) % total;
        HighlightActive(activeIndex);
    }

    public void ResetUI() {
        activeIndex = 0;
        foreach (var anim in coinAnimators) {
            anim.SetBool("Selected", false);
            anim.gameObject.SetActive(false);
        }

    }
}


using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class CoinUI : MonoBehaviour {
    [SerializeField] Transform coinSlotParent;
    [SerializeField] GameObject coinSlotPrefab;

    readonly List<CoinSlotAnimator> slots = new();
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
        while (slots.Count < coins.Count) {
            GameObject slotGO = Instantiate(coinSlotPrefab, coinSlotParent);
            var slot = slotGO.GetComponentInChildren<CoinSlotAnimator>();
            slots.Add(slot);
        }

        for (int i = 0; i < slots.Count; ++i) {
            bool needed = i < coins.Count;
            slots[i].gameObject.SetActive(needed);
            if (needed) {
                slots[i].SetIcon(GetIconForType(coins[i].type));
                if (!slots[i].enabled) {
                    slots[i].ResetState();
                }
            }
        }

        activeIndex = Mathf.Clamp(activeIndex, 0, coins.Count - 1);
        HighlightActive(activeIndex);
    }

    public void PlayCoinDrain(int index) {
        if (index >= 0 && index < slots.Count)
            slots[index].PlayDrain();
    }

    public void HighlightActive(int index) {
        for (int i = 0; i < slots.Count; ++i) {
            Transform highlight = slots[i].transform.parent.Find("Highlight");
            if (highlight) highlight.gameObject.SetActive(i == index);
        }
    }

    public int GetActiveIndex() => activeIndex;

    public void CycleNext(int total) {
        if (total == 0) return;
        activeIndex = (activeIndex + 1) % total;
        HighlightActive(activeIndex);
    }

    public void ResetUI() {
        activeIndex = 0;
        foreach (var s in slots) s.gameObject.SetActive(false);
    }

    public void RemoveSlot(CoinSlotAnimator slot) => slots.Remove(slot);
}


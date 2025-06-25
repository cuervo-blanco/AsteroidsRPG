using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CoinUI : MonoBehaviour {
    [SerializeField] private Transform coinSlotParent;
    [SerializeField] private GameObject coinSlotPrefab;

    private readonly List<CoinSlotAnimator> slots = new();
    private int activeIndex = 0;

    [SerializeField] private Image iconDisplay;

    [Header("Coin Type Sprites")]
    [SerializeField] private Sprite defaultIcon;
    [SerializeField] private Sprite shieldIcon;
    [SerializeField] private Sprite fireRateIcon;
    [SerializeField] private Sprite speedBoostIcon;
    [SerializeField] private Sprite heavyShotIcon;
    [SerializeField] private Sprite slipperyBoostIcon;
    [SerializeField] private Sprite confusionIcon;
    [SerializeField] private Sprite goldFrenzyIcon;
    [SerializeField] private Sprite ghostIcon;
    [SerializeField] private Sprite miniModeIcon;
    [SerializeField] private Sprite overheatIcon;
    [SerializeField] private Sprite randomIcon;

    private Dictionary<MagicCoinType, Sprite> iconMap;

    void Awake() {
        iconMap = new() {
            { MagicCoinType.Default, defaultIcon },
            { MagicCoinType.Shield, shieldIcon },
            { MagicCoinType.FireRate, fireRateIcon },
            { MagicCoinType.SpeedBoost, speedBoostIcon },
            { MagicCoinType.HeavyShot, heavyShotIcon },
            { MagicCoinType.SlipperyBoost, slipperyBoostIcon },
            { MagicCoinType.Confusion, confusionIcon },
            { MagicCoinType.GoldFrenzy, goldFrenzyIcon },
            { MagicCoinType.Ghost, ghostIcon },
            { MagicCoinType.MiniMode, miniModeIcon },
            { MagicCoinType.Overheat, overheatIcon },
            { MagicCoinType.Random, randomIcon },
        };
    }

    public void SetCoinIcon(MagicCoinType type) {
        iconDisplay.sprite = GetIconForType(type);
    }

    private Sprite GetIconForType(MagicCoinType type) {
        if (iconMap != null && iconMap.TryGetValue(type, out var sprite)) {
            return sprite != null ? sprite : defaultIcon;
        }
        return defaultIcon;
    }

    public void UpdateCoinSlots(List<SuperCoin> coins) {
        foreach (var slot in slots) {
            Destroy(slot.gameObject);
        }
        slots.Clear();

        activeIndex = 0;

        for (int i = 0; i < coins.Count; i++) {
            var slotGO = Instantiate(coinSlotPrefab, coinSlotParent);
            var anim = slotGO.GetComponent<CoinSlotAnimator>();
            anim.SetIcon(GetIconForType(coins[i].type));
            anim.SetLabel(coins[i].type.ToString());
            slots.Add(anim);
        }

        HighlightActive(activeIndex);
    }

    public void HighlightActive(int index) {
        for (int i = 0; i < slots.Count; ++i) {
            Transform highlight = slots[i].transform.Find("Highlight");
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

        foreach (var slot in slots) {
            if (slot != null && slot.gameObject != null) {
                Destroy(slot.gameObject);
            }
        }

        slots.Clear();

        if (iconDisplay != null) {
            iconDisplay.sprite = defaultIcon;
        }
    }

    public void RemoveSlot(CoinSlotAnimator slot) => slots.Remove(slot);
}


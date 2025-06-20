using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RocketPowerModule : MonoBehaviour {
    public bool superModeActive = false;
    public float superModeDuration = 10f;

    private List<SuperCoin> superCoins = new List<SuperCoin>();
    public int maxSuperCoins = 3;

    public CoinUI coinUI;
    private RocketController controller;

    void Awake() {
        controller = GetComponent<RocketController>();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.E)) {
            coinUI.CycleNext(superCoins.Count);
        }

        if (Input.GetKeyDown(KeyCode.Y)) {
            TryActivateSuperMode();
        }
    }

    public void AddSuperCharge(MagicCoinType type) {
        if (superCoins.Count >= maxSuperCoins) return;

        superCoins.Add(new SuperCoin(type, superModeDuration));
        coinUI.UpdateCoinSlots(superCoins);
    }

    public void TryActivateSuperMode() {
        if (superModeActive || superCoins.Count <= 0) return;

        int index = Mathf.Clamp(coinUI.GetActiveIndex(), 0, superCoins.Count - 1);
        SuperCoin coin = superCoins[index];
        superCoins.RemoveAt(index);

        coinUI.UpdateCoinSlots(superCoins);
        coinUI.PlayCoinDrain(index);

        StartCoroutine(SuperModeTimer(coin.duration, coin.type));
    }

    private IEnumerator SuperModeTimer(float duration, MagicCoinType type) {
        superModeActive = true;
        Debug.Log("Super Mode Activated: " + type);

        switch (type) {
            case MagicCoinType.FireRate:
                controller.shootCooldown *= 0.5f;
                break;
            case MagicCoinType.Shield:
                controller.SetDamageTolerance(100.0f);
                break;
        }

        yield return new WaitForSeconds(duration);

        switch (type) {
            case MagicCoinType.FireRate:
                controller.shootCooldown *= 2f;
                break;
            case MagicCoinType.Shield:
                controller.SetDamageTolerance(0.0f);
                break;
        }

        superModeActive = false;
        Debug.Log("Super Mode Ended.");
    }

    public void ResetSuperCoins() {
        superCoins.Clear();
        coinUI.ResetUI();
    }
}


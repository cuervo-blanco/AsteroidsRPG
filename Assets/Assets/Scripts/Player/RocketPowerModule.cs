using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RocketPowerModule : MonoBehaviour {
    public List<MagicCoinType> activePowers = new();
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

        if (Input.GetKeyDown(KeyCode.Y) || (Input.touchCount == 2 && Input.GetTouch(1).phase == TouchPhase.Began)) {
            TryActivateSuperMode();
        }
    }

    public void AddSuperCharge(MagicCoinType type) {
        if (superCoins.Count >= maxSuperCoins) return;

        float duration = PowerDurations.GetDuration(type);
        superCoins.Add(new SuperCoin(type, duration));
        coinUI.UpdateCoinSlots(superCoins);
    }

    public void TryActivateSuperMode() {
        if (superCoins.Count > 0) {
            int selectedIndex = coinUI.GetActiveIndex();
            if (selectedIndex < 0 || selectedIndex >= superCoins.Count) return;

            var nextCoin = superCoins[selectedIndex];
            superCoins.RemoveAt(selectedIndex);

            activePowers.Add(nextCoin.type);
            coinUI.UpdateCoinSlots(superCoins);

            StartCoroutine(SinglePowerCoroutine(nextCoin.type, nextCoin.duration));
        }
    }

    IEnumerator SinglePowerCoroutine(MagicCoinType type, float duration) {
        superModeActive = true;

        yield return new WaitForSeconds(duration);

        activePowers.Remove(type);
        if (activePowers.Count == 0) {
            superModeActive = false;
        }
    }

    IEnumerator DrainSuperMode() {
        yield return new WaitForSeconds(superModeDuration);
        superModeActive = false;
        activePowers.Clear();
    }

    public void ResetSuperCoins() {
        superCoins.Clear();
        coinUI.ResetUI();

        superModeActive = false;
        activePowers.Clear();

        controller.shootCooldown = 0.2f;
        controller.moveSpeed = 5f;
        controller.SetDamageTolerance(0f);
        controller.InvertControls(false);
        controller.SetGhostMode(false);
        controller.SetSlipperyMode(false);
        controller.SetScale(1f);
        controller.Invoke("ResetShootTimer", 0.05f);
    }
}


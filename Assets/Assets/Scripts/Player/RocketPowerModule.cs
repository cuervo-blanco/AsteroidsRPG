using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RocketPowerModule : MonoBehaviour {
    public bool superModeActive = false;
    public float superModeDuration = 10f;
    public MagicCoinType currentCoinType = MagicCoinType.Default;

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
        if (superModeActive || superCoins.Count == 0) return;

        int index = Mathf.Clamp(coinUI.GetActiveIndex(), 0, superCoins.Count - 1);

        SuperCoin coin = superCoins[index];
        superCoins.RemoveAt(index);

        coinUI.UpdateCoinSlots(superCoins);

        StartCoroutine(SuperModeTimer(coin.duration, coin.type));
    }

    private IEnumerator SuperModeTimer(float duration, MagicCoinType type) {
        superModeActive = true;
        currentCoinType = type;
        Debug.Log("Super Mode Activated: " + type);

        switch (type) {
            case MagicCoinType.FireRate:
                controller.shootCooldown *= 0.5f;
                break;
            case MagicCoinType.Shield:
                controller.SetDamageTolerance(100.0f);
                break;
            case MagicCoinType.SpeedBoost:
                controller.moveSpeed *= 1.5f;
                break;
            case MagicCoinType.HeavyShot:
                controller.shootCooldown *= 1.2f;
                // Add bullet damage modifier here when supported
                break;
            case MagicCoinType.Confusion:
                controller.InvertControls(true);
                break;
            case MagicCoinType.GoldFrenzy:
                // Optionally boost rock value multiplier
                GameManager.Instance.pointsPerSecond *= 2;
                break;
            case MagicCoinType.Ghost:
                controller.SetGhostMode(true);
                break;
            case MagicCoinType.MiniMode:
                controller.SetScale(0.5f);
                break;
            case MagicCoinType.Overheat:
                controller.shootCooldown *= 0.4f;
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
            case MagicCoinType.SpeedBoost:
                controller.moveSpeed /= 1.5f;
                break;
            case MagicCoinType.HeavyShot:
                controller.shootCooldown /= 1.2f;
                break;
            case MagicCoinType.Confusion:
                controller.InvertControls(false);
                break;
            case MagicCoinType.GoldFrenzy:
                GameManager.Instance.pointsPerSecond /= 2;
                break;
            case MagicCoinType.Ghost:
                controller.SetGhostMode(false);
                break;
            case MagicCoinType.MiniMode:
                controller.SetScale(1f);
                break;
            case MagicCoinType.Overheat:
                controller.shootCooldown /= 0.4f;
                yield return new WaitForSeconds(3f);
                break;
        }

        superModeActive = false;
        currentCoinType = MagicCoinType.Default;
        Debug.Log("Super Mode Ended.");
    }

    public void ResetSuperCoins() {
        superCoins.Clear();
        coinUI.ResetUI();

        superModeActive = false;
        currentCoinType = MagicCoinType.Default;

        controller.shootCooldown = 0.2f;
        controller.moveSpeed = 5f;
        controller.SetDamageTolerance(0f);
        controller.InvertControls(false);
        controller.SetGhostMode(false);
        controller.SetScale(1f);

        controller.Invoke("ResetShootTimer", 0.05f);
    }
}


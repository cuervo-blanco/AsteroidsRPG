using UnityEngine;
using System.Collections;

public class Rocket : MonoBehaviour {
    public bool superModeActive = false;
    public int maxSuperCharges = 3;
    public int superModeCharges = 0;

    public float superModeDuration = 10f;

    public void TryActivateSuperMode() {
        if (superModeActive || superModeCharges <= 0) return;

        superModeCharges--;
        superModeActive = true;
        Debug.Log("Super Mode Activated!");
        StartCoroutine(SuperModeTimer(superModeDuration));
    }

    public void AddSuperCharge() {
        if (superModeCharges < maxSuperCharges) {
            superModeCharges++;
            Debug.Log("Picked up a Magic Coin!");
        }
    }

    private IEnumerator SuperModeTimer(float duration) {
        yield return new WaitForSeconds(duration);
        superModeActive = false;
        Debug.Log("Super Mode Ended.");
    }
}


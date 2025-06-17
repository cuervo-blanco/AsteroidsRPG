using UnityEngine;
using System.Collections;

public class Rocket : MonoBehaviour {
    public bool superModeActive = false;

    public void ActivateSuperMode(float duration = 10f) {
        if (superModeActive) return;

        superModeActive = true;
        Debug.Log("Super Mode Activated!");
        StartCoroutine(SuperModeTimer(duration));
    }

    private IEnumerator SuperModeTimer(float duration) {
        yield return new WaitForSeconds(duration);
        superModeActive = false;
        Debug.Log("Super Mode Ended.");
    }
}


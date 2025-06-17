using UnityEngine;
using UnityEngine.UI;

public class LivesUI : MonoBehaviour {
    Image[] icons;

    void Awake() => icons = GetComponentsInChildren<Image>(true);

    public void SetLives(int count) {
        for (int i = 0; i < icons.Length; i++)
            icons[i].enabled = i < count;
    }
}


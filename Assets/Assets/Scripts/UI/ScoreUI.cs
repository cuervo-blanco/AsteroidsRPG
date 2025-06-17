using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    TMP_Text label;

    void Awake()
    {
        label = GetComponent<TMP_Text>();
        if (label == null)
            Debug.LogWarning("ScoreUI: TMP_Text not found!");
    }

    public void SetScore(int value)
    {
        if (label == null)
        {
            Debug.LogWarning("ScoreUI: Cannot set score, TMP_Text is null");
            return;
        }

        Debug.Log($"ScoreUI: Setting score to {value}");
        label.text = value.ToString("D5");
    }
}


using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreRowUI : MonoBehaviour {
    [SerializeField] TMP_Text rankText;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text moneyText;
    [SerializeField] TMP_Text kmsText;
    [SerializeField] TMP_Text combinedText;
    [SerializeField] Image background;

    public void Set(int rank, ScoreEntry entry, bool highlight = false) {
        if (rankText) rankText.text = rank.ToString();
        if (nameText) nameText.text = entry.name;
        if (moneyText) moneyText.text = "$" + entry.money.ToString("N0");
        if (kmsText) kmsText.text = entry.kms.ToString("F1") + " km";
        if (combinedText) combinedText.text = entry.combined.ToString("D5");

        if (highlight && background != null) {
            background.color = new Color(1f, 1f, 0.5f);
        }
    }
}


[System.Serializable]
public class ScoreEntry {
    public string name;
    public int money;
    public float kms;
    public int combined;

    public ScoreEntry(int money, float kms, int combined) {
        this.money = money;
        this.kms = kms;
        this.combined = combined;
        this.name = "";
    }
}


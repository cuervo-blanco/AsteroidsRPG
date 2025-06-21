using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

public class GameManager : MonoBehaviour {
    public int pointsPerSecond = 10;
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    public CoinUI coinUI;
    public ScoreUI scoreUI;
    public LivesUI livesUI;
    public GameObject startPanel;
    public GameObject gameOverPanel;
    public Button startButton;
    public TMP_Text rockValueLabel;

    [Header("Score")]
    private int rockValue = 0;
    public GameObject scoreBoard;
    public Transform scoreRowParent;
    public GameObject scoreRowPrefab;
    public int maxRows = 5;
    List<ScoreEntry> hiScores = new();
    float score;
    int lives;

    [Header("RockControl")]
    public RockSpawner spawner;
    public RocketController rocket;

    enum State { Waiting, Playing, GameOver }
    State state = State.Waiting;

    void Start() {
        AkUnitySoundEngine.PostEvent("Play_MainTheme", gameObject);
    }

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (!rocket) rocket = FindFirstObjectByType<RocketController>();
        startButton.onClick.AddListener(StartGame);
        ShowStartScreen();
    }

    void ShowStartScreen() {
        state = State.Waiting;
        spawner.Stop();

        rocket.EnableControl(false);
        rocket.fireAnimator.gameObject.SetActive(false);
        rocket.gameObject.SetActive(true);

        startPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        scoreBoard.SetActive(false);

        scoreUI.SetScore(0);
        rockValue = 0;
        UpdateRockValueUI();
    }

    void Update() {
        if (state != State.Playing) return;

        score += pointsPerSecond * Time.deltaTime;
        scoreUI.SetScore(Mathf.FloorToInt(score));
    }

    public void SetLifeScore(int amountOfLives) {
        livesUI.SetLives(amountOfLives);
        if (amountOfLives <= 0) StartCoroutine(GameOverRoutine());
    }

    void StartGame() {
        scoreUI.gameObject.SetActive(true);
        livesUI.gameObject.SetActive(true);
        SetLifeScore(3);
        rockValueLabel.gameObject.SetActive(true);
        state = State.Playing;

        score = 0f;
        rockValue = 0;
        scoreUI.SetScore(0);
        UpdateRockValueUI();
        rocket.ResetLives();

        RocketPowerModule powerModule = rocket.GetComponent<RocketPowerModule>();
        if (powerModule != null) {
            powerModule.ResetSuperCoins();
            powerModule.superModeActive = false;
        }

        rocket.EnableControl(true);
        rocket.gameObject.SetActive(true);

        foreach (var obj in GameObject.FindGameObjectsWithTag("Rock")) {
            Destroy(obj);
        }
        foreach (var obj in GameObject.FindGameObjectsWithTag("Bullet")) {
            Destroy(obj);
        }

        startPanel.SetActive(false);
        gameOverPanel.SetActive(false);

        spawner.Begin();
    }

    IEnumerator GameOverRoutine() {
        spawner.Stop();
        coinUI.ResetUI();
        scoreUI.gameObject.SetActive(false);
        livesUI.gameObject.SetActive(false);
        rockValueLabel.gameObject.SetActive(false);
        state = State.GameOver;

        float kms = score / pointsPerSecond;
        int total = CombinedScore(rockValue, kms);

        var entry = new ScoreEntry(rockValue, kms, total);
        hiScores.Add(entry);
        hiScores = hiScores.OrderByDescending(e => e.combined).Take(maxRows).ToList();
        SaveScores();

        BuildScoreBoard();
        scoreBoard.SetActive(true);
        gameOverPanel.SetActive(true);

        yield return new WaitForSeconds(10f);

        ShowStartScreen();
    }

    public void AddRockValue(int value) {
        rockValue += value;
        UpdateRockValueUI();
    }

    void UpdateRockValueUI() {
        if (rockValueLabel == null) return;

        rockValueLabel.text = rockValue
        .ToString("C0", CultureInfo.GetCultureInfo("en-US"));
    }

    void LoadScores() {
        hiScores.Clear();
        string json = PlayerPrefs.GetString("HiScores", "");
        if (!string.IsNullOrEmpty(json)) {
            hiScores = JsonUtility.FromJson<ScoreList>(json).list;
        }
    }

    void SaveScores() {
        var listWrap = new ScoreList { list = hiScores };
        string json = JsonUtility.ToJson(listWrap);
        PlayerPrefs.SetString("HiScores", json);
    }

    void BuildScoreBoard() {
        foreach (Transform c in scoreRowParent) Destroy(c.gameObject);

        foreach (var s in hiScores.Take(maxRows)) {
            var row = Instantiate(scoreRowPrefab, scoreRowParent);

            TMP_Text[] cells = row.GetComponentsInChildren<TMP_Text>();
            cells[0].text = "$" + s.money.ToString("N0");
            cells[1].text = s.kms.ToString("F1") + " km";
            cells[2].text = s.combined.ToString("D5");
        }
    }

    float NormalisedMoney(float money) => Mathf.Clamp01(money/1000f);
    float NormalisedKms(float kms) => Mathf.Clamp01(kms/100f);

    int CombinedScore(float money, float kms) {
        float m = NormalisedMoney(money);
        float d = NormalisedKms(kms);
        return Mathf.RoundToInt((m * .5f + d * .5f) * 10_000f);
    }

    private class ScoreList {
        public List<ScoreEntry> list;
    }
}


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
    [Header("Start Panel")]
    public GameObject startPanel;
    public GameObject gameOverPanel;
    public Button startButton;

    [Header("GameOver Panel")]
    public Button gameOverButton;

    [Header("Score")]
    public TMP_Text rockValueLabel;
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
    public AK.Wwise.Event ringSound;

    [Header("Pause Menu")]
    public GameObject pauseMenu;
    public Button continueButton;
    public Button quitButtonInPause;
    public Button quitButtonInGameOver;

    private bool isPaused = false;

    enum State { Waiting, Playing, GameOver }
    State state = State.Waiting;

    [Header("Name Prompt")]
    public GameObject namePromptPanel;
    public TMP_InputField nameInput;
    public Button submitButton;

    private ScoreEntry pendingEntry;
    private string lastPlayerName = "";


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

        continueButton.onClick.AddListener(ResumeGame);
        quitButtonInPause.onClick.AddListener(QuitGame);
        quitButtonInGameOver.onClick.AddListener(QuitGame);
        startButton.onClick.AddListener(StartGame);
        startButton.onClick.AddListener(RingSound);

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

        scoreUI.SetScore(0, pointsPerSecond);
        rockValue = 0;
        UpdateRockValueUI();
    }

    void Update() {
        if (state == State.Playing) {
            score += pointsPerSecond * Time.deltaTime;
            scoreUI.SetScore(Mathf.FloorToInt(score), pointsPerSecond);

            if (Input.GetKeyDown(KeyCode.Escape)) {
                if (!isPaused) {
                    PauseGame();
                } else {
                    ResumeGame();
                }
            }
        }
    }

    void PauseGame() {
        isPaused = true;
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
        rocket.EnableControl(false);
    }

    void ResumeGame() {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        rocket.EnableControl(true);
    }

    public void QuitGame() {
        Time.timeScale = 1f;
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void SetLifeScore(int amountOfLives) {
        livesUI.SetLives(amountOfLives);
        if (amountOfLives <= 0) StartCoroutine(GameOverRoutine());
    }

    void RingSound() {
         if (ringSound != null) {
            ringSound.Post(gameObject);
         } else {
            Debug.LogWarning("Wwise Event not assigned!");
        }
    }

    void StartGame() {
        coinUI.ResetUI();
        gameOverButton.gameObject.SetActive(false);
        scoreUI.gameObject.SetActive(true);
        livesUI.gameObject.SetActive(true);
        SetLifeScore(3);
        rockValueLabel.gameObject.SetActive(true);
        state = State.Playing;

        score = 0f;
        rockValue = 0;
        scoreUI.SetScore(0, pointsPerSecond);
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
        pendingEntry = new ScoreEntry(rockValue, kms, total);
        ShowNamePrompt();

        yield break;
    }

    IEnumerator HideGameOverAndShowStart() {
        gameOverButton.gameObject.SetActive(false);
        gameOverPanel.SetActive(false);
        yield return new WaitForSeconds(0.1f);
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

    void BuildScoreBoard(ScoreEntry highlightEntry = null) {
        foreach (Transform c in scoreRowParent) Destroy(c.gameObject);

        var displayScores = hiScores.Take(10).ToList();
        bool highlightShown = false;

        for (int i = 0; i < displayScores.Count; i++) {
            var entry = displayScores[i];
            var row = Instantiate(scoreRowPrefab, scoreRowParent);
            var rowUI = row.GetComponent<ScoreRowUI>();

            if (rowUI != null) {
                bool highlight = entry == highlightEntry && !highlightShown;
                rowUI.Set(i + 1, entry, highlight);
                if (highlight) highlightShown = true;
            } else {
                Debug.LogWarning("ScoreRowUI component missing on prefab");
            }
        }

        if (!displayScores.Contains(highlightEntry) && highlightEntry != null) {
            var row = Instantiate(scoreRowPrefab, scoreRowParent);
            var rowUI = row.GetComponent<ScoreRowUI>();
            if (rowUI != null) {
                int actualRank = hiScores.IndexOf(highlightEntry) + 1;
                rowUI.Set(actualRank, highlightEntry, true);
            }
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

    void ShowNamePrompt() {
        namePromptPanel.SetActive(true);
        nameInput.text = lastPlayerName;
        submitButton.onClick.RemoveAllListeners();
        submitButton.onClick.AddListener(SubmitScore);
    }

    void SubmitScore() {
        string playerName = nameInput.text.Trim();
        if (string.IsNullOrEmpty(playerName)) return;

        namePromptPanel.SetActive(false);
        lastPlayerName = playerName;

        pendingEntry.name = playerName;
        hiScores.Add(pendingEntry);
        hiScores = hiScores.OrderByDescending(e => e.combined).ToList();

        SaveScores();
        BuildScoreBoard(pendingEntry);
        scoreBoard.SetActive(true);
        gameOverPanel.SetActive(true);

        gameOverButton.onClick.RemoveAllListeners();
        gameOverButton.onClick.AddListener(() => StartCoroutine(HideGameOverAndShowStart()));
        gameOverButton.gameObject.SetActive(true);
    }
}


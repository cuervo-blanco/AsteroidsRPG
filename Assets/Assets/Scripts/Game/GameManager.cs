using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public int pointsPerSecond = 10;

    [Header("UI")]
    public ScoreUI scoreUI;
    public LivesUI livesUI;
    public GameObject startPanel;
    public GameObject gameOverPanel;
    public Button startButton;

    [Header("RockControl")]
    public RockSpawner spawner;
    public RocketController rocket;

    enum State { Waiting, Playing, GameOver }
    State state = State.Waiting;

    float score;
    int lives;

    void Awake() {
        if (!rocket) rocket = FindObjectOfType<RocketController>();
        startButton.onClick.AddListener(StartGame);
        ShowStartScreen();
    }

    void ShowStartScreen() {
        spawner.Stop();
        state = State.Waiting;

        rocket.EnableControl(false);
        rocket.fireAnimator.gameObject.SetActive(false);

        startPanel.SetActive(true);
        gameOverPanel.SetActive(false);

        scoreUI.SetScore(0);
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
        state = State.Playing;

        score = 0f;
        scoreUI.SetScore(0);

        rocket.ResetLives();
        rocket.EnableControl(true);

        startPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        spawner.Begin();
    }

    IEnumerator GameOverRoutine() {
        spawner.Stop();
        state = State.GameOver;
        gameOverPanel.SetActive(true);

        yield return new WaitForSeconds(3f);
        ShowStartScreen();
    }

}


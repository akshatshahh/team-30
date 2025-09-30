using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public TextMeshProUGUI gameOverText;  // Canvas/GameOverText
    public bool pauseOnGameOver = true;

    void Awake()
    {
        // Start unpaused and hidden
        Time.timeScale = 1f;
        if (gameOverText) gameOverText.gameObject.SetActive(false);
    }

    public void ShowGameOver(string reason = null)
    {
        if (gameOverText)
        {
            gameOverText.text = string.IsNullOrEmpty(reason)
                ? "GAME OVER"
                : $"GAME OVER\n{reason}";
            gameOverText.gameObject.SetActive(true);
        }
        if (pauseOnGameOver) Time.timeScale = 0f;
    }
}
using UnityEngine;
using TMPro;

public class InstructionsUI : MonoBehaviour
{
    public TextMeshProUGUI instructionsText;   // drag Canvas/InstructionsText here
    public GameObject dimPanel;                // optional: Canvas/InstructionsPanel
    public float autoHideAfter = 6f;           // seconds (0 = never auto-hide)

    float timer;

    void Awake()
    {
        Show(true);
        timer = 0f;
        Time.timeScale = 1f; // ensure not paused
    }

    void Update()
    {
        timer += Time.unscaledDeltaTime;

        // Hide on first meaningful input
        if (Input.anyKeyDown
            || Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f
            || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f)
        {
            Show(false);
        }

        // Or auto-hide after timeout
        if (autoHideAfter > 0f && timer >= autoHideAfter)
            Show(false);
    }

    void Show(bool on)
    {
        if (instructionsText) instructionsText.gameObject.SetActive(on);
        if (dimPanel) dimPanel.SetActive(on);
    }
}
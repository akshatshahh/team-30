using UnityEngine;
using TMPro;

[RequireComponent(typeof(BoxCollider2D))]
public class FinishZoneWin : MonoBehaviour
{
    public string playerTag = "Player";
    public TextMeshProUGUI winText;   // drag the WinText from the Canvas here
    public bool pauseOnWin = false;

    bool used;

    void Awake()
    {
        // Make sure time isn't frozen and the label starts hidden
        Time.timeScale = 1f;
        if (winText)
        {
            Debug.Log($"[Win] Awake: winText ref OK? {(winText!=null)}  activeSelf={winText.gameObject.activeSelf}");
            winText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[Win] Awake: winText is NOT assigned.");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"FinishZone hit by: {other.name} tag:{other.tag}");
        if (used || !other.CompareTag(playerTag)) return;

        used = true;

        if (!winText)
        {
            Debug.LogError("[Win] No winText assigned on FinishZoneWin.");
            return;
        }

        // Force everything on
        var canvas = winText.canvas;
        if (canvas) canvas.gameObject.SetActive(true);

        winText.text = "CONGRATULATIONS!";
        winText.enabled = true;
        winText.gameObject.SetActive(true);

        Debug.Log($"[Win] After enable: activeSelf={winText.gameObject.activeSelf} activeInHierarchy={winText.gameObject.activeInHierarchy}");

        if (pauseOnWin) Time.timeScale = 0f;

        // prevent retrigger
        gameObject.SetActive(false);
    }
}
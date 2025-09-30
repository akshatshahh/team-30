using UnityEngine;
using UnityEngine.UIElements;

public class WinUiController : MonoBehaviour
{
    public UIDocument document;
    public GameObject confettiPrefab;
    public float confettiYOffset = 2f;
    public bool pauseOnWin = false;

    [Header("Emoji sprite")]
    public Sprite emojiSprite;   // assign party.png here in Inspector

    VisualElement overlay;
    Image emojiImg;
    bool shown;

    void Awake()
    {
        if (!document) document = GetComponent<UIDocument>();
    }

    void OnEnable()
    {
        var root = document.rootVisualElement;
        overlay = root.Q<VisualElement>("winRoot");
        emojiImg = root.Q<Image>("emoji");
        if (emojiImg != null && emojiSprite != null) emojiImg.sprite = emojiSprite;

        HideImmediate();
    }

    void HideImmediate()
    {
        if (overlay == null) return;
        overlay.style.display = DisplayStyle.None;
        overlay.style.opacity = 0f;
    }

    public void TriggerWin()
    {
        if (shown) return;
        shown = true;

        if (overlay != null)
        {
            overlay.style.display = DisplayStyle.Flex;
            overlay.schedule.Execute(() => overlay.style.opacity = 1f).StartingIn(10);
        }

        if (confettiPrefab)
        {
            var cam = Camera.main;
            var top = cam.ViewportToWorldPoint(new Vector3(0.5f, 1.1f, 0f));
            top.z = 0f; top.y += confettiYOffset;
            Instantiate(confettiPrefab, top, Quaternion.identity);
        }

        if (pauseOnWin) Time.timeScale = 0f;
    }
}
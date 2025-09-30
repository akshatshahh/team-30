using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class LoseZone : MonoBehaviour
{
    public string playerTag = "Player";
    public GameOverUI ui;
    [TextArea] public string reason = "You fell!";

    void Reset() { GetComponent<BoxCollider2D>().isTrigger = true; }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            if (ui) ui.ShowGameOver(reason);
            // Optional: hide player to make it obvious
            var sr = other.GetComponent<SpriteRenderer>(); if (sr) sr.enabled = false;
            var rb = other.attachedRigidbody; if (rb) rb.simulated = false;
        }
    }
}
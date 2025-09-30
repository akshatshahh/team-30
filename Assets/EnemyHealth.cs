using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("PlayerAttack"))
        {
            Destroy(gameObject); // 敌人消失
        }
    }
}


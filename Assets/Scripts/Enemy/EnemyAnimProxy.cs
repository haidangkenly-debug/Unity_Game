using UnityEngine;

public class EnemyAnimProxy : MonoBehaviour
{
    private EnemyAI enemyAI;

    private void Awake()
    {
        enemyAI = GetComponentInParent<EnemyAI>();
    }

    public void TriggerAttackHit()
    {
        if (enemyAI != null)
        {
            enemyAI.AnimEvent_AttackHit();
        }
    }
}
using UnityEngine;

public class AnimationBridge : MonoBehaviour
{
    private PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
    }
    
    public void AnimEvent_EnableCancel() => playerController.AnimEvent_EnableCancel();
    public void AnimEvent_FinishAttack() => playerController.AnimEvent_FinishAttack();
    public void AnimEvent_TriggerAttack() => playerController.AnimEvent_TriggerAttack();
}
using Mirror;
using UnityEngine;

public class DropItemAnimationSync : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnPickUpChanged))] 
    public bool pickedUp;

    Animator pickUpAnimator;

    public override void OnStartClient()
    {
        base.OnStartClient();

        pickUpAnimator = transform.GetChild(1).GetComponent<Animator>();
    }

    private void OnPickUpChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            PlayPickUpAnimation();
        }
    }

    private void PlayPickUpAnimation()
    {
        if (pickUpAnimator != null)
        {
            pickUpAnimator.SetTrigger("pickUp");
        }
    }
}

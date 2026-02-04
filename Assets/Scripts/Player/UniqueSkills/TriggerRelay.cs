using UnityEngine;

public class TriggerRelay : MonoBehaviour
{
    public EyeOfTheStorm eyeOfTheStorm;
    public bool isEpicenter = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (eyeOfTheStorm != null && collision.CompareTag("Enemy") && eyeOfTheStorm.isTornadoEnabled)
        {
            eyeOfTheStorm.ApplyStormEffect(collision, isEpicenter);
        }
    }
}

using UnityEngine;

public class DustTrail : MonoBehaviour
{
    float timer = 0;

    private void Update()
    {
        if (transform.parent == null)
        {
            timer += Time.deltaTime;

            if (timer > 4f)
            {
                Destroy(gameObject);
            }
        }
    }
}

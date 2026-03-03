using UnityEngine;

[ExecuteInEditMode]
public class MathPractice : MonoBehaviour
{
    public Vector3 a = Vector3.right;
    public Vector3 b = Vector3.forward;

    Vector3 lastA;
    Vector3 lastB;

    void Update()
    {
        // Only recalc if something changed
        if(a != lastA || b != lastB)
        {
            Vector3 cross = Vector3.Cross(a, b);

            Debug.Log($"A = {a}, B = {b}, A X B = {cross}");

            lastA = a;
            lastB = b;
        }
    }
}

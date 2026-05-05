using UnityEngine;

public class ShurikenProjectile : MonoBehaviour
{
    [Tooltip("Axe local de rotation (doit correspondre à celui du ShurikenThrow parent)")]
    public Vector3 spinAxis = Vector3.forward;

    [Tooltip("Vitesse de rotation (degrés/seconde)")]
    public float spinSpeed = 720f;

    [Tooltip("Durée de vie avant auto-destruction (secondes)")]
    public float lifetime = 6f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Rotate(spinAxis, spinSpeed * Time.deltaTime, Space.Self);
    }
}

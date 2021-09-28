using UnityEngine;

public class CreatureController : MonoBehaviour
{
    public float movementSpeed = 5f;

    void Update()
    {
        transform.Translate(Vector3.forward * movementSpeed * Time.deltaTime);
    }
}

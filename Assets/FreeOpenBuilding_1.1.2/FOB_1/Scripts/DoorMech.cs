using UnityEngine;
using UnityEngine.InputSystem;

public class DoorMech : MonoBehaviour
{
    public Vector3 OpenRotation, CloseRotation;
    public float rotSpeed = 1f;
    public bool doorBool;

    private bool playerNearby;

    void Start()
    {
        doorBool = false;
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player")) playerNearby = true;
    }

    void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Player")) playerNearby = false;
    }

    void Update()
    {
        if (playerNearby && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            doorBool = !doorBool;

        Quaternion target = doorBool ? Quaternion.Euler(OpenRotation) : Quaternion.Euler(CloseRotation);
        transform.rotation = Quaternion.Lerp(transform.rotation, target, rotSpeed * Time.deltaTime);
    }
}


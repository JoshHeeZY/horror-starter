using UnityEngine;
using UnityEngine.InputSystem;

public class DrawerMech : MonoBehaviour
{
    public Vector3 OpenPosition, ClosePosition;
    public bool drawerBool;

    private bool playerNearby;
    private float lerpTimer;

    void Start()
    {
        drawerBool = false;
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
            drawerBool = !drawerBool;

        float direction = drawerBool ? 1f : -1f;
        lerpTimer = Mathf.Clamp01(lerpTimer + Time.deltaTime * direction);
        transform.localPosition = Vector3.Lerp(ClosePosition, OpenPosition, lerpTimer);
    }
}


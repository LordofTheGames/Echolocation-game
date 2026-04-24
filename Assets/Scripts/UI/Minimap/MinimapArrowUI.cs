using UnityEngine;

public class MinimapArrowUI : MonoBehaviour
{
    public Transform player;

    void LateUpdate()
    {
        if (player == null) return;

        // float yRotation = player.eulerAngles.y;
        // transform.localEulerAngles = new Vector3(0, 0, -yRotation);
    }
}

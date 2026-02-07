using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DetectObjectOutline : MonoBehaviour
{
    [SerializeField] private Camera cam;
    // maximum distance the ray can reach
    [SerializeField] private float maxDistance = 30f;
    [SerializeField] private float sphereRadius = 0.28f;
    // prevents flickering
    [SerializeField] private float loseDelay = 0.12f;

    [SerializeField] private LayerMask interactMask = ~0; 
    [SerializeField] private GameObject pickupPanel; 

    private OutlineTarget current;
    private float lastValidHitTime;
    private InputAction interactAction;

    private void Awake()
    {
        if (!cam) cam = Camera.main;
        if (pickupPanel) pickupPanel.SetActive(false);
        interactAction = InputSystem.actions.FindAction("Interact");
    }

    private void Update()
    {
        if (interactAction.WasPressedThisFrame() && current != null)
        {
            if (pickupPanel) pickupPanel.SetActive(false);
            current.SetOutlined(false);

            var pickup = current.GetComponent<PickupItem>();
            if (!pickup) pickup = current.GetComponentInParent<PickupItem>();
            if (!pickup) pickup = current.GetComponentInChildren<PickupItem>();

            if (pickup != null)
            {
                pickup.Interact(); 
            }

            current = null;
            return; 
        }
        // create a ray from the center of the screen
        var ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        OutlineTarget best = FindBestTarget(ray);

        if (best != null)
            lastValidHitTime = Time.time;
        // if lost hit, keep old target for a short time
        if (best == null && current != null && Time.time - lastValidHitTime < loseDelay)
            best = current;

        if (best == current)
        {
            if (pickupPanel) pickupPanel.SetActive(current != null);
            return;
        }
        if (current) current.SetOutlined(false);
        current = best;
        if (current) current.SetOutlined(true);

        if (pickupPanel) pickupPanel.SetActive(current != null);
    }

        
    private OutlineTarget FindBestTarget(Ray ray)
    {
        var hits = Physics.SphereCastAll(ray, sphereRadius, maxDistance, interactMask, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0) return null;

        // track the best target and its score (lower = better)
        OutlineTarget best = null;
        float bestScore = float.PositiveInfinity;

        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i].collider;
            if (!col) continue;

            var t = col.GetComponentInParent<OutlineTarget>();
            if (!t) continue;

            // vector from camera to the hit point
            Vector3 to = hits[i].point - ray.origin;
            float distance = to.magnitude;
            // how far the hit is from the center ray (smaller = closer to where the player is looking)
            float centerError = Vector3.Cross(ray.direction, to).magnitude;
            // objects that are closer to the center of the screen and not too far away. 
            float score = centerError * 2f + distance * 1f;

            if (score < bestScore)
            {
                bestScore = score;
                best = t;
            }
        }

        return best;
    }
    public void SetEnabled(bool value)
    {
        enabled = value;
        if (!value) ClearAll();
    }

    private void OnDisable()
    {
        ClearAll();
    }

    private void ClearAll()
    {
        if (current) current.SetOutlined(false);
        current = null;

        if (pickupPanel) pickupPanel.SetActive(false);
    }
}

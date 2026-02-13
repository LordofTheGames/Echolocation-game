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
    public bool ignoreLiftChain;

    private OutlineTarget current;
    private float lastValidHitTime;
    private InputAction interactAction;
    [SerializeField] private GameObject pullPanel;
    [SerializeField] private GameObject pickupPanel;
    [SerializeField] private GameObject hidePanel;
    private GameObject currentPanel;

    private void Awake()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
        if (pickupPanel) pickupPanel.SetActive(false);
        if (hidePanel) hidePanel.SetActive(false);
        if (pullPanel) pullPanel.SetActive(false);
        currentPanel = pickupPanel;
    }

    private void Update()
    {
        if (interactAction.WasPressedThisFrame() && current != null)
        {
            PickupItem pickup = current.GetComponentInParent<PickupItem>(); 
            HideInBox hide = current.GetComponentInParent<HideInBox>();

            if (pickup != null)
            {
                currentPanel.SetActive(false);
                current.SetOutlined(false);
                pickup.Interact(); 
                current = null;
            }
            else if (hide != null)
            {
                hide.Interact(this.gameObject); 
            }

            return; 
        }
        var ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        OutlineTarget best = FindBestTarget(ray);

        if (best != null)
            lastValidHitTime = Time.time;

        if (best == null && current != null && Time.time - lastValidHitTime < loseDelay)
            best = current;

        if (best != current)
        {
            if (current) current.SetOutlined(false);
            DisableAllPanels();

            current = best;

            if (current != null)
            {
                current.SetOutlined(true);

                if (current.gameObject.name == "Lift chain")
                    pullPanel.SetActive(true);
                else if (current.gameObject.name == "Hide Trigger")
                    hidePanel.SetActive(true);
                else
                    pickupPanel.SetActive(true);
            }
        }
    }
private void DisableAllPanels()
{
    if (pullPanel) pullPanel.SetActive(false);
    if (pickupPanel) pickupPanel.SetActive(false);
    if (hidePanel) hidePanel.SetActive(false);
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

            // don't outline lift chain if flag set to true 
            // (stops chain being outlined when lift is in motion or when player is outside lift)
            // flag is set/unset in the LiftControl script attatched to the Controller child of the Lift GameObject
            if (col.gameObject.name == "Lift chain" && ignoreLiftChain)
                continue;
            
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

        if (currentPanel) currentPanel.SetActive(false);
    }
}

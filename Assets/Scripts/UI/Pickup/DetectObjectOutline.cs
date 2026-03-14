using UnityEngine;
using UnityEngine.InputSystem;

public class DetectObjectOutline : MonoBehaviour
{
    [SerializeField] private Camera cam;
    // maximum distance the ray can reach
    [SerializeField] private float maxDistance = 5f;
    [SerializeField] private float sphereRadius = 0.28f;
    // prevents flickering
    [SerializeField] private float loseDelay = 0.12f;

    [SerializeField] private LayerMask interactMask = ~0; 
    [SerializeField] private GameObject pickupPanel; 
    [SerializeField] private GameObject pullPanel; 
    [SerializeField] private GameObject gateHintPanel;   
    [SerializeField] private GameObject gateUnlockPanel;
    [SerializeField] private GameObject doorOpenPanel;
    [SerializeField] private GameObject doorClosePanel;
    [SerializeField] private GameObject breakablePitchPanel;
    [SerializeField] private GameObject breakableVolumePanel;
    [SerializeField] private GameObject hidePanel;
    [SerializeField] private GameObject exitHidePanel;
    public bool ignoreLiftChain;

    private OutlineTarget current;
    private float lastValidHitTime;
    private GameObject currentPanel;
    private bool isHiding;
    private HideInBox currHideBox;

    private void Awake()
    {
        if (pickupPanel) pickupPanel.SetActive(false);
        if (pullPanel) pullPanel.SetActive(false);
        if (gateHintPanel) gateHintPanel.SetActive(false);
        if (gateUnlockPanel) gateUnlockPanel.SetActive(false);
        if (doorOpenPanel) doorOpenPanel.SetActive(false);
        if (doorClosePanel) doorClosePanel.SetActive(false);
        if (breakablePitchPanel) breakablePitchPanel.SetActive(false);
        if (breakableVolumePanel) breakableVolumePanel.SetActive(false);
        if (hidePanel) hidePanel.SetActive(false);
        if (exitHidePanel) exitHidePanel.SetActive(false);

        currentPanel = pickupPanel;
    }

    private void ShowOnly(GameObject panel)
    {
        if (pickupPanel) pickupPanel.SetActive(false);
        if (pullPanel) pullPanel.SetActive(false);
        if (gateHintPanel) gateHintPanel.SetActive(false);
        if (gateUnlockPanel) gateUnlockPanel.SetActive(false);
        if (breakablePitchPanel) breakablePitchPanel.SetActive(false);
        if (breakableVolumePanel) breakableVolumePanel.SetActive(false);
        if (hidePanel) hidePanel.SetActive(false);
        if (exitHidePanel) exitHidePanel.SetActive(false);
        if (doorOpenPanel) doorOpenPanel.SetActive(false);
        if (doorClosePanel) doorClosePanel.SetActive(false);

        currentPanel = panel;
        if (currentPanel) currentPanel.SetActive(true);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && current != null)
        {
            var gate = current.GetComponentInParent<Gate>();
            if(gate == null) PerformInteract();
        }
        else if (context.started && isHiding == true && current == null)
        {
            currHideBox.Interact();
            isHiding = false; 
            ShowOnly(null);
        }
    }
    public void OnUnlock(InputAction.CallbackContext context)
    {
        if (!context.performed || current == null) return;

        if (currentPanel) currentPanel.SetActive(false);
        current.SetOutlined(false);

        var gate = current.GetComponentInParent<Gate>();
        if (gate != null)
            gate.TryUnlock();

        current = null;
    }

    private void PerformInteract()
    {
        currentPanel.SetActive(false);
        current.SetOutlined(false);

        var door = current.GetComponent<LabDoor>();
        if (!door) door = current.GetComponentInParent<LabDoor>();
        if (!door) door = current.GetComponentInChildren<LabDoor>();

        if (door != null)
        {
            door.Interact();
            current = null;
            return;
        }

        var pickup = current.GetComponent<PickupItem>();
        if (!pickup) pickup = current.GetComponentInParent<PickupItem>();
        if (!pickup) pickup = current.GetComponentInChildren<PickupItem>();

        var hide = current.GetComponent<HideInBox>();
        if (!hide) hide = current.GetComponentInParent<HideInBox>();
        if (!hide) hide = current.GetComponentInChildren<HideInBox>();

        if (pickup != null)
        {
            pickup.Interact();
        }
        else if (hide != null && isHiding == false)
        {
            currHideBox = hide;
            hide.Interact();
            isHiding = true;
        }

        current = null;
    }

    private void Update()
    {
        // create a ray from the center of the screen
        var ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        OutlineTarget best = FindBestTarget(ray);

        if (best != null)
        {
            if (best.gameObject.name == "Lift chain")
            {
                ShowOnly(pullPanel);
            }
            else if (best.gameObject.CompareTag("BreakablePitch"))
            {
                ShowOnly(breakablePitchPanel);
            }
            else if (best.gameObject.CompareTag("BreakableVolume"))
            {
                ShowOnly(breakableVolumePanel);
            }
            else if (best.gameObject.CompareTag("Hide"))
            {
                ShowOnly(hidePanel);
            }
            else
            {
                var gate = best.GetComponentInParent<Gate>();
                if (gate != null)
                {
                    ShowOnly(gate.HasKey() ? gateUnlockPanel : gateHintPanel);
                }
                else
                {
                    var door = best.GetComponentInParent<LabDoor>();
                    if (door != null)
                    {
                        if (door.IsMoving)
                        {
                            ShowOnly(null);
                        }
                        else if (door.IsOpen)
                        {
                            ShowOnly(doorClosePanel);
                        }
                        else
                        {
                            ShowOnly(doorOpenPanel);
                        }
                    }
                    else
                    {
                        ShowOnly(pickupPanel);
                    }
                }
            }
        }
        else if (isHiding)
        {
            ShowOnly(exitHidePanel);
        }

        if (best != null)
            lastValidHitTime = Time.time;
        // if lost hit, keep old target for a short time
        if (best == null && current != null && Time.time - lastValidHitTime < loseDelay)
            best = current;

        if (best == current)
        {
            return;
        }
        if (current) current.SetOutlined(false);
        current = best;
        if (current)
        {
            current.SetOutlined(true);
        }
        else
        {
            ShowOnly(null);
        }
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

        ShowOnly(null);

        lastValidHitTime = 0f;
    }
}

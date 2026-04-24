// using System.Numerics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class DetectObjectOutline : MonoBehaviour
{
    [SerializeField] private Camera cam;
    // maximum distance the ray can reach
    [SerializeField] private float maxDistance = 5f;
    // [SerializeField] private float sphereRadius = 0.28f;
    // prevents flickering
    [SerializeField] private float loseDelay = 0.12f;
    [SerializeField] private float pickupAngle = 90f;

    [SerializeField] private LayerMask interactMask = ~0; 
    [SerializeField] private LayerMask obstacleMask = ~0; 
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
    [SerializeField] private ThrowItem throwItem;
    public bool ignoreLiftChain;

    private OutlineTarget current;
    private float lastValidHitTime;
    private GameObject currentPanel;
    private bool isHiding;
    private HideInBox currHideBox;

    private void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!throwItem) throwItem = GetComponent<ThrowItem>();
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

    private T FindInTarget<T>(OutlineTarget target) where T : Component
    {
        if (!target) return null;

        T comp = target.GetComponent<T>();
        if (!comp) comp = target.GetComponentInParent<T>();
        if (!comp) comp = target.GetComponentInChildren<T>();

        return comp;
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

        var button = FindInTarget<DoorButton>(current);
        var pickup = FindInTarget<PickupItem>(current);
        var hide = FindInTarget<HideInBox>(current);
        var crazyReset = FindInTarget<ResetCrazyEffect>(current);

        if (button != null)
        {
            button.Interact();
            current = null;
            return;
        }
        else if (pickup != null)
        {
            pickup.Interact();
        }
        else if (hide != null && isHiding == false)
        {
            currHideBox = hide;
            hide.Interact();
            isHiding = true;
        }
        else if (crazyReset != null)
        {
            crazyReset.ResetEffect();
        }

        current = null;
    }

    private void Update()
    {
        if (!cam) return;
        if (throwItem != null && throwItem.IsHoldingThrowItem)
        {
            ClearAll();
            return;
        }

        OutlineTarget best = FindBestTarget();

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
            else if (best.gameObject.CompareTag("Button"))
            {
                var button = FindInTarget<DoorButton>(best);
                if (button == null || !button.CanInteract || button.IsDoorMoving)
                {
                    ShowOnly(null);
                }
                else if (button.IsDoorOpen)
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
                var gate = best.GetComponentInParent<Gate>();
                if (gate != null)
                {
                    ShowOnly(gate.HasKey() ? gateUnlockPanel : gateHintPanel);
                }
                else
                {
                    ShowOnly(pickupPanel);
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

        
    private OutlineTarget FindBestTarget()
    {
        Vector3 origin = cam.transform.position;
        Vector3 forward = cam.transform.forward;

        Collider[] hits = Physics.OverlapSphere(origin, maxDistance, interactMask, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0) return null;

        // track the best target and its score (lower = better)
        OutlineTarget best = null;
        float bestScore = float.PositiveInfinity;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider col = hits[i];
            if (!col) continue;

            // don't outline lift chain if flag set to true 
            // (stops chain being outlined when lift is in motion or when player is outside lift)
            // flag is set/unset in the LiftControl script attatched to the Controller child of the Lift GameObject
            if (col.gameObject.name == "Lift chain" && ignoreLiftChain)
                continue;

            OutlineTarget t = col.GetComponentInParent<OutlineTarget>();
            if (!t) continue;

            var button = FindInTarget<DoorButton>(t);
            if (button != null && !button.CanInteract)
            {
                continue;
            }

            Vector3 targetPoint = GetBestTargetPoint(col, origin);
            Vector3 toTarget = targetPoint - origin;
            float distance = toTarget.magnitude;

            if (distance > maxDistance || distance <= 0.001f)
                continue;

            Vector3 dirToTarget = toTarget.normalized;

            float angle = Vector3.Angle(forward, dirToTarget);
            if (angle > pickupAngle)
                continue;
            if (!HasLineOfSight(col, origin))
                continue;

            float angleScore = angle * 2f;
            float distanceScore = distance;
            float score = angleScore + distanceScore;

            if (score < bestScore)
            {
                bestScore = score;
                best = t;
            }
        }
        return best;
    }
        private Vector3 GetBestTargetPoint(Collider col, Vector3 origin)
    {
        Bounds b = col.bounds;

        Vector3 center = b.center;
        Vector3 upperCenter = new Vector3(b.center.x, b.center.y + b.extents.y * 0.5f, b.center.z);
        Vector3 top = new Vector3(b.center.x, b.max.y, b.center.z);

        float centerHeightDiff = Mathf.Abs(origin.y - center.y);
        float upperHeightDiff = Mathf.Abs(origin.y - upperCenter.y);
        float topHeightDiff = Mathf.Abs(origin.y - top.y);

        Vector3 best = center;
        float bestDiff = centerHeightDiff;

        if (upperHeightDiff < bestDiff)
        {
            best = upperCenter;
            bestDiff = upperHeightDiff;
        }

        if (topHeightDiff < bestDiff)
        {
            best = top;
        }

        return best;
    }

    private bool HasLineOfSight(Collider col, Vector3 origin)
    {
        Bounds b = col.bounds;

        Vector3[] testPoints = new Vector3[]
        {
            b.center,
            new Vector3(b.center.x, b.center.y + b.extents.y * 0.35f, b.center.z),
            new Vector3(b.center.x, b.max.y, b.center.z)
        };

        for (int i = 0; i < testPoints.Length; i++)
        {
            Vector3 dir = testPoints[i] - origin;
            float dist = dir.magnitude;
            if (dist <= 0.001f) return true;

            if (!Physics.Raycast(origin, dir.normalized, dist, obstacleMask, QueryTriggerInteraction.Ignore))
                return true;
        }

        return false;
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
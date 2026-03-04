using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ThrowItem : MonoBehaviour
{
    [System.Serializable]
    public struct ItemPrefab
    {
        public ItemType type;
        public GameObject prefab;
    }

    [SerializeField] private ItemPrefab[] itemPrefabs;
    [Header("Throwing settings")]
    [SerializeField] private float throwForce = 15f;
    [SerializeField] private float throwHeight = 0.5f;
    [SerializeField] private float landingIndicatorSize = 1f;
    [SerializeField] private float throwSpinSpeed = 3f;  

    [Header("Position settings")]
    [SerializeField] private Transform cubeSpawnPoint;
    [SerializeField] private Vector3 defaultSpawnOffset = new Vector3(0f, -0.2f, 0.7f);

    [Header("Parabolic curve settings")]
    [SerializeField] private float trajectoryWidth = 0.05f;
    [SerializeField] private float trajectoryTimeStep = 0.02f;
    [SerializeField] private int maxTrajectorySteps = 500;

    [Header("Indicator settings")]
    [SerializeField] private GameObject landingIndicatorPrefab;

    private GameObject currentObj;
    private GameObject landingIndicator;
    
    private LineRenderer trajectoryLine;
    private bool isHoldingRightClick = false;
    private Transform cameraTransform;
    private List<Vector3> trajectoryPointsList = new List<Vector3>();
    private Vector3 landingPosition;
    private Vector3 throwDirection;
    private ItemType holdingType;

    private float scrollInput;


    private void Start()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
            cameraTransform = mainCam.transform;

        if (cubeSpawnPoint == null)
        {
            GameObject spawnPointObj = new GameObject("ThrowSpawnPoint");
            spawnPointObj.transform.SetParent(cameraTransform);
            spawnPointObj.transform.localPosition = defaultSpawnOffset;
            cubeSpawnPoint = spawnPointObj.transform;
        }

        CreateTrajectoryLine();
        landingIndicator = Instantiate(landingIndicatorPrefab);
        trajectoryLine.enabled = false;
        landingIndicator.SetActive(false);
    }

    public void OnThrow(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            StartHolding();
        }
        else if (context.canceled)
        {
            StartThrowing();
        }
    }

    public void OnChangeThrowDistance(InputAction.CallbackContext context)
    {
        scrollInput = context.ReadValue<Vector2>().y;
    }

    private void Update()
    {
        if (isHoldingRightClick)
        {
            UpdateHolding();

            if (Mathf.Abs(scrollInput) > 0.01f)
            {
                throwForce += scrollInput * 0.5f;
                scrollInput = 0f; // Reset to prevent infinite adding
            }
        }
    }

    private static readonly Color TrajectoryBrightRed = new Color(1f, 0.2f, 0.2f, 1f);

    private void CreateTrajectoryLine()
    {
        GameObject lineObj = new GameObject("TrajectoryLine");
        trajectoryLine = lineObj.AddComponent<LineRenderer>();

        trajectoryLine.positionCount = 0;
        trajectoryLine.startWidth = trajectoryWidth;
        trajectoryLine.endWidth = trajectoryWidth * 0.5f;
        trajectoryLine.startColor = TrajectoryBrightRed;
        trajectoryLine.endColor = TrajectoryBrightRed;
        trajectoryLine.material = CreateUnlitTrajectoryMaterial();
        trajectoryLine.textureMode = LineTextureMode.Tile;
        trajectoryLine.numCapVertices = 5;
        trajectoryLine.enabled = false;
    }

    private Material CreateUnlitTrajectoryMaterial()
    {
        Shader unlit = Shader.Find("Universal Render Pipeline/Unlit");
        Material mat = new Material(unlit);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", TrajectoryBrightRed);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", TrajectoryBrightRed);
        return mat;
    }

    private void ForceTrajectoryLineColor()
    {
        if (trajectoryLine == null) return;
        trajectoryLine.startColor = TrajectoryBrightRed;
        trajectoryLine.endColor = TrajectoryBrightRed;
        Material mat = trajectoryLine.material;
        if (mat != null)
        {
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", TrajectoryBrightRed);
            if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", TrajectoryBrightRed);
        }
    }

    private bool CanThrowSelected(out ItemType selected)
    {
        selected = default;

        if (InventoryManager.Instance == null) return false;

        selected = InventoryManager.Instance.Selected;
        return InventoryManager.Instance.GetCount(selected) > 0;
    }

    private GameObject GetPrefab(ItemType type)
    {
        for (int i = 0; i < itemPrefabs.Length; i++)
        {
            if (itemPrefabs[i].type == type)
                return itemPrefabs[i].prefab;
        }
        return null;
    }
    private void StartHolding()
    {
        if (!CanThrowSelected(out holdingType))
        return;

        GameObject prefab = GetPrefab(holdingType);
        if (prefab == null)
        {
        return;
        }
        
        isHoldingRightClick = true;

        if (currentObj == null)
        {
            currentObj = Instantiate(prefab, cubeSpawnPoint.position, Quaternion.identity);
            currentObj.transform.SetParent(cubeSpawnPoint);
        }

        trajectoryLine.enabled = true;
        landingIndicator.SetActive(true);
    }

    private void UpdateHolding()
    {
        if (currentObj != null)
        {
            currentObj.transform.position = cubeSpawnPoint.position;
            currentObj.transform.rotation = cubeSpawnPoint.rotation;
        }

        UpdateTrajectory();
    }

    private void UpdateTrajectory()
    {
        if (trajectoryLine == null || currentObj == null) return;

        ForceTrajectoryLineColor();
        throwDirection = CalculateThrowDirection();

        trajectoryPointsList.Clear();
        Vector3 currentPos = currentObj.transform.position;
        trajectoryPointsList.Add(currentPos);
        Vector3 currentVel = throwDirection * throwForce;

        for (int i = 0; i < maxTrajectorySteps; i++)
        {
            currentVel += Physics.gravity * trajectoryTimeStep;
            currentPos += currentVel * trajectoryTimeStep;

            Vector3 lastPoint = trajectoryPointsList[trajectoryPointsList.Count - 1];
            Vector3 rayDir = currentPos - lastPoint;
            float rayDist = rayDir.magnitude;

            if (Physics.Raycast(lastPoint, rayDir.normalized, out RaycastHit hit, rayDist))
            {
                landingPosition = hit.point;
                trajectoryPointsList.Add(landingPosition);
                trajectoryLine.positionCount = trajectoryPointsList.Count;
                trajectoryLine.SetPositions(trajectoryPointsList.ToArray());
                UpdateLandingIndicator(landingPosition, hit.normal);
                return;
            }

            trajectoryPointsList.Add(currentPos);
        }

        trajectoryLine.positionCount = trajectoryPointsList.Count;
        trajectoryLine.SetPositions(trajectoryPointsList.ToArray());
        landingPosition = trajectoryPointsList[trajectoryPointsList.Count - 1];
        UpdateLandingIndicator(landingPosition, Vector3.up);
    }

    private Vector3 CalculateThrowDirection()
    {
        if (cameraTransform == null) return Vector3.forward;

        Vector3 direction = cameraTransform.forward;
        direction.y += throwHeight;

        return direction.normalized;
    }

    private void UpdateLandingIndicator(Vector3 position, Vector3 normal)
    {
        if (landingIndicator == null || cameraTransform == null) return;

        landingIndicator.transform.position = position + normal * 0.1f;
        landingIndicator.transform.rotation = Quaternion.LookRotation(normal);
        landingIndicator.transform.Rotate(90f, 0f, 0f);

        float distance = Vector3.Distance(position, cameraTransform.position);
        float scale = Mathf.Lerp(0.3f, 1.5f, distance * landingIndicatorSize / 50);
        landingIndicator.transform.localScale = new Vector3(scale, 0.05f, scale);
    }

    private void StartThrowing()
    {
        if (!isHoldingRightClick || currentObj == null) return;

        throwDirection = CalculateThrowDirection();

        if (InventoryManager.Instance == null || !InventoryManager.Instance.TryConsume(holdingType, 1))
        {
            CancelHolding(); // no item -> cancel
            return;
        }

        currentObj.transform.SetParent(null);
        var echo = currentObj.GetComponent<CollisionEcho>();
        if (echo == null) echo = currentObj.GetComponentInChildren<CollisionEcho>(true);
        if (echo != null) echo.Arm();

        var timedEmitter = currentObj.GetComponent<TimedEchoEmitter>();
        if (timedEmitter == null) timedEmitter = currentObj.GetComponentInChildren<TimedEchoEmitter>(true);
        if (timedEmitter != null) timedEmitter.SetArmOnNextCollision();

        var sonicGrenade = currentObj.GetComponent<SonicGrenade>();
        if (sonicGrenade == null) sonicGrenade = currentObj.GetComponentInChildren<SonicGrenade>(true);
        if (sonicGrenade != null) sonicGrenade.Arm();

        Rigidbody rb = currentObj.GetComponent<Rigidbody>();
        if (rb == null) rb = currentObj.GetComponentInChildren<Rigidbody>(true);
        if (rb == null)
        {
            rb = currentObj.AddComponent<Rigidbody>();
            Debug.LogWarning("[ThrowItem] No Rigidbody on throwable prefab '" + currentObj.name + "'. Added one at runtime. Add a Rigidbody to the prefab for correct behaviour.");
        }

    if (cameraTransform != null)
        rb.position = cameraTransform.position + cameraTransform.forward * 0.8f;

    rb.isKinematic = false;
    rb.useGravity = true;

    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    rb.interpolation = RigidbodyInterpolation.Interpolate;

    rb.linearVelocity = Vector3.zero;
    rb.angularVelocity = Vector3.zero;

    rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);

    Vector3 right = Vector3.Cross(throwDirection, Vector3.up).normalized;
    if (right.sqrMagnitude < 0.01f) right = Vector3.Cross(throwDirection, Vector3.forward).normalized;
    rb.angularVelocity = right * throwSpinSpeed + Vector3.up * (throwSpinSpeed * 0.5f);

    currentObj = null;

    trajectoryLine.enabled = false;
    landingIndicator.SetActive(false);
    isHoldingRightClick = false;
}
    

    private void CancelHolding()
    {
        if (currentObj != null)
        {
            Destroy(currentObj);
            currentObj = null;
        }

        if (trajectoryLine != null) trajectoryLine.enabled = false;

        if (landingIndicator != null) landingIndicator.SetActive(false);

        isHoldingRightClick = false;
    }
    private void OnDestroy()
    {
        if (currentObj != null) Destroy(currentObj);
        if (trajectoryLine != null) Destroy(trajectoryLine.gameObject);
        if (landingIndicator != null) Destroy(landingIndicator);
    }
}
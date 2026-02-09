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
    // [SerializeField] private GameObject cubePrefab;
    [SerializeField] private float throwForce = 15f;
    [SerializeField] private float throwHeight = 0.5f;
    [SerializeField] private float landingIndicatorSize = 1f;
    [SerializeField] private float throwSpinSpeed = 3f;  

    [Header("Position settings")]
    [SerializeField] private Transform cubeSpawnPoint;
    [SerializeField] private Vector3 defaultSpawnOffset = Vector3.zero;

    [Header("Parabolic curve settings")]
    [SerializeField] private float trajectoryWidth = 0.05f;
    [SerializeField] private float trajectoryTimeStep = 0.02f;
    [SerializeField] private int maxTrajectorySteps = 500;

    [Header("Indicator settings")]
    [SerializeField] private GameObject landingIndicatorPrefab;

    // private GameObject currentCube;       
    private GameObject currentObj;
    private GameObject landingIndicator;
    
    private LineRenderer trajectoryLine;
    private bool isHoldingRightClick = false;
    private Transform cameraTransform;
    private List<Vector3> trajectoryPointsList = new List<Vector3>();
    private Vector3 landingPosition;
    private Vector3 throwDirection;

    InputAction throwAction;
    InputAction changeThrowDistance;
    private ItemType holdingType;

    private void Start()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
            cameraTransform = mainCam.transform;

        throwAction = InputSystem.actions.FindAction("Throw");
        changeThrowDistance = InputSystem.actions.FindAction("Change throw distance");

        if (cubeSpawnPoint == null)
        {
            // GameObject spawnPointObj = new GameObject("CubeSpawnPoint");
            GameObject spawnPointObj = new GameObject("ThrowSpawnPoint");
            spawnPointObj.transform.SetParent(cameraTransform);
            spawnPointObj.transform.localPosition = defaultSpawnOffset;
            cubeSpawnPoint = spawnPointObj.transform;
        }

        CreateTrajectoryLine();

        CreateLandingIndicator();

        trajectoryLine.enabled = false;
        landingIndicator.SetActive(false);
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
        Shader unlit = Shader.Find("Universal Render Pipeline/Unlit")
            ?? Shader.Find("Unlit/Color")
            ?? Shader.Find("Sprites/Default");
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

    private void CreateLandingIndicator()
    {
        if (landingIndicatorPrefab != null)
        {
            landingIndicator = Instantiate(landingIndicatorPrefab);
        }
        else
        {
            landingIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            landingIndicator.name = "LandingIndicator";

            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f, 0.5f, 0f, 0.7f);
            mat.SetFloat("_Mode", 3); 
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;

            landingIndicator.GetComponent<Renderer>().material = mat;

            Destroy(landingIndicator.GetComponent<Collider>());
        }

        landingIndicator.SetActive(false);
    }

    private void Update()
    {
        if (throwAction == null) return;
        if (throwAction.WasPressedThisFrame())
            StartHolding();

        if (throwAction.IsPressed() && isHoldingRightClick)

            UpdateHolding();

        if (throwAction.WasReleasedThisFrame() && isHoldingRightClick)
            StartThrowing();

        // if (throwAction.IsPressed())
        //     throwForce += changeThrowDistance.ReadValue<Vector2>().y * 0.5f;
        if (throwAction.IsPressed() && changeThrowDistance != null)
            throwForce += changeThrowDistance.ReadValue<Vector2>().y * 0.5f;
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

            Rigidbody rb = currentObj.GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true;

            MeshRenderer cubeRenderer = currentObj.GetComponent<MeshRenderer>();
            if (cubeRenderer != null)
            {
                Material cubeMat = cubeRenderer.material;
                Color cubeColor = cubeMat.color;
                cubeColor.a = 0.8f;
                cubeMat.color = cubeColor;
            }
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

            // RaycastHit hit;
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
        // if (!isHoldingRightClick || currentCube == null) return;
        if (!isHoldingRightClick || currentObj == null) return;

        throwDirection = CalculateThrowDirection();

        if (InventoryManager.Instance == null || !InventoryManager.Instance.TryConsume(holdingType, 1))
        {
            CancelHolding(); // no item -> cancel
            return;
        }

        currentObj.transform.SetParent(null);

        // Enable physics, add spin, and apply throw force so the rock rolls and rotates in the air
        Rigidbody rb = currentObj.GetComponent<Rigidbody>();
            if (rb == null) rb = currentObj.GetComponentInChildren<Rigidbody>(true);

    if (rb == null)
    {
        Debug.LogError("[ThrowItem] Throw failed: Rigidbody not found on object/root children.");
        CancelHolding();
        return;
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
        // if (rb != null)
        // {
        //     rb.isKinematic = false;
        //     rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        //     // Spin like before: rotation in the air as you throw
        //     Vector3 right = Vector3.Cross(throwDirection, Vector3.up).normalized;
        //     if (right.sqrMagnitude < 0.01f) right = Vector3.Cross(throwDirection, Vector3.forward).normalized;
        //     rb.angularVelocity = right * throwSpinSpeed + Vector3.up * (throwSpinSpeed * 0.5f);
        // }

        // // Release reference so the rock stays in the world; next throw will spawn a new one
        // currentObj = null;

        // trajectoryLine.enabled = false;
        // landingIndicator.SetActive(false);
        // isHoldingRightClick = false;
    

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

    // private void OnDrawGizmosSelected()
    // {
    //     if (cubeSpawnPoint != null)
    //     {
    //         Gizmos.color = Color.green;
    //         Gizmos.DrawWireSphere(cubeSpawnPoint.position, 0.1f);
    //         Gizmos.DrawLine(cubeSpawnPoint.position, cubeSpawnPoint.position + cubeSpawnPoint.forward * 0.3f);
    //     }
    // }
    private void OnDestroy()
    {
        if (currentObj != null) Destroy(currentObj);
        if (trajectoryLine != null) Destroy(trajectoryLine.gameObject);
        if (landingIndicator != null) Destroy(landingIndicator);
    }
}
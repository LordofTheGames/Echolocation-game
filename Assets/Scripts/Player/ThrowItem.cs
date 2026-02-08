using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ThrowItem : MonoBehaviour
{
    [Header("Throwing settings")]
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private float throwForce = 15f;
    [SerializeField] private float throwHeight = 0.5f;
    [SerializeField] private float landingIndicatorSize = 1f;
    [SerializeField] private float throwSpinSpeed = 3f;  // 投掷时旋转角速度，可在 Inspector 调整

    [Header("Position settings")]
    [SerializeField] private Transform cubeSpawnPoint;       
    [SerializeField] private Vector3 defaultSpawnOffset = Vector3.zero;

    [Header("Parabolic curve settings")]
    [SerializeField] private float trajectoryWidth = 0.05f;
    [SerializeField] private float trajectoryTimeStep = 0.02f;  
    [SerializeField] private int maxTrajectorySteps = 500;   

    [Header("Indicator settings")]
    [SerializeField] private GameObject landingIndicatorPrefab; 

    private GameObject currentCube;              
    private GameObject landingIndicator;         
    private LineRenderer trajectoryLine;         
    private bool isHoldingRightClick = false;    
    private Transform cameraTransform;
    private List<Vector3> trajectoryPointsList = new List<Vector3>();
    private Vector3 landingPosition;             
    private Vector3 throwDirection;              

    InputAction throwAction;
    InputAction changeThrowDistance;

    private void Start()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
            cameraTransform = mainCam.transform;

        throwAction = InputSystem.actions.FindAction("Throw");
        changeThrowDistance = InputSystem.actions.FindAction("Change throw distance");

        if (cubeSpawnPoint == null)
        {
            GameObject spawnPointObj = new GameObject("CubeSpawnPoint");
            spawnPointObj.transform.SetParent(cameraTransform);
            spawnPointObj.transform.localPosition = defaultSpawnOffset;
            cubeSpawnPoint = spawnPointObj.transform;
        }

        CreateTrajectoryLine();

        CreateLandingIndicator();
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
        if (throwAction.WasPressedThisFrame())
            StartHolding();

        if (throwAction.IsPressed() && isHoldingRightClick)
            UpdateHolding();

        if (throwAction.WasReleasedThisFrame() && isHoldingRightClick)
            StartThrowing();

        if (throwAction.IsPressed())
            throwForce += changeThrowDistance.ReadValue<Vector2>().y * 0.5f;
    }

    private void StartHolding()
    {
        isHoldingRightClick = true;

        if (cubePrefab != null && currentCube == null)
        {
            currentCube = Instantiate(cubePrefab, cubeSpawnPoint.position, Quaternion.identity);
            currentCube.transform.SetParent(cubeSpawnPoint);

            Rigidbody rb = currentCube.GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true;

            MeshRenderer cubeRenderer = currentCube.GetComponent<MeshRenderer>();
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
        if (currentCube != null)
        {
            currentCube.transform.position = cubeSpawnPoint.position;
            currentCube.transform.rotation = cubeSpawnPoint.rotation;
        }

        UpdateTrajectory();
    }

    private void UpdateTrajectory()
    {
        if (trajectoryLine == null || currentCube == null) return;

        ForceTrajectoryLineColor();
        throwDirection = CalculateThrowDirection();

        trajectoryPointsList.Clear();
        Vector3 currentPos = currentCube.transform.position;
        trajectoryPointsList.Add(currentPos);
        Vector3 currentVel = throwDirection * throwForce;

        for (int i = 0; i < maxTrajectorySteps; i++)
        {
            currentVel += Physics.gravity * trajectoryTimeStep;
            currentPos += currentVel * trajectoryTimeStep;

            Vector3 lastPoint = trajectoryPointsList[trajectoryPointsList.Count - 1];
            Vector3 rayDir = currentPos - lastPoint;
            float rayDist = rayDir.magnitude;

            RaycastHit hit;
            if (Physics.Raycast(lastPoint, rayDir.normalized, out hit, rayDist))
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
        if (landingIndicator == null) return;

        landingIndicator.transform.position = position + normal * 0.1f;
        landingIndicator.transform.rotation = Quaternion.LookRotation(normal);
        landingIndicator.transform.Rotate(90f, 0f, 0f);

        float distance = Vector3.Distance(position, cameraTransform.position);
        float scale = Mathf.Lerp(0.3f, 1.5f, distance * landingIndicatorSize / 50);
        landingIndicator.transform.localScale = new Vector3(scale, 0.05f, scale);
    }

    private void StartThrowing()
    {
        if (!isHoldingRightClick || currentCube == null) return;

        currentCube.transform.SetParent(null);

        // Enable physics, add spin, and apply throw force so the rock rolls and rotates in the air
        Rigidbody rb = currentCube.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
            // Spin like before: rotation in the air as you throw
            Vector3 right = Vector3.Cross(throwDirection, Vector3.up).normalized;
            if (right.sqrMagnitude < 0.01f) right = Vector3.Cross(throwDirection, Vector3.forward).normalized;
            rb.angularVelocity = right * throwSpinSpeed + Vector3.up * (throwSpinSpeed * 0.5f);
        }

        // Release reference so the rock stays in the world; next throw will spawn a new one
        currentCube = null;

        trajectoryLine.enabled = false;
        landingIndicator.SetActive(false);
        isHoldingRightClick = false;
    }

    private void OnDestroy()
    {
        if (currentCube != null)
        {
            Destroy(currentCube);
        }

        if (trajectoryLine != null)
        {
            Destroy(trajectoryLine.gameObject);
        }

        if (landingIndicator != null)
        {
            Destroy(landingIndicator);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (cubeSpawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(cubeSpawnPoint.position, 0.1f);
            Gizmos.DrawLine(cubeSpawnPoint.position, cubeSpawnPoint.position + cubeSpawnPoint.forward * 0.3f);
        }
    }
}
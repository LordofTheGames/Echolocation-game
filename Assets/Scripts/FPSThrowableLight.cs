using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class FPSThrowableLight : MonoBehaviour
{
    [Header("Throwing settings")]
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private float throwForce = 20f;
    [SerializeField] private float throwHeight = 1.5f;
    [SerializeField] private float maxThrowDistance = 50f;

    [Header("Position settings")]
    [SerializeField] private Transform cubeSpawnPoint;       
    [SerializeField] private Vector3 defaultSpawnOffset = new Vector3(0.3f, -0.2f, 0.5f);

    [Header("Parabolic curve settings")]
    [SerializeField] private Material trajectoryMaterial;    
    [SerializeField] private Color trajectoryLineColor = new Color(1f, 0.2f, 0.2f, 1f); // bright red
    [SerializeField] private float trajectoryWidth = 0.05f;
    [SerializeField] private float trajectoryTimeStep = 0.02f;  // 每步时间，越小线越密
    [SerializeField] private int maxTrajectorySteps = 500;      // 最大步数，防止无限循环

    [Header("Indicator settings")]
    [SerializeField] private GameObject landingIndicatorPrefab; 

    private GameObject currentCube;              
    private GameObject landingIndicator;         
    private LineRenderer trajectoryLine;         
    private bool isHoldingRightClick = false;    
    private bool isThrowing = false;             
    private Camera playerCamera;
    private Transform cameraTransform;
    private List<Vector3> trajectoryPointsList = new List<Vector3>();
    private Vector3 landingPosition;             
    private Vector3 throwDirection;              
    private Vector3 cubeStartPosition;           

    InputAction throwAction;

    private void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera != null)
        {
            cameraTransform = playerCamera.transform;
        }

        throwAction = InputSystem.actions.FindAction("Throw");

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

    private void CreateTrajectoryLine()
    {
        GameObject lineObj = new GameObject("TrajectoryLine");
        trajectoryLine = lineObj.AddComponent<LineRenderer>();

        trajectoryLine.positionCount = 0;
        trajectoryLine.startWidth = trajectoryWidth;
        trajectoryLine.endWidth = trajectoryWidth * 0.5f;
        trajectoryLine.startColor = trajectoryLineColor;
        trajectoryLine.endColor = trajectoryLineColor;
        trajectoryLine.material = trajectoryMaterial != null ? trajectoryMaterial :
            new Material(Shader.Find("Sprites/Default"));
        trajectoryLine.textureMode = LineTextureMode.Tile;
        trajectoryLine.numCapVertices = 5;
        trajectoryLine.enabled = false;
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
        {
            StartHolding();
        }

        if (throwAction.IsPressed() && isHoldingRightClick && !isThrowing)
        {
            UpdateHolding();
        }

        if (throwAction.WasReleasedThisFrame() && isHoldingRightClick && !isThrowing)
        {
            StartThrowing();
        }
    }

    private void StartHolding()
    {
        isHoldingRightClick = true;
        isThrowing = false;

        if (cubePrefab != null && currentCube == null)
        {
            currentCube = Instantiate(cubePrefab, cubeSpawnPoint.position, Quaternion.identity);
            currentCube.transform.SetParent(cubeSpawnPoint);

            // Keep rock kinematic while held so it follows the hand; enabled on throw
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

        throwDirection = CalculateThrowDirection();

        trajectoryPointsList.Clear();
        cubeStartPosition = currentCube.transform.position;
        trajectoryPointsList.Add(cubeStartPosition);

        Vector3 currentPos = cubeStartPosition;
        Vector3 currentVel = throwDirection * throwForce;
        bool hitSomething = false;

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
                // 射线碰到地面或墙壁，轨迹线延伸到碰撞点
                landingPosition = hit.point;
                trajectoryPointsList.Add(landingPosition);
                trajectoryLine.positionCount = trajectoryPointsList.Count;
                trajectoryLine.SetPositions(trajectoryPointsList.ToArray());
                UpdateLandingIndicator(landingPosition, hit.normal, facePlayer: false);
                hitSomething = true;
                return;
            }

            trajectoryPointsList.Add(currentPos);
        }

        // 未碰到任何表面（超出最大步数），显示到最后一格并让圆圈面向玩家
        trajectoryLine.positionCount = trajectoryPointsList.Count;
        trajectoryLine.SetPositions(trajectoryPointsList.ToArray());
        landingPosition = trajectoryPointsList[trajectoryPointsList.Count - 1];
        UpdateLandingIndicator(landingPosition, Vector3.up, facePlayer: true);
    }

    private Vector3 CalculateThrowDirection()
    {
        if (cameraTransform == null) return Vector3.forward;

        Vector3 direction = cameraTransform.forward;
        direction.y += throwHeight;

        return direction.normalized;
    }

    private void UpdateLandingIndicator(Vector3 position, Vector3 normal, bool facePlayer = false)
    {
        if (landingIndicator == null) return;

        landingIndicator.transform.position = position + normal * 0.1f;

        if (facePlayer)
        {
            // No surface hit - make circle face the player
            Vector3 toPlayer = (cameraTransform.position - position).normalized;
            landingIndicator.transform.rotation = Quaternion.FromToRotation(Vector3.up, toPlayer);
        }
        else
        {
            // Surface hit - orient circle to lie on the surface
            landingIndicator.transform.rotation = Quaternion.LookRotation(normal);
            landingIndicator.transform.Rotate(90f, 0f, 0f);
        }

        float distance = Vector3.Distance(position, cameraTransform.position);
        float scale = Mathf.Lerp(0.3f, 1.5f, distance / maxThrowDistance);
        landingIndicator.transform.localScale = new Vector3(scale, 0.05f, scale);
    }

    private void StartThrowing()
    {
        if (!isHoldingRightClick || currentCube == null || isThrowing) return;

        currentCube.transform.SetParent(null);

        // Enable physics and apply throw force so the rock rolls naturally
        Rigidbody rb = currentCube.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        }

        // Release reference so the rock stays in the world; next throw will spawn a new one
        currentCube = null;

        trajectoryLine.enabled = false;
        landingIndicator.SetActive(false);
        isThrowing = false;
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
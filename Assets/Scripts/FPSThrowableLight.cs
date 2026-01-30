using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class FPSThrowableLight : MonoBehaviour
{
    [Header("Throwing settings")]
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private GameObject lightPrefab;
    [SerializeField] private float throwForce = 20f;
    [SerializeField] private float throwHeight = 1.5f;
    [SerializeField] private float maxThrowDistance = 50f;
    [SerializeField] private float cubeThrowSpeed = 15f;

    [Header("Position settings")]
    [SerializeField] private Transform cubeSpawnPoint;       
    [SerializeField] private Vector3 defaultSpawnOffset = new Vector3(0.3f, -0.2f, 0.5f);

    [Header("Parabolic curve settings")]
    [SerializeField] private Material trajectoryMaterial;    
    [SerializeField] private Color trajectoryStartColor = Color.yellow;
    [SerializeField] private Color trajectoryEndColor = Color.red;
    [SerializeField] private float trajectoryWidth = 0.05f;
    [SerializeField] private int trajectoryPoints = 50;       
    [SerializeField] private float predictionTime = 3f;       

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

        trajectoryLine.positionCount = trajectoryPoints;
        trajectoryLine.startWidth = trajectoryWidth;
        trajectoryLine.endWidth = trajectoryWidth * 0.5f;
        trajectoryLine.startColor = trajectoryStartColor;
        trajectoryLine.endColor = trajectoryEndColor;
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

        if (isThrowing && currentCube != null)
        {
            UpdateThrowing();
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

        float timeStep = predictionTime / trajectoryPoints;
        Vector3 currentPos = cubeStartPosition;
        Vector3 currentVel = throwDirection * throwForce;

        bool hitSomething = false;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            currentVel += Physics.gravity * timeStep;
            currentPos += currentVel * timeStep;

            trajectoryPointsList.Add(currentPos);

            if (i > 0)
            {
                Vector3 lastPoint = trajectoryPointsList[i - 1];
                Vector3 rayDir = currentPos - lastPoint;
                float rayDist = rayDir.magnitude;

                RaycastHit hit;
                if (Physics.Raycast(lastPoint, rayDir.normalized, out hit, rayDist))
                {
                    landingPosition = hit.point;
                    trajectoryPointsList[i] = landingPosition;
                    trajectoryLine.positionCount = i + 1;

                    UpdateLandingIndicator(landingPosition, hit.normal);

                    hitSomething = true;
                    break;
                }
            }

            float distanceFromStart = Vector3.Distance(currentPos, cameraTransform.position);
            if (distanceFromStart > maxThrowDistance)
            {
                landingPosition = currentPos;
                trajectoryPointsList[i] = landingPosition;
                trajectoryLine.positionCount = i + 1;

                UpdateLandingIndicator(landingPosition, Vector3.up);

                hitSomething = true;
                break;
            }
        }

        if (!hitSomething)
        {
            trajectoryLine.positionCount = trajectoryPoints;
            landingPosition = trajectoryPointsList[trajectoryPointsList.Count - 1];
            UpdateLandingIndicator(landingPosition, Vector3.up);
        }

        trajectoryLine.SetPositions(trajectoryPointsList.ToArray());
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

        if (normal != Vector3.up)
        {
            landingIndicator.transform.rotation = Quaternion.LookRotation(normal);
            landingIndicator.transform.Rotate(90f, 0f, 0f);
        }
        else
        {
            landingIndicator.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }

        float distance = Vector3.Distance(position, cameraTransform.position);
        float scale = Mathf.Lerp(0.3f, 1.5f, distance / maxThrowDistance);
        landingIndicator.transform.localScale = new Vector3(scale, 0.05f, scale);
    }

    private void StartThrowing()
    {
        if (!isHoldingRightClick || currentCube == null || isThrowing) return;

        isThrowing = true;

        currentCube.transform.SetParent(null);

        trajectoryLine.enabled = false;
        landingIndicator.SetActive(false);
    }

    private void UpdateThrowing()
    {
        if (currentCube == null) return;

        Vector3 targetPos = GetNextPointOnTrajectory();

        Vector3 moveDirection = (targetPos - currentCube.transform.position).normalized;
        float moveDistance = cubeThrowSpeed * Time.deltaTime;

        currentCube.transform.position = Vector3.MoveTowards(
            currentCube.transform.position,
            targetPos,
            moveDistance
        );

        currentCube.transform.Rotate(500f * Time.deltaTime, 300f * Time.deltaTime, 200f * Time.deltaTime);

        float distanceToLanding = Vector3.Distance(currentCube.transform.position, landingPosition);
        if (distanceToLanding < 0.5f)
        {
            CreateLightAtLanding();
            Destroy(currentCube);
            currentCube = null;

            isThrowing = false;
            isHoldingRightClick = false;
        }
    }

    private Vector3 GetNextPointOnTrajectory()
    {
        if (trajectoryPointsList.Count == 0) return landingPosition;

        int closestIndex = 0;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < trajectoryPointsList.Count; i++)
        {
            float dist = Vector3.Distance(currentCube.transform.position, trajectoryPointsList[i]);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestIndex = i;
            }
        }

        int nextIndex = Mathf.Min(closestIndex + 1, trajectoryPointsList.Count - 1);
        return trajectoryPointsList[nextIndex];
    }

    private void CreateLightAtLanding()
    {
        if (lightPrefab == null) return;

        RaycastHit hit;
        Vector3 spawnPos = landingPosition;
        Quaternion spawnRot = Quaternion.identity;

        if (Physics.Raycast(landingPosition + Vector3.up * 2f, Vector3.down, out hit, 5f))
        {
            spawnPos = hit.point + hit.normal * 0.1f;
            spawnRot = Quaternion.LookRotation(-hit.normal);
        }

        GameObject light = Instantiate(lightPrefab, spawnPos, spawnRot);

        StartCoroutine(LightSpawnEffect(light));
    }

    private System.Collections.IEnumerator LightSpawnEffect(GameObject lightObj)
    {
        Light pointLight = lightObj.GetComponentInChildren<Light>();
        if (pointLight != null)
        {
            float originalIntensity = pointLight.intensity;
            pointLight.intensity = 0f;

            float fadeTime = 0.5f;
            float elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeTime;
                pointLight.intensity = Mathf.Lerp(0f, originalIntensity, progress);
                yield return null;
            }

            pointLight.intensity = originalIntensity;
        }
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
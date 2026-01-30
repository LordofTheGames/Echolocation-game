using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class FPSThrowableLight : MonoBehaviour
{
    [Header("投掷设置")]
    [SerializeField] private GameObject cubePrefab;           // Cube预制体
    [SerializeField] private GameObject lightPrefab;          // 灯光预制体
    [SerializeField] private float throwForce = 20f;          // 投掷力度
    [SerializeField] private float throwHeight = 1.5f;        // 投掷高度
    [SerializeField] private float maxThrowDistance = 50f;    // 最大投掷距离
    [SerializeField] private float cubeThrowSpeed = 15f;      // Cube飞行速度

    [Header("位置设置")]
    [SerializeField] private Transform cubeSpawnPoint;        // Cube生成位置（可自定义）
    [SerializeField] private Vector3 defaultSpawnOffset = new Vector3(0.3f, -0.2f, 0.5f); // 默认偏移

    [Header("抛物线设置")]
    [SerializeField] private Material trajectoryMaterial;     // 抛物线材质
    [SerializeField] private Color trajectoryStartColor = Color.yellow;
    [SerializeField] private Color trajectoryEndColor = Color.red;
    [SerializeField] private float trajectoryWidth = 0.05f;
    [SerializeField] private int trajectoryPoints = 50;       // 抛物线点数
    [SerializeField] private float predictionTime = 3f;       // 预测时间

    [Header("指示器设置")]
    [SerializeField] private GameObject landingIndicatorPrefab; // 落地指示器预制体

    private GameObject currentCube;              // 当前显示的Cube
    private GameObject landingIndicator;         // 落地位置指示器
    private LineRenderer trajectoryLine;         // 抛物线渲染器
    private bool isHoldingRightClick = false;    // 是否按住右键
    private bool isThrowing = false;             // 是否正在投掷中
    private Camera playerCamera;
    private Transform cameraTransform;
    private List<Vector3> trajectoryPointsList = new List<Vector3>();
    private Vector3 landingPosition;             // 落地位置
    private Vector3 throwDirection;              // 投掷方向
    private Vector3 cubeStartPosition;           // Cube投掷起始位置

    InputAction throwAction;

    private void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera != null)
        {
            cameraTransform = playerCamera.transform;
        }

        throwAction = InputSystem.actions.FindAction("Throw");

        // 如果没有指定Cube生成点，使用相机位置+默认偏移
        if (cubeSpawnPoint == null)
        {
            GameObject spawnPointObj = new GameObject("CubeSpawnPoint");
            spawnPointObj.transform.SetParent(cameraTransform);
            spawnPointObj.transform.localPosition = defaultSpawnOffset;
            cubeSpawnPoint = spawnPointObj.transform;
        }

        // 创建并设置LineRenderer
        CreateTrajectoryLine();

        // 创建落地指示器
        CreateLandingIndicator();
    }

    private void CreateTrajectoryLine()
    {
        GameObject lineObj = new GameObject("TrajectoryLine");
        trajectoryLine = lineObj.AddComponent<LineRenderer>();

        // 设置LineRenderer参数
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
            // 创建默认的圆柱体指示器
            landingIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            landingIndicator.name = "LandingIndicator";

            // 设置材质和颜色
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f, 0.5f, 0f, 0.7f);
            mat.SetFloat("_Mode", 3); // 设置为Transparent模式
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;

            landingIndicator.GetComponent<Renderer>().material = mat;

            // 移除碰撞器
            Destroy(landingIndicator.GetComponent<Collider>());
        }

        landingIndicator.SetActive(false);
    }

    private void Update()
    {
        // 检测右键按下
        if (throwAction.WasPressedThisFrame())
        {
            StartHolding();
        }

        // 检测右键按住
        if (throwAction.IsPressed() && isHoldingRightClick && !isThrowing)
        {
            UpdateHolding();
        }

        // 检测右键松开
        if (throwAction.WasReleasedThisFrame() && isHoldingRightClick && !isThrowing)
        {
            StartThrowing();
        }

        // 更新投掷过程
        if (isThrowing && currentCube != null)
        {
            UpdateThrowing();
        }
    }

    private void StartHolding()
    {
        isHoldingRightClick = true;
        isThrowing = false;

        // 在指定位置生成Cube
        if (cubePrefab != null && currentCube == null)
        {
            currentCube = Instantiate(cubePrefab, cubeSpawnPoint.position, Quaternion.identity);
            currentCube.transform.SetParent(cubeSpawnPoint);

            // 为Cube添加投掷效果
            MeshRenderer cubeRenderer = currentCube.GetComponent<MeshRenderer>();
            if (cubeRenderer != null)
            {
                // 设置半透明材质
                Material cubeMat = cubeRenderer.material;
                Color cubeColor = cubeMat.color;
                cubeColor.a = 0.8f;
                cubeMat.color = cubeColor;
            }
        }

        // 显示抛物线和落地指示器
        trajectoryLine.enabled = true;
        landingIndicator.SetActive(true);
    }

    private void UpdateHolding()
    {
        if (currentCube != null)
        {
            // 更新Cube位置（跟随生成点）
            currentCube.transform.position = cubeSpawnPoint.position;
            currentCube.transform.rotation = cubeSpawnPoint.rotation;
        }

        // 更新抛物线预测
        UpdateTrajectory();
    }

    private void UpdateTrajectory()
    {
        if (trajectoryLine == null || currentCube == null) return;

        // 计算投掷方向
        throwDirection = CalculateThrowDirection();

        // 计算抛物线点
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

            // 检查碰撞
            if (i > 0)
            {
                Vector3 lastPoint = trajectoryPointsList[i - 1];
                Vector3 rayDir = currentPos - lastPoint;
                float rayDist = rayDir.magnitude;

                RaycastHit hit;
                if (Physics.Raycast(lastPoint, rayDir.normalized, out hit, rayDist))
                {
                    // 记录落地位置
                    landingPosition = hit.point;
                    trajectoryPointsList[i] = landingPosition;
                    trajectoryLine.positionCount = i + 1;

                    // 更新落地指示器
                    UpdateLandingIndicator(landingPosition, hit.normal);

                    hitSomething = true;
                    break;
                }
            }

            // 检查最大距离
            float distanceFromStart = Vector3.Distance(currentPos, cameraTransform.position);
            if (distanceFromStart > maxThrowDistance)
            {
                landingPosition = currentPos;
                trajectoryPointsList[i] = landingPosition;
                trajectoryLine.positionCount = i + 1;

                // 更新落地指示器
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

        // 更新LineRenderer
        trajectoryLine.SetPositions(trajectoryPointsList.ToArray());
    }

    private Vector3 CalculateThrowDirection()
    {
        if (cameraTransform == null) return Vector3.forward;

        // 结合相机前向和高度
        Vector3 direction = cameraTransform.forward;
        direction.y += throwHeight;

        return direction.normalized;
    }

    private void UpdateLandingIndicator(Vector3 position, Vector3 normal)
    {
        if (landingIndicator == null) return;

        landingIndicator.transform.position = position + normal * 0.1f;

        // 根据法线方向旋转指示器
        if (normal != Vector3.up)
        {
            landingIndicator.transform.rotation = Quaternion.LookRotation(normal);
            landingIndicator.transform.Rotate(90f, 0f, 0f); // 让圆柱体平放在表面上
        }
        else
        {
            landingIndicator.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }

        // 根据距离调整指示器大小
        float distance = Vector3.Distance(position, cameraTransform.position);
        float scale = Mathf.Lerp(0.3f, 1.5f, distance / maxThrowDistance);
        landingIndicator.transform.localScale = new Vector3(scale, 0.05f, scale);
    }

    private void StartThrowing()
    {
        if (!isHoldingRightClick || currentCube == null || isThrowing) return;

        isThrowing = true;

        // 让Cube脱离父级
        currentCube.transform.SetParent(null);

        // 隐藏抛物线和指示器
        trajectoryLine.enabled = false;
        landingIndicator.SetActive(false);
    }

    private void UpdateThrowing()
    {
        if (currentCube == null) return;

        // 沿着抛物线移动Cube
        Vector3 targetPos = GetNextPointOnTrajectory();

        // 计算移动方向和旋转
        Vector3 moveDirection = (targetPos - currentCube.transform.position).normalized;
        float moveDistance = cubeThrowSpeed * Time.deltaTime;

        // 移动Cube
        currentCube.transform.position = Vector3.MoveTowards(
            currentCube.transform.position,
            targetPos,
            moveDistance
        );

        // 让Cube旋转（模拟投掷效果）
        currentCube.transform.Rotate(500f * Time.deltaTime, 300f * Time.deltaTime, 200f * Time.deltaTime);

        // 检查是否到达落地位置
        float distanceToLanding = Vector3.Distance(currentCube.transform.position, landingPosition);
        if (distanceToLanding < 0.5f)
        {
            // 到达落地位置，生成灯光
            CreateLightAtLanding();

            // 销毁Cube
            Destroy(currentCube);
            currentCube = null;

            isThrowing = false;
            isHoldingRightClick = false;
        }
    }

    private Vector3 GetNextPointOnTrajectory()
    {
        if (trajectoryPointsList.Count == 0) return landingPosition;

        // 找到当前Cube位置最近的抛物线点
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

        // 返回下一个点（如果还有的话）
        int nextIndex = Mathf.Min(closestIndex + 1, trajectoryPointsList.Count - 1);
        return trajectoryPointsList[nextIndex];
    }

    private void CreateLightAtLanding()
    {
        if (lightPrefab == null) return;

        // 检查地面法线
        RaycastHit hit;
        Vector3 spawnPos = landingPosition;
        Quaternion spawnRot = Quaternion.identity;

        if (Physics.Raycast(landingPosition + Vector3.up * 2f, Vector3.down, out hit, 5f))
        {
            spawnPos = hit.point + hit.normal * 0.1f;
            spawnRot = Quaternion.LookRotation(-hit.normal);
        }

        // 生成灯光
        GameObject light = Instantiate(lightPrefab, spawnPos, spawnRot);

        // 可选：添加生成效果
        StartCoroutine(LightSpawnEffect(light));
    }

    private System.Collections.IEnumerator LightSpawnEffect(GameObject lightObj)
    {
        Light pointLight = lightObj.GetComponentInChildren<Light>();
        if (pointLight != null)
        {
            float originalIntensity = pointLight.intensity;
            pointLight.intensity = 0f;

            // 淡入效果
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
        // 清理资源
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

    // 在编辑器中可视化生成点
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
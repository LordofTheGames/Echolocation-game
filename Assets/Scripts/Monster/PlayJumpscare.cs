using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class PlayJumpscare : MonoBehaviour
{

    public Jumpscare JumpscareEvent;

    private CinemachineCamera monsterCam;
    private CinemachineCamera playerCam;
    private Camera cam;
    private MouseLook ml;
    private PlayerMovement pm;
    private RendererCutoff cutoff;
    private SkinnedMeshRenderer meshRenderer;
    private Light jumpscareLight;
    private Light playerLight;
    private CinemachineBrain brain;

    void Awake()
    {
        monsterCam = GetComponentInChildren<CinemachineCamera>();
        cutoff = GetComponent<RendererCutoff>();
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        jumpscareLight = GetComponentInChildren<Light>();

        GameObject player = GameObject.Find("Player");
        playerCam = player.GetComponentInChildren<CinemachineCamera>();
        cam = player.GetComponentInChildren<Camera>();
        ml = player.GetComponentInChildren<MouseLook>();
        pm = player.GetComponent<PlayerMovement>();
        playerLight = player.GetComponentInChildren<Light>();

        brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    void OnEnable()
    {
        JumpscareEvent.Event += jumpscare;
    }

    void OnDisable()
    {
        JumpscareEvent.Event -= jumpscare;
    }

    void jumpscare()
    {
        StartCoroutine(coroutine());
    }

    IEnumerator coroutine()
    {
        playerCam.transform.rotation = cam.transform.rotation;
        ml.enabled = false;
        pm.enabled = false;
        playerCam.enabled = true;
        yield return new WaitForSeconds(0.1f);
        cutoff.enabled = false;
        meshRenderer.enabled = true;
        jumpscareLight.enabled = true;
        playerLight.enabled = false;
        monsterCam.enabled = true;

        yield return new WaitUntil(() => brain.IsBlending);
        yield return new WaitUntil(() => !brain.IsBlending);
        yield return new WaitForSeconds(2.2f);
        monsterCam.enabled = false;
        // yield return new WaitUntil(() => brain.IsBlending);
        // yield return new WaitUntil(() => !brain.IsBlending);
        yield return new WaitForSeconds(0.1f);

        playerCam.enabled = false;
        jumpscareLight.enabled = false;
        playerLight.enabled = true;
        meshRenderer.enabled = false;
        cutoff.enabled = true;
        ml.enabled = true;
        pm.enabled = true;
    }
}

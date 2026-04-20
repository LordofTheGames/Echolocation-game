using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class ChaseStart : MonoBehaviour
{

    public bool GlassBroken = false;
    
    private CinemachineCamera monsterCam;
    private CinemachineCamera playerCam;
    private Camera cam;
    private MouseLook ml;
    private PlayerMovement pm;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (GlassBroken && other.CompareTag("Player"))
        {
            GlassBroken = false;
            playerCam = other.GetComponentInChildren<CinemachineCamera>();
            monsterCam = GameObject.Find("MonsterCam").GetComponent<CinemachineCamera>();
            cam = other.GetComponentInChildren<Camera>();
            ml = other.GetComponentInChildren<MouseLook>();
            pm = other.GetComponent<PlayerMovement>();

            playerCam.transform.rotation = cam.transform.rotation;
            ml.enabled = false;
            pm.enabled = false;
            playerCam.enabled = true;
            StartCoroutine(WaitForCutscene()); 
        }
    }

    IEnumerator WaitForCutscene()
    {
        yield return new WaitForSeconds(0.1f);
        monsterCam.enabled = true;

        var brain = Camera.main.GetComponent<CinemachineBrain>();
        yield return new WaitUntil(() => brain.IsBlending);
        yield return new WaitUntil(() => !brain.IsBlending);
        yield return new WaitForSeconds(3f);
        monsterCam.enabled = false;
        yield return new WaitUntil(() => brain.IsBlending);
        yield return new WaitUntil(() => !brain.IsBlending);

        yield return new WaitForSeconds(0.1f);
        playerCam.enabled = false;
        ml.enabled = true;
        pm.enabled = true;
    }
}

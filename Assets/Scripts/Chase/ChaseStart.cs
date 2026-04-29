using System.Collections;
using Unity.Behavior;
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
    private CrazyTimer ct;
    private BehaviorGraphAgent agent;
    private RendererCutoff cutoff;
    private GameObject finalChaseMessage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject monster = GameObject.Find("Monster");
        agent = monster.GetComponent<BehaviorGraphAgent>();     
        cutoff = monster.GetComponent<RendererCutoff>();
        monsterCam = GameObject.Find("MonsterCam").GetComponent<CinemachineCamera>();
        GameObject player = GameObject.Find("Player");
        playerCam = player.GetComponentInChildren<CinemachineCamera>();
        cam = player.GetComponentInChildren<Camera>();
        ml = player.GetComponentInChildren<MouseLook>();
        pm = player.GetComponent<PlayerMovement>();
        ct = player.GetComponent<CrazyTimer>();
        finalChaseMessage = GameObject.Find("Final Chase Message");
    }

    void OnTriggerEnter(Collider other)
    {
        if (GlassBroken && other.CompareTag("Player"))
        {
            agent.BlackboardReference.SetVariableValue("chasePosition", true);
            cutoff.Disable();

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
        ct.SprintEffectTime = 70;
        ct.EffectTime = 1000;
        ct.ResetEffect();
        monsterCam.enabled = true;

        var brain = Camera.main.GetComponent<CinemachineBrain>();
        yield return new WaitUntil(() => brain.IsBlending);
        yield return new WaitUntil(() => !brain.IsBlending);
        yield return new WaitForSeconds(1.5f);
        monsterCam.enabled = false;
        yield return new WaitUntil(() => brain.IsBlending);
        yield return new WaitUntil(() => !brain.IsBlending);

        // Add "Run!" message
        Transform runTransform = finalChaseMessage.transform.Find("Canvas/Run");
        runTransform.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.5f);
        playerCam.enabled = false;
        ml.enabled = true;
        pm.enabled = true;
        agent.BlackboardReference.SetVariableValue("chaseStart", true);

        yield return new WaitForSeconds(3);
        runTransform.gameObject.SetActive(false);
    }
}

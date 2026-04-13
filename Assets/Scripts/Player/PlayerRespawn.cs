using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Unity.Behavior;

public class PlayerRespawn : MonoBehaviour
{
    public Image blackScreenImage; 
    public float fadeInTime = 1.0f; 
    public float fadeOutTime = 1.0f; 
    public float waitTime = 1.0f;

    public Vector3 RespawnPosition;
    public HideInBox CurrentBox;

    public static event System.Action OnPlayerExit;

    private bool isRespawning = false;
    private GameObject navmeshEdges;
    private BehaviorGraphAgent agent;

    void Start()
    {
        agent = GameObject.Find("Monster").GetComponent<BehaviorGraphAgent>();
        agent.BlackboardReference.SetVariableValue("respawnTime", fadeInTime);
        navmeshEdges = GameObject.Find("NavMesh Edges");
        Color c = blackScreenImage.color;
        c.a = 0;
        blackScreenImage.color = c;
    }

    public void Respawn()
    {
        if (!isRespawning) StartCoroutine(RespawnSequence());
        gameObject.GetComponent<CrazyTimer>().ResetEffect();
    }

    private IEnumerator RespawnSequence()
    {
        isRespawning = true;

        PlayerMovement pm = gameObject.GetComponent<PlayerMovement>();
        CharacterController cc = gameObject.GetComponent<CharacterController>();
        pm.enabled = false;
        cc.enabled = false;

        //fade to black
        float timer = 0f;
        Color c = blackScreenImage.color;
        while (timer < fadeInTime)
        {
            timer += Time.deltaTime;
            c.a = timer / fadeInTime;
            blackScreenImage.color = c;
            yield return null; 
        }
        c.a = 1;
        blackScreenImage.color = c;

        if (CurrentBox != null && CurrentBox.isHiding && !CurrentBox.isTransitioning)
        {
            CurrentBox.isHiding = false; 
            OnPlayerExit?.Invoke();

            gameObject.GetComponent<PlayerMovement>().enabled = true;
            MouseLook ml = gameObject.GetComponentInChildren<MouseLook>();
            ml.isHiding = false; 
            ml.hidingTransition = false;
            navmeshEdges.SetActive(true);
        }

        // wait at black screen
        yield return new WaitForSeconds(waitTime);

        transform.position = RespawnPosition;
        pm.enabled = true;
        cc.enabled = true;

        //fade back in
        timer = 0f;
        c = blackScreenImage.color;
        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            c.a = 1f - (timer / fadeOutTime); 
            blackScreenImage.color = c;
            yield return null;
        }
        c.a = 0;
        blackScreenImage.color = c;

        isRespawning = false; 
    }
}



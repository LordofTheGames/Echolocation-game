using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Unity.Behavior;

public class PlayerRespawn : MonoBehaviour
{
    public Vector3 RespawnPosition;
    public HideInBox CurrentBox;
    public static event System.Action OnPlayerExit;
    private GameObject navmeshEdges;
    public Image blackScreenImage; 
    public float fadeWaitTime = 1.0f; 

    private bool isRespawning = false;

    void Start()
    {
        navmeshEdges = GameObject.Find("NavMesh Edges");
        
        if (blackScreenImage != null)
        {
            Color c = blackScreenImage.color;
            c.a = 0f;
            blackScreenImage.color = c;
        }
    }

    public void Respawn()
    {
        if (!isRespawning) StartCoroutine(RespawnSequence());
    }

    private IEnumerator RespawnSequence()
    {
        isRespawning = true;

        PlayerMovement pm = gameObject.GetComponent<PlayerMovement>();
        if (pm != null) pm.enabled = false;

        //fade to black
        if (blackScreenImage != null)
        {
            float timer = 0f;
            Color c = blackScreenImage.color;
            while (timer < fadeWaitTime)
            {
                timer += Time.deltaTime;
                c.a = timer / fadeWaitTime;
                blackScreenImage.color = c;
                yield return null; 
            }
        }

        if (CurrentBox != null && CurrentBox.isHiding && !CurrentBox.isTransitioning)
        {
            CurrentBox.isHiding = false; 
            OnPlayerExit?.Invoke();

            MouseLook ml = gameObject.GetComponentInChildren<MouseLook>();
            if (ml != null) { ml.isHiding = false; ml.hidingTransition = false; }
            if (navmeshEdges != null) navmeshEdges.SetActive(true);
        }

        CharacterController cc = gameObject.GetComponent<CharacterController>();
        cc.enabled = false;
        transform.position = RespawnPosition;
        cc.enabled = true;


        //fade back in
        if (blackScreenImage != null)
        {
            float timer = 0f;
            Color c = blackScreenImage.color;
            while (timer < fadeWaitTime)
            {
                timer += Time.deltaTime;
                c.a = 1f - (timer / fadeWaitTime); 
                blackScreenImage.color = c;
                yield return null;
            }
        }

        if (pm != null) pm.enabled = true;
        isRespawning = false; 
    }
}



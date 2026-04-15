using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Unity.Behavior;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerRespawn : MonoBehaviour
{
    public Image blackScreenImage; 
    public TMP_Text livesText;
    public int Lives = 3;

    public float fadeInTime = 1.0f; 
    public float fadeOutTime = 1.0f; 
    public float textFadeInTime = 1.0f; 
    public float textFadeOutTime = 1.0f; 
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
        Color ct = livesText.color;
        ct.a = 0;
        livesText.color = ct;
    }

    public void Respawn()
    {
        if (!isRespawning) StartCoroutine(RespawnSequence());
        gameObject.GetComponent<CrazyTimer>().ResetEffect();
    }

    private IEnumerator RespawnSequence()
    {
        isRespawning = true;

        if (Lives == 0) livesText.text = "";  // black screen to transition to game over screen
        else if (Lives == 1) livesText.text = "1\nlife remaining";
        else livesText.text = Lives + "\nlives remaining";

        PlayerMovement pm = gameObject.GetComponent<PlayerMovement>();
        CharacterController cc = gameObject.GetComponent<CharacterController>();
        pm.enabled = false;
        cc.enabled = false;

        //fade to black
        float timer = 0f;
        float offset = fadeInTime - textFadeInTime;
        Color c = blackScreenImage.color;
        Color ct = livesText.color;
        while (timer < fadeInTime)
        {
            timer += Time.deltaTime;
            c.a = timer / fadeInTime;
            blackScreenImage.color = c;
            if (timer > offset)
            {
                ct.a = (timer - offset) / textFadeInTime;
                livesText.color = ct;
            }
            yield return null; 
        }
        c.a = 1;
        ct.a = 1;
        blackScreenImage.color = c;
        livesText.color = ct;

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

        if (Lives == 0)
        {
            SceneManager.LoadScene("GameOver");
            yield break;
        }

        // wait at black screen
        yield return new WaitForSeconds(waitTime);

        transform.position = RespawnPosition;
        pm.enabled = true;
        cc.enabled = true;

        //fade back in
        timer = 0f;
        c = blackScreenImage.color;
        ct = livesText.color;
        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            c.a = 1f - (timer / fadeOutTime); 
            blackScreenImage.color = c;
            ct.a = Mathf.Max(0, 1f - (timer / textFadeOutTime));
            livesText.color = ct;
            yield return null;
        }
        c.a = 0;
        blackScreenImage.color = c;
        ct.a = 0;
        livesText.color = ct;

        isRespawning = false; 
    }
}



using UnityEngine;
using System.Collections;
using Unity.Behavior;

public class PlayerRespawn : MonoBehaviour
{
    public Vector3 RespawnPosition;
    public HideInBox CurrentBox;
    public static event System.Action OnPlayerExit;
    private GameObject navmeshEdges;

    void Start()
    {
        navmeshEdges = GameObject.Find("NavMesh Edges");
    }

    public void Respawn()
    {
        // reset all hiding variables etc. if hiding
        if (CurrentBox != null && CurrentBox.isHiding && !CurrentBox.isTransitioning)
        {
            CurrentBox.isHiding = false; 
            OnPlayerExit?.Invoke();

            CharacterController cc = gameObject.GetComponent<CharacterController>();
            cc.enabled = false;
            transform.position = RespawnPosition;
            cc.enabled = true;

            // Re-enable player movements
            gameObject.GetComponent<PlayerMovement>().enabled = true;
            MouseLook ml = gameObject.GetComponentInChildren<MouseLook>();
            ml.isHiding = false; 
            ml.hidingTransition = false; 
            navmeshEdges.SetActive(true);
        }
        else
        {
            CharacterController cc = gameObject.GetComponent<CharacterController>();
            cc.enabled = false;
            transform.position = RespawnPosition;
            cc.enabled = true;
        }
    }
}

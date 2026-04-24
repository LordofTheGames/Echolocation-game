using System.Collections;
using UnityEngine;

public class LabDoor : MonoBehaviour
{
    [SerializeField] private Transform leftDoorHinge;
    [SerializeField] private Transform rightDoorHinge;

    [SerializeField] private float leftOpenY = 85f;
    [SerializeField] private float rightOpenY = -85f;
    [SerializeField] private float closedY = 0f;

    [SerializeField] private float rotateSpeed = 180f;

    public float MonsterDistance = 8;

    private Quaternion leftClosedRot;
    private Quaternion rightClosedRot;
    private Quaternion leftOpenRot;
    private Quaternion rightOpenRot;

    private Coroutine rotateRoutine;

    private bool isOpen;
    private bool isMoving;

    public bool IsOpen => isOpen;
    public bool IsMoving => isMoving;

    private GameObject monster;
    private GameObject player;

    private void Awake()
    {
        if (leftDoorHinge == null || rightDoorHinge == null)
        {
            Debug.LogError($"[{name}] LabDoor is missing hinge references.");
            enabled = false;
            return;
        }

        leftClosedRot = Quaternion.Euler(0f, closedY, 0f);
        rightClosedRot = Quaternion.Euler(0f, closedY, 0f);

        leftOpenRot = Quaternion.Euler(0f, leftOpenY, 0f);
        rightOpenRot = Quaternion.Euler(0f, rightOpenY, 0f);

        leftDoorHinge.localRotation = leftClosedRot;
        rightDoorHinge.localRotation = rightClosedRot;

        isOpen = false;
        isMoving = false;

        monster = GameObject.Find("Monster");
        player = GameObject.Find("Player");
    }

    public void Interact()
    {
        if (isMoving) return;

        if (isOpen)
            CloseDoors();
        else
            OpenDoors();
    }

    public void OpenDoors()
    {
        if (rotateRoutine != null)
            StopCoroutine(rotateRoutine);

        isOpen = true;
        rotateRoutine = StartCoroutine(RotateDoors(leftOpenRot, rightOpenRot));
    }

    public void CloseDoors()
    {
        if (rotateRoutine != null)
            StopCoroutine(rotateRoutine);

        isOpen = false;
        rotateRoutine = StartCoroutine(RotateDoors(leftClosedRot, rightClosedRot));
    }

    private IEnumerator RotateDoors(Quaternion leftTarget, Quaternion rightTarget)
    {
        isMoving = true;

        while (
            Quaternion.Angle(leftDoorHinge.localRotation, leftTarget) > 0.1f ||
            Quaternion.Angle(rightDoorHinge.localRotation, rightTarget) > 0.1f
        )
        {
            leftDoorHinge.localRotation = Quaternion.RotateTowards(
                leftDoorHinge.localRotation,
                leftTarget,
                rotateSpeed * Time.deltaTime
            );

            rightDoorHinge.localRotation = Quaternion.RotateTowards(
                rightDoorHinge.localRotation,
                rightTarget,
                rotateSpeed * Time.deltaTime
            );

            yield return null;
        }

        leftDoorHinge.localRotation = leftTarget;
        rightDoorHinge.localRotation = rightTarget;

        isMoving = false;
        rotateRoutine = null;
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, player.transform.position) <= MonsterDistance)
        {
            OpenDoors();
            StartCoroutine(closeDoorMonster());
        }
    }
     private IEnumerator closeDoorMonster()
    {
        while (Vector3.Distance(transform.position, player.transform.position) <= MonsterDistance)
            yield return new WaitForSeconds(1);
        CloseDoors();
    }
}
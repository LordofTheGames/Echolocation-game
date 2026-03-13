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
    [SerializeField] private float autoCloseDelay = 1.5f;

    private Quaternion leftClosedRot;
    private Quaternion rightClosedRot;
    private Quaternion leftOpenRot;
    private Quaternion rightOpenRot;

    private Coroutine rotateRoutine;
    private Coroutine closeRoutine;

    private bool isOpen;
    private bool isMoving;

    private int openedFromSide = 0; // -1 or +1

    private bool waitingForPass = false;

    public bool IsOpen => isOpen;
    public bool IsMoving => isMoving;
    public bool WaitingForPass => waitingForPass;
    public int OpenedFromSide => openedFromSide;

    private void Awake()
    {
        if (leftDoorHinge == null || rightDoorHinge == null)
        {
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
        waitingForPass = false;
    }

    public void Interact(Transform player)
    {
        if (isMoving) return;
        if (player == null) return;

        if (!isOpen)
        {
            openedFromSide = GetPlayerSide(player.position);
            OpenDoors();
        }
    }

    private int GetPlayerSide(Vector3 worldPos)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);

        return localPos.z >= 0f ? 1 : -1;
    }

    public void OpenDoors()
    {
        CancelAutoClose();

        if (rotateRoutine != null)
            StopCoroutine(rotateRoutine);

        isOpen = true;
        waitingForPass = true;

        rotateRoutine = StartCoroutine(RotateDoors(leftOpenRot, rightOpenRot));
    }

    public void CloseDoors()
    {
        CancelAutoClose();

        if (rotateRoutine != null)
            StopCoroutine(rotateRoutine);

        isOpen = false;
        waitingForPass = false;

        rotateRoutine = StartCoroutine(RotateDoors(leftClosedRot, rightClosedRot));
    }

    public void TryMarkPassed(Transform player)
    {
        if (!isOpen) return;
        if (!waitingForPass) return;
        if (player == null) return;

        int currentSide = GetPlayerSide(player.position);

        if (currentSide != openedFromSide)
        {
            waitingForPass = false;
            CloseAfterDelay();
        }
    }

    public void CloseAfterDelay()
    {
        CancelAutoClose();
        closeRoutine = StartCoroutine(CloseAfterDelayRoutine());
    }

    public void CancelAutoClose()
    {
        if (closeRoutine != null)
        {
            StopCoroutine(closeRoutine);
            closeRoutine = null;
        }
    }

    private IEnumerator CloseAfterDelayRoutine()
    {
        yield return new WaitForSeconds(autoCloseDelay);

        if (isOpen && !isMoving)
        {
            CloseDoors();
        }

        closeRoutine = null;
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
}
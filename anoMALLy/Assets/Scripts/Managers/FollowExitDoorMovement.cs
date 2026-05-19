using UnityEngine;

public class FollowExitDoorMovement : MonoBehaviour
{
    [Header("Objeto que se mueve")]
    [SerializeField] Transform targetToFollow;

    [Header("Objetos que deben seguirlo")]
    [SerializeField] Transform[] objectsToMove;

    [Header("Opciones")]
    [SerializeField] bool followPosition = true;
    [SerializeField] bool followRotation = false;

    Vector3 targetStartPosition;
    Quaternion targetStartRotation;

    Vector3[] objectStartPositions;
    Quaternion[] objectStartRotations;

    private void Start()
    {
        if (targetToFollow == null)
        {
            Debug.LogWarning("No se ha asignado Target To Follow en " + gameObject.name);
            return;
        }

        targetStartPosition = targetToFollow.position;
        targetStartRotation = targetToFollow.rotation;

        objectStartPositions = new Vector3[objectsToMove.Length];
        objectStartRotations = new Quaternion[objectsToMove.Length];

        for (int i = 0; i < objectsToMove.Length; i++)
        {
            if (objectsToMove[i] == null) continue;

            objectStartPositions[i] = objectsToMove[i].position;
            objectStartRotations[i] = objectsToMove[i].rotation;
        }
    }

    private void LateUpdate()
    {
        if (targetToFollow == null) return;

        Vector3 positionOffset = targetToFollow.position - targetStartPosition;
        Quaternion rotationOffset = targetToFollow.rotation * Quaternion.Inverse(targetStartRotation);

        for (int i = 0; i < objectsToMove.Length; i++)
        {
            if (objectsToMove[i] == null) continue;

            if (followPosition)
            {
                objectsToMove[i].position = objectStartPositions[i] + positionOffset;
            }

            if (followRotation)
            {
                objectsToMove[i].rotation = rotationOffset * objectStartRotations[i];
            }
        }
    }
}
using System.Collections;
using UnityEngine;

public class Leg : MonoBehaviour
{
    public IKSolver ikSolver;
    public Transform target;

    public enum LegGroup
    {
        group1,
        group2
    }
    public LegGroup legGroup;

    [Header("Movement")]
    public float moveDuration = 0.1f;
    public float minMoveDistance = 1f;
    [Tooltip("When true, leg overshoots target to make movement more natural")]
    public bool overshoot = true;
    [Range(0f, 1f), Tooltip("Fraction of minMoveDistance from target we want to overshoot by")]
    public float stepOvershoot = 0.25f;

    [Header("Raycasting")]
    public float maxDistance = 5f;
    public float heightOffset = 0.75f;
    public LayerMask ground;

    public Transform tip { get; private set; }
    public Vector3 surfaceNormal { get; private set; }
    Vector3 targetPosition;
    bool isMoving;

    void Start()
    {
        ikSolver.Init();
        tip = ikSolver.joints[ikSolver.joints.Length - 1];
        targetPosition = target.position;
        ikSolver.Update(targetPosition);
    }

    void LateUpdate()
    {
        ikSolver.Update(targetPosition);
    }

    public void TryMove()
    {
        if (isMoving || !CastRay()) return;

        float distanceToTarget = Vector3.Distance(target.position, tip.position);
        if (distanceToTarget >= minMoveDistance)
        {
            StartCoroutine(MoveToTarget());
        }
    }

    IEnumerator MoveToTarget()
    {
        isMoving = true;
        float timeElapsed = 0f;

        Vector3 startPoint = tip.position;
        Vector3 endPoint = GetEndPoint();
        Vector3 centerPoint = (startPoint + endPoint) / 2;
        // Lift center of the ground, so we get a curve later on
        centerPoint += target.up * Vector3.Distance(startPoint, endPoint) / 2f;

        do
        {
            timeElapsed += Time.deltaTime;
            float normalizedTime = timeElapsed / moveDuration;

            // Quadratic bezier curve to move the leg in a curve
            targetPosition = Vector3.Lerp(
                Vector3.Lerp(startPoint, centerPoint, normalizedTime),
                Vector3.Lerp(centerPoint, endPoint, normalizedTime),
                normalizedTime
            );

            yield return null;
        }
        while (timeElapsed < moveDuration);

        isMoving = false;
    }

    bool CastRay()
    {
        RaycastHit hit;
        if (Physics.Raycast(target.position + new Vector3(0f, heightOffset), -target.up, out hit, maxDistance, ground))
        {
            target.position = hit.point;
            surfaceNormal = hit.normal;
            return true;
        }

        // When ground not found just set the target pos as the target object itself
        targetPosition = target.position;
        surfaceNormal = Vector3.up;
        return false;
    }

    Vector3 GetEndPoint()
    {
        if (overshoot)
        {
            // Overshooting target to make movement more natural
            Vector3 dirToTarget = (target.position - tip.position).normalized;
            float overshootDistance = minMoveDistance * stepOvershoot;
            Vector3 overshootVector = dirToTarget * overshootDistance;
            // Since the target can be higher or lower then the leg tip,
            // we restrict the overshoot vector to be level with the ground
            // by projecting it on the world XZ plane
            overshootVector = Vector3.ProjectOnPlane(overshootVector, Vector3.up);
            return target.position + overshootVector;
        }

        return target.position;
    }

    public bool IsMoving()
    {
        return isMoving;
    }
}

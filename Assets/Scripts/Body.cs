using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Rotate body based on leg positions

public class Body : MonoBehaviour
{
    [Header("Position")]
    public float heightOffset = 2f;
    public float moveSpeed = 5f;

    [Header("Rotation")]
    public bool UpdateRotation = true;
    public float rotateSpeed = 5f;

    private Leg[] legs;

    public void Init(Leg[] legs)
    {
        this.legs = legs;
    }

    public void UpdateBody()
    {
        LegInformation legInfo = GetLegInformation();

        // Interpolate old position to new
        Vector3 bodyPos = legInfo.middlePoint + transform.up * heightOffset;
        transform.position = Vector3.Lerp(transform.position, bodyPos, moveSpeed * Time.deltaTime);

        if(UpdateRotation)
        {
            // Get body up vector
            Vector3 bodyRight = Vector3.Cross(legInfo.normal, transform.forward);
            Vector3 bodyForward = Vector3.Cross(bodyRight, legInfo.normal);

            // Interpolate old rotation to new
            Quaternion bodyRotation = Quaternion.LookRotation(bodyForward, legInfo.normal);
            transform.rotation = Quaternion.Slerp(transform.rotation, bodyRotation, rotateSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Get's middle position between legs and targeted body's up vector
    /// </summary>
    LegInformation GetLegInformation()
    {
        Vector3 point = Vector3.zero;
        Vector3 bodyUp = Vector3.zero;

        foreach (Leg leg in legs)
        {
            point += leg.tip.position;
            bodyUp += leg.surfaceNormal;
        }

        point = point / legs.Length;

        return new LegInformation(point, bodyUp.normalized);
    }

    struct LegInformation
    {
        public Vector3 middlePoint;
        public Vector3 normal;

        public LegInformation(Vector3 middlePoint, Vector3 normal)
        {
            this.middlePoint = middlePoint;
            this.normal = normal;
        }
    }
}

using UnityEngine;

[System.Serializable]
public class IKSolver
{
    public Transform[] joints;
    public Transform pole;

    public int iterations = 10;
    [Min(0f), Tooltip("Minimal distance from target")]
    public float delta = .1f;

    Vector3[] pos;
    float[] jointsDist;
    float totalLength;

    Vector3[] startDirToChild;
    Quaternion[] startJointRot;

    public void Init()
    {
        pos = new Vector3[joints.Length];
        jointsDist = new float[joints.Length];
        totalLength = 0f;

        startDirToChild = new Vector3[joints.Length];
        startJointRot = new Quaternion[joints.Length];

        // Get joint distances/directions/rotations
        for(int i = joints.Length - 1; i >= 0; i--)
        {
            if(joints.Length - 1 != i)
            {
                startDirToChild[i] = joints[i + 1].position - joints[i].position;
                jointsDist[i] = startDirToChild[i].magnitude;
                totalLength += jointsDist[i];
            }

            startJointRot[i] = joints[i].rotation;
        }
    }

    public void Update(Vector3 target)
    {
        // Get joint positions to not do any computations on the joints directly
        for (int i = 0; i < joints.Length; i++)
        {
            pos[i] = joints[i].position;
        }

        // Check if joints are long enough to reach the target
        if ((target - joints[0].position).sqrMagnitude >= totalLength * totalLength)
        {
            // If joints are not long enough to reach the target, then strech them
            StrechToTarget(target);
        }
        else
        {
            SolveIK(target);
            MoveTowardsPole();
        }

        // Set new joint positions and rotations
        joints[pos.Length - 1].rotation = startJointRot[pos.Length - 1];
        for (int i = 0; i < pos.Length - 1; i++)
        {
            if (pos.Length - 1 != i)
            {
                joints[i].rotation = Quaternion.FromToRotation(startDirToChild[i], pos[i + 1] - pos[i]) * startJointRot[i];
            }

            joints[i].position = pos[i];
        }
    }

    void SolveIK(Vector3 target)
    {
        for (int i = 0; i < iterations; i++)
        {
            // Backward reaching
            pos[pos.Length - 1] = target;
            for (int j = pos.Length - 2; j > 0; j--)
            {
                pos[j] = pos[j + 1] + (pos[j] - pos[j + 1]).normalized * jointsDist[j];
            }

            // Forward reaching
            for (int j = 1; j < pos.Length; j++)
            {
                pos[j] = pos[j - 1] + (pos[j] - pos[j - 1]).normalized * jointsDist[j - 1];
            }
                

            // If is close enough to target, break
            if ((pos[pos.Length - 1] - target).sqrMagnitude < delta * delta) break;
        }
    }

    void MoveTowardsPole()
    {
        if (pole == null) return;

        for (int i = 1; i < pos.Length - 1; i++)
        {
            // Create a plane perpendicular to vector between next and last bone, on the last bone
            Plane plane = new Plane(pos[i + 1] - pos[i - 1], pos[i - 1]);

            // Get projected pole and joint position on the plane
            Vector3 projectedPole = plane.ClosestPointOnPlane(pole.position);
            Vector3 projectedJoint = plane.ClosestPointOnPlane(pos[i]);

            // Calculate angle we have to move the joint
            float angle = Vector3.SignedAngle(projectedJoint - pos[i - 1], projectedPole - pos[i - 1], plane.normal);

            // Rotate the joint around the axis on the plane normal, and set it back in position
            pos[i] = Quaternion.AngleAxis(angle, plane.normal) * (pos[i] - pos[i - 1]) + pos[i - 1];
        }
    }

    /// <summary>
    /// Positions all joints in a straight line towards the target
    /// </summary>
    void StrechToTarget(Vector3 target)
    {
        Vector3 dir = (target - pos[0]).normalized;
        for (int i = 1; i < pos.Length; i++)
        {
            pos[i] = pos[i - 1] + dir * jointsDist[i - 1];
        }
    }
}

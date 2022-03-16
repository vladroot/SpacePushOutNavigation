using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle
{
    public Collider collider;
    public Transform transform;
    public float pushDistance;
    public float maxEffectDistance;
}

public class Navigation : MonoBehaviour
{
    [SerializeField] private Collider player;
    [SerializeField] private Transform destination;

    private Dictionary<Collider, Obstacle> obstacles;
    private Rigidbody playerRigidbody;
    private Transform playerTransform;

    private RaycastHit[] navCastResults;

    private void Start()
    {
        navCastResults = new RaycastHit[100];
        playerRigidbody = player.GetComponent<Rigidbody>();
        playerTransform = player.transform;
        Collider[] colliders = FindObjectsOfType<Collider>(false);

        obstacles = new Dictionary<Collider, Obstacle>();
        foreach (Collider c in colliders)
        {
            if (c == player)
                continue;

            obstacles.Add(c, new Obstacle
            {
                collider = c,
                transform = c.transform,
                pushDistance = c.bounds.extents.magnitude,
                maxEffectDistance = c.bounds.extents.magnitude * 3f
            });
        }
    }

    private void FixedUpdate()
    {
        if ((playerTransform.position - destination.position).magnitude < .1f)
            return;

        Vector3 needDir = destination.position - playerTransform.position;
        Vector3 needDirNorm = needDir.normalized;
        foreach (Collider key in obstacles.Keys)
        {
            Obstacle o = obstacles[key];
            Vector3 toObstacle = o.transform.position - playerTransform.position;
            float obstacleDist = toObstacle.magnitude;
            if (obstacleDist > o.maxEffectDistance || Vector3.Dot(playerRigidbody.velocity, toObstacle) < 0f)
                continue;

            float part = o.maxEffectDistance - obstacleDist;
            if (part <= 0f)
                continue;

            part = part / o.maxEffectDistance;

            Vector3 correction = CalcCorrectionVector(o.transform.position, toObstacle);
            needDirNorm += correction * o.pushDistance * part;
        }

        playerRigidbody.velocity = needDirNorm.normalized;
        Debug.DrawRay(playerTransform.position, playerRigidbody.velocity, Color.red, Time.fixedDeltaTime);

        Vector3 CalcCorrectionVector(Vector3 obstaclePos, Vector3 toObstacle)
        {
            Vector3 projection = Vector3.Project(toObstacle, needDir);
            Vector3 projPos = playerTransform.position + projection;
            Vector3 correction = projPos - obstaclePos;
            if (correction.magnitude == 0f)
                correction = Vector3.up;
            else
                correction = correction.normalized;
            Debug.DrawRay(playerTransform.position, correction, Color.green, Time.fixedDeltaTime);
            return correction;
        }
    }

}

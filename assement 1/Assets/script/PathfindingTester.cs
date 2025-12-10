using System.Collections.Generic;
using UnityEngine;

public class PathfindingTester : MonoBehaviour
{
    private AStarManager AStarManager = new AStarManager();
    private List<GameObject> Waypoints = new List<GameObject>();
    private List<Connection> ConnectionArray = new List<Connection>();

    [SerializeField] private GameObject start;
    [SerializeField] private GameObject end;
    [SerializeField] private GameObject parcelPrefab;
    [SerializeField] private float inputSpeed = 15f;

    Vector3 OffSet = new Vector3(0, 0.3f, 0);

    private float baseSpeed = 15f;
    private float currentSpeed = 15f;

    private float turnSpeedMultiplier = 0.8f;

    private float timeElapsed = 0f;
    private float distanceTravelled = 0f;

    public int parcelCount = 5;

    public int currentTarget = 0;
    private Vector3 currentTargetPos;
    private int moveDirection = 1;
    public bool agentMove = true;
    private bool delivered = false;

    public PerformanceUI ui;

    public float CurrentSpeed => currentSpeed;

    public void SetSpeed(float s)
    {
        currentSpeed = s;
    }

    void Start()
    {
        baseSpeed = inputSpeed;
        currentSpeed = inputSpeed;

        GameObject[] objs = GameObject.FindGameObjectsWithTag("waypoint");
        foreach (GameObject waypoint in objs)
        {
            var w = waypoint.GetComponent<VisGraphWaypointManager>();
            if (w) Waypoints.Add(waypoint);
        }

        foreach (GameObject waypoint in Waypoints)
        {
            var w = waypoint.GetComponent<VisGraphWaypointManager>();
            foreach (VisGraphConnection c in w.Connections)
            {
                if (c.ToNode != null)
                {
                    Connection con = new Connection();
                    con.FromNode = waypoint;
                    con.ToNode = c.ToNode;
                    AStarManager.AddConnection(con);
                }
            }
        }

        ConnectionArray = AStarManager.PathfindAStar(start, end);
    }

    void Update()
    {
        if (!agentMove) return;

        timeElapsed += Time.deltaTime;
        distanceTravelled += currentSpeed * Time.deltaTime;

        if (ui != null)
        {
            ui.timeElapsed = timeElapsed;
            ui.distanceTravelled = distanceTravelled;
            ui.currentSpeed = currentSpeed;
            ui.parcelCount = parcelCount;
        }

        if (ConnectionArray == null || ConnectionArray.Count == 0) return;

        currentTarget = Mathf.Clamp(currentTarget, 0, ConnectionArray.Count - 1);

        currentTargetPos = moveDirection > 0 ?
            ConnectionArray[currentTarget].ToNode.transform.position :
            ConnectionArray[currentTarget].FromNode.transform.position;

        currentTargetPos.y = transform.position.y;

        Vector3 dir = currentTargetPos - transform.position;
        float dist = dir.magnitude;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 6f * Time.deltaTime);

        float angle = Vector3.Angle(transform.forward, dir.normalized);
        float turnModifier = angle > 20f ? turnSpeedMultiplier : 1f;

        transform.position += dir.normalized * currentSpeed * turnModifier * Time.deltaTime;

        if (dist < 1f)
        {
            currentTarget += moveDirection;

            if (currentTarget >= ConnectionArray.Count)
            {
                delivered = true;
                parcelCount = 0;
                DropParcel();
                UpdateSpeed();
                moveDirection = -1;
                currentTarget = ConnectionArray.Count - 1;
            }

            if (currentTarget < 0)
            {
                if (delivered)
                {
                    parcelCount = 5;
                    UpdateSpeed();
                    agentMove = false;

                    Vector3 p = start.transform.position;
                    p.y = transform.position.y;
                    transform.position = p;
                    return;
                }

                moveDirection = 1;
                currentTarget = 0;
            }
        }
    }

    void DropParcel()
    {
        Vector3 dropPos = end.transform.position;
        dropPos.y = transform.position.y;
        Instantiate(parcelPrefab, dropPos, Quaternion.identity).name = "DeliveredParcel";
    }

    void UpdateSpeed()
    {
        currentSpeed = baseSpeed * 1.1f;

        var avoid = GetComponent<RaycastAvoidanceMover>();
        if (avoid != null)
            avoid.UpdateBaseSpeed(currentSpeed);
    }

    public void ForceReturnToPath()
    {
        if (ConnectionArray == null || ConnectionArray.Count == 0) return;

        Vector3 pos = transform.position;
        Vector3 forward = transform.forward;
        float closestDist = Mathf.Infinity;
        int bestIndex = currentTarget;

        for (int i = currentTarget; i < ConnectionArray.Count; i++)
        {
            Vector3 wp = ConnectionArray[i].ToNode.transform.position;
            Vector3 to = (wp - pos).normalized;

            float dot = Vector3.Dot(forward, to);
            if (dot < 0.2f) continue;

            float d = Vector3.Distance(pos, wp);
            if (d < closestDist)
            {
                closestDist = d;
                bestIndex = i;
            }
        }

        currentTarget = bestIndex;
    }

    void OnDrawGizmos()
    {
        if (ConnectionArray == null) return;

        foreach (Connection c in ConnectionArray)
        {
            if (c == null || c.FromNode == null || c.ToNode == null) continue;

            Gizmos.color = Color.white;
            Gizmos.DrawLine(
                c.FromNode.transform.position + OffSet,
                c.ToNode.transform.position + OffSet
            );
        }
    }
}

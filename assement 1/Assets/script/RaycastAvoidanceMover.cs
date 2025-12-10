using UnityEngine;

public class RaycastAvoidanceMover : MonoBehaviour
{
    private PathfindingTester self;

    public float rayDistance = 6f;
    public float sideOffset = 2f;
    public float moveSpeed = 4f;
    public float returnSpeed = 4f;
    public float slowdownFactor = 0.6f;

    private float baseSpeed;

    private enum State { Idle, Avoiding, Waiting, Returning }
    private State state = State.Idle;

    private Vector3 stepPos;
    private Vector3 returnPos;

    private float waitTimer = 0f;
    private bool logged = false;

    void Start()
    {
        self = GetComponent<PathfindingTester>();
        baseSpeed = self.CurrentSpeed;
    }

    void Update()
    {
        switch (state)
        {
            case State.Idle:
                Detect();
                break;
            case State.Avoiding:
                StepAside();
                break;
            case State.Waiting:
                Wait();
                break;
            case State.Returning:
                ReturnToLane();
                break;
        }
    }

    void Detect()
    {
        bool hitCar = false;

        if (RayHit(transform.forward)) hitCar = true;
        if (RayHit(transform.forward + transform.right * 0.5f)) hitCar = true;
        if (RayHit(transform.forward - transform.right * 0.5f)) hitCar = true;

        if (hitCar)
        {
            if (!logged)
            {
                Debug.Log("Collision detected");
                logged = true;
            }
            StartAvoid();
        }
        else logged = false;
    }

    bool RayHit(Vector3 direction)
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position + Vector3.up * 0.4f, direction.normalized, out hit, rayDistance))
        {
            PathfindingTester other = hit.collider.GetComponent<PathfindingTester>();
            if (other == null) return false;
            if (self.CurrentSpeed < other.CurrentSpeed) return true;
        }
        return false;
    }

    void StartAvoid()
    {
        if (state != State.Idle) return;

        state = State.Avoiding;
        self.agentMove = false;
        self.SetSpeed(0f);

        stepPos = transform.position + transform.right * sideOffset;
        returnPos = transform.position - transform.right * sideOffset;
    }

    void StepAside()
    {
        transform.position = Vector3.MoveTowards(transform.position, stepPos, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, stepPos) < 0.05f)
        {
            waitTimer = 3f;
            state = State.Waiting;
        }
    }

    void Wait()
    {
        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0f)
            state = State.Returning;
    }

    void ReturnToLane()
    {
        transform.position = Vector3.MoveTowards(transform.position, returnPos, returnSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, returnPos) < 0.05f)
        {
            self.ForceReturnToPath();
            self.agentMove = true;
            self.SetSpeed(baseSpeed);
            state = State.Idle;
        }
    }

    public void UpdateBaseSpeed(float newSpeed)
    {
        baseSpeed = newSpeed;
        if (state == State.Idle)
            self.SetSpeed(newSpeed);
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Vector3 start = transform.position + Vector3.up * 0.4f;
        Gizmos.color = Color.yellow;

        Gizmos.DrawLine(start, start + transform.forward * rayDistance);
        Gizmos.DrawLine(start, start + (transform.forward + transform.right * 0.5f).normalized * rayDistance);
        Gizmos.DrawLine(start, start + (transform.forward - transform.right * 0.5f).normalized * rayDistance);
    }
}

using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    private PathfindingTester self;
    private float originalSpeed;

    private void Start()
    {
        self = GetComponent<PathfindingTester>();
        originalSpeed = self.CurrentSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        PathfindingTester otherAgent = other.GetComponent<PathfindingTester>();
        if (otherAgent == null) return;

        // If THIS one is slower → slow down
        if (self.CurrentSpeed < otherAgent.CurrentSpeed)
        {
            SlowDown();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PathfindingTester otherAgent = other.GetComponent<PathfindingTester>();
        if (otherAgent == null) return;

        ResumeNormalSpeed();
    }


    private void SlowDown()
    {
        // half speed to let faster one pass
        self.SetSpeed(self.CurrentSpeed * 0.5f);

        // optional small sidestep
        transform.position += transform.right * 0.5f;
    }

    private void ResumeNormalSpeed()
    {
        self.SetSpeed(originalSpeed);
    }
}

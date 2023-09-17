using UnityEngine;

public class ManualMovement: MonoBehaviour
{
    [SerializeField]
    private float speed = 10f;
    
    private Rigidbody rb;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;
    }

    private void Update()
    {
        // manually place End Cell
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MazeManager.Singleton.mazeGraph.PlaceGoal(transform.localPosition);
        }
    }

    private void FixedUpdate()
    {
        var moveVector = Vector3.zero;
        moveVector.x = Input.GetAxis("Horizontal");
        moveVector.z = Input.GetAxis("Vertical");
        rb.AddForce(moveVector * speed);
    }
}
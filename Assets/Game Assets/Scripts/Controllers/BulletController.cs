using UnityEngine;

public class BulletController : MonoBehaviour
{
    public int color = 0;

    public float speed = 135;
    public float radius = 0.85f;

    private Vector3 velocity;

    private bool isMoving = false;

    private Transform BubbleObject;

    public void SetType(int _color)
    {
        BubbleObject = Instantiate(LevelController.Instance.Bubbles[_color]);
        BubbleObject.position = transform.position;
        BubbleObject.parent = transform;
        color = _color;
    }

    public void FireInDirection(Vector3 forwardForce, float bulletSpeed)
    {
        speed = bulletSpeed;
        // Set the velocity of the sphere to the specified direction and speed
        velocity = forwardForce.normalized * speed;
        // Set isMoving flag to true to start updating the sphere's position
        isMoving = true;
    }

    void Update()
    {
        if (isMoving)
        {
            float deltaTime = Time.deltaTime;

            // Perform sphere cast for collision detection
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, radius, velocity.normalized, out hitInfo, velocity.magnitude * Time.deltaTime))
            {
                Vector3 normal = hitInfo.normal;
                Vector3 reflection = Vector3.Reflect(velocity.normalized, normal);
                velocity = reflection * velocity.magnitude;
                
                if (hitInfo.transform.tag == "Bubble")
                {
                    BubbleController bc = hitInfo.transform.GetComponent<BubbleController>();
                    if (bc.CollisionDelegate != null)
                    {
                        velocity = Vector3.zero;
                        isMoving = false;
                        bc.CollisionDelegate(transform);
                    }
                }

                if (hitInfo.transform.name == "Floor")
                {
                    Kill();
                }
            }

            // Update the sphere's position
            velocity.z = 0;
            Vector3 displacement = velocity * deltaTime;
            transform.position += displacement;
        }
    }

    public void Kill()
    {
        // Reset the Bullet when destroyed //
        Destroy(gameObject);
        LevelController.Instance.PrepBullet();
    }
}
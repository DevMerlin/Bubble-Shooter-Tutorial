using UnityEngine;

public class BubbleController : MonoBehaviour
{
    private Transform BubbleObject;

    CollisionDetectionDelegate collisionDelegate;
    public delegate void CollisionDetectionDelegate(Transform bullet);

    public CollisionDetectionDelegate CollisionDelegate
    {
        set
        {
            collisionDelegate = value;
        }

        get
        {
            return collisionDelegate;
        }
    }

    public void OnCollision(Transform bullet)
    {
        if (collisionDelegate != null)
        {
            collisionDelegate(bullet);
        }
    }

    public void SetType(int color)
    {
        BubbleObject = Instantiate(LevelController.Instance.Bubbles[color]);
        BubbleObject.position = transform.position;
        BubbleObject.parent = transform;
    }

    public void Kill()
    {
        transform.parent = null;
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        CollisionDelegate = null;
    }
}
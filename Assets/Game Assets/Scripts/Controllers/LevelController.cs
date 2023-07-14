using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance;

    public Transform[] Bubbles;

    public Transform Grid;
    public Transform Cannon;
    public Transform LaunchPoint;
    public Transform GameOverLine;

    public Transform BulletPrefab;
    public Transform BubblePrefab;

    public float GameOverPosition;

    public float dropSpeed = 5.0f;

    public float rotationSpeed = 10f;
    public float maxAngle = 80f;
    public float minAngle = -80f;

    public float width = 8;
    public float height = 4;

    private float currentAngle = 0f;

    public float bubbleSpacing = 3.5f;
    public float bubbleRadius = 1.15f;
    public float bulletSpeed = 45;
    
    private float rowOffset = 0;

    public bool canMove = false;
    public bool canFire = false;
    public bool ready = false;

    private BulletController nextBullet;

    private List<Transform> BubbleList;
    private Dictionary<Vector2Int, Bubble> BubbleGrid;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            Debug.LogError("An instance of this singleton already exists.");
        } else {
            Instance = this;
        }
    }

    void Start()
    {
        GameOverPosition = GameOverLine.position.y;
        Setup();
    }

    private void Setup()
    {
        CreateLevel();
        PrepBullet();
        canMove = true;
        ready = true;
    }

    void CreateLevel()
    {
        BubbleList = new List<Transform>();
        BubbleGrid = new Dictionary<Vector2Int, Bubble>();
        rowOffset = bubbleSpacing / 2;

        for (int row = 0; row < width; row++)
        {
            for (int column = 0; column < height; column++)
            {
                if (column % 2 == 1 || row < width - 1)
                {
                    Add(new Vector2Int(column, row), GetColor());
                }
            }
        }
    }

    public void Add(Vector2Int pos, int Type = 0)
    {
        // Check if the position already exists in the dictionary
        // Instantiate the bubble and set its properties
        Transform _BubbleObj = Instantiate(BubblePrefab);
        _BubbleObj.position = GridToWorld(pos.x, pos.y);
        BubbleList.Add(_BubbleObj);
        
        Bubble bubble = new Bubble(pos, Type);
        BubbleController Controller = _BubbleObj.GetComponent<BubbleController>();
        Controller.CollisionDelegate = onBubbleCollision;

        bubble.RegisterRemovalCallback(
            (cell) =>
            {
                OnBubbleRemoval(_BubbleObj, cell);
            }
        );

        Controller.SetType(Type);
        BubbleGrid.Add(pos, bubble);

        _BubbleObj.localScale = new Vector3(bubbleRadius, bubbleRadius, bubbleRadius);
        _BubbleObj.parent = Grid;
        _BubbleObj.name = "Bubble:" + pos.x + ":" + pos.y;

    }

    public Vector3 GridToWorld(int row, int column)
    {
        Vector3 pos = new Vector3();
        bool isEven = row % 2 == 0;
        pos.x = (column - (width - 1) / 2f + (isEven ? rowOffset: 0)) * bubbleSpacing;
        pos.y = (row - (height - 1) / 2f) * bubbleSpacing;
        pos.z = 0;
        // Adjust the position by the parent's rotation
        pos = Grid.rotation * pos;
        return pos + Grid.position;
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        worldPos -= Grid.position;
        worldPos = Quaternion.Inverse(Grid.rotation) * worldPos;

        // Calculate the row and column based on the adjusted world position
        int row = Mathf.RoundToInt((worldPos.y / bubbleSpacing) + (height - 1) / 2f);
        int column = Mathf.RoundToInt((worldPos.x / bubbleSpacing) - ((row % 2 == 0 ? rowOffset : 0)) + (width - 1) / 2f);

        // Return the result as a Vector2Int
        return new Vector2Int(row, column);
    }

    /// <summary>
    /// Bubble Collision detected
    /// </summary>
    /// <param name="bullet"></param>
    void onBubbleCollision(Transform bullet)
    {
        Vector2Int pos = WorldToGrid(bullet.position);
        BulletController bc = bullet.GetComponent<BulletController>();

        if (!BubbleGrid.ContainsKey(pos))
        {
            Add(pos, bc.color);
            List<Vector2Int> Matches = GetMatchingBubbles(pos, bc.color);
            // Check for all neighboring matches greater or equal to three here //
            Debug.Log(Matches.Count);

            if (Matches.Count >= 3)
            {
                foreach (Vector2Int v in Matches)
                {
                    Remove(v);
                }
            }

        }

        bc.Kill();
    }

    private List<Vector2Int> GetMatchingBubbles(Vector2Int startPosition, int type)
    {
        if (!BubbleGrid.ContainsKey(startPosition)) Debug.LogError("This starting point is invalid!");

        List<Vector2Int> matches = new List<Vector2Int>() { startPosition };
        List<Vector2Int> uncheckedNeighbors = new List<Vector2Int>() { startPosition };

        while (uncheckedNeighbors.Count > 0)
        {
            Vector2Int current = uncheckedNeighbors[0];
            uncheckedNeighbors.RemoveAt(0);

            List<Vector2Int> neighbors = GetSameTypeNeighbors(current, type);
            foreach (Vector2Int neighbor in neighbors)
            {
                if (!matches.Contains(neighbor) && BubbleGrid.ContainsKey(neighbor))
                {
                    matches.Add(neighbor);
                    uncheckedNeighbors.Add(neighbor);
                }
            }
        }

        return matches;
    }

    public List<Vector2Int> GetSameTypeNeighbors(Vector2Int pos, int type = 0)
    {
        HashSet<Vector2Int> neighbors = new HashSet<Vector2Int>(GetNeighbors(pos));
        List<Vector2Int> sameTypeNeighbors = new List<Vector2Int>();

        foreach (Vector2Int point in neighbors)
        {
            if (BubbleGrid.TryGetValue(point, out Bubble cell))
            {
                if (cell.Type == type)
                {
                    sameTypeNeighbors.Add(point);
                }
            }
        }

        return sameTypeNeighbors;
    }

    // Get the neighbors of a given position
    private List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        // Left
        neighbors.Add(new Vector2Int(pos.x - 1, pos.y));

        // Right
        neighbors.Add(new Vector2Int(pos.x + 1, pos.y));

        // Up
        neighbors.Add(new Vector2Int(pos.x, pos.y + 1));

        // Down
        neighbors.Add(new Vector2Int(pos.x, pos.y - 1));

        if (pos.x % 2 == 0)
        {
            // Even Rows
            neighbors.Add(new Vector2Int(pos.x - 1, pos.y + 1));
            neighbors.Add(new Vector2Int(pos.x + 1, pos.y + 1));
        } else { 
            // Odd Rows
            neighbors.Add(new Vector2Int(pos.x - 1, pos.y - 1));
            neighbors.Add(new Vector2Int(pos.x + 1, pos.y - 1));
        }

        return neighbors;
    }


    void OnBubbleRemoval(Transform t, Bubble cell)
    {
        BubbleController bc = t.GetComponent<BubbleController>();
        bc.Kill();
        BubbleList.Remove(t);
    }

    void Remove(Vector2Int r)
    {
        BubbleGrid[r].TriggerRemoval();
        BubbleGrid.Remove(r);
    }

    int GetColor()
    {
        return Random.Range(0, Bubbles.Length - 1);
    }

    /// <summary>
    /// Prepare the next bullet
    /// </summary>
    public void PrepBullet()
    {
        Transform bulletGO = Instantiate(BulletPrefab);
        BulletController bullet = bulletGO.GetComponent<BulletController>();
        bullet.SetType(GetColor());

        nextBullet = bullet;
        Vector3 pos = Vector3.zero;
        pos.y += 0.05f;

        bulletGO.position = pos;
        bulletGO.localScale = new Vector3(1.45f, 1.45f, 1.45f);
        canFire = true;
    }

    /// <summary>
    /// Fire bullets
    /// </summary>
    private void FireBullet()
    {
        if (nextBullet)
        {
            nextBullet.transform.localScale = new Vector3(bubbleRadius, bubbleRadius, bubbleRadius);
            nextBullet.transform.position = LaunchPoint.position;
            nextBullet.FireInDirection(LaunchPoint.up, bulletSpeed);
            nextBullet = null;
            canFire = false;
        }
    }

    private void MoveBubbles()
    {
        if (canMove)
        {
            float deltaY = dropSpeed * Time.deltaTime;
            Grid.position -= new Vector3(0, deltaY, 0);
        }
    }

    private void ScanBubblePositions()
    {
        if (BubbleGrid.Count > 0)
        {
            foreach (Transform t in BubbleList)
            {
                if (t.position.y <= GameOverPosition)
                {
                    // Game Over - Lose! //
                    ready = false;
                    canFire = false;
                }
            }
        } else {
            // Game Over - Win! //
            ready = false;
            canFire = false;
        }
    }

    private void ControlCannon()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 cannonPosition = Camera.main.WorldToScreenPoint(Cannon.position);
        Vector3 direction = mousePosition - cannonPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        currentAngle = Mathf.Clamp(90f - angle, minAngle, maxAngle);

        if (Input.GetMouseButtonDown(0) && canFire)
        {
            FireBullet();
        }

        // Rotate the object on the Y axis based on the calculated angle
        if (Cannon)
        {
            Cannon.localRotation = Quaternion.Euler(0f, 180f, currentAngle);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (ready)
        {
            ControlCannon();
            MoveBubbles();
            ScanBubblePositions();
        }
    }
}
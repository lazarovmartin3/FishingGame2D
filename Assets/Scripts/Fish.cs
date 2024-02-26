//using Unity.Netcode;
using UnityEngine;

public class Fish : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f; // Speed of the fish's movement
    [SerializeField] private float changeDirectionInterval = 2f; // Interval for changing direction

    private Vector3 randomDirection; // Random direction for the fish to move
    private float timer; // Timer for changing direction
    private Camera mainCamera; // Reference to the main camera
    private bool isHooked = false;
    private Transform hook;

    public enum FishType
    { Common, Rare };

    public int id;

    [SerializeField] private FishType fishType;
    private float commonHookProbability = 70f;
    private float rareHookProbability = 50f;

    private void Start()
    {
        // Set the initial random direction
        randomDirection = GetRandomDirection();
        FlipOrientation(randomDirection);

        // Set the initial timer value
        timer = changeDirectionInterval;

        // Get the reference to the main camera
        mainCamera = Camera.main;
    }

    private void Update()
    {
        /*if (isHooked && hook)
        {
            transform.position = hook.position;
        }
        else
        {
            // Update the timer
            timer -= Time.deltaTime;

            // Check if it's time to change direction
            if (timer <= 0f)
            {
                // Get a new random direction
                randomDirection = GetRandomDirection();
                FlipOrientation(randomDirection);

                // Reset the timer
                timer = changeDirectionInterval;
            }

            // Move the fish in the current direction
            transform.Translate(randomDirection * moveSpeed * Time.deltaTime);

            Vector3 clampedPosition = mainCamera.WorldToViewportPoint(transform.position);
            if (clampedPosition.x <= 0f || clampedPosition.x >= 1f || clampedPosition.y <= 0f || clampedPosition.y >= 1f)
            {
                // Change direction immediately
                randomDirection = GetRandomDirection();
                FlipOrientation(randomDirection);
            }
        }*/
    }

    public void UpdatePosition(Vector2 position)
    {
        transform.position = position;
    }

    // Function to get a random direction
    private Vector3 GetRandomDirection()
    {
        // Generate random x and y values for the direction
        float randomX = Random.Range(-1f, 1f);
        float randomY = Random.Range(-1f, 1f);

        // Return the normalized random direction
        return new Vector3(randomX, randomY, 0f).normalized;
    }

    private void FlipOrientation(Vector3 direction)
    {
        if (direction.x >= 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
        }
        else
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
    }

    public bool TryHook()
    {
        float hookProbability = fishType == FishType.Rare ? rareHookProbability : commonHookProbability;
        // Generate a random number between 0 and 1
        float randomValue = Random.value;
        randomValue *= 100;

        if (randomValue <= hookProbability)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Hooked(Transform hook)
    {
        isHooked = true;
        this.hook = hook;
    }

    public void Escape()
    {
        isHooked = false;
        hook = null;
    }

    public FishType GetFishType()
    {
        return fishType;
    }

    //[ServerRpc(RequireOwnership = false)]
    //public void DestroyServerRpc()
    //{
    //    GetComponent<NetworkObject>().Despawn();
    //    Destroy(this.gameObject);
    //}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Ground")
        {
            randomDirection = GetRandomDirection();
            FlipOrientation(randomDirection);
        }
    }
}
using UnityEngine;

/* Script that handles logic and interactions for wolf enemy. It randomly moves to
 * one of the four corners of the map and positions itself. The wolf locks onto
 * player and does a ranged attack towards player. End game state will trigger if
 * attack hits player or player runs into the wolf.
 */

public class EnemyWolfController : MonoBehaviour
{
    // Dependancies
    public GameObject spawnPositions;
    public GameObject target;
    public GameObject projectilePrefab;
    public GameObject menuHandler;

    // Adjustable properties
    public float speed = 8.0f;

    // Movement/Positioning 
    private Vector3 _targetPosition;
    private float _runToMovePointTimer = 3.0f;
    private bool _atTargetPosition;

    // Attack pattern
    private float _maxBorkTimer = 7.0f;
    private float _borkTimer;

    // Sound
    private bool isSpawnSoundPlayed = false;

    private void Start()
    {
        transform.position = new Vector3(0, -8, 0); // Initial spawn position at bottom of map
        PickRandomPosition();
        RotateTowards(_targetPosition);
    }

    private void FixedUpdate()
    {
        _runToMovePointTimer -= Time.fixedDeltaTime;
        _borkTimer -= Time.fixedDeltaTime;

        if (_runToMovePointTimer > 0)
        {
            if (_targetPosition != transform.position)
            {
                if (!isSpawnSoundPlayed)
                {
                    SoundController.PlaySound("EnemyWolfSpawn");
                    isSpawnSoundPlayed = true;
                }
                Move();
            }
            else
            {
                _atTargetPosition = true;
            }

        }

        if (_atTargetPosition)
        {
            RotateTowards(target.transform.position);
            if (_borkTimer < 0)
            {
                CreateProjectile();
            }
        }
    }

    // If player runs into the wolf, end the game
    private void OnCollisionEnter2D(Collision2D collision)
    {
        WolfController player = collision.gameObject.GetComponent<WolfController>();

        if (player != null)
        {
            menuHandler.GetComponent<MenuHandler>().ActivateLoseMenu();
        }

    }

    // If player projectile hits the wolf, play sound and destroy the wolf
    private void OnTriggerEnter2D(Collider2D other)
    {
        ProjectileController projectile = other.GetComponent<ProjectileController>();

        if (projectile != null)
        {
            SoundController.PlaySound("EnemyWolfDeath");
            Destroy(gameObject);
        }
    }

    private void Move()
    {
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, speed * Time.deltaTime);
    }

    // Randomly choose number from 0 - number of spawn positions. Set wolf position
    // to the chosen position.
    private void PickRandomPosition()
    {
        int spawnNumber = Random.Range(0, spawnPositions.transform.childCount);
        _targetPosition = spawnPositions.transform.GetChild(spawnNumber).transform.position;
    }

    // Rotates sprite towards inputted Vector3
    private void RotateTowards(Vector3 position)
    {
        Vector3 targetVector = new Vector3(position.x - transform.position.x, position.y - transform.position.y, 0);
        float angle = Mathf.Atan2(targetVector.y, targetVector.x) * Mathf.Rad2Deg - 90.0f; // -90 degrees is to adjust for sprite angle
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    // Initialization of projectile that will come from the wolf's mouth and move towards the player
    private void CreateProjectile()
    {
        GameObject bork = Instantiate<GameObject>(projectilePrefab);
        bork.GetComponent<EnemyProjectileController>().menuHandler = menuHandler;
        bork.transform.position = transform.GetChild(0).position;
        bork.transform.rotation = this.transform.rotation;
        bork.GetComponent<Rigidbody2D>().AddForce(transform.up * speed);
        _borkTimer = _maxBorkTimer;
    }
}

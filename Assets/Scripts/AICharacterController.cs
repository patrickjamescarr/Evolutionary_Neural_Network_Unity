using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Linq;

public enum States
{
    Seek = 0,
    Attack = 1,
    Flee = 2,
    FindHealth = 3,
    FindMagic = 4,
    Dead = 5,
    Victory = 6
}

public class AICharacterController : MonoBehaviour
{
    public AIPath aiPath;
    IsometricCharacterRenderer isoRenderer;
    private AIDestinationSetter destinationSetter;
    private SpriteRenderer spriteRenderer;

    private readonly string GreenTeamTag = "GreenTeam";
    private readonly string PurpleTeamTag = "PurpleTeam";
    private readonly string HealthTag = "Health";
    private readonly string MagicTag = "Magic";

    private readonly float HealthLow = 4.0f;
    private readonly float attackRange = 2.0f;

    private string enemyTag;

    private float _health = 10.0f;
    private float _magic = 10.0f;

    private GameObject _target;
    private GameObject persuer;
    protected GameObject[] enemies;

    protected States state;

    protected bool hasTarget;
    protected bool hasPersuer;
    protected bool healthAvailable = true;
    protected bool magicAvailable = true;
    protected float distanceToTarget = 0.0f;
    protected float distanceToPersuer = 0.0f;

    private bool findingHealth;
    private bool findingMagic;


    private float lastAttacked = 0.0f;

    protected float timeOfDeath;
    protected float timeOfVictory;
    protected float timeOfCreation;

    protected int enemiesKilled = 0;
    protected int hitsLanded = 0;
    protected int hitsTaken = 0;

    protected int attackFitness = 0;
    protected int fleeFitness = 0;
    protected int healthFitness = 0;
    protected int magicFitness = 0;

    public GameObject Target
    {
        get
        {
            return _target;
        }
    }

    public float Health
    {
        get
        {
            return _health;
        }
    }

    public float Magic
    {
        get
        {
            return _magic;
        }
    }

    private void Awake()
    {
        OnAwake();
    }

    protected void OnAwake()
    {
        timeOfCreation = Time.time;
        isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();
        destinationSetter = GetComponent<AIDestinationSetter>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        enemyTag = transform.CompareTag(GreenTeamTag) ? PurpleTeamTag : GreenTeamTag;
    }

    void Update()
    {
        UpdateEnemyStatus();

        // set the state
        SetState();

        // perform action
        Act();

        Animate();
    }

    protected void UpdateEnemyStatus()
    {
        // Get current enemies
        enemies = new List<GameObject>(GameObject.FindGameObjectsWithTag(enemyTag)).FindAll(o => o.transform.root == transform.root).ToArray();

        // Target
        hasTarget = _target != null;
        distanceToTarget = hasTarget ? Vector2.Distance(_target.transform.position, transform.position) : -1f;

        // Persuer
        persuer = FindClosestGameobject(FindPersuers().ToArray());

        hasPersuer = persuer && persuer.GetComponent<AICharacterController>().Health > 0;
        distanceToPersuer = hasPersuer ? Vector2.Distance(persuer.transform.position, transform.position) : -1f;
    }

    protected void UpdateHealthAndMagicStatus()
    {
        healthAvailable = new List<GameObject>(GameObject.FindGameObjectsWithTag(HealthTag)).FindAll(o => o.transform.root == transform.root).Any();
        magicAvailable = new List<GameObject>(GameObject.FindGameObjectsWithTag(MagicTag)).FindAll(o => o.transform.root == transform.root).Any();
    }

    protected void Animate()
    {
        // move in desired direction
        isoRenderer.SetDirection(new Vector2(aiPath.desiredVelocity.x, aiPath.desiredVelocity.y));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(HealthTag))
        {
            StartCoroutine(AddHealth());
            Destroy(collision.gameObject);
            findingHealth = false;
        }

        if (collision.CompareTag(MagicTag))
        {
            StartCoroutine(AddMagic());
            Destroy(collision.gameObject);
            findingMagic = false;
        }
    }

    protected void Act()
    {
        switch (state)
        {
            case States.Seek:
                Seek();
                return;
            case States.Attack:
                Attack();
                return;
            case States.Flee:
                Flee();
                return;
            case States.FindHealth:
                FindHealth();
                return;
            case States.FindMagic:
                FindMagic();
                return;
            case States.Dead:
                Die();
                return;
            case States.Victory:
                CancelCurrentDestination();
                return;
            default:
                return;
        }
    }

    private void FindMagic()
    {
        aiPath.maxSpeed = 0.5f;
        aiPath.endReachedDistance = 0.0f;

        if(Magic < 4)
        {
            magicFitness++;
        }
        else
        {
            magicFitness--;
        }

        if(findingMagic && aiPath.velocity.magnitude == 0.0f)
        {
            SetClosestObjectWithTagAsTarget(MagicTag, _target);
        }

        if (!findingMagic || _target == null)
        {
            findingMagic = true;

            SetClosestObjectWithTagAsTarget(MagicTag);
        }
    }

    private void FindHealth()
    {
        aiPath.maxSpeed = 1f;
        aiPath.endReachedDistance = 0.0f;

        if (Health < 5)
        {
            healthFitness++;
        }
        else
        {
            healthFitness--;
        }

        if (findingHealth && aiPath.velocity.magnitude == 0.0f)
        {
            SetClosestObjectWithTagAsTarget(MagicTag, _target);
        }

        if (!findingHealth || _target == null)
        {
            findingHealth = true;

            SetClosestObjectWithTagAsTarget(HealthTag);

            healthAvailable = _target != null;
        }
    }

    private void SetClosestObjectWithTagAsTarget(string tag, GameObject exclude = null)
    {
        var objects = new List<GameObject>(GameObject.FindGameObjectsWithTag(tag)).FindAll(o => o.transform.root == transform.root);

        if(exclude)
        {
            objects = objects.FindAll(o => o != exclude);
        }

        _target = FindClosestGameobject(objects.ToArray());

        if (_target == null) return;

        destinationSetter.target = _target.transform;
    }

    private void Die()
    {
        CancelCurrentDestination();

        spriteRenderer.material.color = Color.gray;

        if(transform.name.Equals("Learning_AI"))
        {
            transform.gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void CancelCurrentDestination()
    {
        Seeker seeker = GetComponent<Seeker>();
        seeker.CancelCurrentPathRequest();
        aiPath.destination = transform.position;
        destinationSetter.target = null;
        _target = null;
    }

    private void Seek()
    {
        aiPath.endReachedDistance = 1.5f;
        aiPath.maxSpeed = 0.5f;

        if (!hasTarget || !_target.transform.CompareTag(enemyTag))
        {
            _target = FindClosestGameobject(enemies.ToArray());

            destinationSetter.target = _target != null ? _target.transform : null;
        }
    }

    private void Attack()
    {
        aiPath.endReachedDistance = 1.5f;
        aiPath.maxSpeed = 0.5f;

        if (transform.name.Equals("Learning_AI"))
        {
            if (_magic <= 0 || (_health <= HealthLow && healthAvailable))
            {
                attackFitness--;
            }
            else
            {
                attackFitness++;
            }

            SetClosestObjectWithTagAsTarget(enemyTag);

            if(_target != null)
            {
                distanceToTarget = Vector2.Distance(_target.transform.position, transform.position);
            }
        }

        if (Time.time - lastAttacked > 1.5f && _magic >= 1)
        {
            lastAttacked = Time.time;

            if (_target == null || distanceToTarget > attackRange) return;

            var enemy = _target.GetComponent<AICharacterController>();

            if (enemy == null) return;

            StartCoroutine(enemy.TakeDamage());

            hitsLanded++;

            if(enemy.Health == 0)
            {
                enemiesKilled++;
            }

            _magic--;
        }
    }

    private void Flee()
    {
        if (!hasPersuer)
        {
            fleeFitness--;
            return;
        }

        aiPath.endReachedDistance = 0.2f;
        aiPath.maxSpeed = 1;

        // get the positions
        var position = transform.position;
        var persuerPosition = persuer.transform.position;

        if (_health <= HealthLow && Vector2.Distance(persuerPosition, position) >= attackRange)
        {
            fleeFitness++;
        }
        else
        {
            fleeFitness--;
        }

        // zero out the z coordinates
        position.z = 0f;
        persuerPosition.z = 0f;

        // get the flee direction
        var fleeVector = position - persuerPosition;

        // set the flee destination to be 5 units away from the persurer
        fleeVector.Normalize();
        fleeVector *= 5.0f;

        // set a path
        ABPath path = ABPath.Construct(position, fleeVector);
        Seeker seeker = GetComponent<Seeker>();
        seeker.StartPath(path);
    }

    private List<GameObject> FindPersuers()
    {
        List<GameObject> persuers = new List<GameObject>();

        for (int i = 0; i < enemies.Length; i++)
        {
            var enemy = enemies[i].GetComponent<AICharacterController>();

            if (enemy != null && enemy.Health > 0 && enemy.Target != null && enemy.Target.name.Equals(name))
            {
                persuers.Add(enemy.gameObject);
            }
        }

        return persuers;
    }

    protected int EnemyCount()
    {
        var count = 0;

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] == null) continue;

            var enemyAi = enemies[i].GetComponent<AICharacterController>();

            if (enemyAi != null && enemyAi.Health > 0)
            {
                count++;
            }
        }

        return count;
    }

    protected void SetState()
    {
        if (_health <= 0)
        {
            SetStateDead();
        }
        else if (EnemyCount() == 0)
        {
            SetStateVictory();
        }
        else if ((_health > HealthLow || !healthAvailable) && _magic > 0 && ((hasTarget && distanceToTarget > attackRange) || !hasTarget))
        {
            state = States.Seek;
        }
        else if ((_health > HealthLow || !healthAvailable) && _magic > 0 && hasTarget && distanceToTarget <= attackRange)
        {
            state = States.Attack;
        }
        else if (_health < HealthLow && hasTarget && distanceToTarget <= attackRange)
        {
            state = States.Flee;
        }
        else if (_health <= HealthLow && healthAvailable)
        {
            state = States.FindHealth;
        }
        else if (_magic <= 0f)
        {
            state = States.FindMagic;
        }
    }

    protected void SetStateVictory()
    {
        timeOfVictory = Time.time;
        state = States.Victory;
    }

    protected void SetStateDead()
    {
        timeOfDeath = Time.time;
        state = States.Dead;
    }

    protected GameObject FindClosestGameobject(GameObject[] objects)
    {
        GameObject closestObject = null;

        var shortedDistance = Mathf.Infinity;

        var currentPos = transform.position;

        foreach (var obj in objects)
        {
            var distance = Vector2.Distance(obj.transform.position, currentPos);

            // var enemyAlive = obj.GetComponent<AICharacterAnimationController>().Health > 0;

            if (distance < shortedDistance)
            {
                closestObject = obj;
                shortedDistance = distance;
            }
        }
        return closestObject;
    }

    private IEnumerator AddHealth()
    {
        _health += 5;

        if (_health > 10)
        {
            _health = 10;
        }

        for (var n = 0; n < 5; n++)
        {
            spriteRenderer.material.color = Color.white;
            yield return new WaitForSecondsRealtime(0.1f);
            spriteRenderer.material.color = Color.green;
            yield return new WaitForSecondsRealtime(0.1f);
        }
        spriteRenderer.material.color = Color.white;
    }

    private IEnumerator AddMagic()
    {
        _magic += 5;

        if (_magic > 10)
        {
            _magic = 10;
        }

        for (var n = 0; n < 5; n++)
        {
            spriteRenderer.material.color = Color.white;
            yield return new WaitForSecondsRealtime(0.1f);
            spriteRenderer.material.color = Color.blue;
            yield return new WaitForSecondsRealtime(0.1f);
        }
        spriteRenderer.material.color = Color.white;
    }

    public IEnumerator TakeDamage()
    {
        _health--;
        hitsTaken++;

        for (var n = 0; n < 5; n++)
        {
            if(spriteRenderer != null) spriteRenderer.material.color = Color.white;
            yield return new WaitForSecondsRealtime(0.1f);
            if (spriteRenderer != null) spriteRenderer.material.color = Color.red;
            yield return new WaitForSecondsRealtime(0.1f);
        }
        if (spriteRenderer != null) spriteRenderer.material.color = Color.white;
    }
}

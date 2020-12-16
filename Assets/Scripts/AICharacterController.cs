using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Linq;

public enum States
{
    Seek,
    Attack,
    Flee,
    FindHealth,
    FindMagic,
    Dead,
    Victory
}

public class AICharacterController : MonoBehaviour
{
    public NeuralNetwork network;

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
    private GameObject[] enemies;

    private States state;

    private bool hasTarget;
    private bool hasPersuer;
    private bool findingHealth;
    private bool findingMagic;
    private bool healthAvailable = true;
    private float distanceToTarget = 0.0f;
    private float lastAttacked = 0.0f;

    private float timeOfDeath;
    private float timeOfVictory;

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
        isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();
        destinationSetter = GetComponent<AIDestinationSetter>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        enemyTag = transform.CompareTag(GreenTeamTag) ? PurpleTeamTag : GreenTeamTag;
    }

    void Update()
    {
        enemies = new List<GameObject>(GameObject.FindGameObjectsWithTag(enemyTag)).FindAll(o => o.transform.root == transform.root).ToArray();

        hasTarget = _target != null;

        if (hasTarget)
        {
            distanceToTarget = Vector2.Distance(_target.transform.position, transform.position);
        }

        persuer = FindPersuer();
        hasPersuer = persuer && persuer.GetComponent<AICharacterController>().Health > 0;

        // set the state
        SetState();

        // perform action
        Act();

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

    private void Act()
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

        if (!_target) return;

        destinationSetter.target = _target.transform;
    }

    private void Die()
    {
        CancelCurrentDestination();
        spriteRenderer.material.color = Color.gray;
        if(transform.name.Equals("Learning_AI"))
        {
            transform.tag = "Dead Learning AI";
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

            aiPath.OnTargetReached();
        }
    }

    private void Attack()
    {
        aiPath.endReachedDistance = 1.5f;
        aiPath.maxSpeed = 0.5f;

        if (Time.time - lastAttacked > 1.5f && _magic >= 1)
        {
            lastAttacked = Time.time;

            if (!_target) return;

            var enemy = _target.GetComponent<AICharacterController>();

            StartCoroutine(enemy.TakeDamage());

            _magic--;
        }
    }

    private void Flee()
    {
        if (!hasPersuer)
        {
            return;
        }

        aiPath.endReachedDistance = 0.2f;
        aiPath.maxSpeed = 1;

        // get the positions
        var position = transform.position;
        var persuerPosition = persuer.transform.position;

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

    private GameObject FindPersuer()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            var enemy = enemies[i].GetComponent<AICharacterController>();

            if (enemy.Health > 0 && enemy.Target != null && enemy.Target.name.Equals(name))
            {
                return enemy.gameObject;
            }
        }

        return null;
    }

    private void SetState()
    {
        if (_health <= 0)
        {
            timeOfDeath = Time.time;
            state = States.Dead;
        }
        else if (enemies.Length == 0)
        {
            timeOfVictory = Time.time;
            state = States.Victory;
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

    private GameObject FindClosestGameobject(GameObject[] objects)
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

        for (var n = 0; n < 5; n++)
        {
            spriteRenderer.material.color = Color.white;
            yield return new WaitForSecondsRealtime(0.1f);
            spriteRenderer.material.color = Color.red;
            yield return new WaitForSecondsRealtime(0.1f);
        }
        spriteRenderer.material.color = Color.white;
    }

    public void UpdateFitness()
    {
        float fitness;

        if (state.Equals(States.Dead))
        {
            fitness = timeOfDeath;
        }
        else if (state.Equals(States.Victory))
        {
            fitness = timeOfVictory;
        }
        else
        {
            fitness = Time.time;
        }

        network.fitness = fitness;//updates fitness of network for sorting
    }

}

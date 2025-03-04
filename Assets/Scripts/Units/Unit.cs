using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public abstract class Unit : MonoBehaviour
{
    [SerializeField] protected SO_Character characterS0;
    protected bool IsAhero;

    #region State
    public CharacterState State { get; protected set; }
    protected bool isAttacking;
    #endregion

    #region Stats
    private int maxHealth;
    private int health;

    private int attack;
    private int magic;
    private int speed;
    private float minRange;
    private float maxRange;
    protected float attSpeed;

    protected string opponentLayerName;
    #endregion

    #region ObjectComponent
    protected Rigidbody2D rb;
    protected Animator anim;
    private SpriteRenderer sprite;
    #endregion

    #region Sounds
    [Header("Sound")]
    [SerializeField] protected AudioSource hitSound;
    #endregion

    [Header("Opponent")]
    [SerializeField] protected GameObject opponent;


    #region Unity Function
    // Start is called before the first frame update
    protected virtual void Start()
    {
        SetupStats();

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        sprite.color = Color.white;

        State = CharacterState.Idle;

        isAttacking = false;
        OnSearchClosestOpponent();

    }

    protected virtual void FixedUpdate()
    {
        DetectZone(minRange, maxRange);
        StateControler();
    }

    protected virtual void OnEnable()
    {
        CharacterEvent.OnAttackHit += TakeDamage;
        CharacterEvent.OnHeal += OnHeal;
    }

    protected virtual void OnDisable()
    {
        CharacterEvent.OnAttackHit -= TakeDamage;
        CharacterEvent.OnHeal -= OnHeal;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxRange);
    }
    #endregion

    /// <summary>
    /// Set the stats of the player
    /// </summary>
    private void SetupStats()
    {
        maxHealth = characterS0.Health;
        health = maxHealth;

        attack = characterS0.Attack;
        magic = characterS0.Magic;
        speed = characterS0.Speed;

        minRange = characterS0.MinRange;
        maxRange = characterS0.MaxRange;

        opponentLayerName = characterS0.OpponentLayer;
    }

    #region state control function

    /// <summary>
    /// Get the numbers of opponents between 2 ranges and define the state of the character
    /// </summary>
    /// <param name="min">the minimum distance at wich an opponent can be to OnAttack</param>
    /// <param name="max">the maximum distance at wich an opponent has to be to OnAttack</param>
    protected void DetectZone(float min, float max)
    {
        //Get the opponents in range
        var opponentInFleeZone = Physics2D.OverlapCircle(transform.position, min, LayerMask.NameToLayer(opponentLayerName));
        
        var opponentInAttackZone = Physics2D.OverlapCircle(transform.position, max, LayerMask.NameToLayer(opponentLayerName));

        //determine the state of the opponent depending on the number of enemy in it's detection zone
        if (opponentInFleeZone != null)
        {
            State = CharacterState.Fleeing;
        }
        else if (opponentInAttackZone == null && opponentInFleeZone == null)
        {
            State = CharacterState.Moving;
        }
        else if (opponentInAttackZone != null)
        {
            State = CharacterState.Attacking;
            Debug.Log("fire");
        }
    }

    /// <summary>
    /// Switch the comportement of the character depending of it's current state
    /// </summary>
    private void StateControler()
    {
        if (opponent != null)
        {
            switch (State)
            {
                case CharacterState.Moving:
                    OnMove(speed);
                    break;
                case CharacterState.Fleeing:
                    OnFlee(speed);
                    break;
                case CharacterState.Attacking:
                    if (!isAttacking && opponent.GetComponent<Unit>().State != CharacterState.Dying)
                    {
                        OnAttack(Mathf.Max(attack, magic));
                    }
                    break;
                default:
                    DetectZone(minRange, maxRange);
                    break;
            }
        }
        else 
            OnSearchClosestOpponent();
    }
    #endregion

    #region movement function

    /// <summary>
    /// Get the closest opponent from the character
    /// </summary>
    protected abstract void OnSearchClosestOpponent(); 

    
    protected virtual void OnMove(int speed)
    {
        float x = opponent.transform.position.x - gameObject.transform.position.x;
        float y = opponent.transform.position.y - gameObject.transform.position.y;

        Vector2 dir = new(x, y);

        rb.linearVelocity = dir.normalized * speed;

        anim.SetTrigger("IsMoving");
    }

    
    protected virtual void OnFlee(int speed)
    {
        float x = gameObject.transform.position.x - opponent.transform.position.x;
        float y = gameObject.transform.position.y - opponent.transform.position.y;

        Vector2 dir = new(x, y);

        rb.linearVelocity = dir.normalized * speed;
        anim.SetTrigger("IsMoving");
    }
    #endregion

    
    protected abstract void OnAttack(int attack);


    protected virtual void TakeDamage(int damage, int objectId)
    {
        if (objectId != gameObject.GetInstanceID())
            return;

        if (health - damage >= 0)
        {
            health -= damage;
        }
        else OnDeath();
        StartCoroutine(DamageFeedback());
    }

    private IEnumerator DamageFeedback()
    {
        sprite.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sprite.color = Color.white;
    }

    private void OnHeal(int heal, GameObject gameObject)
    {
        if (gameObject == this.gameObject)
        {
            if (State != CharacterState.Dying)
            {
                if (health + heal < maxHealth)
                {
                    health += heal;
                }
                else if ((health + heal) > maxHealth)
                {
                    health = maxHealth;
                }
            }
        }
    }

    /// <param name="gameObject">this gameObject used as an id check</param>
    /// <param name="killer">the game object that kill this gameobject</param>
    private void OnDeath()
    {
        State = CharacterState.Dying;
        StartCoroutine(DeathCoroutine());
    }
    protected abstract IEnumerator DeathCoroutine();
}

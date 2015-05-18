using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour, ITakeDamage
{
    public ParticleSystem jetpack;
    public ParticleSystem gunFlames;
    public ParticleSystem gunShells;
    public ParticleSystem touchTheGroundEffect;
    public Transform ProjectileFireLocation;
    private GameObject sceneCamera;

    public bool IsDead { get; private set; }

    public int MaxHealth = 100;

    public int Health { get; private set; }

    public GameObject HurtEffect;
    public Projectile Projectile;
    public float FireRate;
    public AudioClip PlayerJumpSfx;
    public AudioClip PlayerHitSfx;
    public AudioClip PlayerShootSfx;
    private CharacterController2D _controller;
    private Animator _animator;
    private float _normalizedHorizontalSpeed;
    private float jumpButtonPressTime = 0;
    private bool _isFacingRight = true;
    // INPUT AXIS
    private float HorizontalMove;
    private float VerticalMove;
    private float OriginalGravity;
    public JumpEnemy enemy;
    public float lowDistanceX, lowDistanceY;
    private bool canJump;
    public enum JumpState
    {
        Nothing,
        Near,
        Far,
        Height
    }
    private JumpState currentJumpState;

    // Tun El
    public Queue<int> jumpTrack = new Queue<int>();

    // movement config
    public float gravity = -25f;
    private bool isUsingGravity = true;

    private Vector3 _velocity;
    private Vector3 currentPosition;

    void Awake()
    {
        sceneCamera = GameObject.FindGameObjectWithTag("MainCamera");
        _controller = GetComponent<CharacterController2D>();
        Health = MaxHealth;
        canJump = false;
    }

    public void Start()
    {
        _animator = GetComponent<Animator>();
        _isFacingRight = transform.localScale.x > 0;

        //OriginalGravity = _controller.Parameters.Gravity;

        //_controller.State.CanMoveFreely = true;
        currentJumpState = JumpState.Nothing;

        // Jump Track add 3 steps for Enemy
        jumpTrack.Enqueue(1);
        jumpTrack.Enqueue(1);
        jumpTrack.Enqueue(1);

        // listen to some events for illustration purposes
        _controller.onControllerCollidedEvent += onControllerCollider;
        _controller.onTriggerEnterEvent += onTriggerEnterEvent;
        _controller.onTriggerExitEvent += onTriggerExitEvent;
        _controller.onTriggerStayEvent += onTriggerStayEvent;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        UpdateAnimator();

        if (!IsDead)
        {
            if (isUsingGravity)
            {
                // apply gravity before moving
                _velocity.y += gravity * Time.deltaTime;

                _controller.move(_velocity * Time.deltaTime);
            }

            if (_controller.transform.position.y < -5f)
            {
                LevelManager.Instance.KillPlayer();
            }
        }
        else
        {
            // apply gravity before moving
            _velocity.y += gravity * Time.deltaTime;

            _controller.move(_velocity * Time.deltaTime);

        }
        
    }

    void LateUpdate()
    {
        
    }

    void OnBecameInvisible()
    {
        _controller.transform.position = new Vector2(-100, 0);
        gameObject.SetActive(false);
        enabled = false;
    }

    public void Kill()
    {
        GetComponent<Collider2D>().enabled = false;
        canJump = false;
        IsDead = true;
        Health = 0;
    }

    public void FinishLevel()
    {
        enabled = false;
        _controller.enabled = false;
        GetComponent<Collider2D>().enabled = false;

    }

    public void RespawnAt(Transform spawnPoint)
    {
        IsDead = false;
        GetComponent<Collider2D>().enabled = true;
        //_controller.HandleCollisions = true;
        transform.position = spawnPoint.position;
        Health = MaxHealth;
    }

    public void TakeDamage(int damage, GameObject instigator)
    {
        //AudioSource.PlayClipAtPoint(PlayerHitSfx,transform.position);

        // When the player takes damage, we create an auto destroy hurt particle system
        Instantiate(HurtEffect, transform.position, transform.rotation);
        // we prevent the player from colliding with layer 12 (Projectiles) and 13 (Enemies)
        Physics2D.IgnoreLayerCollision(9, 12, true);
        Physics2D.IgnoreLayerCollision(9, 13, true);
        // We make the player sprite flicker
        StartCoroutine(Flicker());

        Health -= damage;
        if (Health <= 0)
        {
            LevelManager.Instance.KillPlayer();
        }
    }

    IEnumerator Flicker()
    {
        Color whateverColor = new Color32(255, 20, 20, 255); //edit r,g,b and the alpha values to what you want
        for (var n = 0; n < 10; n++)
        {
            GetComponent<Renderer>().material.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            GetComponent<Renderer>().material.color = whateverColor;
            yield return new WaitForSeconds(0.05f);
        }
        GetComponent<Renderer>().material.color = Color.white;
        // makes the player colliding again with layer 12 (Projectiles) and 13 (Enemies)
        Physics2D.IgnoreLayerCollision(9, 12, false);
        Physics2D.IgnoreLayerCollision(9, 13, false);
    }

    public void GiveHealth(int health, GameObject instigator)
    {
        Health = Mathf.Min(Health + health, MaxHealth);
    }

    private void UpdateAnimator()
    {
        _animator.SetBool("Grounded", _controller.isGrounded);
        _animator.SetFloat("Speed", Mathf.Abs(_controller.velocity.x));
        _animator.SetFloat("vSpeed", _controller.velocity.y);
    }

    void AddGroundTouchEffect()
    {
        // Manages the ground touching effect
        Instantiate(touchTheGroundEffect, new Vector2(transform.position.x, transform.position.y - transform.localScale.y / 2), transform.rotation);
    }

    void RecalculateRays()
    {
        _controller.recalculateDistanceBetweenRays();
    }

    public void JumpNear()
    {
        //currentJumpState = JumpState.Near;
        if (canJump)
        {
            StartCoroutine(JumpNearByTranslate());
            jumpTrack.Enqueue(1);
        }
    }

    public void JumpFar()
    {
        //currentJumpState = JumpState.Far;
        if (canJump)
        {
            StartCoroutine(JumpFarByTranslate());
            jumpTrack.Enqueue(2);
        }
    }

    public void JumpHeight()
    {
        //currentJumpState = JumpState.Height;
        if (canJump)
        {
            StartCoroutine(JumpHeightByTranslate());
            jumpTrack.Enqueue(3);
        }
    }

    #region Event Listeners

    void onControllerCollider(RaycastHit2D hit)
    {
        if (_controller.collisionState.becameGroundedThisFrame)
        {
            if (!canJump && hit.transform.tag == "Wood")
            {
                currentPosition = hit.transform.position;
                currentPosition.y += 0.0f;
                isUsingGravity = false;
                transform.position = currentPosition;
                canJump = true;
            }
        }
        
        // bail out on plain old ground hits cause they arent very interesting
        if (hit.normal.y == 1f)
            return;
        
        // logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
        //Debug.Log( "flags: " + _controller.collisionState + ", hit.normal: " + hit.normal );
    }


    void onTriggerEnterEvent(Collider2D other)
    {

        if (other.gameObject.tag == "Enemy")
        {
            Debug.Log("Player Die");
            other.gameObject.SetActive(false);
            LevelManager.Instance.KillPlayer();
        }
        else if (other.gameObject.tag == "BrokenWood")
        {
            other.gameObject.SetActive(false);
        }
        else if (other.gameObject.tag == "Buzzsaw")
        {
            LevelManager.Instance.KillPlayer();
        }
        else if (other.gameObject.tag == "Jumper")
        {
            gameObject.transform.position = new Vector3(other.transform.position.x + 0.4f,
                                             gameObject.transform.position.y,
                                             gameObject.transform.position.z);
            StartCoroutine(JumpFarByTranslate());
        }
    }


    void onTriggerExitEvent(Collider2D col)
    {
        
    }

    void onTriggerStayEvent(Collider2D col)
    {
        
    }
    #endregion

    void JumpByIndex(int index)
    {
        if (index == 1)
            StartCoroutine(JumpNearByTranslate());
        else if (index == 2)
            StartCoroutine(JumpFarByTranslate());
        else if (index == 3)
            StartCoroutine(JumpHeightByTranslate());
    }

    IEnumerator JumpNearByTranslate()
    {
        AddGroundTouchEffect();

        canJump = false;
        isUsingGravity = false;
        float timeJump = 0.2f;
        float jumpPower = 35.0f;
        float Height = 0.0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(startPos.x + 3.6f, startPos.y, startPos.z);
        float verticalVelocity = jumpPower;
        float curTime = 0.0f;

        while (curTime < timeJump)
        {
            float rate = curTime / timeJump;
            Height += verticalVelocity * Time.deltaTime;
            verticalVelocity = Mathf.Lerp(jumpPower, -jumpPower, rate);

            Vector3 basePosition = Vector3.Lerp(startPos, endPos, rate);
            Vector3 resultPosition = basePosition + (Vector3.up * Height);
            transform.position = resultPosition;
            curTime += Time.deltaTime;
            yield return new WaitForSeconds(0); 
        }
        
        isUsingGravity = true;
    }

    IEnumerator JumpFarByTranslate()
    {
        AddGroundTouchEffect();

        canJump = false;
        isUsingGravity = false;
        float timeJump = 0.3f;
        float jumpPower = 35.0f;
        float Height = 0.0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(startPos.x + 7.2f, startPos.y, startPos.z);
        float verticalVelocity = jumpPower;
        float curTime = 0.0f;

        while (curTime < timeJump)
        {
            float rate = curTime / timeJump;
            Height += verticalVelocity * Time.deltaTime;
            verticalVelocity = Mathf.Lerp(jumpPower, -jumpPower, rate);

            Vector3 basePosition = Vector3.Lerp(startPos, endPos, rate);
            Vector3 resultPosition = basePosition + (Vector3.up * Height);
            transform.position = resultPosition;
            curTime += Time.deltaTime;
            yield return new WaitForSeconds(0);
        }
        
        isUsingGravity = true;
    }

    IEnumerator JumpHeightByTranslate()
    {
        AddGroundTouchEffect();

        canJump = false;
        isUsingGravity = false;
        float timeJump = 0.25f;
        float jumpPower = 35.0f;
        float Height = 0.0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(startPos.x + 3.6f, startPos.y + 5.4f, startPos.z);
        float verticalVelocity = jumpPower;
        float curTime = 0.0f;

        while (curTime < timeJump)
        {
            float rate = curTime / timeJump;
            Height += verticalVelocity * Time.deltaTime;
            verticalVelocity = Mathf.Lerp(jumpPower, -jumpPower, rate);

            Vector3 basePosition = Vector3.Lerp(startPos, endPos, rate);
            Vector3 resultPosition = basePosition + (Vector3.up * Height);
            transform.position = resultPosition;
            curTime += Time.deltaTime;
            yield return new WaitForSeconds(0);
        }
        
        isUsingGravity = true;
    }
}

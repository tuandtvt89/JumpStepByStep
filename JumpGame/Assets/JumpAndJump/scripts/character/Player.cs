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
    
    private bool _isFacingRight = true;
    
    public JumpEnemy enemy;
    
    private bool canJump;
    public enum JumpState
    {
        Nothing,
        Near,
        Far,
        Height
    }

    struct ItemTrack
    {
        public int jumpIndex;
        public GameObject trackObject;
    }


    private JumpState currentJumpState;

    // Tun El
    public Queue<int> jumpTrack = new Queue<int>();

    // Find next postion
    public LayerMask platformMask = 0;
    public GameObject Spotter;

    // movement config
    public float gravity = -25f;
    private bool isUsingGravity = true;

    private Vector3 _velocity;
    private Vector3 currentPosition;

    private Vector3 mouseDownStartPos = Vector3.zero;
    private float minDistance = 70.0f;
    private float screenWidth = 0.0f;
    private Vector3 direction = Vector3.zero;
    private bool touchInLeftSide = false;

    // Tutorial 
    public GameObject leftTapTut;
    public GameObject rightTapTut;
    public GameObject swipeTut;

    private bool inTutorial = true;
    public enum TutorialState
    {
        Begin,
        JumpNear,
        JumpFar,
        JumpHeight
    }
    private TutorialState tutorialState;

    // Jump Input
    public float jumpPower = 35.0f;
    private Vector3 jumpStartPos;
    private Vector3 jumpEndPos;
    private float jumpHeight = 0.0f;
    private float jumpVerticalVelocity = 35.0f;
    private float jumpCurTime = 0.0f;

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

        _animator.Play(Animator.StringToHash("Fall"));

        screenWidth = Screen.width;
        
        int passingTutorial = PlayerPrefs.GetInt("passTutorial", 0);
        if (passingTutorial == 0)
            inTutorial = true;
        else
            inTutorial = false;
         
        enemy.SetTutorialState(inTutorial);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        // Event touch 
        if (Input.GetMouseButtonDown(0))
        {
            mouseDownStartPos = Input.mousePosition;

            if (mouseDownStartPos.x < screenWidth / 2)
                touchInLeftSide = true;
            else
                touchInLeftSide = false;
        }

        if (Input.GetMouseButtonUp(0))
        {
            direction = Input.mousePosition - mouseDownStartPos;

            if (direction.y > 0 && direction.magnitude > minDistance)
            {
                if (inTutorial && tutorialState != TutorialState.JumpFar)
                    return;

                if (inTutorial)
                {
                    tutorialState = TutorialState.JumpHeight;
                    swipeTut.SetActive(false);
                    canJump = true;
                }

                JumpHeight();
            }
            else if (direction.magnitude <= minDistance)
            {
                if (touchInLeftSide)
                {
                    if (inTutorial && tutorialState != TutorialState.Begin)
                        return;

                    if (inTutorial)
                    {
                        tutorialState = TutorialState.JumpNear;
                        leftTapTut.SetActive(false);
                        canJump = true;
                    }

                    JumpNear();
                    
                }
                else
                {
                    if (inTutorial && tutorialState != TutorialState.JumpNear)
                        return;

                    if (inTutorial)
                    {
                        tutorialState = TutorialState.JumpFar;
                        rightTapTut.SetActive(false);
                        canJump = true;
                    }

                    JumpFar();
                }
            }
        }

        WaitForJumpInput();

        if (!IsDead)
        {
            if (isUsingGravity)
            {
                // apply gravity before moving
                //_velocity.y += gravity * Time.deltaTime;
                _controller.move(new Vector3(0, gravity * Time.deltaTime, 0));
            }
        }
        else
        {
            // apply gravity before moving
            //_velocity.y += gravity * Time.deltaTime;
            _controller.move(new Vector3(0, gravity * Time.deltaTime, 0));

        }
        
    }
    
    public void Kill()
    {
        GetComponent<Collider2D>().enabled = false;
        canJump = false;
        IsDead = true;
        Health = 0;
        transform.gameObject.SetActive(false);
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

    void AddGroundTouchEffect()
    {
        // Manages the ground touching effect
        Instantiate(touchTheGroundEffect, new Vector2(transform.position.x, transform.position.y - transform.localScale.y / 2), transform.rotation);
    }

    void RecalculateRays()
    {
        _controller.recalculateDistanceBetweenRays();
    }

    /*
    private bool findNextPostionBySpotter(Vector2 start, Vector2 direction, float distance, ref Vector2 nextPostion) {
        RaycastHit2D[] hits = Physics2D.RaycastAll(start, direction, distance, platformMask);
        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit2D hit = hits[i];
                if (hit.transform.tag == "EndLevel")
                    continue;
                
                Debug.Log("Name: " + hit.transform.name);
            }
            return true;
        }
        else
            return false;
    }
    */
    public void JumpNear()
    {
        if (canJump)
        {
            // Add Score
            LevelManager.Instance.AddScore(1);

            AddGroundTouchEffect();

            canJump = false;
            isUsingGravity = false;
            currentJumpState = JumpState.Near;
            jumpStartPos = transform.position;
            jumpEndPos = new Vector3(jumpStartPos.x + 3.6f, jumpStartPos.y, jumpStartPos.z);

            _animator.Play(Animator.StringToHash("Jump"));
            jumpTrack.Enqueue(1);
        }
    }

    public void JumpFar()
    {
        if (canJump)
        {
            // Add Score
            LevelManager.Instance.AddScore(2);

            AddGroundTouchEffect();

            canJump = false;
            isUsingGravity = false;
            jumpStartPos = transform.position;
            jumpEndPos = new Vector3(jumpStartPos.x + 7.2f, jumpStartPos.y, jumpStartPos.z);

            currentJumpState = JumpState.Far;

            _animator.Play(Animator.StringToHash("Jump"));
            jumpTrack.Enqueue(2);
        }
    }

    public void JumpHeight()
    {
        if (canJump)
        {
            // Add Score
            LevelManager.Instance.AddScore(1);

            AddGroundTouchEffect();

            canJump = false;
            isUsingGravity = false;
            currentJumpState = JumpState.Height;
            jumpStartPos = transform.position;
            jumpEndPos = new Vector3(jumpStartPos.x + 3.6f, jumpStartPos.y + 5.4f, jumpStartPos.z);

            _animator.Play(Animator.StringToHash("Jump"));
            jumpTrack.Enqueue(3);
        }
    }

    private void WaitForJumpInput() {
        if (currentJumpState == JumpState.Near)
        {
            JumpByTranslate(0.2f);
        }
        else if (currentJumpState == JumpState.Far)
        {
            JumpByTranslate(0.3f);
        }
        else if (currentJumpState == JumpState.Height)
        {
            JumpByTranslate(0.25f);
        }
    }

    private void JumpByTranslate(float timeJump)
    {
        if (jumpCurTime < timeJump)
        {
            float rate = jumpCurTime / timeJump;
            jumpHeight += jumpVerticalVelocity * Time.deltaTime;
            jumpVerticalVelocity = Mathf.Lerp(jumpPower, -jumpPower, rate);

            Vector3 basePosition = Vector3.Lerp(jumpStartPos, jumpEndPos, rate);
            Vector3 resultPosition = basePosition + (Vector3.up * jumpHeight);
            _controller.move(resultPosition - transform.position);
            jumpCurTime += Time.deltaTime;
        }
        else {
            isUsingGravity = true;
            jumpHeight = 0.0f;
            jumpCurTime = 0.0f;
            jumpVerticalVelocity = jumpPower;
            currentJumpState = JumpState.Nothing;
        }
    }
    
    #region Event Listeners

    void onControllerCollider(RaycastHit2D hit)
    {
        if (_controller.collisionState.becameGroundedThisFrame)
        {
            if (canJump)
                return;

            if (hit.transform.tag == "Wood")
            {
                _controller.move(hit.transform.position - transform.position);
                canJump = true;

                _animator.Play(Animator.StringToHash("Idle"));

                if (inTutorial)
                {
                    canJump = false;
                    if (tutorialState == null)
                    {
                        tutorialState = TutorialState.Begin;
                    }

                    if (tutorialState == TutorialState.Begin)
                    {
                        leftTapTut.SetActive(true);
                    }
                    else if (tutorialState == TutorialState.JumpNear){
                        rightTapTut.SetActive(true);
                    }
                    else if (tutorialState == TutorialState.JumpFar)
                    {
                        swipeTut.SetActive(true);
                    }
                    else
                    {
                        inTutorial = false;
                        PlayerPrefs.SetInt("passTutorial", 1);
                        enemy.SetTutorialState(false);

                        // Tiny trick help enemy can jump again
                        enemy.transform.position += new Vector3(0f, 0.5f, 0f);
                    }
                }
            }
            else if (hit.transform.tag == "Crocodile")
            {   
                _controller.move(hit.transform.position - transform.position);
                canJump = true;

                _animator.Play(Animator.StringToHash("Idle"));

                CrocodileEnemy crocodileEnemy = hit.transform.gameObject.GetComponent<CrocodileEnemy>();
                crocodileEnemy.onAttack += this.onCrocodileAttack;
                crocodileEnemy.WakeUp();
            }
            else if (hit.transform.tag == "Snake")
            {   
                _controller.move(hit.transform.position - transform.position);
                canJump = true;

                _animator.Play(Animator.StringToHash("Idle"));

                SnakeEnemy snakeEnemy = hit.transform.gameObject.GetComponent<SnakeEnemy>();
                snakeEnemy.onAttack += this.onSnakeAttack;
                snakeEnemy.Attack();
            }
            else if (hit.transform.tag == "Hippo")
            {   
                _controller.move(hit.transform.position - transform.position);
                canJump = true;

                _animator.Play(Animator.StringToHash("Idle"));

                HippoEnemy hippoEnemy = hit.transform.gameObject.GetComponent<HippoEnemy>();
                hippoEnemy.onAttack += this.onHippoAttack;
                hippoEnemy.WakeUp();
            }
            else if (hit.transform.tag == "Turtle")
            {
                _controller.move(hit.transform.position - transform.position);
                canJump = true;

                _animator.Play(Animator.StringToHash("Idle"));

                TurtleEnemy turtleEnemy = hit.transform.gameObject.GetComponent<TurtleEnemy>();
                turtleEnemy.onAttack += this.onTurtleAttack;
                turtleEnemy.WakeUp();
            }
            else if (hit.transform.tag == "TeleportWood")
            {
                _controller.move(hit.transform.position - transform.position);
                canJump = true;

                _animator.Play(Animator.StringToHash("Idle"));

                TeleportWood teleportWood = hit.transform.gameObject.GetComponent<TeleportWood>();
                teleportWood.AddTargetTrack();
            }
            else if (hit.transform.tag == "BrokenPlatform")
            {
                _controller.move(hit.transform.position - transform.position);
                canJump = true;

                _animator.Play(Animator.StringToHash("Idle"));

                BrokenPlatform brokenPlatform = hit.transform.gameObject.GetComponent<BrokenPlatform>();
                brokenPlatform.onBroken += this.onBrokenPlatform;
                brokenPlatform.WakeUp();
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
        else if (other.gameObject.tag == "DeadObstruction")
        {
            LevelManager.Instance.KillPlayer();
        }
        else if (other.gameObject.tag == "Jumper")
        {
            //_controller.move(other.transform.position - transform.position);
            // Add Score
            LevelManager.Instance.AddScore(2);

            AddGroundTouchEffect();

            isUsingGravity = false;
            jumpStartPos = transform.position;
            jumpEndPos = new Vector3(jumpStartPos.x + 7.2f, jumpStartPos.y, jumpStartPos.z);

            currentJumpState = JumpState.Far;

            _animator.Play(Animator.StringToHash("Jump"));
        }
    }

    void onTriggerExitEvent(Collider2D col)
    {
        
    }

    void onTriggerStayEvent(Collider2D col)
    {
        
    }

    void onCrocodileAttack(float posX)
    {
        if (_controller.collisionState.becameGroundedThisFrame || _controller.isGrounded || _controller.collisionState.wasGroundedLastFrame)
        {
            if (transform.position.x <= posX + 0.5f)
                LevelManager.Instance.KillPlayer();
        }
    }

    void onSnakeAttack(float posX)
    {
        if (_controller.collisionState.becameGroundedThisFrame || _controller.isGrounded || _controller.collisionState.wasGroundedLastFrame)
        {
            if (transform.position.x <= posX + 0.5f)
                LevelManager.Instance.KillPlayer();
        }
    }

    void onHippoAttack(float posX)
    {
        if (_controller.collisionState.becameGroundedThisFrame || _controller.isGrounded || _controller.collisionState.wasGroundedLastFrame)
        {
            if (transform.position.x <= posX + 0.5f)
                LevelManager.Instance.KillPlayer();
        }
    }

    void onTurtleAttack(float posX)
    {
        if (_controller.collisionState.becameGroundedThisFrame || _controller.isGrounded || _controller.collisionState.wasGroundedLastFrame)
        {
            if (transform.position.x <= posX + 0.5f)
                LevelManager.Instance.KillPlayer();
        }
    }

    void onBrokenPlatform(float posX)
    {
		if (_controller.collisionState.becameGroundedThisFrame || _controller.isGrounded || _controller.collisionState.wasGroundedLastFrame)
		{
			if (transform.position.x <= posX + 0.5f)
				canJump = false;
		}
    }
    #endregion

}

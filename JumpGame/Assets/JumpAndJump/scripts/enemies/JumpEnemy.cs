using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JumpEnemy : MonoBehaviour
{

    public BoxCollider2D HeadCollider;
    public ParticleSystem touchTheGroundEffect;
    private CharacterController2D _controller;
    private Animator _animator;
    private float _normalizedHorizontalSpeed;
    private float jumpButtonPressTime = 0;
    private bool _isFacingRight = true;
    // INPUT AXIS
    private float HorizontalMove;
    private float VerticalMove;
    private float OriginalGravity;
    public float forceToLowJumpX, forceToLowJumpY, forceToFarJumpX, forceToFarJumpY, forceToHighJumpX, forceToHighJumpY;
    public enum JumpState
    {
        Nothing,
        Near,
        Far,
        Height
    }
    private JumpState currentJumpState;
    public float timeToThink;
    private float currentTimeToThink;
    private int jumpIndex;
    public GameObject player;
    private bool canJump;

    // Tun El
    // movement config
    public float gravity = -25f;
    private bool isUsingGravity = true;
    private Player playerScript;
    private Vector3 _velocity;
    private Vector3 currentPosition;
    private BoxCollider2D _boxCollider;

    public Transform startPos;

    // Tutorial
    private bool inTutorial = false;

    void Awake()
    {
        _controller = GetComponent<CharacterController2D>();
        
        currentTimeToThink = 0.0f;
    }

    public void Start()
    {
        _animator = GetComponent<Animator>();
        _isFacingRight = transform.localScale.x > 0;
        _boxCollider = GetComponent<BoxCollider2D>();
        transform.position = new Vector2(transform.position.x, startPos.position.y);

        playerScript = player.GetComponent<Player>();

        canJump = false;

        // listen to some events for illustration purposes
        _controller.onControllerCollidedEvent += onControllerCollider;
        _controller.onTriggerEnterEvent += onTriggerEnterEvent;
        _controller.onTriggerExitEvent += onTriggerExitEvent;
        _controller.onTriggerStayEvent += onTriggerStayEvent;
        _animator.Play(Animator.StringToHash("Jump"));
    }

    public void Update()
    {
        //UpdateAnimator();

        if (isUsingGravity)
        {
            // apply gravity before moving
            _velocity.y += gravity * Time.deltaTime;

            _controller.move(_velocity * Time.deltaTime);
        }
    }

    public void SetTutorialState(bool mInTutorial)
    {
        this.inTutorial = mInTutorial;
    }

    private void UpdateAnimator()
    {

        //_animator.SetBool("isGrounded", _controller.isGrounded);
        //_animator.SetFloat("Speed", Mathf.Abs(_controller.velocity.x));
        //_animator.SetFloat("vSpeed", _controller.velocity.y);
    }

    public void JumpNear()
    {
        currentJumpState = JumpState.Near;
    }

    public void JumpFar()
    {
        currentJumpState = JumpState.Far;
    }

    public void JumpHeight()
    {
        currentJumpState = JumpState.Height;
    }

    private Vector2 GetForceByJumpState()
    {
        Vector2 JumpWithForce = Vector2.zero;
        if (currentJumpState == JumpState.Near)
            JumpWithForce = new Vector2(forceToLowJumpX, forceToLowJumpY);
        else if (currentJumpState == JumpState.Far)
            JumpWithForce = new Vector2(forceToFarJumpX, forceToFarJumpY);
        else if (currentJumpState == JumpState.Height)
            JumpWithForce = new Vector2(forceToHighJumpX, forceToHighJumpY);
        return JumpWithForce;
    }

    void AddGroundTouchEffect()
    {
        // Manages the ground touching effect
        Instantiate(touchTheGroundEffect, new Vector2(transform.position.x, transform.position.y - transform.localScale.y / 2), transform.rotation);
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

                _animator.Play(Animator.StringToHash("Idle"));

                if (inTutorial)
                    return;

                isUsingGravity = false;
                canJump = true;

                if (playerScript.jumpTrack.Count > 0)
                    StartCoroutine(JumpAndThink(playerScript.jumpTrack.Dequeue()));
            }
            else if (hit.transform.tag == "Crocodile")
            {
                _controller.move(hit.transform.position - transform.position);
                isUsingGravity = false;
                canJump = true;

                _animator.Play(Animator.StringToHash("Idle"));

                if (playerScript.jumpTrack.Count > 0)
                    JumpByIndex(playerScript.jumpTrack.Dequeue());

                CrocodileEnemy crocodileEnemy = hit.transform.gameObject.GetComponent<CrocodileEnemy>();
                crocodileEnemy.WakeUpWithoutAttacking();
            }
            else if (hit.transform.tag == "Snake")
            {
                _controller.move(hit.transform.position - transform.position);
                isUsingGravity = false;
                canJump = true;

                _animator.Play(Animator.StringToHash("Idle"));

                if (playerScript.jumpTrack.Count > 0)
                    JumpByIndex(playerScript.jumpTrack.Dequeue());

            }
            else if (hit.transform.tag == "Hippo")
            {
                _controller.move(hit.transform.position - transform.position);
                isUsingGravity = false;
                canJump = true;

                _animator.Play(Animator.StringToHash("Idle"));

                if (playerScript.jumpTrack.Count > 0)
                    JumpByIndex(playerScript.jumpTrack.Dequeue());

                HippoEnemy hippoEnemy = hit.transform.gameObject.GetComponent<HippoEnemy>();
                hippoEnemy.WakeUpWithoutAttacking();
            }
            else if (hit.transform.tag == "Turtle")
            {
                _controller.move(hit.transform.position - transform.position);
                isUsingGravity = false;
                canJump = true;

                _animator.Play(Animator.StringToHash("Idle"));

                if (playerScript.jumpTrack.Count > 0)
                    JumpByIndex(playerScript.jumpTrack.Dequeue());

                TurtleEnemy turtleEnemy = hit.transform.gameObject.GetComponent<TurtleEnemy>();
                turtleEnemy.WakeUpWithoutAttacking();
            }
        }

        // bail out on plain old ground hits cause they arent very interesting
        if (hit.normal.y == 1f)
            return;

    }


    void onTriggerEnterEvent(Collider2D other)
    {
        if (other.gameObject.tag == "BrokenWood")
        {
            other.gameObject.SetActive(false);
        }
        else if (other.gameObject.tag == "Jumper")
        {
            gameObject.transform.position = new Vector3(other.transform.position.x + 0.4f,
                                             gameObject.transform.position.y,
                                             gameObject.transform.position.z);
            StartCoroutine(JumpFarByTranslate());
        }
    }

    void onTriggerStayEvent(Collider2D other)
    {
        if (other.gameObject.tag == "TeleportWood")
        {
            if (isUsingGravity) // Check when finish jumping
            {
                isUsingGravity = false;
                canJump = true;

                _animator.Play(Animator.StringToHash("Idle"));

                if (playerScript.jumpTrack.Count > 0)
                    JumpByIndex(playerScript.jumpTrack.Dequeue());
            }
        }
        else if (other.gameObject.tag == "BrokenPlatform")
        {
            if (isUsingGravity) // Check when finish jumping
            {
                isUsingGravity = false;
                canJump = true;

                _animator.Play(Animator.StringToHash("Idle"));

                if (playerScript.jumpTrack.Count > 0)
                    JumpByIndex(playerScript.jumpTrack.Dequeue());
            }
        }
        else if (other.gameObject.tag == "TeleportWoodVerticle")
        {
            var teleportWoodVerticleObject = other.transform.FindChild("TeleportWood");
            if (teleportWoodVerticleObject)
            {
                TeleportWood teleportWoodScript = teleportWoodVerticleObject.gameObject.GetComponent<TeleportWood>();


                if (isUsingGravity) // Check when finish jumping
                {
                    if (true)
                    {
                        isUsingGravity = false;
                        canJump = true;

                        _animator.Play(Animator.StringToHash("Idle"));

                        if (playerScript.jumpTrack.Count > 0)
                            JumpByIndex(playerScript.jumpTrack.Dequeue());
                    }
                }
            }
            
        }
    }

    void onTriggerExitEvent(Collider2D other)
    {

    }
    #endregion

    public void Pause()
    {
        canJump = false;
        enabled = false;
        StopAllCoroutines();
    }

    IEnumerator JumpAndThink(int index)
    {
        if (canJump)
        {
            int distance = Mathf.FloorToInt((player.transform.position.x - transform.position.x) / 3.5f);
            if (distance < 4)
                timeToThink = 0.5f;
            else
                timeToThink = 0.0f;
            yield return new WaitForSeconds(timeToThink);
            JumpByIndex(index);
            _animator.Play(Animator.StringToHash("Jump"));
        }
    }

    void JumpByIndex(int index)
    {
        if (canJump)
        {
            if (index == 1)
                StartCoroutine(JumpNearByTranslate());
            else if (index == 2)
                StartCoroutine(JumpFarByTranslate());
            else if (index == 3)
                StartCoroutine(JumpHeightByTranslate());
        }
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
            _controller.move(resultPosition - transform.position);
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
            _controller.move(resultPosition - transform.position);
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
            _controller.move(resultPosition - transform.position);
            curTime += Time.deltaTime;
            yield return new WaitForSeconds(0);
        }

        isUsingGravity = true;
    }
}

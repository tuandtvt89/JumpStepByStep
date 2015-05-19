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

    void Awake()
    {
        _controller = GetComponent<CharacterController2D>();
        
        currentTimeToThink = 0.0f;
    }

    public void Start()
    {
        _animator = GetComponent<Animator>();
        _isFacingRight = transform.localScale.x > 0;

        playerScript = player.GetComponent<Player>();

        canJump = false;

        // listen to some events for illustration purposes
        _controller.onControllerCollidedEvent += onControllerCollider;
        _controller.onTriggerEnterEvent += onTriggerEnterEvent;
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

    void LateUpdate()
    {
        
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
            if (!canJump && hit.transform.tag == "Wood")
            {

                _controller.move(hit.transform.position - transform.position);
                isUsingGravity = false;
                canJump = true;

                Debug.Log("IsGrounded");
                _animator.Play(Animator.StringToHash("Idle"));

                if (playerScript.jumpTrack.Count > 0)
                    StartCoroutine(JumpAndThink(playerScript.jumpTrack.Dequeue()));
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
    #endregion

    public void Pause()
    {
        canJump = false;
        //enabled = false;
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

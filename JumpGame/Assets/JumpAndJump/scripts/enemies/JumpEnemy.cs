using UnityEngine;
using System.Collections;

public class JumpEnemy : MonoBehaviour
{

    public BoxCollider2D HeadCollider;
    public ParticleSystem touchTheGroundEffect;
    private CharacterController2D _controller;
    private Animator _animator;
    private bool _isFacingRight = true;
   
    public enum JumpState
    {
        Nothing,
        Near,
        Far,
        Height
    }
    private JumpState currentJumpState;
    public float timeToThink;
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

    // Jump Input
    public float jumpPower = 35.0f;
    private Vector3 jumpStartPos;
    private Vector3 jumpEndPos;
    private float jumpHeight = 0.0f;
    private float jumpVerticalVelocity = 35.0f;
    private float jumpCurTime = 0.0f;

    void Awake()
    {
        _controller = GetComponent<CharacterController2D>();
    }

    public void Start()
    {
        _animator = GetComponent<Animator>();
        _isFacingRight = transform.localScale.x > 0;
        _boxCollider = GetComponent<BoxCollider2D>();
        transform.position = new Vector2(transform.position.x, startPos.position.y);

        currentJumpState = JumpState.Nothing;

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

        if (isUsingGravity)
        {
            // apply gravity before moving
            //_velocity.y += gravity * Time.deltaTime;
            _controller.move(new Vector3(0, gravity * Time.deltaTime, 0));
        }

        WaitForJumpInput();
    }

    public void SetTutorialState(bool mInTutorial)
    {
        this.inTutorial = mInTutorial;
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
                    StartCoroutine(JumpAndThink(playerScript.jumpTrack.Dequeue()));

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
            _controller.move(other.transform.position - transform.position);
            isUsingGravity = false;
            canJump = true;

            _animator.Play(Animator.StringToHash("Idle"));
            JumpByIndex(2);
        }
		else if (other.gameObject.tag == "BrokenPlatform")
		{
			_controller.move(other.transform.position - transform.position);
			isUsingGravity = false;
			canJump = true;
			
			_animator.Play(Animator.StringToHash("Idle"));
			
			if (playerScript.jumpTrack.Count > 0)
				JumpByIndex(playerScript.jumpTrack.Dequeue());
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
                timeToThink = 0.1f;
            yield return new WaitForSeconds(timeToThink);
            JumpByIndex(index);
        }
    }

    void JumpByIndex(int index)
    {
        if (canJump)
        {
            AddGroundTouchEffect();

            canJump = false;
            isUsingGravity = false;
            jumpStartPos = transform.position;
            _animator.Play(Animator.StringToHash("Jump"));

            if (index == 1)
            {
                currentJumpState = JumpState.Near;
                jumpEndPos = new Vector3(jumpStartPos.x + 3.6f, jumpStartPos.y, jumpStartPos.z);
            }
            else if (index == 2)
            {
                currentJumpState = JumpState.Far;
                jumpEndPos = new Vector3(jumpStartPos.x + 7.2f, jumpStartPos.y, jumpStartPos.z);
            }
            else if (index == 3)
            {
                currentJumpState = JumpState.Height;
                jumpEndPos = new Vector3(jumpStartPos.x + 3.6f, jumpStartPos.y + 5.4f, jumpStartPos.z);
            }
        }
    }

    private void WaitForJumpInput()
    {
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
        else
        {
            isUsingGravity = true;
            jumpHeight = 0.0f;
            jumpCurTime = 0.0f;
            jumpVerticalVelocity = jumpPower;
            currentJumpState = JumpState.Nothing;
        }
    }
}

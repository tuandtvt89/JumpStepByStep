using UnityEngine;
using System.Collections;

public class SnakeEnemy : MonoBehaviour {

    public float waitToAttackTime = 0.4f;
    public float timeToWakeUp = 0.8f;
    public float timeToSleep = 1f;
    public delegate void SnakeAttackHandler();
    public event SnakeAttackHandler onAttack;

    private Animator _animator;
    private EdgeCollider2D _collider;
    private bool isReallyWaked;

    // Use this for initialization
    void Start()
    {
        _animator = GetComponent<Animator>();

        _collider = GetComponent<EdgeCollider2D>();

        isReallyWaked = false;

        StartCoroutine(WaitAndWakeUp());
    }

    public void Attack()
    {
        StartCoroutine("WaitAndAttack");
    }

    private IEnumerator WaitAndAttack()
    {
        isReallyWaked = true;
        _collider.enabled = true;
        _animator.Play(Animator.StringToHash("Waked"));
        yield return new WaitForSeconds(waitToAttackTime);
        _animator.Play(Animator.StringToHash("Attack"));
        if (onAttack != null)
        {
            onAttack();
        }

        // Return to normal
        yield return new WaitForSeconds(0.5f);
        _animator.Play(Animator.StringToHash("Waked"));
    }

    private IEnumerator WaitAndWakeUp()
    {
        while (!isReallyWaked)
        {
            _animator.Play(Animator.StringToHash("Idle"));
            _collider.enabled = false;
            yield return new WaitForSeconds(timeToWakeUp);
            
            _animator.Play(Animator.StringToHash("Waked"));
            _collider.enabled = true;
            yield return new WaitForSeconds(timeToSleep);
        }
        
    }
}

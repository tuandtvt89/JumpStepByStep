using UnityEngine;
using System.Collections;

public class TurtleEnemy : MonoBehaviour {

    public float waitToHideTime = 0.5f;
    public delegate void TurtleAttackHandler(float posX);
    public event TurtleAttackHandler onAttack;

    private Animator _animator;
    // Use this for initialization
    void Start()
    {
        _animator = GetComponent<Animator>();
        _animator.Play(Animator.StringToHash("Idle"));
    }

    public void WakeUp()
    {
        StartCoroutine("WaitAndAttack");
    }

    public void WakeUpWithoutAttacking()
    {
        StartCoroutine("WaitAndSleep");
    }

    private IEnumerator WaitAndAttack()
    {
        _animator.Play(Animator.StringToHash("Waked"));
        yield return new WaitForSeconds(waitToHideTime);
        _animator.Play(Animator.StringToHash("Hide"));
        if (onAttack != null)
        {
            onAttack(transform.position.x);
        }

        // Return to normal
        yield return new WaitForSeconds(0.5f);
        _animator.Play(Animator.StringToHash("Idle"));
    }

    private IEnumerator WaitAndSleep()
    {
        _animator.Play(Animator.StringToHash("Waked"));

        // Return to normal
        yield return new WaitForSeconds(0.5f);
        _animator.Play(Animator.StringToHash("Idle"));
    }
}

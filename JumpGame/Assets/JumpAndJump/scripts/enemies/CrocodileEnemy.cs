﻿using UnityEngine;
using System.Collections;

public class CrocodileEnemy : MonoBehaviour {
    public float waitToAttackTime = 0.5f;
    public delegate void CrocodileAttackHandler(float posX);
    public event CrocodileAttackHandler onAttack;

    private Animator _animator;
    // Use this for initialization
    void Start() {
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
        yield return new WaitForSeconds(waitToAttackTime);
        _animator.Play(Animator.StringToHash("Attack"));
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

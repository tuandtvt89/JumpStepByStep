using UnityEngine;
using System.Collections;

public class BrokenPlatform : MonoBehaviour {

    private float waitToBrokenTime = 0.3f;
    private Animator _animator;
    private EdgeCollider2D _collider;

    public delegate void BrokenPlatformHandler();
    public event BrokenPlatformHandler onBroken;
    // Use this for initialization
    void Start()
    {
        _animator = GetComponent<Animator>();
        _collider = GetComponent<EdgeCollider2D>();
        _animator.Play(Animator.StringToHash("Broken_Idle"));
    }

    public void WakeUp()
    {
        StartCoroutine("WaitAndAttack");
    }

    private IEnumerator WaitAndAttack()
    {
        _animator.Play(Animator.StringToHash("Broken_Break"));
        yield return new WaitForSeconds(waitToBrokenTime);
        _collider.enabled = false;
        if (onBroken != null)
        {
            onBroken();
        }
        yield return new WaitForSeconds(0.9f - waitToBrokenTime);
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
    }
}

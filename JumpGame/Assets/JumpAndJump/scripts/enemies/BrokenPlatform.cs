using UnityEngine;
using System.Collections;

public class BrokenPlatform : MonoBehaviour {

    private float waitToBrokenTime = 0.35f;
    private Animator _animator;
    private EdgeCollider2D _collider;

	public delegate void BrokenPlatformHandler(float posX);
    public event BrokenPlatformHandler onBroken;

	private int levelNumber;
    // Use this for initialization
    void Start()
    {
        _animator = GetComponent<Animator>();
        _collider = GetComponent<EdgeCollider2D>();

		levelNumber = LevelManager.Instance.levelNumber;
		if (levelNumber == 0)
        	_animator.Play(Animator.StringToHash("Broken_Idle"));
		else if (levelNumber == 1)
			_animator.Play(Animator.StringToHash("Broken_Idle_Night"));
		else
			_animator.Play(Animator.StringToHash("Broken_Idle_Sunset"));
    }

    public void WakeUp()
    {
        StartCoroutine("WaitAndAttack");
    }

    private IEnumerator WaitAndAttack()
    {
		if (levelNumber == 0)
			_animator.Play(Animator.StringToHash("Broken_Break"));
		else if (levelNumber == 1)
			_animator.Play(Animator.StringToHash("Broken_Break_Night"));
		else
			_animator.Play(Animator.StringToHash("Broken_Break_Sunset"));
        
        yield return new WaitForSeconds(waitToBrokenTime);
        _collider.enabled = false;
        if (onBroken != null)
        {
            onBroken(transform.position.x);
        }
        yield return new WaitForSeconds(0.6f - waitToBrokenTime);
        gameObject.GetComponent<SpriteRenderer>().enabled = false;

		yield return new WaitForSeconds(10.0f);
		gameObject.GetComponent<SpriteRenderer>().enabled = true;
		_collider.enabled = true;
		if (levelNumber == 0)
			_animator.Play(Animator.StringToHash("Broken_Idle"));
		else if (levelNumber == 1)
			_animator.Play(Animator.StringToHash("Broken_Idle_Night"));
		else
			_animator.Play(Animator.StringToHash("Broken_Idle_Sunset"));
    }
}

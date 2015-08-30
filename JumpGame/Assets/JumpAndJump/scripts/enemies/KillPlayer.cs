using UnityEngine;
using System.Collections;

public class KillPlayer : MonoBehaviour 
{
	

	void Start () 
	{
		
	}
	
	
	public void LateUpdate () 
	{

	}
	
	public void OnTriggerEnter2D(Collider2D other)
	{
		var player=other.GetComponent<Player>();
		if (player==null)
			return;
		LevelManager.Instance.KillPlayer ();
	}
}

using UnityEngine;
using System.Collections;

public class StartScreen : MonoBehaviour 
{
	public string FirstLevel;
	
	void Start () 
	{
	
	}
	
	void Update () 
	{
		if (!Input.GetButtonDown("Jump"))
			return;
		Application.LoadLevel(FirstLevel);
	
	}
}

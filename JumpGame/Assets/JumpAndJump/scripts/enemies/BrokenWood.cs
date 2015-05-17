using UnityEngine;
using System.Collections;

public class BrokenWood : MonoBehaviour
{
		private Animator animator;
		// Use this for initialization
		void Start ()
		{
				animator = gameObject.GetComponent<Animator> ();
		}
	
		// Update is called once per frame
		void Update ()
		{
	
		}

		public void OnTriggerEnter2D (Collider2D other)
		{
				
				animator.SetBool ("Broken", true);
		}

		
}

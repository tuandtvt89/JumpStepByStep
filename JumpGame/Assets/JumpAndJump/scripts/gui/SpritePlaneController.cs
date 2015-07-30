using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class SpritePlaneController : MonoBehaviour {

	public List<Sprite> arraySprite = new List<Sprite>();
	// Use this for initialization
	void Start () {
		int levelNumber = LevelManager.Instance.levelNumber;
		if (levelNumber != 0) {
			foreach (Transform child in transform)
				child.gameObject.SetActive(false);
		}
		GetComponent<SpriteRenderer> ().sprite = arraySprite.ElementAt (levelNumber);
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class ThinkscrollBackgroundHandler : MonoBehaviour {

	public List<GameObject> backgroundArray = new List<GameObject>();
	// Use this for initialization
	void Start () {
		int levelNumber = LevelManager.Instance.levelNumber;

		foreach (GameObject child in backgroundArray) {
			child.gameObject.SetActive(false);
		}

		backgroundArray.ElementAt (levelNumber).SetActive (true);

	}
}

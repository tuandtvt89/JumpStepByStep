using UnityEngine;
using System.Collections;
using Prime31;

public class GGPlayServiceHandler : MonoBehaviour {

	// Use this for initialization
	void Start () {
        PlayGameServices.init("797863827416.apps.googleusercontent.com", true);

        StartCoroutine(CheckLoginAndStart());
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    IEnumerator CheckLoginAndStart()
    {
        yield return new WaitForSeconds(1);
        if (!PlayGameServices.isSignedIn())
        {
            PlayGameServices.authenticate();
        }
    }

    public void ShowAchievement() {
        if (!PlayGameServices.isSignedIn())
            PlayGameServices.authenticate();
        else
            PlayGameServices.showAchievements();
    }

    public void ShowLeaderBoard()
    {
        if (!PlayGameServices.isSignedIn())
            PlayGameServices.authenticate();
        else
            PlayGameServices.showLeaderboard("CgkI2K-Po5wXEAIQAQ");
    }
}

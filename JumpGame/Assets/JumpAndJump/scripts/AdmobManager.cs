using UnityEngine;
using System.Collections;
using Prime31;

public class AdmobManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
        //AdMob.createBanner("ca-app-pub-3705088179152525/3782204294", "", AdMobBanner.SmartBanner, AdMobLocation.TopCenter);

        AdMob.requestInterstital("ca-app-pub-3705088179152525/5258937492", "");
        //AdMob.hideBanner(true);
    }
	
}

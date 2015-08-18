using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using Prime31;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public Player Player { get; private set; }

    public JumpEnemy JumpEnemy { get; private set; }

    public CameraController mCamera { get; private set; }

    public TimeSpan RunningTime { get { return DateTime.UtcNow - _started; } }

    public GameObject GameOverPopUp;
    //level platform
    public List<GameObject> levelArray = new List<GameObject>();
    private List<GameObject> levelList = new List<GameObject>();
    private GameObject firstLevel, secondLevel, thirdLevel, fourthLevel;
    public int currentLevelIndex;

    public int CurrentTimeBonus
    {
        get
        {
            var secondDifference = (int)(BonusCutoffSeconds - RunningTime.TotalSeconds);
            return Mathf.Max(0, secondDifference) * BonusSecondMultiplier;
        }
    }

    private List<CheckPoint> _checkpoints;
    private int _currentCheckPointIndex;
    private DateTime _started;
    private int _savedPoints;
    public CheckPoint DebugSpawn;
    public int BonusCutoffSeconds = 30;
    public int BonusSecondMultiplier = 1;

    private float bottomPostionY = 0f;

	public int levelNumber;

    public Text scoreText; 

    public void Awake()
    {
        _savedPoints = GameManager.Instance.Points;
        Instance = this;

        // Find the bottom of the scene
        float camHalfHeight = Camera.main.orthographicSize;
        bottomPostionY = Camera.main.transform.position.y - camHalfHeight + 5f;

        // Random levelNumber
        levelNumber = UnityEngine.Random.Range (0, 3);
    }

    public void Start()
    {
        // Init score
        GameManager.Instance.ResetPoints(0);
        scoreText = scoreText.GetComponent<Text>();

        // Show banner admob
        AdMob.hideBanner(false);

        FeedLevel();
        _checkpoints = FindObjectsOfType<CheckPoint>().OrderBy(t => t.transform.position.x).ToList();
        _currentCheckPointIndex = _checkpoints.Count > 0 ? 0 : -1;

        Player = FindObjectOfType<Player>();
        mCamera = FindObjectOfType<CameraController>();
        JumpEnemy = FindObjectOfType<JumpEnemy>();

            _started = DateTime.UtcNow;

        var listeners = FindObjectsOfType<MonoBehaviour>().OfType<IPlayerRespawnListener>();
        foreach (var listener in listeners)
        {
            for (var i = _checkpoints.Count - 1; i >= 0; i--)
            {
                var distance = ((MonoBehaviour)listener).transform.position.x - _checkpoints[i].transform.position.x;
                if (distance < 0)
                    continue;

                _checkpoints[i].AssignObjectToCheckPoint(listener);
                break;
            }
        }

#if UNITY_EDITOR
        if (DebugSpawn != null)
        {
            DebugSpawn.SpawnPlayer(Player);
        }
        else if (_currentCheckPointIndex != -1)
        {
            _checkpoints[_currentCheckPointIndex].SpawnPlayer(Player);
        }
#else
				if (_currentCheckPointIndex != -1) {			
						_checkpoints [_currentCheckPointIndex].SpawnPlayer (Player);
				}
#endif
    }

    public void AddScore(int score)
    {
        GameManager.Instance.AddPoints(score);
        scoreText.text = "" + GameManager.Instance.Points;
    }

    public void Update()
    {
        
    }

    public void GotoLevel(string levelName)
    {
        StartCoroutine(GotoLevelCo(levelName));
    }

    public void RePlay()
    {
        Application.LoadLevel("Level");
    }

    public void BackToHome() {
        Application.LoadLevel(0);
    }

    private IEnumerator GotoLevelCo(string levelName)
    {
        Player.FinishLevel();
        GameManager.Instance.AddPoints(CurrentTimeBonus);
        yield return new WaitForSeconds(2f);

        if (string.IsNullOrEmpty(levelName))
            Application.LoadLevel("StartScreen");
        else
            Application.LoadLevel(levelName);

    }

    public void KillPlayer()
    {
        StartCoroutine(KillPlayerCo());
    }

    IEnumerator KillPlayerCo()
    {
        int randomNumber = UnityEngine.Random.Range(0, 2);
        if (AdMob.isInterstitalReady() && randomNumber == 0) {
            AdMob.displayInterstital();

            AdMob.requestInterstital("ca-app-pub-3705088179152525/5258937492", "");
        }
            
        mCamera.cameraFollowsPlayer = false;
        Player.Kill();
        yield return new WaitForSeconds(0.5f);
        JumpEnemy.Pause();
        GameOverPopUp.SetActive(true);

        // Submit score
        PlayGameServices.submitScore("CgkI2K-Po5wXEAIQAQ", GameManager.Instance.Points);

        GameManager.Instance.ResetPoints(_savedPoints);
    }

    //level function
    void FeedLevel()
    {
        for (int i = 0; i < levelArray.Count(); i++)
        {
            levelList.Add(levelArray[i]);
            levelArray[i].transform.position = new Vector3(-40.0f, bottomPostionY, 0);
        }
        firstLevel = levelList[0];
        levelList.Remove(firstLevel);
        firstLevel.transform.position = new Vector3(0, bottomPostionY, 0);
        secondLevel = levelList[UnityEngine.Random.Range(0, levelList.Count())];
        secondLevel.transform.position = new Vector3(32.3f, bottomPostionY, 0);
        levelList.Remove(secondLevel);
        thirdLevel = levelList[UnityEngine.Random.Range(0, levelList.Count())];
        thirdLevel.transform.position = new Vector3(32.3f * 2, bottomPostionY, 0);
        levelList.Remove(thirdLevel);
        fourthLevel = levelList[UnityEngine.Random.Range(0, levelList.Count())];
        fourthLevel.transform.position = new Vector3(32.3f * 3, bottomPostionY, 0);
        levelList.Remove(fourthLevel);
        currentLevelIndex = 0;
    }

    public void SwapLevel()
    {
        if (currentLevelIndex > 1)
        {
            levelList.Add(firstLevel);
            firstLevel.transform.position = new Vector3(-40, bottomPostionY, 0);
            firstLevel = secondLevel;
            secondLevel = thirdLevel;
            thirdLevel = fourthLevel;
            fourthLevel = levelList[UnityEngine.Random.Range(0, levelList.Count())];
            levelList.Remove(fourthLevel);
            fourthLevel.transform.position = new Vector3(32.3f * (currentLevelIndex + 2), bottomPostionY, 0);
        }
        currentLevelIndex++;
    }
}


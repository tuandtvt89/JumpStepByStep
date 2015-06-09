using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TeleportWood : MonoBehaviour {

    public delegate void TargetTeleportHandler();
    public event TargetTeleportHandler inTarget;
    public enum Navigation
    {
        Vertical,
        Horizontal
    };

    public Navigation navigation;
    public float distanceX = 3.6f;
    public float distanceY = 5.4f;
    public float lifeTime = 1.0f;
    public int stepNumber = 3;
    
    List<Vector2> tracks;
    Transform _transform;
    SpriteRenderer sprite;
    Queue<Vector2> targetTrack;
    int currentTrack = 0;
    int lastJumpTrack = -1;

	// Use this for initialization
	void Start () {
        tracks = new List<Vector2>();
        _transform = transform;
        sprite = _transform.gameObject.GetComponent<SpriteRenderer>();
        StartCoroutine(Teleport());
        targetTrack = new Queue<Vector2>();
	}

    private List<Vector2> GenerateTrack(Navigation navi, int stepNumber) {
        List<Vector2> tempTracks = new List<Vector2>();
        Vector2 currentPosition = transform.localPosition;

        for (int i = 0; i < stepNumber; i++) {
            if (navi == Navigation.Horizontal)
            {
                Vector2 temp = new Vector2(distanceX * i, 0);
                tempTracks.Add(currentPosition + temp);
            }
            else {
                Vector2 temp = new Vector2(0, distanceY * i);
                tempTracks.Add(currentPosition + temp);
            }
        }

        return tempTracks;
    }

    private IEnumerator Teleport() {
        tracks = GenerateTrack(navigation, stepNumber);

        bool increase = true;
        while (true)
        {
            if (increase && currentTrack < stepNumber - 1)
                currentTrack++;

            if (!increase && currentTrack > 0)
                currentTrack--;

            if (currentTrack == 0)
                increase = true;

            if (currentTrack == stepNumber - 1)
                increase = false;
            
            StartCoroutine("FadeOut", 0.3f);
            _transform.localPosition = tracks[currentTrack];
            StartCoroutine("FadeIn", 0.3f);
            yield return new WaitForSeconds(lifeTime);
        }
    }

    public void AddTargetTrack() {
        targetTrack.Enqueue(tracks[currentTrack]);
    }

    public float GetLastTrackHeight()
    {
        float height = 0f;

        if (targetTrack.Count != 0)
        {
            Vector2 lastTrackPos = targetTrack.Dequeue();
            height = lastTrackPos.y;
        }

        return height;
    }

    private IEnumerator FadeIn(float time)
    {
        float startTime = Time.time;
        while ((Time.time - startTime) < time)
        {
            sprite.color = new Color(1f, 1f, 1f, (Time.time - startTime) / time);
            yield return 0;
        }
    }

    private IEnumerator FadeOut(float time)
    {
        float startTime = Time.time;
        while ((Time.time - startTime) < time)
        {
            sprite.color = new Color(1f, 1f, 1f, 1.0f - (Time.time - startTime) / time);
            yield return 0;
        }
    }
}

using UnityEngine;
using System.Collections;

public class PressButton : UIEventTrigger {
    public GameObject Player;   

    float pressTime = -1f;
    float tressHold = 0.5f; // tressHold = 0.5 seconds
    bool buttonIsPressed = false;

    void OnPress(bool isPressed)
    {
        if (isPressed)
        {
            if (pressTime < 0)
                pressTime = Time.time;

            buttonIsPressed = true;
            StartCoroutine("checkHoldButton");
        }
        else
        {
            float deltaTime = Time.time - pressTime;
            if (deltaTime < tressHold)
            {
                buttonIsPressed = false;
                pressTime = -1f;
                Player.GetComponent<Player>().JumpNear();
            }
        }
    }

    IEnumerator checkHoldButton() {
        while (buttonIsPressed) {
            if (Time.time - pressTime > tressHold)
            {
                buttonIsPressed = false;
                pressTime = -1f;
                Player.GetComponent<Player>().JumpFar();
            }

            yield return new WaitForSeconds(0.05f);
        }    
    }
}

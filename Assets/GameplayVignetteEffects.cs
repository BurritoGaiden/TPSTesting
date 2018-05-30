using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameplayVignetteEffects : MonoBehaviour {

    public Image uiElement;
    public Image altElement;
    public enum fadeState { not, fading};
    public fadeState thisFadeState = fadeState.not;
    public float fadingTo;
    public float scalingTo;
    public float boopScalingTo;
    public float timeSinceShotAt;

    void Awake() {
        EnemyAPC.HitPlayer += Hit;
        //StartCoroutine("ScaleUpLoop");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X)) {
            
                StartCoroutine(Scaler(altElement, altElement.GetComponent<RectTransform>().localScale.x, 3));
            
        }

        if (Input.GetKeyDown(KeyCode.C)) {
            
                StartCoroutine(Scaler(altElement, altElement.GetComponent<RectTransform>().localScale.x, 2));
            
        }

        if (altElement.GetComponent<RectTransform>().localScale.x == .62f)
        {
            if (boopScalingTo != 1)
            {
                StartCoroutine(Scaler(altElement, altElement.GetComponent<RectTransform>().localScale.x, 1, .00001f));
                boopScalingTo = 1;
            }
        }
        else if (altElement.GetComponent<RectTransform>().localScale.x == 1) {
            if (boopScalingTo != .62f) {
                StartCoroutine(Scaler(altElement, altElement.GetComponent<RectTransform>().localScale.x, .62f, .00001f));
                boopScalingTo = .62f;
            }
        }

        if (PlayerController.thisMoveState == MoveState.STATE_REGULAR) {
            if(fadingTo != .5f)
            {
                StopCoroutine("FadeCanvasGroup");
                FadeReg();
                fadingTo = .5f;
            }
        }

        if (PlayerController.thisMoveState == MoveState.STATE_COVER)
        {
            if (fadingTo != 1f)
            {
                StopCoroutine("FadeCanvasGroup");
                FadeIn();
                fadingTo = 1f;
            }
        }

        if (timeSinceShotAt < 5)
        {
            if (scalingTo != 4.6f)
            {
                //StopCoroutine("Scaler");
                ScaleIn();
                scalingTo = 4.6f;
                print("scaling in");
            }
        }
        else if(timeSinceShotAt == 5)
        {
            if (scalingTo != 6.4f)
            {
               // StopCoroutine("Scaler");
                ScaleOut();
                scalingTo = 6.4f;
                print("scaling out");
            }
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            timeSinceShotAt = 0;
        }

        if (timeSinceShotAt < 5) {
            timeSinceShotAt += Time.deltaTime;
            if (timeSinceShotAt > 5) {
                timeSinceShotAt = 5;
            }
        }
    }

    public void Hit() {
        timeSinceShotAt = 0;
    }

    public void ScaleIn() {
        StartCoroutine(Scaler(uiElement, uiElement.GetComponent<RectTransform>().localScale.x, 4.6f));
    }

    public void ScaleOut() {
        StartCoroutine(Scaler(uiElement, uiElement.GetComponent<RectTransform>().localScale.x, 6.4f));
    }

    public void FadeIn() {
        StartCoroutine(FadeCanvasGroup(uiElement, uiElement.color.a, 1));
    }

    public void FadeReg() {
        StartCoroutine(FadeCanvasGroup(uiElement, uiElement.color.a, .5f));
    }

    public void FadeOut() {
        StartCoroutine(FadeCanvasGroup(uiElement, uiElement.color.a,0));
    }

    public IEnumerator Scaler(Image cg, float start, float end, float lerpTime = .5f)
        {

        float _timeStartedLerping = Time.time;
        float timeSinceStarted = Time.time - _timeStartedLerping;
        float percentageComplete = timeSinceStarted / lerpTime;

        while (true)
        {
            timeSinceStarted = Time.time - _timeStartedLerping;
            percentageComplete = timeSinceStarted / lerpTime;

            Vector3 temp = cg.GetComponent<RectTransform>().localScale;
            temp.x = Mathf.Lerp(start, end, percentageComplete);
            temp.y = Mathf.Lerp(start, end, percentageComplete);
            cg.GetComponent<RectTransform>().localScale = temp;
            if (percentageComplete >= 1) break;

            yield return new WaitForEndOfFrame();
        }
        print("done with scaling");
    }

    public IEnumerator FadeCanvasGroup(Image cg, float start, float end, float lerpTime = .5f) {
        float _timeStartedLerping = Time.time;
        float timeSinceStarted = Time.time - _timeStartedLerping;
        float percentageComplete = timeSinceStarted / lerpTime;

        while (true) {
            timeSinceStarted = Time.time - _timeStartedLerping;
            percentageComplete = timeSinceStarted / lerpTime;

            float currentValue = Mathf.Lerp(start, end, percentageComplete);

            cg.color = new Vector4(cg.color.r,cg.color.g,cg.color.b, currentValue);

            if (percentageComplete >= 1) break;

            yield return new WaitForEndOfFrame();
        }
        print("done with opacity adjust");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour {

    public bool run;

    public Text pushableText;
    public Text coverText;

    public Image buttonPromptImage;
    public Image healthVignette;
    public Image healthMeter;
    public Image[] UIImages;
    public Text[] UIText;

    void Awake() {
        LevelScript.b_SetUIElementEnabled += SetElement;
        EnemyAPC.HitPlayer += UpdateHealthMeter;
    }

    void UpdateHealthMeter() {
        healthMeter.fillAmount = CharacterHealth.health / 100;
    }

    void SetElement(string desiredObject, bool desiredState) {
        for (int i = 0; i < UIImages.Length; i++) {
            if (UIImages[i].name == desiredObject) {
                UIImages[i].enabled = desiredState;
            }
        }
    }
}

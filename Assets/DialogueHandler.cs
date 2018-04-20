using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueHandler : MonoBehaviour
{
    //Input Data
    public int desiredLineIndex;

    //Current Data
    private int currentLineIndex;
    private string currentLineString;
    private float currentTimeTillTextOff;

    //Resources
    public AudioClip[] dialogueClips;
    public string[] dialogueStrings;

    //Interfaces
    public AudioSource audioSource;
    public Text textDisplayBox;

    void Start() {
        LevelScript.ThisDialogue += PresentDialogue;
    }

    // Update is called once per frame
    void Update()
    {

        //Input: Commented out if scripting
        /*
        if (Input.GetKeyDown(KeyCode.R))
        {
            PresentDialogue(desiredLineIndex);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (desiredLineIndex < (dialogueClips.Length - 1)) desiredLineIndex++;
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            if (desiredLineIndex >= 1) desiredLineIndex--;
        }
        */
        //Text "stay" check
        if (currentTimeTillTextOff > 0)
        {
            currentTimeTillTextOff -= Time.deltaTime;
        }

        if (currentTimeTillTextOff <= 0)
        {
            RemoveText();
        }
    }

    //Tells the game to present text and audio, set static data, then clear everything after clip has finished
    void PresentDialogue(int chosenLine)
    {
        currentLineIndex = chosenLine;
        currentLineString = dialogueStrings[chosenLine];


        PlayDialogueClip(chosenLine);
        DisplayText(chosenLine);

        currentTimeTillTextOff = dialogueClips[chosenLine].length;
    }

    //Plays the audio clip
    void PlayDialogueClip(int chosenLine)
    {
        audioSource.Stop();
        audioSource.clip = dialogueClips[chosenLine];
        audioSource.Play();

        //audioSource.PlayOneShot(dialogueClips[chosenLine]);
    }

    //Presents text
    void DisplayText(int chosenLine)
    {
        textDisplayBox.text = dialogueStrings[chosenLine];

    }

    //Removes text
    void RemoveText()
    {
        textDisplayBox.text = "";
        currentLineIndex = 0;
        currentLineString = "";
        audioSource.Stop();
    }
}

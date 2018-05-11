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
    public static float currentTimeTillTextOff;

    //Resources
    public AudioClip[] dialogueClips;
    public string[] dialogueStrings;

    //Interfaces
    public AudioSource audioSource;
    public Text textDisplayBox;

    void Start() {
        LevelScript.PlayThisDialogue += PresentDialogue;
    }

    // Update is called once per frame
    void Update()
    {
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
        //If the chosen line is in the array length of the dialogue clips and there are clips
        if (dialogueClips[chosenLine] != null)
        {
            PlayDialogueClip(chosenLine);
        }
        if (dialogueStrings[chosenLine] != null)
        {
            DisplayText(chosenLine);
            
        }
        else
            print("This line doesn't exist");

        if (dialogueClips.Length > 0 && dialogueClips[chosenLine] != null && dialogueStrings[chosenLine] != null)
        {
             currentTimeTillTextOff = dialogueClips[chosenLine].length;
        }
        else if (dialogueClips[chosenLine] == null && dialogueStrings[chosenLine] != null)
            currentTimeTillTextOff = (float)dialogueStrings[chosenLine].Length * .1f;

        Debug.Log(currentTimeTillTextOff);
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
        if(textDisplayBox.text !="") Debug.Log("BEGONE THOT");
        textDisplayBox.text = "";
        currentLineIndex = 0;
        currentLineString = "";
        audioSource.Stop();
    }
}

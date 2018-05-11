using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objective{

    //The name of the objective
    public string name;
    //The description of the objective
    public string description;
    //The type of objective: 1 - collect, 2 - kill, 3 - go to, 4 - do/be a thing here
    public int objectiveType;
    //The name-base the Objective is looking to assign targets to
    public string nameBase;
    //Target List: Objects in scene the game will add visual markers to during that objective
    public List<Transform> targetList = new List<Transform>();

    //Objective Constructor
    public Objective(string thisName, string thisDesc, int thisType, string targetNameBase) {
        name = thisName;
        description = thisDesc;
        objectiveType = thisType;

        //Get all targets in scene
        GameObject[] targetArray = GameObject.FindGameObjectsWithTag("Target");

        //For every target in scene, check if their name base matches this objective's
        for (int i = 0; i < targetArray.Length; i++) {
            //Get first characters of name string equal to length of targetnamebase and check against
            if (targetArray[i].name.Length < targetNameBase.Length) continue;

            string currentNameBase = targetArray[i].name.Substring(0, targetNameBase.Length);
            if (currentNameBase == targetNameBase)
            {
                targetList.Add(targetArray[i].transform);
            }
        }
        
    }
}
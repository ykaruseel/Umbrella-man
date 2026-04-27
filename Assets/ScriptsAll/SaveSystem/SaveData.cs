using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    //Player
    public Vector3 playerPosition;
    public Quaternion playerRotation;
    public float playerRotationY;
    public float playerRotationX;

    //TV dec
    public bool isTVDecOn;
    public Vector3 tvDecPosition;
    public Quaternion tvDecRotation;
    
    //TV
    public bool isTVOn;
    public bool isFirstDayTVOn;
    public bool isTVPlaced;
    public string tvTag;
    public Vector3 tvPosition;
    public Quaternion tvRotation;

    //Plant
    public bool isPlantOn;
    public bool isFirstDayPlantOn;
    public bool isPlantPlaced;
    public string plantTag;
    public Vector3 plantPosition;
    public Quaternion plantRotation;

    //Art
    public bool isArtOn;
    public bool isFirstDayArtOn;
    public bool isArtPlaced;
    public string artTag;
    public Vector3 artPosition;
    public Quaternion artRotation;

    //TableLamp
    public bool isTableLampOn;
    public bool isFirstDayTableLampOn;
    public bool isTableLampPlaced;
    public string tableLampTag;
    public Vector3 tableLampPosition;
    public Quaternion tableLampRotation;

    //Flashlight
    public bool isFlashlightOn;
    public bool isFlashlightInInventory;
    public Vector3 flashlightPosition;
    public Quaternion flashlightRotation;

    //Quest
    public int currentQuestIndex;
    public List<string> completedGoalIDs = new List<string>();

    //Tutorial
    public List<HintType> savedShownHints;

    //Enemy
    public bool isEnemyActive;
    public bool isChasing;
    public Vector3 enemyPosition;
    public Quaternion enemyRotation;

    //LesterGramaphone
    public bool isLesterGramophoneOn;

    //TriggerQ1
    public bool isBoxColliderOnQ1;
    public bool isFocusTriggerOnQ1;
    public bool isFocusTriggerHasTriggeredQ1;

    //TriggerQ3TV
    public bool isBoxColliderOnQ3TV;
    public bool isFocusTriggerOnQ3TV;
    public bool isFocusTriggerHasTriggeredQ3TV;

    //TriggerQ3
    public bool isBoxColliderOnQ3;
    public bool isFocusTriggerOnQ3;
    public bool isFocusTriggerHasTriggeredQ3;

    //TriggerQ4
    public bool isBoxColliderOnQ4;
    public bool isFocusTriggerOnQ4;
    public bool isFocusTriggerHasTriggeredQ4;

    //TriggerQ5
    public bool isBoxColliderOnQ5;
    public bool isFocusTriggerOnQ5;
    public bool isFocusTriggerHasTriggeredQ5;

    //TriggerQ6
    public bool isBoxColliderOnQ6;
    public bool isFocusTriggerOnQ6;
    public bool isFocusTriggerHasTriggeredQ6;

    //TriggerQ8
    public bool isBoxColliderOnQ8;
}

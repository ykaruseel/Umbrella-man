using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private string savePath;

    public static bool IsNewGame = true;

    public GameObject player;

    public GameObject tvDec;

    public GameObject tv;

    public GameObject plant;

    public GameObject art;

    public GameObject lamp;

    public GameObject firstDayTV;

    public GameObject firstDayPlant;

    public GameObject firstDayArt;

    public GameObject firstDayTableLamp;

    public GameObject flashlightGO;

    public Flashlight flashlight;

    public GameObject enemy;

    public GameObject TriggerQ1;

    public GameObject TriggerQ3TV;

    public GameObject TriggerQ3;

    public GameObject TriggerQ4;

    public GameObject TriggerQ5;

    public GameObject TriggerQ6;

    public GameObject TriggerQ8;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "save.json");
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F5))
        {
            SaveGame();
        }
        else if (Input.GetKeyDown(KeyCode.F9))
        {
            LoadGame();
        }
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        data.playerPosition = player.transform.position;
        data.playerRotation = player.transform.rotation;

        player.GetComponent<PlayerController>().GetRotation(out data.playerRotationY, out data.playerRotationX);

        data.isTVDecOn = tvDec.activeSelf;
        data.tvDecPosition = tvDec.transform.position;
        data.tvDecRotation = tvDec.transform.rotation;

        data.isTVOn = tv.activeSelf;
        data.isFirstDayTVOn = firstDayTV.activeSelf;
        data.isTVPlaced = tv.GetComponent<PlaceableItem>().isPlaced;
        data.tvTag = tv.tag;
        data.tvPosition = tv.transform.position;
        data.tvRotation = tv.transform.rotation;

        data.isPlantOn = plant.activeSelf;
        data.isFirstDayPlantOn = firstDayPlant.activeSelf;
        data.isPlantPlaced = plant.GetComponent<PlaceableItem>().isPlaced;
        data.plantTag = plant.tag;
        data.plantPosition = plant.transform.position;
        data.plantRotation = plant.transform.rotation;

        data.isArtOn = art.activeSelf;
        data.isFirstDayArtOn = firstDayArt.activeSelf;
        data.isArtPlaced = art.GetComponent<PlaceableItem>().isPlaced;
        data.artTag = art.tag;
        data.artPosition = art.transform.position;
        data.artRotation = art.transform.rotation;

        data.isTableLampOn = lamp.activeSelf;
        data.isFirstDayTableLampOn = firstDayTableLamp.activeSelf;
        data.isTableLampPlaced = lamp.GetComponent<PlaceableItem>().isPlaced;
        data.tableLampTag = lamp.tag;
        data.tableLampPosition = lamp.transform.position;
        data.tableLampRotation = lamp.transform.rotation;

        data.isFlashlightOn = flashlightGO.activeSelf;
        data.isFlashlightInInventory = flashlight.enabled;
        data.flashlightPosition = flashlightGO.transform.position;
        data.flashlightRotation = flashlightGO.transform.rotation;

        data.currentQuestIndex = QuestManagerV2.Instance.GetCurrentQuest();
        data.completedGoalIDs = QuestManagerV2.Instance.GetCompletedGoals();

        data.savedShownHints = new List<HintType>(TutorialManager.Instance.GetShownHints());

        data.isEnemyActive = enemy.activeSelf;
        data.isChasing = enemy.GetComponent<KnifeManAI>().isChasing;
        data.enemyPosition = enemy.transform.position;
        data.enemyRotation = enemy.transform.rotation;

        data.isLesterGramophoneOn = QuestEvents.Instance.emitter.IsPlaying();

        data.isBoxColliderOnQ1 = TriggerQ1.GetComponent<BoxCollider>().enabled;
        data.isFocusTriggerOnQ1 = TriggerQ1.GetComponent<FocusTrigger>().enabled;
        data.isFocusTriggerHasTriggeredQ1 = TriggerQ1.GetComponent<FocusTrigger>().hasTriggered;

        data.isBoxColliderOnQ3TV = TriggerQ3TV.GetComponent<BoxCollider>().enabled;
        data.isFocusTriggerOnQ3TV = TriggerQ3TV.GetComponent<FocusTrigger>().enabled;
        data.isFocusTriggerHasTriggeredQ3TV = TriggerQ3TV.GetComponent<FocusTrigger>().hasTriggered;

        data.isBoxColliderOnQ3 = TriggerQ3.GetComponent<BoxCollider>().enabled;
        data.isFocusTriggerOnQ3 = TriggerQ3.GetComponent<FocusTrigger>().enabled;
        data.isFocusTriggerHasTriggeredQ3 = TriggerQ3.GetComponent<FocusTrigger>().hasTriggered;

        data.isBoxColliderOnQ4 = TriggerQ4.GetComponent<BoxCollider>().enabled;
        data.isFocusTriggerOnQ4 = TriggerQ4.GetComponent<FocusTrigger>().enabled;
        data.isFocusTriggerHasTriggeredQ4 = TriggerQ4.GetComponent<FocusTrigger>().hasTriggered;

        data.isBoxColliderOnQ5 = TriggerQ5.GetComponent<BoxCollider>().enabled;
        data.isFocusTriggerOnQ5 = TriggerQ5.GetComponent<FocusTrigger>().enabled;
        data.isFocusTriggerHasTriggeredQ5 = TriggerQ5.GetComponent<FocusTrigger>().hasTriggered;

        data.isBoxColliderOnQ6 = TriggerQ6.GetComponent<BoxCollider>().enabled;
        data.isFocusTriggerOnQ6 = TriggerQ6.GetComponent<FocusTrigger>().enabled;
        data.isFocusTriggerHasTriggeredQ6 = TriggerQ6.GetComponent<FocusTrigger>().hasTriggered;    

        data.isBoxColliderOnQ8 = TriggerQ8.GetComponent<BoxCollider>().enabled;

        string json = JsonUtility.ToJson(data, true);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));

            using (FileStream fileStream = new FileStream(savePath, FileMode.Create))
            {
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.Write(json);
                }
            }

            Debug.Log("Game saved");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Save error: " + e);
        }
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("Save file not found!");
            return;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            CharacterController cc = player.GetComponent<CharacterController>();

            cc.enabled = false;
            player.transform.position = data.playerPosition;
            player.transform.rotation = data.playerRotation;
            player.GetComponent<PlayerController>().SetRotation(data.playerRotationY,data.playerRotationX);
            cc.enabled = true;


            tvDec.SetActive(data.isTVDecOn);
            tvDec.transform.position = data.tvDecPosition;
            tvDec.transform.rotation = data.tvDecRotation;

            tv.SetActive(data.isTVOn);
            firstDayTV.SetActive(data.isFirstDayTVOn);
            tv.GetComponent<PlaceableItem>().isPlaced = data.isTVPlaced;
            tv.tag = data.tvTag;
            tv.transform.position = data.tvPosition;
            tv.transform.rotation = data.tvRotation;

            plant.SetActive(data.isPlantOn);
            firstDayPlant.SetActive(data.isFirstDayPlantOn);
            plant.GetComponent<PlaceableItem>().isPlaced = data.isPlantPlaced;
            plant.tag = data.plantTag;
            plant.transform.position = data.plantPosition;
            plant.transform.rotation = data.plantRotation;

            art.SetActive(data.isArtOn);
            firstDayArt.SetActive(data.isFirstDayArtOn);
            art.GetComponent<PlaceableItem>().isPlaced = data.isArtPlaced;
            art.tag = data.artTag;
            art.transform.position = data.artPosition;
            art.transform.rotation = data.artRotation;

            lamp.SetActive(data.isTableLampOn);
            firstDayTableLamp.SetActive(data.isFirstDayTableLampOn);
            lamp.GetComponent<PlaceableItem>().isPlaced = data.isTableLampPlaced;
            lamp.tag = data.tableLampTag;
            lamp.transform.position = data.tableLampPosition;
            lamp.transform.rotation = data.tableLampRotation;

            flashlightGO.SetActive(data.isFlashlightOn);
            flashlight.enabled = data.isFlashlightInInventory;
            flashlightGO.transform.position = data.flashlightPosition;
            flashlightGO.transform.rotation = data.flashlightRotation;

            enemy.SetActive(data.isEnemyActive);
            enemy.GetComponent<KnifeManAI>().isChasing = data.isChasing;
            enemy.transform.position = data.enemyPosition;
            enemy.transform.rotation = data.enemyRotation;

            if (data.isLesterGramophoneOn)
                QuestEvents.Instance.emitter.Play();
            else
                QuestEvents.Instance.emitter.Stop();

            TriggerQ1.GetComponent<BoxCollider>().enabled = data.isBoxColliderOnQ1;
            TriggerQ1.GetComponent<FocusTrigger>().enabled = data.isFocusTriggerOnQ1;
            TriggerQ1.GetComponent<FocusTrigger>().hasTriggered = data.isFocusTriggerHasTriggeredQ1;

            TriggerQ3TV.GetComponent<BoxCollider>().enabled = data.isBoxColliderOnQ3TV;
            TriggerQ3TV.GetComponent<FocusTrigger>().enabled = data.isFocusTriggerOnQ3TV;
            TriggerQ3TV.GetComponent<FocusTrigger>().hasTriggered = data.isFocusTriggerHasTriggeredQ3TV;

            TriggerQ3.GetComponent<BoxCollider>().enabled = data.isBoxColliderOnQ3;
            TriggerQ3.GetComponent<FocusTrigger>().enabled = data.isFocusTriggerOnQ3;
            TriggerQ3.GetComponent<FocusTrigger>().hasTriggered = data.isFocusTriggerHasTriggeredQ3;

            TriggerQ4.GetComponent<BoxCollider>().enabled = data.isBoxColliderOnQ4;
            TriggerQ4.GetComponent<FocusTrigger>().enabled = data.isFocusTriggerOnQ4;
            TriggerQ4.GetComponent<FocusTrigger>().hasTriggered = data.isFocusTriggerHasTriggeredQ4;

            TriggerQ5.GetComponent<BoxCollider>().enabled = data.isBoxColliderOnQ5;
            TriggerQ5.GetComponent<FocusTrigger>().enabled = data.isFocusTriggerOnQ5;
            TriggerQ5.GetComponent<FocusTrigger>().hasTriggered = data.isFocusTriggerHasTriggeredQ5;

            TriggerQ6.GetComponent<BoxCollider>().enabled = data.isBoxColliderOnQ6;
            TriggerQ6.GetComponent<FocusTrigger>().enabled = data.isFocusTriggerOnQ6;
            TriggerQ6.GetComponent<FocusTrigger>().hasTriggered = data.isFocusTriggerHasTriggeredQ6;

            TriggerQ8.GetComponent<BoxCollider>().enabled = data.isBoxColliderOnQ8;

            QuestManagerV2.Instance.SetQuestFromLoad(data.currentQuestIndex, data.completedGoalIDs);

            TutorialManager.Instance.LoadShownHints(data.savedShownHints);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Load error: " + e);
        }
    }

    private void OnApplicationQuit()
    {
        IsNewGame = false;
    }
}

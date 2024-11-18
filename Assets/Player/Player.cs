using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Player : MonoBehaviour
{
    private void Start()
    {
        Test test = new Test();
        test.playerCharacter.Add(1);
        string jsonster = JsonUtility.ToJson(test);
        string path = Application.persistentDataPath + "/playerData.json";
        File.WriteAllText(path, jsonster);
        Debug.Log("JSON saved to" + path);
    }
}

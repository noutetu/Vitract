using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGenerator : MonoBehaviour
{
    [SerializeField] List<CharacterFlame> characterFlames;

    private void Start() {
        for (int i = 0; i < characterFlames.Count; i++)
        {
            characterFlames[i].OnTouch += InstantiateCharacter;
        }
    }



    private void InstantiateCharacter(Character prefab)
    {
        Instantiate(prefab,transform);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    [SerializeField] CharacterBase unit000;
    [SerializeField] CharacterBase unit001;
    [SerializeField] CharacterBase unit002;

    public void MakeUnit000()
    {
        Character enemyCharacter = Instantiate(unit000.Prefab,transform);
        enemyCharacter.gameObject.tag = "Enemy";
        enemyCharacter.isPlayer = false;
    }
    public void MakeUnit001()
    {
        Character enemyCharacter = Instantiate(unit001.Prefab,transform);
        enemyCharacter.gameObject.tag = "Enemy";
        enemyCharacter.isPlayer = false;
    }
    public void MakeUnit002()
    {
        Character enemyCharacter = Instantiate(unit002.Prefab,transform);
        enemyCharacter.gameObject.tag = "Enemy";
        enemyCharacter.isPlayer = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    [SerializeField] CharacterBase unit000;
    [SerializeField] CharacterBase unit001;
    [SerializeField] CharacterBase unit002;
    [SerializeField] CharacterBase unit003;

    public void MakeUnit000()
    {
        MakeUnit(unit000);
    }

    public void MakeUnit001()
    {
        MakeUnit(unit001);
    }
    public void MakeUnit002()
    {
        MakeUnit(unit002);
    }
    public void MakeUnit003()
    {
        MakeUnit(unit003);
    }

    private void MakeUnit(CharacterBase unit)
    {
        Character enemyCharacter = Instantiate(unit.Prefab, transform);
        enemyCharacter.SetHpBar();
        enemyCharacter.gameObject.tag = "Enemy";
        enemyCharacter.gameObject.layer = LayerMask.NameToLayer("Enemy");
        enemyCharacter.isPlayer = false;
    }
}

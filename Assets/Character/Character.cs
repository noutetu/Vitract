using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterAnimator))]
public class Character : MonoBehaviour
{
    [SerializeField] CharacterBase characterBase; 

    private new string name;
    private float maxHp;
    private float atk;
    private float speed;
    private float range;
    private CharacterType characterType;

    private void Awake()
    {
        // CharacterBase のデータを Character クラスのフィールドにコピー
        if (characterBase != null)
        {
            name = characterBase.Name;
            maxHp = characterBase.MaxHp;
            atk = characterBase.Atk;
            speed = characterBase.Speed;
            range = characterBase.Range;
            characterType = characterBase.CharacterType;
        }
        else
        {
            Debug.LogError("CharacterBase is not assigned.");
        }
    }

    // 他のメソッドで CharacterBase のデータを使用することができます
}

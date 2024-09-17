using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
[System.Serializable]
public class CharacterBase : ScriptableObject
{
    //名前
    [SerializeField] new string name;
    public string Name { get => name;}

    //体力
    [SerializeField] float maxHp;
    public float MaxHp { get => maxHp;}

    //攻撃力
    [SerializeField] float atk;
    public float Atk { get => atk;}

    //スピード
    [SerializeField] float speed;
    public float Speed { get => speed;}

    //射程
    [SerializeField] float range;
    public float Range { get => range;}

    //キャラクタータイプ
    [SerializeField] CharacterType characterType;
    public CharacterType CharacterType { get => characterType;}

    //キャラクター画像
    [SerializeField] Sprite sprite;
    public Sprite Sprite { get => sprite;}

    //ソードマンなら射程を0に
    private void OnEnable() 
    {
        if(characterType == CharacterType.SwordMan)
        {
            range = 0;
        }
    }
}

public enum CharacterType
{
    SwordMan,
    BowMan,
    Magician,
}

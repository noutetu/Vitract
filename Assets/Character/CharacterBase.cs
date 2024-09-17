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
    //キャラクタータイプ
    [SerializeField] CharacterType characterType;
    public CharacterType CharacterType { get => characterType;}
}

public enum CharacterType
{
    SwordMan,
    BowMan,
    Magician,
}

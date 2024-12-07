using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
[System.Serializable]
public class CharacterBase : ScriptableObject
{
    [SerializeField] int characterID; 

    // 対応するプレハブ
    [SerializeField] Character prefab;
    public Character Prefab { get => prefab;}

    //名前
    [SerializeField] new string name;
    public string Name { get => name;}

    //コスト
    [SerializeField] int cost;
    public int Cost { get => cost;}


    //最大体力
    [SerializeField] float maxHp;
    public float MaxHp { get => maxHp;}

    //防御力
    [SerializeField] float defence;
    public float Defence { get => defence;}

    //魔法耐性
    [SerializeField] float magicDefence;
    public float MagicDefence { get => magicDefence;}

    //ブロック数
    [SerializeField] int canBlockCount;
    public int CanBlockCount { get => canBlockCount;}

    //攻撃力
    [SerializeField] float atk;
    public float Atk { get => atk;}

    //攻撃アニメーション速度
    [SerializeField] float attackSpeed;
    public float AttackSpeed { get => attackSpeed;}
    
    //攻撃のクールタイム
    [SerializeField] float attackCoolTime;
    public float AttackCoolTime { get => attackCoolTime;}

    //スピード
    [SerializeField] float speed;
    public float Speed { get => speed;}

    //射程
    [SerializeField] float range;
    public float Range { get => range;}

    // スキル
    [SerializeField] SkillData specialSkill;
    public SkillData SpecialSkill { get => specialSkill;}

    [SerializeField] SkillData normalSkill;
    public SkillData NormalSkill { get => normalSkill;}

    //キャラクタータイプ
    [SerializeField] CharacterType characterType;
    public CharacterType CharacterType { get => characterType;}

    //キャラクター画像
    [SerializeField] Sprite sprite;
    public Sprite Sprite { get => sprite;}

    // サウンド
    [SerializeField] AudioClip normalAttackSound;
    public AudioClip NormalAttackSound { get => normalAttackSound;}
    [SerializeField] AudioClip specialAttackSound;
    public AudioClip SpecialAttackSound { get => specialAttackSound;}

    ///---------------------------------------------メソッド----------------------------------------------------------------------
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

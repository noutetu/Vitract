using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterAnimator))]
[RequireComponent(typeof(CharaMover))]

public class Character : MonoBehaviour
{
    [SerializeField] private CharacterBase characterBase;
    private CharacterAnimator anim;
    private CharaMover charaMover;

    private new string name;
    private float maxHp;
    private float atk;
    private float speed;
    private float range;
    private int cost;
    private CharacterType characterType;
    
    private CharacterState characterState;
    public CharacterState CharacterState { get => characterState; set => characterState = value; }

    private void Awake()
    {
        anim = GetComponent<CharacterAnimator>();
        charaMover = GetComponent<CharaMover>();
    }

    private void Update()
    {
        HundleCharacter();
    }

    private void HundleCharacter()
    {
        // 状態による処理の切り替えを switch 文で行う
        switch (CharacterState)
        {
            case CharacterState.Run:
                HandleRunState();
                break;
            case CharacterState.Attack:
                HandleAttackState();
                break;
            case CharacterState.SkillAttack:
                HandleSkillAttackState();
                break;
            case CharacterState.Die:
                HandleDieState();
                break;
            case CharacterState.Debuff:
                HandleDebuffState();
                break;
        }
    }

    private void HandleRunState()
    {
        anim.RunAnim(speed / 2);
        charaMover.Move(speed);
    }

    private void HandleAttackState()
    {
        anim.NormalAttackAnim();
    }

    private void HandleSkillAttackState()
    {
        anim.SkillAttackAnim();
    }

    private void HandleDieState()
    {
        anim.DeadAnim();
    }

    private void HandleDebuffState()
    {
        anim.DebuffAnim();
    }

    private void OnEnable()
    {
        InitCharacter();
    }

    private void InitCharacter()
    {
        if (characterBase != null)
        {
            // CharacterBase のデータをフィールドにコピー
            name = characterBase.Name;
            maxHp = characterBase.MaxHp;
            atk = characterBase.Atk;
            speed = characterBase.Speed;
            range = characterBase.Range;
            cost = characterBase.Cost;
            characterType = characterBase.CharacterType;
        }
        else
        {
            Debug.LogError("CharacterBase is not assigned.");
        }
    }

    public void DisplayLogCharacterInfo()
    {
        Debug.Log($"Name: {name}");
        Debug.Log($"Max HP: {maxHp}");
        Debug.Log($"Attack: {atk}");
        Debug.Log($"Speed: {speed}");
        Debug.Log($"Range: {range}");
        Debug.Log($"Cost: {cost}");
        Debug.Log($"Character Type: {characterType}");
    }
}

public enum CharacterState
{
    Run,
    Attack,
    SkillAttack,
    Die,
    Debuff,
}

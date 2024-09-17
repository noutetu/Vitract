using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    private float attackSpeed;
    private float speed;
    private float range;
    private int cost;
    private CharacterType characterType;

    private CharacterState characterState;
    public CharacterState CharacterState { get => characterState; set => characterState = value; }

    private void OnEnable()
    {
        InitCharacter();
    }

    private void Awake()
    {
        anim = GetComponent<CharacterAnimator>();
        charaMover = GetComponent<CharaMover>();
    }

    private void Update()
    {
        HundleCharacter();
    }

    private void OnCollisionStay2D(Collision2D other)
    {
       if(other.gameObject.tag != "Ground")
       {
            characterState = CharacterState.Attack;
       }
    }
    private void OnCollisionExit2D(Collision2D other)
    {
        characterState = CharacterState.Run;
    }

    private void HundleCharacter()
    {
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

    private IEnumerator HandleAttackWithDelay()
    {
        characterState = CharacterState.Wait; // 攻撃後は待機状態にする
        HandleAttackState(); // 攻撃アニメーションを実行
        yield return new WaitForSeconds(attackSpeed); // 指定された時間待機
    }
    private void HandleAttackState()
    {
        anim.NormalAttackAnim(attackSpeed);
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

    private void InitCharacter()
    {
        if (characterBase != null)
        {
            // CharacterBase のデータをフィールドにコピー
            name = characterBase.Name;
            maxHp = characterBase.MaxHp;
            atk = characterBase.Atk;
            attackSpeed = characterBase.AttackSpeed;
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
    Wait,
}

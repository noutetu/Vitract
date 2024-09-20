using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharacterAnimator))]
[RequireComponent(typeof(CharaMover))]

public class Character : MonoBehaviour
{
    [SerializeField] private CharacterBase characterBase;
    public bool isPlayer;
    private CharacterAnimator anim;
    private CharaMover charaMover;

    private new string name;
    private int cost;
    private float maxHp;
    private float currentHp;
    private float deffence;
    private float magicDeffence;
    private int canBlockCount;
    private float atk;
    private float attackSpeed;
    private float attackCoolTime;
    private float speed;
    private float range;
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

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag != "Ground" && other.gameObject.tag != gameObject.tag)
        {
            StartCoroutine(HandleAttackState());
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
        }
    }

    private void HandleRunState()
    {
        anim.RunAnim(speed / 2);
        charaMover.Move(speed, isPlayer);
    }

    private IEnumerator HandleAttackState()
    {
        //クールタイム待機
        yield return new WaitForSeconds(attackCoolTime);
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
            currentHp = characterBase.CurrentHp;
            deffence = characterBase.Defence;
            magicDeffence = characterBase.MagicDefence;
            canBlockCount = characterBase.CanBlockCount;
            atk = characterBase.Atk;
            attackSpeed = characterBase.AttackSpeed;
            attackCoolTime = characterBase.AttackCoolTime;
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

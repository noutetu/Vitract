using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharaMover))]

public class Character : MonoBehaviour
{
    //攻撃を与える対象
    Character enemyCharacter;

    [SerializeField] private HPBar hpBar;
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
        anim = GetComponentInChildren<CharacterAnimator>();
        anim.OnAttack += HitAttack;
        charaMover = GetComponent<CharaMover>();
    }

    private void Start()
    {
        hpBar.SetHP((float)currentHp / maxHp);
    }

    private void Update()
    {
        HundleCharacter();
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

    private void HandleIdleState()
    {
        anim.IdleAnim();
    }

    private void HandleRunState()
    {
        anim.RunAnim(speed / 2);
        charaMover.Move(speed, isPlayer);
    }

//ーーーーーーーーーーーー----------------攻撃に関する処理-----------------------------------
    private void OnCollisionEnter2D(Collision2D other)
    {
        AttackEvent(other);
    }

    private void AttackEvent(Collision2D other)
    {
        enemyCharacter = other.gameObject.GetComponent<Character>();

        if (isPlayer)
        {
            if (other.gameObject.tag == "Enemy" || other.gameObject.tag == "EnemyCastle")
            {
                StartCoroutine(HandleAttackState());
            }
        }
        else
        {
            if (other.gameObject.tag == "Player" || other.gameObject.tag == "PlayerCastle")
            {
                StartCoroutine(HandleAttackState());
            }
        }
    }

    private IEnumerator HandleAttackState()
    {
        //クールタイム待機
        yield return new WaitForSeconds(0);
        anim.NormalAttackAnim(attackSpeed);
    }

    private void HandleSkillAttackState()
    {
        anim.SkillAttackAnim();
    }
    //unityアニメーションイベントに設定
    public void HitAttack()
    {
        enemyCharacter.TakeDamage(atk);
    }

    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        if (currentHp <= 0) { currentHp = 0; }
        hpBar.UpdateHP((float)currentHp / maxHp);
        //StartCoroutine(hpBar.SetHPSmooth((float)currentHp / maxHp));
    }
//------------------------------------------------------------------------------------------

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
            currentHp = maxHp;
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

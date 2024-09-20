using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharaMover))]

public class Character : MonoBehaviour
{
    //攻撃を与える対象
    public Character enemyCharacter;

    [SerializeField] private HPBar hpBar;
    [SerializeField] private CharacterBase characterBase;
    public bool isPlayer;
    private CharacterAnimator anim;
    private CharaMover charaMover;
    private CharacterState characterState;
    public CharacterState CharacterState { get => characterState; set => characterState = value; }
    //------------------------------キャラクターのステータス----------------------------------------------
    private new string name;                //名前
    private int cost;                       //コスト
    private float maxHp;                    //最大体力
    private float currentHp;                //現在体力
    private float deffence;                 //防御力
    private float magicDeffence;            //魔法耐性
    private int canBlockCount;              //最大ブロック数
    private int currentBlockCount;          //現在のブロック数
    private float atk;                      //攻撃力
    private float attackSpeed;              //攻撃速度
    private float attackCoolTime;           //攻撃クールタイム
    private float speed;                    //スピード
    private float range;                    //攻撃範囲
    private CharacterType characterType;    //タイプ
    public bool isDead;
    //-------------------------------------------------------------------------------------------------


    private void OnEnable()
    {
        InitCharacter();
    }
    private void OnDisable()
    {
        anim.OnAttack -= HitAttack;
        anim.OnDead -= Dead;
    }


    private void Awake()
    {
        anim = GetComponentInChildren<CharacterAnimator>();
        anim.OnAttack += HitAttack;
        anim.OnDead += Dead;
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
            case CharacterState.Die:
                HandleDieState();
                break;
        }
    }
    //アイドル状態
    private void HandleIdleState()
    {
        anim.IdleAnim();
    }
    //走る状態
    private void HandleRunState()
    {
        anim.RunAnim(speed / 2);
        charaMover.Move(speed, isPlayer);
    }
    //死んだ状態
    private void HandleDieState()
    {
        anim.DeadAnim();
    }
    //デバフ状態
    private void HandleDebuffState()
    {
        anim.DebuffAnim();
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
        characterState = CharacterState.Attack;
        //クールタイム待機
        yield return new WaitForSeconds(0);
        anim.NormalAttackAnim(attackSpeed);
    }

    private void HandleSkillAttackState()
    {
        anim.SkillAttackAnim();
    }

    public bool TakeDamage(float damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0);  // currentHpが0を下回らないように
        hpBar.UpdateHP(currentHp / maxHp);

        if (currentHp <= 0)
        {
            Debug.Log("HPが0になった");
            characterState = CharacterState.Die;
            return true;  // キャラクターが死亡したことを示す
        }

        return false;  // キャラクターがまだ生きていることを示す
    }

    //------------------------------------------------------------------------------------------
    //--------------------------unityアニメーションイベントに設定------------------------------------
    public void HitAttack()
    {
        bool isDead = enemyCharacter.TakeDamage(atk);
        if (isDead)
        {
            enemyCharacter = null;
        }
    }
    public void Dead()
    {
        Destroy(gameObject);
    }
    //-------------------------------------------------------------------------------------------
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

using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharaMover))]
public class Character : MonoBehaviour
{
    // ------------- キャラクターのステータス ------------------
    
    private Character enemyCharacter; // 攻撃対象
    private bool isFirstAttack;
    [SerializeField] private HPBar hpBar;
    [SerializeField] private CharacterBase characterBase;

    public bool isPlayer; // プレイヤーかどうか
    private bool isDead;   // 死亡フラグ

    private CharacterAnimator anim;
    private CharaMover charaMover;
    private CharacterState characterState;

    // ------------- キャラクターのステータス ------------------
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

    // ------------- プロパティ ------------------
    public CharacterState CharacterState
    {
        get => characterState;
        set => characterState = value;
    }

    // ------------- Unity ライフサイクル ------------------
    private void Awake()
    {
        anim = GetComponentInChildren<CharacterAnimator>();
        charaMover = GetComponent<CharaMover>();

        anim.OnAttack += HitAttack;
        anim.OnDead += Dead;
    }

    private void OnEnable()
    {
        InitCharacter();
    }

    private void OnDisable()
    {
        anim.OnAttack -= HitAttack;
        anim.OnDead -= Dead;
    }

    private void Start()
    {
        hpBar.SetHP(currentHp / maxHp);
    }

    private void FixedUpdate()
    {
        HandleCharacterState();
    }

    // ------------- キャラクターの状態管理 ------------------
    private void HandleCharacterState()
    {
        switch (CharacterState)
        {
            case CharacterState.Run:
                MoveCharacter();
                break;
            case CharacterState.Die:
                HandleDieState();
                break;
        }
    }

    private void MoveCharacter()
    {
        anim.RunAnim(speed / 2);
        charaMover.Move(speed, isPlayer);
    }

    private void HandleDieState()
    {
        anim.DeadAnim();
    }

    // ------------- 衝突イベント ------------------
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground") || IsOwnBase(other.gameObject.tag)) return;

        enemyCharacter = other.gameObject.GetComponent<Character>();
        if (enemyCharacter != null)
        {
            AttackEvent();
        }
    }

    private bool IsOwnBase(string tag)
    {
        return (isPlayer && tag == "PlayerCastle") || (!isPlayer && tag == "EnemyCastle");
    }

    // ------------- 攻撃処理 ------------------
    private void AttackEvent()
    {
        if (!isDead)
        {
            StartCoroutine(HandleAttackState());
        }
    }

    private IEnumerator HandleAttackState()
    {
        if(!isFirstAttack)
        {
            yield return new WaitForSeconds(attackCoolTime);
        }
        if(isDead){yield break;}
        CharacterState = CharacterState.Attack;
        anim.NormalAttackAnim(attackSpeed);
    }

    public bool TakeDamageAndCheckDead(float damage)
    {
        currentHp = Mathf.Max(currentHp - damage, 0);
        hpBar.UpdateHP(currentHp / maxHp);

        if (currentHp <= 0)
        {
            isDead = true;
            characterState = CharacterState.Die;
            return true;
        }
        return false;
    }
    //------------ unity animation event -------------
    public void HitAttack()
    {
        if (isDead) return; // キャラクターが死んでいるなら処理を終了

        bool enemyIsDead = enemyCharacter.TakeDamageAndCheckDead(atk);

        if (enemyIsDead)
        {
            enemyCharacter = null;
            isFirstAttack = true;
            characterState = CharacterState.Run;
        }
        else
        {
            isFirstAttack = false;
            AttackEvent();
        }
    }
    public void Dead()
    {
        Destroy(gameObject);
    }
    // ------------- キャラクターの初期化 ------------------
    private void InitCharacter()
    {
        if (characterBase == null)
        {
            Debug.LogError("CharacterBase is not assigned.");
            return;
        }

        name = characterBase.Name;
        maxHp = characterBase.MaxHp;
        currentHp = maxHp;
        atk = characterBase.Atk;
        attackSpeed = characterBase.AttackSpeed;
        attackCoolTime = characterBase.AttackCoolTime;
        speed = characterBase.Speed;
        range = characterBase.Range;
        deffence = characterBase.Defence;
        magicDeffence = characterBase.MagicDefence;
        canBlockCount = characterBase.CanBlockCount;
        cost = characterBase.Cost;
        characterType = characterBase.CharacterType;
        isFirstAttack = true;
    }

    // ------------- ログ出力 ------------------
    public void DisplayLogCharacterInfo()
    {
        Debug.Log($"Name: {name}, Max HP: {maxHp}, Attack: {atk}, Speed: {speed}, Range: {range}, Cost: {cost}, Type: {characterType}");
    }
}

public enum CharacterState
{
    Run,
    Attack,
    SkillAttack,
    Die,
    Debuff,
    Wait
}

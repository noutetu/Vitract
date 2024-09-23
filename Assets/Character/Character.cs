using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharaMover))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Character : MonoBehaviour, IDamageable
{
    // ------------- キャラクターのステータス ------------------

    private Character enemyCharacter; // 現在攻撃対象のキャラクター
    private List<Character> enemies;  // 攻撃対象の敵リスト
    private Castle enemyCastle;       // 攻撃対象の城
    private bool isFirstAttack;       // 最初の攻撃フラグ

    [SerializeField] private HPBar hpBar;               // HPバーの参照
    [SerializeField] private CharacterBase characterBase; // キャラクターのベースデータ

    public bool isPlayer;             // プレイヤーかどうか
    private bool isDead;              // 死亡フラグ

    private CharacterAnimator anim;   // アニメーション制御
    private CharaMover charaMover;    // 移動制御
    private CharacterState characterState; // キャラクターの現在の状態

    // ------------- キャラクターのステータス ------------------
    private string name;               // キャラクターの名前
    private int cost;                  // コスト
    private float maxHp;               // 最大体力
    private float currentHp;           // 現在の体力
    private float deffence;            //防御力
    private float magicDeffence;       //魔法防御力
    private float atk;                 // 攻撃力
    private float attackSpeed;         // 攻撃速度
    private float attackCoolTime;      // 攻撃クールタイム
    private float speed;               // 移動速度
    private float range;               // 射程
    private CharacterType characterType;  // キャラクターのタイプ

    // ------------- プロパティ ------------------
    public CharacterState CharacterState
    {
        get => characterState;
        set => characterState = value;
    }

    // ------------- Unity ライフサイクル ------------------

    private void Awake()
    {
        // コンポーネントの取得
        anim = GetComponentInChildren<CharacterAnimator>();
        charaMover = GetComponent<CharaMover>();

        // アニメーションイベントの登録
        anim.OnAttack += HitAttack;
        anim.OnDead += Dead;
    }

    private void OnEnable()
    {
        // キャラクターの初期化
        InitCharacter(); 
        // HPバーの初期化
        hpBar.SetHP(currentHp / maxHp);
    }

    private void OnDisable()
    {
        // イベントの登録解除
        anim.OnAttack -= HitAttack;
        anim.OnDead -= Dead;
    }

    private void OnDestroy()
    {
        //DOTweenの破棄
        DOTween.Kill(this);
    }

    private void Start()
    {
       
    }

    private void FixedUpdate()
    {
        // キャラクターの状態に応じて処理を実行
        HandleCharacterState();
    }

    // ------------- キャラクターの状態管理 ------------------

    private void HandleCharacterState()
    {
        switch (CharacterState)
        {
            case CharacterState.Run:
                HandleRunState();
                break;
            case CharacterState.Die:
                HandleDieState();
                break;
            case CharacterState.Idle:
                HandleIdleState();
                break;
        }
    }

    // 走行状態の処理
    private void HandleRunState()
    {
        anim.RunAnim(speed / 2); // 走行アニメーションの再生
        charaMover.Move(speed, isPlayer);  // キャラクターを移動させる
    }

    // 死亡状態の処理
    private void HandleDieState()
    {
        charaMover.Stop();      // 移動を停止
        anim.DeadAnim();        // 死亡アニメーションの再生
    }

    // 待機状態の処理
    private void HandleIdleState()
    {
        anim.IdleAnim();        // 待機アニメーションの再生
    }

    // ------------- 衝突イベント処理 ------------------

    private void OnCollisionEnter2D(Collision2D other)
    {
        // 地面や自分の拠点との衝突は無視
        if (other.gameObject.CompareTag("Ground") || IsOwnBase(other.gameObject.tag)) return;

        // 敵キャラクターとの衝突処理
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Enemy"))
        {
            Character collidedCharacter = other.gameObject.GetComponent<Character>();

            // まだリストにない敵キャラクターを登録
            if (collidedCharacter != null && !enemies.Contains(collidedCharacter))
            {
                enemies.Add(collidedCharacter);
            }

            // 最初の敵キャラクターを攻撃対象とする
            enemyCharacter = enemies[0];
            if (enemyCharacter != null)
            {
                AttackEvent();  // 攻撃イベントの開始
            }
        }

        // 敵の城との衝突処理
        else if (other.gameObject.CompareTag("PlayerCastle") || other.gameObject.CompareTag("EnemyCastle"))
        {
            enemyCastle = other.gameObject.GetComponent<Castle>();

            // 城に攻撃を行う
            if (enemyCastle != null)
            {
                AttackEvent();  // 攻撃イベントの開始
            }
        }
    }

    // 自分の拠点かどうかを確認
    private bool IsOwnBase(string tag)
    {
        return (isPlayer && (tag == "PlayerCastle" || tag == "Player")) ||
               (!isPlayer && (tag == "EnemyCastle" || tag == "Enemy"));
    }

    // ------------- 攻撃処理 ------------------

    private void AttackEvent()
    {
        if (!isDead)
        {
            StartCoroutine(HandleAttackState());  // 攻撃状態の開始
        }
    }

    private IEnumerator HandleAttackState()
    {
        // 最初の攻撃でなければ、クールタイムを待機
        if (!isFirstAttack)
        {
            CharacterState = CharacterState.Idle;
            yield return new WaitForSeconds(attackCoolTime);
        }

        if (isDead) yield break;  // 死亡していれば攻撃を中断

        CharacterState = CharacterState.Attack;  // 攻撃状態に遷移
        anim.NormalAttackAnim(attackSpeed);      // 攻撃アニメーションの再生
    }

    // 次の敵キャラクターを選択
    private Character SelectNextEnemy()
    {
        enemies.RemoveAt(0);  // リストの先頭を削除し、次の敵を取得
        return enemies.Count > 0 ? enemies[0] : null;
    }

    // ダメージ処理と死亡判定
    private bool HandleDamageAndCheckDead(IDamageable target)
    {
        bool targetIsDead = target.TakeDamageAndCheckDead(atk);  // ダメージを与える
        isFirstAttack = targetIsDead;  // ターゲットが死んだ場合、最初の攻撃をリセット
        return targetIsDead;
    }

    // ダメージを受けた時の処理
    public bool TakeDamageAndCheckDead(float damage)
    {
        currentHp = Mathf.Max(currentHp - damage, 0);  // HPを更新
        hpBar.UpdateHP(currentHp / maxHp);  // HPバーを更新

        if (currentHp <= 0)
        {
            isDead = true;
            CharacterState = CharacterState.Die;  // 死亡状態に遷移
            return true;
        }
        return false;
    }

    // ------------- アニメーションイベント ------------------

    public void HitAttack()
    {
        // 敵キャラクターへの攻撃
        if (enemyCharacter != null && HandleDamageAndCheckDead(enemyCharacter))
        {
            // 次の敵を選択し、再度攻撃
            enemyCharacter = SelectNextEnemy();
            if (enemyCharacter != null)
            {
                AttackEvent();
            }
            else if (!isDead)
            {
                CharacterState = CharacterState.Run;  // 敵がいない場合は走行状態に戻る
            }
        }
        // 敵の城への攻撃
        else if (enemyCastle != null)
        {
            if (HandleDamageAndCheckDead(enemyCastle))
            {
                enemyCastle = null;  // 城が破壊された場合
                if (!isDead)
                {
                    CharacterState = CharacterState.Idle;  // 城が破壊された後は待機状態に
                }
            }
            else
            {
                AttackEvent();  // 城が破壊されなければ再度攻撃
            }
        }
    }

    // 死亡処理
    public void Dead()
    {
        Destroy(gameObject);  // キャラクターを削除
    }
    //--------------DOTWEEN--------------
    public void SmoothApper()
    {
        // 透明からフェードインさせるために全てのSpriteRendererの透明度を設定
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (var spriteRenderer in spriteRenderers)
        {
            // 最初は完全に透明に設定
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;

            // 透明度を1（完全に表示）まで1秒かけてフェードイン
            spriteRenderer.DOFade(1f, 0.5f);  // 1秒かけてフェードイン
        }
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
        cost = characterBase.Cost;
        characterType = characterBase.CharacterType;
        isFirstAttack = true;

        enemies = new List<Character>();
    }

    // ------------- ログ出力 ------------------
    public void DisplayLogCharacterInfo()
    {
        Debug.Log
        ($"Name: {name}, Max HP: {maxHp}, Attack: {atk}, Speed: {speed}, Range: {range}, Cost: {cost}, Type: {characterType}");
    }
}

public interface IDamageable
{
    bool TakeDamageAndCheckDead(float damage);
}

public enum CharacterState
{
    Run,
    Attack,
    SkillAttack,
    Die,
    Debuff,
    Idle,
}
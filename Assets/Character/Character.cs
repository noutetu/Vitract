using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharaMover))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public abstract class Character : MonoBehaviour, IDamageable
{
    // TODO Magicianクラスが敵を倒した時の挙動がおかしい 2回リストに登録されてる？
    // ------------- キャラクターのステータス ------------------

    protected IDamageable enemyObject; // 現在攻撃対象のキャラクター
    protected List<IDamageable> enemies;  // 攻撃対象の敵リスト

    [SerializeField] private HPBar hpBar;               // HPバーの参照
    [SerializeField] private CharacterBase characterBase; // キャラクターのベースデータ

    bool canAttack;
    public bool isPlayer;             // プレイヤーかどうか
    private bool isDead;              // 死亡フラグ
    public bool IsDead { get => isDead; }

    private CharacterAnimator anim;   // アニメーション制御
    private CharaMover charaMover;    // 移動制御
    protected CharacterState characterState; // キャラクターの現在の状態

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
    protected float range;               // 射程
    private CharacterType characterType;  // キャラクターのタイプ



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

    protected virtual void Start()
    {

    }

    protected virtual void FixedUpdate()
    {
        // 攻撃対象がすでに死んでいるかを確認
        UpdateEnemies();
        // 自分が死んでいないか確認
        if (isDead) { characterState = CharacterState.Die; }
        // 城が破壊されていたらアイドル状態に
        if (GameManager.Instance.isGameEnd)
        {
            characterState = CharacterState.Idle;
            canAttack = false;
        }

        HandleState();
        if (enemyObject != null && canAttack)
        {
            AttackEvent();
        }
    }

    // ------------- キャラクターの状態管理 ------------------

    private void HandleState()
    {
        switch (characterState)
        {
            case CharacterState.Run:
                HandleRunningState();
                break;
            case CharacterState.Die:
                HandleDyingState();
                break;
            case CharacterState.Idle:
                HandleIdllingState();
                break;
        }
    }

    // 走行状態の処理
    private void HandleRunningState()
    {
        anim.RunAnim(speed / 2); // 走行アニメーションの再生
        charaMover.Move(speed * 2, isPlayer);  // キャラクターを移動させる
    }

    // 死亡状態の処理
    private void HandleDyingState()
    {
        charaMover.Stop();      // 移動を停止
        anim.DeadAnim();        // 死亡アニメーションの再生
    }

    // 待機状態の処理
    private void HandleIdllingState()
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
            IDamageable collidedCharacter = other.gameObject.GetComponent<IDamageable>();

            // まだリストにない敵キャラクターを登録
            if (collidedCharacter != null)
            {
                RegisterAtEnemies(collidedCharacter);
            }

            // 最初の敵キャラクターを攻撃対象とする
            SetNextEnemy();
            //敵キャラクターがいて、現在攻撃中でなければ
            if (enemyObject != null && canAttack)
            {
                AttackEvent();  // 攻撃イベントの開始
            }
        }
    }

    // 味方かどうかを確認
    private bool IsOwnBase(string tag)
    {
        return (isPlayer && (tag == "PlayerCastle" || tag == "Player")) ||
               (!isPlayer && (tag == "EnemyCastle" || tag == "Enemy"));
    }

    // ------------- 攻撃処理 ------------------

    private void AttackEvent()
    {
        canAttack = false;
        if (!IsDead)
        {
            if (enemyObject != null)
            {
                HandleAttackState();
            }
        }
    }

    private void HandleAttackState()
    {
        if (IsDead) return;
        if (enemyObject == null) { return; }
        characterState = CharacterState.Attack;
        anim.NormalAttackAnim(attackSpeed);
    }
    // ------------- ダメージ処理と死亡判定 --------------------
    private bool HandleDamageAndCheckDead(IDamageable target)
    {
        if (target == null) return true;
        bool targetIsDead = target.TakeDamageAndCheckDead(atk);  // ダメージを与える
        return targetIsDead;
    }

    public bool TakeDamageAndCheckDead(float damage)
    {
        if (isDead) return true;  // 既に死亡している場合はすぐに終了

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

    // ------------- 攻撃関連処理 -------------

    // 次の敵を探し、必要であれば攻撃再開か走行状態に遷移
    private void HandleNextEnemyOrRun()
    {
        RemoveInEnemies(enemyObject);
        // 次の敵キャラクターを選択する
        SetNextEnemy();

        // 敵キャラクターが存在する場合は攻撃を再開
        if (enemyObject != null)
        {
            ScheduleNextAttack();
        }
        else
        {
            canAttack = true;
            characterState = CharacterState.Run;  // 敵がいない場合は走行状態に戻る
        }
    }


    // クールタイムを待って次の攻撃を実行
    private void ScheduleNextAttack()
    {
        //待機モーション
        HandleIdllingState();
        DOVirtual.DelayedCall(attackCoolTime / GameManager.Instance.gameSpeed, () =>
        {
            canAttack = true;
            if (!IsDead && anim != null)
            {
                AttackEvent();
            }  
        });
    }



    // ------------- リスト関連 ------------------
    protected void RemoveInEnemies(IDamageable enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
        }
    }
    protected void RegisterAtEnemies(IDamageable targetEnemy)
{
    // ターゲットがすでに死んでいたらreturn
    Character character = targetEnemy as Character;
    if (character != null)
    {
        if(character.isDead) { return;}
    }

    // enemiesリストにまだ登録されていない場合のみ登録する
    if (!enemies.Contains(targetEnemy))
    {
        enemies.Add(targetEnemy);
    }
}

    protected void SetNextEnemy()
    {
        enemyObject = enemies.Count > 0 ? enemies[0] : null;
    }
    private void UpdateEnemies()
    {
        // 破壊されたオブジェクトをリストから削除して
        enemies.RemoveAll(enemy => enemy == null);
        if (enemies.Count > 0)
        {
            // まだ敵がいるなら攻撃対象に設定
            SetNextEnemy();
            return;
        }
        characterState = CharacterState.Run;

    }
    // ------------- アニメーションイベント ------------------

    private void HitAttack()
    {
        // 敵キャラクターへの攻撃
        if (enemyObject != null)
        {
            // 敵が死んだ場合
            if (HandleDamageAndCheckDead(enemyObject))
            {
                HandleNextEnemyOrRun();
                return;
            }

            // 敵がまだ生きている場合
            ScheduleNextAttack();
        }
    }

    // 死亡処理
    public void Dead()
    {
        Destroy(gameObject);  // キャラクターを削除
    }
    //--------------DOTWEEN--------------
    public void SmoothAppear()
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
        canAttack = true;
        enemies = new List<IDamageable>();
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

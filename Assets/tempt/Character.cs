using System;
using DG.Tweening;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using Vitract.Character.Effects;

[RequireComponent(typeof(TargetList))]
[RequireComponent(typeof(CharacterMotionFacade))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public abstract class Character : MonoBehaviour, IDamageable
{
    // TODO　UniRxのリファクタリング
    // ------------- キャラクターのステータス ------------------

    protected IDamageable enemyObject; // 現在攻撃対象のキャラクター

    [SerializeField] private HPBar hpBar;               // HPバーの参照
    [SerializeField] private CharacterBase characterBase; // キャラクターのベースデータ

    [SerializeField] protected bool canAttack;
    public bool isPlayer;             // プレイヤーかどうか
    private bool isDead;              // 死亡フラグ
    public bool IsDead { get => isDead; }

    // ------------- コンポーネント ------------------
    private CharacterMotionFacade MotionFacade;
    protected CharacterState characterState; // キャラクターの現在の状態
    [SerializeField] protected TargetList targetList;

    // ------------- キャラクターのステータス ------------------

    private string name;               // キャラクターの名前
    private int cost;                  // コスト
    private float maxHp;               // 最大体力
    public ReactiveProperty<float> currentHp { get; set; } = new ReactiveProperty<float>();           // 現在の体力
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
        MotionFacade = GetComponent<CharacterMotionFacade>();
        MotionFacade.Initialize(HitAttack, Dead);
        targetList = GetComponent<TargetList>();
        // 敵リストに変更があった場合に通知を受けるようにする
        targetList.enemies.ObserveCountChanged()
            .Subscribe(count =>
            {
                if (count > 0)
                {
                    if (enemyObject == null)
                    {
                        enemyObject = targetList.SetNextEnemy();
                    }
                }
                else
                {
                    characterState = CharacterState.Run;
                    enemyObject = null;
                }
            })
            .AddTo(this);
    }

    private void OnEnable()
    {
        // キャラクターの初期化
        InitCharacter();
        // HPバーの初期化
        hpBar.SetHP(currentHp.Value / maxHp);
    }

    private void OnDisable()
    {
        // イベントの登録解除
        MotionFacade.DeInitialize(HitAttack, Dead);
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
        // 自分が死んでいないか確認
        if (isDead) { characterState = CharacterState.Die; }
        // 城が破壊されていたらアイドル状態に
        if (GameManager.Instance.isGameEnd)
        {
            characterState = CharacterState.Idle;
            canAttack = false;
        }

        HandleState();
        if (enemyObject != null)
        {
            characterState = CharacterState.Idle;
            if (canAttack)
            {
                AttackEvent();
            }
        }
    }

    // ------------- キャラクターの状態管理 ------------------

    private void HandleState()
    {
        switch (characterState)
        {
            case CharacterState.Run:
                MotionFacade.RunMotion(speed, isPlayer);
                break;
            case CharacterState.Die:
                MotionFacade.DeathMotion();
                break;
            case CharacterState.Idle:
                MotionFacade.IdleMotion();
                break;
        }
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


            targetList.RegisterAtEnemies(collidedCharacter);

            // HPが0以下になったときにリストから削除する購読を追加
            collidedCharacter.currentHp
                .Skip(1) // 初期値をスキップして、変化があった時のみ反応
                .Where(hp => hp <= 0)
                .Subscribe(_ =>
                {
                    enemyObject = null;
                })
                .AddTo(this); // 購読を管理リストに追加


            // 最初の敵キャラクターを攻撃対象とする
            enemyObject = targetList.SetNextEnemy();
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

    protected void AttackEvent()
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
        MotionFacade.NormalAttackMotion(attackSpeed);
    }
    // ------------- ダメージ処理と死亡判定 --------------------
    private void HandleDamage(IDamageable target)
    {
        if (target == null) return;
        target.TakeDamage(atk);  // ダメージを与える
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;  // 既に死亡している場合はすぐに終了

        currentHp.Value = Mathf.Max(currentHp.Value - damage, 0);
        hpBar.UpdateHP(currentHp.Value / maxHp);

        if (currentHp.Value <= 0)
        {
            isDead = true;
            characterState = CharacterState.Die;
            return;
        };
    }

    // ------------- 攻撃関連処理 -------------

    // 次の敵を探し、必要であれば攻撃再開か走行状態に遷移
    private void HandleNextEnemyOrRun()
    {
        // 敵キャラクターが存在する場合は攻撃を再開
        if (enemyObject != null)
        {
            ScheduleNextAttack();
        }
        else
        {
            characterState = CharacterState.Run;  // 敵がいない場合は走行状態に戻る
            // クールタイム後再度攻撃
            Observable.Timer(TimeSpan.FromSeconds(attackCoolTime / GameManager.Instance.gameSpeed))
                .Subscribe(_ =>
                {
                    canAttack = true;
                })
                .AddTo(this); // thisはMonoBehaviourを指し、購読のライフタイムを管理
        }
    }


    // クールタイムを待って次の攻撃を実行
    private void ScheduleNextAttack()
    {
        //待機モーション
        MotionFacade.IdleMotion();
        // クールタイム後再度攻撃
        Observable.Timer(TimeSpan.FromSeconds(attackCoolTime / GameManager.Instance.gameSpeed))
            .Subscribe(_ =>
            {
                canAttack = true;
                if (!IsDead)
                {
                    AttackEvent();
                }
            })
            .AddTo(this); // thisはMonoBehaviourを指し、購読のライフタイムを管理
    }

    // ------------- アニメーションイベント ------------------

    private void HitAttack()
    {
        // 敵キャラクターへの攻撃
        if (enemyObject != null)
        {
            // 敵が死んだ場合
            HandleDamage(enemyObject);

            // 敵がまだ生きている場合
            HandleNextEnemyOrRun();
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
        currentHp.Value = maxHp;
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
    ReactiveProperty<float> currentHp { get; set; }
    void TakeDamage(float damage);
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
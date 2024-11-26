using System;
using DG.Tweening;
using UniRx;
using UnityEngine;
using Vitract.Character.Effects;

[RequireComponent(typeof(Skill))]
[RequireComponent(typeof(CharacterMotionFacade))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public abstract class Character : MonoBehaviour, IDamageable
{
    // ------------- キャラクターのステータス ------------------
    // TODO 　攻撃処理をなんとかする
    protected IDamageable enemyObject; // 現在攻撃対象のキャラクター

    [SerializeField] private HPBar hpBar;                 // HPバーの参照
    [SerializeField] private CharacterBase characterBase; // キャラクターのベースデータ
    private Subject<Unit> cooldownSubject
                        = new Subject<Unit>();      // 攻撃クールダウン管理用の Subject
    [SerializeField] protected bool canAttack;
    public bool isPlayer;
    private bool isDead;
    public bool IsDead { get => isDead; }

    protected Skill currentSkill;
    protected Skill normalSkill;
    [SerializeField] protected Skill specialSkill;

    // ------------- コンポーネント ------------------
    private CharacterMotionFacade MotionFacade; //アニメーションと移動管理
    protected CharacterState characterState; // キャラクターの現在の状態
    protected ReactiveCollection<IDamageable> targetList;

    // ------------- キャラクターのステータス ------------------

    private string name;               // キャラクターの名前
    public string Name { get => name; set => name = value; }

    private int cost;                  // コスト
    private float maxHp;               // 最大体力
    public ReactiveProperty<float> currentHp { get; set; } = new ReactiveProperty<float>();
    private float deffence;            //防御力
    private float magicDeffence;       //魔法防御力

    private float atk;                 // 攻撃力
    public float Atk { get => atk; set => atk = value; }

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

        normalSkill = GetComponent<Skill>();
        normalSkill.Initialize(attackCoolTime, (attacker, target) =>
        {
            float damage = attacker.Atk; // Attackerの攻撃力を利用
            if (target != null)
            {
                target.TakeDamage(damage); // Targetにダメージを与える
            }
        });
        specialSkill = 

        currentSkill = normalSkill;

        targetList = new ReactiveCollection<IDamageable>();
        // リストの要素が増えた時の処理
        targetList.ObserveAdd().Subscribe(item =>
        {
            SetNextEnemy();
            SubscribeToEnemyHealth(targetList[targetList.Count - 1]);
        });
        // リストの要素が減った時の処理
        targetList.ObserveRemove().Subscribe(item =>
        {
            //　敵リストに敵がいるならターゲットに設定
            if (targetList.Count > 0)
            {
                if (enemyObject == null)
                {
                    SetNextEnemy();
                }
            }
            //　敵がいないなら走る状態に
            else
            {
                characterState = CharacterState.Run;
                enemyObject = null;
            }
        });
        // 敵がいるかどうかを監視し、いるなら待機して攻撃可能状態を待つ
        Observable.EveryUpdate()
                    .Where(_ => enemyObject != null)
                    .Subscribe(_ =>
                    {
                        // enemyObjectがnullでないときにのみ実行する処理
                        characterState = CharacterState.Idle;
                        if (canAttack)
                        {
                            AttackEvent();
                        }
                    })
                    .AddTo(this);

        // クールダウンが完了したときに攻撃可能にする購読
        cooldownSubject
            .SelectMany(_ => Observable.Timer(TimeSpan.FromSeconds(attackCoolTime / GameManager.Instance.gameSpeed)))
            .Subscribe(_ =>
            {
                canAttack = true;
                Debug.Log("攻撃が再び可能です");
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
        targetList.Dispose();
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
            case CharacterState.Attack:
                MotionFacade.NormalAttackMotion(attackSpeed);
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
            IDamageable enemy = DetectEnemy(other.gameObject);
            if (enemy != null)
            {
                AddEnemyToList(enemy);
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
        if (!IsDead && enemyObject != null)
        {
            canAttack = false;
            characterState = CharacterState.Attack;
            MotionFacade.NormalAttackMotion(attackSpeed);
        }
    }



    // ------------- ダメージ処理と死亡判定 --------------------

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



    // ------------- アニメーションイベント ------------------

    private void HitAttack()
    {
        // クールタイム後再度攻撃
        cooldownSubject.OnNext(Unit.Default);
        // 敵キャラクターへの攻撃
        if (enemyObject != null)
        {
            // 敵が死んだ場合
            currentSkill.Activate(this, enemyObject);
        }
    }

    // 死亡処理
    public void Dead()
    {
        Destroy(gameObject);  // キャラクターを削除
    }
    //--------------敵感知--------------

    protected IDamageable DetectEnemy(GameObject other)
    {
        return other.GetComponent<IDamageable>();
    }

    protected void AddEnemyToList(IDamageable detectedObject)
    {
        // 敵リストに追加
        targetList.Add(detectedObject);
    }

    protected void SubscribeToEnemyHealth(IDamageable enemy)
    {
        // HPが0以下になったときにリストから削除する購読を追加
        enemy.currentHp
            //.Skip(1) // 初期値をスキップして、変化があった時のみ反応
            .Where(hp => hp <= 0)
            .Subscribe(_ =>
            {
                RemoveEnemyAndResetTarget(enemy);
            })
            .AddTo(this) // 購読を管理リストに追加
            .AddTo(enemy as Component);
    }

    protected void RemoveEnemyAndResetTarget(IDamageable enemy)
    {
        enemyObject = null;
        targetList.Remove(enemy);
    }

    protected void SetNextEnemy()
    {
        enemyObject = targetList[0];
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

        Name = characterBase.Name;
        maxHp = characterBase.MaxHp;
        currentHp.Value = maxHp;
        Atk = characterBase.Atk;
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
        ($"Name: {Name}, Max HP: {maxHp}, Attack: {Atk}, Speed: {speed}, Range: {range}, Cost: {cost}, Type: {characterType}");
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
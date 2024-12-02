using System;
using DG.Tweening;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using Vitract.Character.Effects;

[RequireComponent(typeof(Skill))]
[RequireComponent(typeof(CharacterMotionFacade))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public abstract class Character : MonoBehaviour, IDamageable
{
    // ------------- キャラクターのステータス ------------------
    // TODO もう全部unirxにするぞ
    // TODO リスト増えた時と減った時、数が変わった時通知 クリア
    // TODO スキルの攻撃可能が切り替わった時通知　　クリア
    // TODO なんかちゃんとアイドル状態にならない
    // TODO 待機中に歩き続ける　クリア
    // TODO Magicianクラスだけ2連続攻撃する クリア
    // TODO 攻撃とスキル発動をきちんと分ける
    // TODO specialSkillを入れたらplayer側がダメージを受けないバグ


    protected IDamageable enemyObject; // 現在攻撃対象のキャラクター

    private HPBar hpBar;              // HPバーの参照
    public HPBar uiHpBar;
    [SerializeField] private CharacterBase characterBase; // キャラクターのベースデータ
    public bool isPlayer;
    private bool isDead;
    public bool IsDead { get => isDead; }
    // スキル
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
        InitCharacter();
        hpBar = GetComponentInChildren<HPBar>();

        MotionFacade = GetComponent<CharacterMotionFacade>();
        MotionFacade.Initialize(HitAttack, Dead);

        // スキル初期設定------------------------
        normalSkill = GetComponent<Skill>();
        normalSkill.NormalInitialize(attackCoolTime, (attacker, target) =>
        {
            float damage = attacker.Atk;
            if (target != null)
            {
                target.TakeDamage(damage);
            }
        });
        currentSkill = normalSkill;
        if(specialSkill != null)
        {
            specialSkill.Awake();
            currentSkill = specialSkill;
        }



        // 通常攻撃のCanAttackを購読
        normalSkill.CanUseSkill
            .Skip(1)
            .DistinctUntilChanged() // 値が変わった時のみ反応（重複通知を防ぐ）
            .Subscribe(value =>
            {
                if (value)
                {
                    // `CanUseSkill`が`true`になったときの処理
                    if (enemyObject != null)
                    {
                        AttackEvent();
                    }
                    Debug.Log("スキルが使用可能になりました。");
                }
                else
                {
                    // `CanUseSkill`が`false`になったときの処理
                    characterState = CharacterState.Idle;
                    Debug.Log("スキルがクールダウン中です。");
                }
            })
            .AddTo(this);

        // スキル攻撃のCanAttackを購読
        specialSkill.CanUseSkill
            .Skip(1)
            .DistinctUntilChanged() // 値が変わった時のみ反応（重複通知を防ぐ）
            .Subscribe(value =>
            {
                if (value)
                {
                    // `CanUseSkill`が`true`になったときの処理
                    if (enemyObject != null)
                    {
                        AttackEvent();
                    }
                    Debug.Log("スキルが使用可能になりました。");
                }
                else
                {
                    // `CanUseSkill`が`false`になったときの処理
                    characterState = CharacterState.Idle;
                    Debug.Log("スキルがクールダウン中です。");
                }
            })
            .AddTo(this);
        // リスト初期設定----------------------------------
        targetList = new ReactiveCollection<IDamageable>();
        // リストの要素が増えた時の処理
        targetList.ObserveAdd().Subscribe(item =>
        {
            SetNextEnemy();
            SubscribeToEnemyHealth(targetList[targetList.Count - 1]);
            if (enemyObject != null)
            {
                if (currentSkill.CanUseSkill.Value)
                {
                    AttackEvent();
                }
                else
                {
                    characterState = CharacterState.Idle;
                }
            }
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
                    if (currentSkill.CanUseSkill.Value)
                    {
                        AttackEvent();
                    }
                    else
                    {
                        characterState = CharacterState.Idle;
                    }
                }
            }
            //　敵がいないなら走る状態に
            else
            {
                characterState = CharacterState.Run;
                enemyObject = null;
            }
        });
    }


    private void OnEnable()
    {
        // キャラクターの初期化
        InitCharacter();
    }

    public void SetHpBar()
    {
        if (uiHpBar != null)
        {
            uiHpBar.SubscribeValue(currentHp, maxHp);
        }
        hpBar.SubscribeValue(currentHp, maxHp);
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
            characterState = CharacterState.Attack;
            MotionFacade.NormalAttackMotion(attackSpeed);
        }
        //　攻撃した後は必ず待機状態に移行
        // characterState = CharacterState.Idle;
    }



    // ------------- ダメージ処理と死亡判定 --------------------

    public void TakeDamage(float damage)
    {
        if (isDead) return;  // 既に死亡している場合はすぐに終了

        currentHp.Value = Mathf.Max(currentHp.Value - damage, 0);

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
        if (targetList.Contains(detectedObject)) { return; }
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
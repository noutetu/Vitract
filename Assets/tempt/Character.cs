using DG.Tweening;
using UniRx;
using UnityEngine;

[RequireComponent(typeof(CharacterMotionFacade))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public abstract class Character : MonoBehaviour, IDamageable
{
    // TODO 二体以上ブロックした時に最初だけ連続攻撃するバグ

    // ================= フィールド =================
    // キャラクターのステータス
    protected IDamageable enemyObject; // 現在攻撃対象のキャラクター
    public HPBar uiHpBar; // UI上のHPバー
    [SerializeField] private CharacterBase characterBase; // キャラクターのベースデータ
    public bool isPlayer; // プレイヤーかどうかのフラグ
    private bool isDead; // キャラクターが死亡しているかどうか
    public bool IsDead => isDead; // 死亡状態の取得

    // スキル
    protected SkillData currentSkill; // 現在のスキル
    protected SkillData normalSkillInstance; // 通常スキルのインスタンス
    protected SkillData specialSkillInstance; // 特殊スキルのインスタンス

    // コンポーネント
    private HPBar hpBar; // HPバーのコンポーネント
    private CharacterMotionFacade motionFacade; // アニメーションと移動管理のファサード
    protected CompositeDisposable disposables = new CompositeDisposable(); // 購読の破棄管理
    protected ReactiveCollection<IDamageable> targetList; // ターゲットのリスト

    // キャラクターのパラメータ
    public string Name { get; set; } // キャラクター名
    public ReactiveProperty<float> currentHp { get; set; } = new ReactiveProperty<float>(); // 現在のHP
    public float Atk { get; set; } // 攻撃力
    public float AttackSpeed { get; set; } // 攻撃速度
    public float AttackCoolTime { get; set; } // 攻撃クールタイム
    private float maxHp; // 最大HP
    private float speed; // 移動速度
    protected float range; // 射程距離
    private float deffence; // 防御力
    private float magicDeffence; // 魔法防御力
    private int cost; // コスト
    private CharacterType characterType; // キャラクターのタイプ
    protected CharacterState characterState; // キャラクターの現在の状態

    // ================= Unity ライフサイクル =================
    private void Awake()
    {
        InitCharacter(); // キャラクターの初期化
        InitializeComponents(); // コンポーネントの初期化
        InitializeSkills(); // スキルの初期化
        SubscribeToSkillCooldown(); // スキルのクールダウン購読の設定
        InitializeTargetList(); // ターゲットリストの初期化
    }

    private void OnEnable() => InitCharacter(); // 有効化時の初期化

    private void OnDisable()
    {
        motionFacade.DeInitialize(HitAttack, Dead); // アニメーションイベントの解除
        disposables.Clear(); // 購読解除
        targetList.Dispose(); // ターゲットリストの破棄
    }

    private void OnDestroy()
    {
        DOTween.Kill(this); // DOTweenアニメーションの破棄
        disposables.Clear(); // 購読解除
    }

    protected virtual void Start() { }

    protected virtual void FixedUpdate()
    {
        if (isDead)
        {
            characterState = CharacterState.Die; // キャラクターが死亡した場合の状態設定
        }
        else if (GameManager.Instance.isGameEnd)
        {
            characterState = CharacterState.Idle; // ゲーム終了時は待機状態に
        }
        HandleState(); // 現在の状態に応じた処理を実行
    }

    // ================= 初期化メソッド =================
    private void InitCharacter()
    {
        if (characterBase == null)
        {
            Debug.LogError("CharacterBase is not assigned.");
            return;
        }

        // キャラクターパラメータをベースデータから設定
        Name = characterBase.Name;
        maxHp = characterBase.MaxHp;
        currentHp.Value = maxHp;
        Atk = characterBase.Atk;
        AttackSpeed = characterBase.AttackSpeed;
        AttackCoolTime = characterBase.AttackCoolTime;
        speed = characterBase.Speed;
        range = characterBase.Range;
        deffence = characterBase.Defence;
        magicDeffence = characterBase.MagicDefence;
        cost = characterBase.Cost;
        characterType = characterBase.CharacterType;
    }

    private void InitializeComponents()
    {
        hpBar = GetComponentInChildren<HPBar>(); // HPバーの取得
        motionFacade = GetComponent<CharacterMotionFacade>(); // アニメーション管理の取得
        motionFacade.Initialize(HitAttack, Dead); // アニメーションイベントの初期化
    }

    private void InitializeSkills()
    {
        normalSkillInstance = Instantiate(characterBase.NormalSkill); // 通常スキルのインスタンス化
        if (characterBase.SpecialSkill != null)
        {
            specialSkillInstance = Instantiate(characterBase.SpecialSkill); // 特殊スキルのインスタンス化（存在する場合）
        }
        currentSkill = normalSkillInstance; // 現在のスキルを通常スキルに設定
    }

    private void InitializeTargetList()
    {
        targetList = new ReactiveCollection<IDamageable>(); // ターゲットリストの初期化
        targetList.ObserveAdd().Subscribe(item =>
        {
            characterState = CharacterState.Idle;
            if (enemyObject == null)
            {
                SetNextEnemy(); // 新しいターゲットを設定
            }
            if (currentSkill.CanUseSkill.Value)
            {
                AttackEvent(); // 攻撃イベントを呼び出し
            }
            else
            {

            }
        }).AddTo(disposables);

        targetList.ObserveRemove().Subscribe(item =>
        {
            
        }).AddTo(disposables);
    }

    // ================= 状態管理メソッド =================
    private void HandleState()
    {
        switch (characterState)
        {
            case CharacterState.Run:
                motionFacade.RunMotion(speed, isPlayer); // 走るモーション
                break;
            case CharacterState.Die:
                motionFacade.DeathMotion(); // 死ぬモーション
                break;
            case CharacterState.Idle:
                motionFacade.IdleMotion(); // 待機モーション
                break;
        }
    }

    // ================= 衝突イベント処理 =================
    private void OnCollisionEnter2D(Collision2D other)
    {
        // 地面または自分の拠点と衝突した場合は無視
        if (other.gameObject.CompareTag("Ground") || IsOwnBase(other.gameObject.tag)) return;

        // 敵キャラクターと衝突した場合
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Enemy"))
        {
            IDamageable enemy = DetectEnemy(other.gameObject); // 敵を感知
            if (enemy != null && !targetList.Contains(enemy))
            {
                AddEnemyToList(enemy); // 敵をターゲットリストに追加
            }
        }
    }

    private bool IsOwnBase(string tag)
    {
        // 自分の陣地かどうかを判定
        return (isPlayer && (tag == "PlayerCastle" || tag == "Player")) ||
               (!isPlayer && (tag == "EnemyCastle" || tag == "Enemy"));
    }

    // ================= 攻撃処理 =================
    protected void AttackEvent()
    {
        if (!IsDead && enemyObject != null && currentSkill.CanUseSkill.Value)
        {
            motionFacade.NormalAttackMotion(AttackSpeed); // 通常攻撃アニメーション
        }
    }

    // ================= ダメージ処理と死亡判定 =================
    public void TakeDamage(float damage)
    {
        // if (isDead) return;

        currentHp.Value = Mathf.Max(currentHp.Value - damage, 0); // ダメージを受けた後のHPを計算

        if (currentHp.Value <= 0)
        {
            isDead = true; // HPが0以下の場合、死亡状態に
            characterState = CharacterState.Die;
        }
    }

    // ================= アニメーションイベント =================
    private void HitAttack()
    {
        // 敵に攻撃を与える
        if (enemyObject != null)
        {
            Debug.Log("敵への攻撃！！");
            currentSkill.Activate(this, enemyObject); // スキルを発動
        }
    }

    public void Dead() => Destroy(gameObject); // キャラクターの削除

    // ================= 敵感知処理 =================
    protected IDamageable DetectEnemy(GameObject other) => other.GetComponent<IDamageable>(); // 敵を感知

    protected void AddEnemyToList(IDamageable detectedObject)
    {
        if (!targetList.Contains(detectedObject))
        {
            targetList.Add(detectedObject); // 新しい敵をリストに追加
            SubscribeToEnemyHealth(detectedObject);
        }
    }

    protected void SubscribeToEnemyHealth(IDamageable enemy)
    {
        // 敵のHPが0以下になったときの処理
        enemy.currentHp
            .Where(hp => hp <= 0)
            .Subscribe(_ => RemoveEnemyAndResetTarget(enemy))
            .AddTo(this)
            .AddTo(enemy as Component);
    }

    protected void RemoveEnemyAndResetTarget(IDamageable enemy)
    {
        targetList.Remove(enemy); // ターゲットリストから削除
        enemyObject = null; // 現在のターゲットをリセット

        
        if (targetList.Count > 0)
        {
            SetNextEnemy(); // 次のターゲットを設定
            if (currentSkill.CanUseSkill.Value)
            {
                AttackEvent(); // 次の敵に攻撃
            }
            return;
        }
        characterState = CharacterState.Run;
    }


    protected void SetNextEnemy() => enemyObject = targetList[0]; // 次のターゲットを設定

    // ================= その他のメソッド =================
    public void SmoothAppear()
    {
        // キャラクターをフェードインさせる処理
        foreach (var spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
        {
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;
            spriteRenderer.DOFade(1f, 0.5f);
        }
    }

    public void SetHpBar()
    {
        // HPバーの設定
        if (uiHpBar != null)
        {
            uiHpBar.SubscribeValue(currentHp, maxHp);
        }
        hpBar.SubscribeValue(currentHp, maxHp);
    }

    private void SubscribeToSkillCooldown()
    {
        normalSkillInstance.CanUseSkill
            .Skip(1)
            .DistinctUntilChanged()
            .Subscribe(value =>
            {
                if (value)
                {
                    if (enemyObject != null)
                    {
                        AttackEvent();
                    }
                    Debug.Log("スキルが使用可能になりました。");
                }
                else
                {
                    Debug.Log("スキルがクールダウン中です。");
                }
            })
            .AddTo(this);

        if (specialSkillInstance != null)
        {
            specialSkillInstance.CanUseSkill
            .Skip(1)
            .DistinctUntilChanged()
            .Subscribe(value =>
            {
                if (value)
                {
                    if (enemyObject != null)
                    {
                        AttackEvent();
                    }
                    Debug.Log("スキルが使用可能になりました。");
                }
                else
                {
                    Debug.Log("スキルがクールダウン中です。");
                }
            })
            .AddTo(this);
        }
    }
}

public interface IDamageable
{
    ReactiveProperty<float> currentHp { get; set; }
    bool IsDead { get; } // キャラクターが死亡しているかどうかを示すプロパティ
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

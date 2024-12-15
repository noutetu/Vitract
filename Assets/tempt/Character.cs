using DG.Tweening;
using UniRx;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(CharacterMotionFacade))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public abstract class Character : MonoBehaviour, IDamageable
{
    // TODO 二体以上ブロックした時に最初だけ連続攻撃するバグ　クリア
    // TakeDamege(敵が死んだらCanUseのまますぐにAttackEventが呼ばれる)
    // その後StartCooldownをしていたからダメだった。　
    // TODO 音楽をつける 
    // TODO エフェクトをつける
    // TODO 攻撃のアニメーション管理をちゃんとする
    // TODO ジャンプ攻撃のアニメーションで直後の通常攻撃が反応しないバグ。　クリア
    // なんかよくわからんけどアニメーションイベントの秒数変更したらいけた。なんで？

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
    protected AudioSource audioSource;
    protected SpriteRenderer[] spriteRenderers;

    // キャラクターのパラメータ
    public string Name { get; set; } // キャラクター名
    public ReactiveProperty<float> currentHp { get; set; } = new ReactiveProperty<float>(); // 現在のHP
    public float Atk { get; set; } // 攻撃力
    public float AttackSpeed { get; set; } // 攻撃速度
    public float AttackCoolTime { get; set; } // 攻撃クールタイム
    private float maxHp; // 最大HP
    private float speed; // 移動速度
    protected float range; // 射程距離
    public float deffence; // 防御力
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
        
        SmoothAppear();
    }

    

    private void OnDisable()
    {
        if (spriteRenderers != null)
        {
            foreach (var spriteRenderer in spriteRenderers)
            {
                if (spriteRenderer != null)
                {
                    DOTween.Kill(spriteRenderer, true); // SpriteRenderer関連のTweenを停止
                }
            }
        }

        Debug.Log("DoTween is killed");
        DOTween.Kill(this); // DOTweenアニメーションの破棄
        motionFacade.DeInitialize(HitAttack, Dead); // アニメーションイベントの解除
        disposables.Dispose(); // 購読解除
        targetList?.Dispose(); // ターゲットリストの破棄
    }

    private void OnDestroy()
    {
        
    }

    protected virtual void Start() { }

    protected virtual void FixedUpdate()
    {
        if (isDead)
        {
            characterState = CharacterState.Die; // キャラクターが死亡した場合の状態設定
            Debug.Log("キャラが死亡した");
            motionFacade.PlayAnim(AnimType.Dead,1f); // 死ぬモーション
            return;
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
        AttackCoolTime = characterBase.CoolTime;
        speed = characterBase.Speed;
        range = characterBase.Range;
        deffence = characterBase.Defence;
        magicDeffence = characterBase.MagicDefence;
        cost = characterBase.Cost;
        characterType = characterBase.CharacterType;
        characterState = CharacterState.Run;

    }

    private void InitializeComponents()
    {
        hpBar = GetComponentInChildren<HPBar>(); // HPバーの取得
        motionFacade = GetComponent<CharacterMotionFacade>(); // アニメーション管理の取得
        motionFacade.Initialize(HitAttack, Dead); // アニメーションイベントの初期化
        audioSource = GetComponent<AudioSource>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    private void InitializeSkills()
    {
        normalSkillInstance = Instantiate(characterBase.NormalSkill); // 通常攻撃のインスタンス化
        if (characterBase.SpecialSkill != null)
        {
            specialSkillInstance = Instantiate(characterBase.SpecialSkill); // スキルのインスタンス化
        }
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

                if (currentSkill?.CanUseSkill.Value == true)
                {
                    AttackEvent(); // 攻撃イベントを呼び出し
                }
            }
        }).AddTo(this);
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
                motionFacade.PlayAnim(AnimType.Dead,1f); // 死ぬモーション
                break;
            case CharacterState.Idle:
                motionFacade.PlayAnim(AnimType.Idle,1); // 待機モーション
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
        if (!IsDead && enemyObject != null)
        {
            if (currentSkill == specialSkillInstance)
            {
                motionFacade.PlayAnim(specialSkillInstance.AnimType,AttackSpeed,specialSkillInstance.AttackType); // 通常攻撃アニメーション
                return;
            }
            if (currentSkill == normalSkillInstance)
            {
                motionFacade.PlayAnim(normalSkillInstance.AnimType,AttackSpeed,normalSkillInstance.AttackType); // 通常攻撃アニメーション
            }
            
        }
        Debug.Log ("AttackEvent");
    }

    // ================= ダメージ処理と死亡判定 =================
    public void TakeDamage(float damage)
    {
        motionFacade.DamageAnimation(spriteRenderers);
        float damageAmount = damage - deffence;
        float minDamage = damage / 10 ; // 最低保証ダメージ
        if(damageAmount < minDamage){damageAmount = minDamage;}
        currentHp.Value = Mathf.Max(currentHp.Value - damageAmount, 0); // ダメージを受けた後のHPを計算

        Debug.Log($"{this.gameObject.name}は{damageAmount}くらった");

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
            audioSource?.PlayOneShot(currentSkill?.AttackSound);
            currentSkill?.Activate(this, enemyObject); // スキルを発動
        }
    }

    public void Dead()
    {
        Destroy(gameObject); // キャラクターの削除
    }

    // ================= 敵感知処理 =================
    protected IDamageable DetectEnemy(GameObject other) => other.GetComponent<IDamageable>(); // 敵を感知

    protected void AddEnemyToList(IDamageable detectedObject)
    {
        if (!targetList.Contains(detectedObject) && !detectedObject.IsDead)
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
            .AddTo(disposables)
            .AddTo(enemy as Component);
    }

    protected void RemoveEnemyAndResetTarget(IDamageable enemy)
    {
        targetList.Remove(enemy); // ターゲットリストから削除
        enemyObject = null; // 現在のターゲットをリセット

        if (targetList.Count > 0)
        {
            SetNextEnemy(); // 次のターゲットを設定

            if (currentSkill?.CanUseSkill.Value == true)
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
        foreach (var spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer == null) continue; // SpriteRendererが破棄されている場合スキップ

            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;
            spriteRenderer.DOFade(1f, 0.5f)
                .OnKill(() => Debug.Log("Tween killed to prevent null reference."));
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
        normalSkillInstance?.CanUseSkill
            .DistinctUntilChanged()
            .Subscribe(value =>
            {
                UpdateCurrentSkill();
                Debug.Log(value ? "通常スキルが使用可能になりました。" : "通常スキルがクールダウン中です。");
            })
            .AddTo(this);

        specialSkillInstance?.CanUseSkill
            .DistinctUntilChanged()
            .Subscribe(value =>
            {
                UpdateCurrentSkill();
                Debug.Log(value ? "特殊スキルが使用可能になりました。" : "特殊スキルがクールダウン中です。");
            })
            .AddTo(this);
    }

    private void UpdateCurrentSkill()
    {
        // specialSkillが使用可能な場合は常に優先
        if (specialSkillInstance != null && specialSkillInstance.CanUseSkill.Value)
        {
            currentSkill = specialSkillInstance;
            Debug.Log("currentSkill == special");
        }
        else if (normalSkillInstance != null && normalSkillInstance.CanUseSkill.Value)
        {
            currentSkill = normalSkillInstance;
            Debug.Log("currentSkill == normal");
        }
        else
        {
            currentSkill = null;
        }


        switch (currentSkill.SkillType)
        {
            case SkillType.Attack:
                Debug.Log("Handling an Attack skill.");
                // currentSkillがセットされ、かつターゲットがいる場合は攻撃イベントをトリガー
                if (currentSkill != null && enemyObject != null)
                {
                    AttackEvent();
                }
                break;

            case SkillType.Heal:
                Debug.Log("Handling a Heal skill.");
                break;

            case SkillType.Buff:
                Debug.Log("Handling a Buff skill.");
                motionFacade.PlayAnim(currentSkill.AnimType,3);
                currentSkill.Activate(this,this);
                break;

            case SkillType.Debuff:
                Debug.Log("Handling a Debuff skill.");
                break;
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

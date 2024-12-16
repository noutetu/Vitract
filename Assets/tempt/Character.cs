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
    // TODO スキルの種類ごとに処理タイミングとアニメーションを変えられるようにしたい
    // TODO Edがスペシャルスキルしか使わない

    // ================= フィールド =================
    // キャラクターのステータス
    protected IDamageable enemyObject; // 現在攻撃対象のキャラクター
    [SerializeField] private CharacterBase characterBase; // キャラクターのベースデータ
    public bool isPlayer; // プレイヤーかどうかのフラグ
    private bool isDead; // キャラクターが死亡しているかどうか
    public bool IsDead => isDead; // 死亡状態の取得

    // スキル
    SkillData normalSkill;
    SkillData specialSkill;

    // コンポーネント
    public HPBar uiHpBar; // UI上のHPバー
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

    public float MaxHp { get; protected set; }

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
        InitializeSkill();
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
        motionFacade.DeInitialize(DoNormal,DoSpecial, Dead); // アニメーションイベントの解除
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
            motionFacade.PlayAnim(AnimType.Dead, 1f); // 死ぬモーション
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
        MaxHp = characterBase.MaxHp;
        currentHp.Value = MaxHp;
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
        motionFacade.Initialize(DoNormal,DoSpecial, Dead); // アニメーションイベントの初期化
        
        audioSource = GetComponent<AudioSource>();
        
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    private void InitializeTargetList()
    {
        targetList = new ReactiveCollection<IDamageable>(); // ターゲットリストの初期化
        targetList.ObserveAdd().Subscribe(item =>
        {
            characterState = CharacterState.Idle;
            if (enemyObject == null)
            {
                Debug.Log("敵にぶつかった");
                SetEnemy(); // 新しいターゲットを設定
                AttackEvent();
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
                motionFacade.PlayAnim(AnimType.Dead, 1f); // 死ぬモーション
                break;
            case CharacterState.Idle:
                motionFacade.PlayAnim(AnimType.Idle, 1); // 待機モーション
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
            if(specialSkill.CanUseSkill.Value)
            {
                StartSkill(specialSkill);
                return;
            }
            StartSkill(normalSkill);
            return;
        }
    }

    // ================= ダメージ処理と死亡判定 =================
    public void TakeDamage(float damage)
    {
        motionFacade.DamageAnimation(spriteRenderers);
        float damageAmount = damage - deffence;
        float minDamage = damage / 10; // 最低保証ダメージ
        if (damageAmount < minDamage) { damageAmount = minDamage; }
        currentHp.Value = Mathf.Max(currentHp.Value - damageAmount, 0); // ダメージを受けた後のHPを計算

        Debug.Log($"{this.gameObject.name}は{damageAmount}くらった");

        if (currentHp.Value <= 0)
        {
            isDead = true; // HPが0以下の場合、死亡状態に
            characterState = CharacterState.Die;
        }
    }

    // ================= アニメーションイベント =================
    private void DoNormal()
    {
        // 敵に攻撃を与える
        if (enemyObject != null)
        {
            normalSkill.Activate(this,enemyObject);
            audioSource.PlayOneShot(normalSkill.SkillSound);
            Debug.Log("敵への攻撃！！");
        }

    }
    private void DoSpecial()
    {
        // 敵に攻撃を与える
        if (enemyObject != null)
        {
            specialSkill.Activate(this,enemyObject);
            audioSource.PlayOneShot(specialSkill.SkillSound);
            if(normalSkill.CanUseSkill.Value)
            {
                StartSkill(normalSkill);
            }
            Debug.Log("敵への攻撃！！");
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

        if (targetList.Count > 0 && enemyObject == null)
        {
            SetEnemy(); // 次のターゲットを設定
            return;
        }

        characterState = CharacterState.Run;
    }

    protected void SetEnemy()
    {
        enemyObject = targetList[0]; // 次のターゲットを設定
    }

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
    public void InitializeSkill()
    {
        normalSkill = Instantiate(characterBase.NormalSkill); // 通常攻撃のインスタンス化
        if (characterBase.SpecialSkill != null)
        {
            specialSkill = Instantiate(characterBase.SpecialSkill); // スキルのインスタンス化
        }

        SubscribeToSkillCooldown();
    }
    private void SubscribeToSkillCooldown()
    {
        normalSkill?.CanUseSkill
            .DistinctUntilChanged()
            .Subscribe(value =>
            {
                if(value == true)
                {
                    StartSkill(normalSkill);
                    return;
                }
                else
                {
                    if(enemyObject != null)
                    {
                        motionFacade.PlayAnim(AnimType.Idle,1);
                        Debug.Log("待機しろーーーーー");
                    }
                }
            })
            .AddTo(this);

        specialSkill?.CanUseSkill
            .DistinctUntilChanged()
            .Subscribe(value =>
            {
                if(value == true)
                {
                    StartSkill(specialSkill);
                    return;
                }
                else
                {
                    if(enemyObject != null)
                    {
                        motionFacade.PlayAnim(AnimType.Idle,1);
                    }
                }
            })
            .AddTo(this);
    }

public void StartSkill(SkillData skill)
    {
        switch (skill.SkillType)
        {
            case SkillType.Attack:
            if(enemyObject == null){return;}
            Debug.Log("スキル発動");
            motionFacade.PlayAttackAnim(skill.AttackType,1);
                return ;

            case SkillType.Heal:
                return;

            case SkillType.Buff:
            motionFacade.PlayAnim(AnimType.Buff,1);
            audioSource.PlayOneShot(skill.SkillSound);
            skill.Activate(this,enemyObject);
                return;
        }
    }
    public void SetHpBar()
    {
        // HPバーの設定
        if (uiHpBar != null)
        {
            uiHpBar.SubscribeValue(currentHp, MaxHp);
        }
        hpBar.SubscribeValue(currentHp, MaxHp);
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
    Buff,
    Debuff,
    Idle,
}

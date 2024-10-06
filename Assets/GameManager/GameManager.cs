using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // シングルトンのインスタンス
    public static GameManager Instance { get; private set; }

    [SerializeField] Castle playerCastle;
    [SerializeField] Castle enemyCastle;
    public bool isPlayerDefeated { get; private set; }  // 外部からは読み取り専用にする
    public bool isEnemyDefeated { get; private set; }   // 同上

    private void Awake()
    {
        // インスタンスの初期化。複数のインスタンスが存在しないようにする
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーン間でオブジェクトを破棄しないようにする
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        isPlayerDefeated = false;
        isEnemyDefeated = false;
        playerCastle.OnDead = PlayerLost;
        enemyCastle.OnDead = EnemyLost;
    }

    public void PlayerLost()  // 他のクラスからも呼び出せるようにpublicにする
    {
        Debug.Log("Playerのまけ");
        isPlayerDefeated = true;
    }

    public void EnemyLost()   // 同上
    {
        Debug.Log("Enemyのまけ");
        isEnemyDefeated = true;
    }
}

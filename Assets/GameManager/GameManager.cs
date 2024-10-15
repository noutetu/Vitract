using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // シングルトンのインスタンス
    public static GameManager Instance { get; private set; }

    public bool isPlayerDefeated { get; private set; }  // 外部からは読み取り専用にする
    public bool isEnemyDefeated { get; private set; }   // 同上
    public bool isGameEnd;

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
        isGameEnd = false;
    }

    public void GameResult(bool isPlayer)  // 他のクラスからも呼び出せるようにpublicにする
    {
        if (isPlayer)
        {
            Debug.Log("Playerのまけ");
            isPlayerDefeated = true;
        }
        if (!isPlayer)
        {
            Debug.Log("Enemyのまけ");
            isEnemyDefeated = true;
        }
        isGameEnd = true;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BGMManager : MonoBehaviour
{
    // シングルトン
    public static BGMManager Instance { get; private set; }

    private AudioSource audioSource;

    [Serializable]
    public class AudioPlayLocationClipPair
    {
        public AudioPlayLocation location;
        public AudioClip clip;
    }

    [SerializeField] private List<AudioPlayLocationClipPair> bgmList = new List<AudioPlayLocationClipPair>();
    private Dictionary<AudioPlayLocation, AudioClip> bgmDictionary = new Dictionary<AudioPlayLocation, AudioClip>();

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
        audioSource = GetComponent<AudioSource>();

        // bgmList から bgmDictionary に変換
        foreach (var pair in bgmList)
        {
            if (!bgmDictionary.ContainsKey(pair.location))
            {
                bgmDictionary.Add(pair.location, pair.clip);
            }
            else
            {
                Debug.LogWarning($"Duplicate key detected: {pair.location}");
            }
        }
    }
    public void PlayBGM(AudioPlayLocation audioPlayLocation)
    {
        if (bgmDictionary.TryGetValue(audioPlayLocation, out AudioClip bgm))
        {
            audioSource.clip = bgm; // 再生するクリップを設定
            audioSource.Play();     // 再生
        }
        else
        {
            Debug.LogWarning($"指定された AudioPlayLocation '{audioPlayLocation}' に対応する BGM が見つかりません。");
        }
    }

}

public enum AudioPlayLocation
{
    None,               // デフォルト値、何も指定されていない場合
    MainMenu,           // メインメニュー
    InGame,             // ゲームプレイ中
    Battle,             // 戦闘中
    Victory,            // 勝利シーン
    Defeat,             // 敗北シーン
    GameOver,           // ゲームオーバー
    Shop,               // ショップ画面
    PauseMenu,          // 一時停止メニュー
    CutScene,           // カットシーン
    LevelStart,         // レベル開始時
    LevelEnd,           // レベル終了時
    Exploration,        // 探索シーン
    BossBattle,         // ボス戦
    Dialogue,           // 会話シーン
    Settings,           // 設定画面
}

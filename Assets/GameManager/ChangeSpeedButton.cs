using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeSpeedButton : MonoBehaviour
{
    [SerializeField] Text changeGameSpeedText;

    private void Start()
    {
        changeGameSpeedText.text = $"SPEED ✖️ {GameManager.Instance.gameSpeed}";
    }

    public void ChangeGameSpeed()
    {
        GameManager.Instance.gameSpeed += 0.5f;
        if (GameManager.Instance.gameSpeed >= 3)
        {
            GameManager.Instance.gameSpeed = 1;
        }
        changeGameSpeedText.text = $"SPEED ✖️ {GameManager.Instance.gameSpeed}";
    }
}

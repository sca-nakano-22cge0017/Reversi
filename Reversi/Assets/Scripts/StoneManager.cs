using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// コマの色変更
/// </summary>
public class StoneManager : MonoBehaviour
{
    SpriteRenderer sr;

    Color white = new Color(1, 1, 1, 1);
    Color black = new Color(0, 0, 0, 1);

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetState(MainController.eStoneState state)
    {
        switch (state)
        {
            case MainController.eStoneState.EMPTY:
                this.gameObject.SetActive(false); // 非表示
                break;

            case MainController.eStoneState.WHITE:
                if(!this.gameObject.activeSelf) this.gameObject.SetActive(true); // 表示
                sr.color = white; // 色変更
                break;

            case MainController.eStoneState.BLACK:
                if (!this.gameObject.activeSelf) this.gameObject.SetActive(true); // 表示
                sr.color = black; // 色変更
                break;
        }
    }
}

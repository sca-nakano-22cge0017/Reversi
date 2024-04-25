using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneManager : MonoBehaviour
{
    SpriteRenderer sr;

    Color white = new Color(1, 1, 1, 1);
    Color black = new Color(0, 0, 0, 1);

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetState(MainController.eStoneState state)
    {
        switch (state)
        {
            case MainController.eStoneState.EMPTY:
                this.gameObject.SetActive(false); // ��\��
                break;

            case MainController.eStoneState.WHITE:
                if(!this.gameObject.activeSelf) this.gameObject.SetActive(true); // �\��
                sr.color = white; // �F�ύX
                break;

            case MainController.eStoneState.BLACK:
                if (!this.gameObject.activeSelf) this.gameObject.SetActive(true); // �\��
                sr.color = black; // �F�ύX
                break;
        }
    }
}

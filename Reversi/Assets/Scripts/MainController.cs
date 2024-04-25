using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : MonoBehaviour
{
    [SerializeField] Camera mainCamera;

    [SerializeField] GameObject stoneObj;
    [SerializeField] Transform stoneParent;
    public enum eStoneState { EMPTY, WHITE, BLACK };

    const int boardX = 8; // ���}�X
    const int boardY = 8; // �c�}�X
    int whiteScore; // ���̖���
    int blackScore; // ���̖���

    GameObject[,] stones = new GameObject[boardY, boardX];
    StoneManager[,] stoneManagers = new StoneManager[boardY, boardX];
    eStoneState[,] stoneState = new eStoneState[boardY, boardX]; // �Տ�̐΂̐F

    bool isFirstPlayer = true; // ��肩�ǂ���

    // Ray
    const float maxDistance = 10;
    LayerMask layerMask;

    void Start()
    {
        layerMask = 1 << LayerMask.NameToLayer("Checker");

        for (int y = 0; y < boardY; y++)
        {
            for(int x = 0; x < boardX; x++)
            {
                // ����
                var stone = Instantiate(stoneObj, stoneParent);
                StoneManager stoneManager = stone.GetComponent<StoneManager>();

                // �ʒu�ύX
                stone.transform.localPosition = new Vector3(x, y, 0);

                // �z��ɏ�������
                stones[x, y] = stone;
                stoneManagers[x, y] = stoneManager;
                stoneState[x, y] = eStoneState.EMPTY;
            }
        }

        stoneState[3, 3] = eStoneState.WHITE;
        stoneState[3, 4] = eStoneState.BLACK;
        stoneState[4, 3] = eStoneState.BLACK;
        stoneState[4, 4] = eStoneState.WHITE;

        whiteScore = 2;
        blackScore = 2;
    }

    void Update()
    {
        PutStone();

        // �R�}�̕`����X�V
        for(int y = 0; y < boardY; y++)
        {
            for(int x = 0; x < boardX; x++)
            {
                stoneManagers[x, y].SetState(stoneState[x, y]);
            }
        }
    }

    /// <summary>
    /// �R�}��u������
    /// </summary>
    void PutStone()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit2D = Physics2D.Raycast((Vector2)ray.origin, (Vector2)ray.direction, maxDistance, layerMask);

            if(hit2D.collider)
            {
                Vector2 pos = hit2D.collider.gameObject.GetComponent<Transform>().localPosition;
                int x = (int)pos.x;
                int y = (int)pos.y;

                // �����u����Ă��Ȃ��ꍇ
                if(stoneState[x, y] == eStoneState.EMPTY)
                {
                    if(isFirstPlayer) stoneState[x, y] = eStoneState.BLACK;
                    else stoneState[x, y] = eStoneState.WHITE;

                    isFirstPlayer = !isFirstPlayer; // ��Ԍ��
                }
            }
        }
    }
}

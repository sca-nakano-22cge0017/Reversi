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
        // ����u���R�}�̐F
        eStoneState color = isFirstPlayer ? eStoneState.BLACK : eStoneState.WHITE;

        bool isTurnable = false; // �u���ꏊ�����邩�ǂ���

        for (int y = 0; y < boardY; y++)
        {
            for (int x = 0; x < boardX; x++)
            {
                // �����u����Ă��Ȃ��ꍇ
                if (stoneState[x, y] == eStoneState.EMPTY)
                {
                    // �Ђ�����Ԃ���R�}����������break
                    if (Turn(x, y, color, true))
                    {
                        isTurnable = true;
                        break;
                    }
                }
            }

            if(isTurnable) break;
        }

        if(!isTurnable)
        {
            Debug.Log("pass");
            isFirstPlayer = !isFirstPlayer; // ��Ԍ��
        }

        if (Input.GetMouseButtonDown(0) && isTurnable)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit2D = Physics2D.Raycast((Vector2)ray.origin, (Vector2)ray.direction, maxDistance, layerMask);

            if(hit2D.collider)
            {
                Vector2 pos = hit2D.collider.gameObject.GetComponent<Transform>().localPosition;
                int x = (int)pos.x;
                int y = (int)pos.y;

                // �����u����Ă��Ȃ��ꍇ �Ђ�����Ԃ���R�}������Ȃ�u����
                if (stoneState[x, y] == eStoneState.EMPTY && Turn(x, y, color, false))
                {
                    stoneState[x, y] = isFirstPlayer ? eStoneState.BLACK : eStoneState.WHITE;
                    isFirstPlayer = !isFirstPlayer; // ��Ԍ��
                }
            }
        }
    }

    int[] turnCheckX = new int[] {-1, 0, 1, -1, 1, -1, 0, 1};
    int[] turnCheckY = new int[] {-1, -1, -1, 0, 0, 1, 1, 1};
    const int dirNum = 8; // ����������

    /// <summary>
    /// �Ђ�����Ԃ���΂̍��W�f�[�^
    /// </summary>
    class TurnableStone
    {
        public int posX;
        public int posY;

        public TurnableStone(int x, int y)
        {
            posX = x;
            posY = y;
        }
    }

    /// <summary>
    /// �Ђ�����Ԃ�����
    /// </summary>
    /// <param name="putPosX">�R�}��u�����ʒu�@X���W</param>
    /// <param name="putPosY">�R�}��u�����ʒu�@Y���W</param>
    /// <param name="color">�u�����R�}�̐F</param>
    /// <param name="isCheckOnly">true�̂Ƃ��m�F����݂̂ɂƂǂ߂�</param>
    /// <returns>�u���邩�ǂ���</returns>
    bool Turn(int putPosX, int putPosY, eStoneState color, bool isCheckOnly)
    {
        bool isTurnable = false; // �Ђ�����Ԃ��R�}�����邩 = �u���邩�ǂ���

        // �m�F�������
        int dirX = 0;
        int dirY = 0;

        List<TurnableStone> turnableStone = new List<TurnableStone>();

        // 8�����m�F
        for (int c = 0; c < dirNum; c++)
        {
            // �m�F����R�}�̍��W
            int checkPosX = putPosX;
            int checkPosY = putPosY;

            // �m�F�����w��
            dirX = turnCheckX[c];
            dirY = turnCheckY[c];
            eStoneState targetState; // �m�F����R�}��State

            // 1��ɂ�����Ђ�����Ԃ���R�}�̃f�[�^���ꎞ�ۑ�����
            List<TurnableStone> turnableStone_OneLine = new List<TurnableStone>();

            while (true)
            {
                checkPosX += dirX;
                checkPosY += dirY;

                // �ՖʊO�łȂ��ꍇ
                if (checkPosX >= 0 && checkPosX < boardX && checkPosY >= 0 && checkPosY < boardY)
                {
                    targetState = stoneState[checkPosX, checkPosY];
                }
                else break; //�͈͊O�Ȃ玟�̕�����

                // �����u���Ă��Ȃ������玟�̕�����
                if (targetState == eStoneState.EMPTY) break;

                // ����̃R�}�̐F��������
                if(targetState != color)
                {
                    turnableStone_OneLine.Add(new TurnableStone(checkPosX, checkPosY));
                }

                // ���g�̃R�}��������
                if(targetState == color)
                {
                    // ���̃��C���łЂ�����Ԃ���R�}�̍��W�f�[�^������
                    foreach(var ts_ol in turnableStone_OneLine)
                    {
                        turnableStone.Add(ts_ol);
                    }

                    break;
                }
            }
        }

        // �Ђ�����Ԃ���R�}���Ȃ������ꍇ��false��Ԃ�
        if(turnableStone.Count <= 0)
        {
            isTurnable = false;
            return isTurnable;
        }

        if(!isCheckOnly)
        {
            // �Ђ�����Ԃ�
            foreach (var ts in turnableStone)
            {
                int posX = ts.posX;
                int posY = ts.posY;
                stoneState[posX, posY] = color;
            }
        }
        isTurnable = true;

        return isTurnable;
    }
}

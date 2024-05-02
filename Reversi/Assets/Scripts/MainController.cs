using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
    [SerializeField] Camera mainCamera;

    [SerializeField] GameObject stoneObj;
    [SerializeField] Transform stoneParent;
    public enum eStoneState { EMPTY, WHITE, BLACK };

    const int boardX = 8; // ���}�X
    const int boardY = 8; // �c�}�X
    int blackScore = 0;
    int whiteScore = 0;

    GameObject[,] stones = new GameObject[boardY, boardX];
    StoneManager[,] stoneManagers = new StoneManager[boardY, boardX];
    eStoneState[,] stoneState = new eStoneState[boardY, boardX]; // �Տ�̐΂̐F

    bool isFirstPlayer = true; // ��肩�ǂ���

    int passNum = 0; // �p�X�̉�

    // Ray
    const float maxDistance = 10;
    LayerMask layerMask;

    bool isGameEnd = false;

    // UI
    [SerializeField] Text whiteScoreText;
    [SerializeField] Text blackScoreText;
    [SerializeField] Text pass;
    [SerializeField] Text gameEnd;
    [SerializeField] Text win;

    // �R�}���u���邩�ǂ�����\������
    [SerializeField] GameObject canPutObj;
    [SerializeField] Transform canPutParent;
    SpriteRenderer[,] canPut = new SpriteRenderer[boardY, boardX];

    // �^�C�}�[
    [SerializeField] Timer blackTimer;
    [SerializeField] Timer whiteTimer;
    Timer playerTimer;
    Timer lastPlayerTimer;

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

        for (int y = 0; y < boardY; y++)
        {
            for (int x = 0; x < boardX; x++)
            {
                // ����
                var c = Instantiate(canPutObj, canPutParent);

                // �ʒu�ύX
                c.transform.localPosition = new Vector3(x, y, 0);

                // �z��ɏ�������
                canPut[x, y] = c.GetComponent<SpriteRenderer>();
                canPut[x, y].enabled = false;
            }
        }

        blackTimer.gameObject.SetActive(false);
        whiteTimer.gameObject.SetActive(false);

        Timer();
    }

    void Update()
    {
        if(!isGameEnd)
        {
            PutStone();

            // �R�}�̕`����X�V
            for (int y = 0; y < boardY; y++)
            {
                for (int x = 0; x < boardX; x++)
                {
                    stoneManagers[x, y].SetState(stoneState[x, y]);
                }
            }

            CountAmount();
            GameEndCheck();
        }
    }

    /// <summary>
    /// �����𐔂���
    /// </summary>
    void CountAmount()
    {
        int bScore = 0;
        int wScore = 0;

        for (int y = 0; y < boardY; y++)
        {
            for (int x = 0; x < boardX; x++)
            {
                if(stoneState[x, y] == eStoneState.BLACK) bScore++;
                if(stoneState[x, y] == eStoneState.WHITE) wScore++;
            }
        }

        blackScore = bScore;
        whiteScore = wScore;

        whiteScoreText.text = wScore.ToString() + "��";
        blackScoreText.text = bScore.ToString() + "��";
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
                    // �Ђ�����Ԃ���R�}����������
                    if (Turn(x, y, color, true))
                    {
                        isTurnable = true;
                        canPut[x, y].enabled = true; // �u����ꏊ�ɉ摜��\�����ċ���
                    }
                }
            }

            if(isTurnable)
            {
                passNum = 0;
            }
        }

        if(!isTurnable)
        {
            isFirstPlayer = !isFirstPlayer; // ��Ԍ��
            passNum++;

            StartCoroutine(Pass());

            // �p�X����������
            if(passNum >= 2)
            {
                // �Q�[���I��
                isGameEnd = true;
            }
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

                    CanPutDispInitialize();

                    Timer();
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

    /// <summary>
    /// �^�C�}�[�Ǘ�
    /// </summary>
    void Timer()
    {
        playerTimer = isFirstPlayer ? blackTimer : whiteTimer;
        playerTimer.gameObject.SetActive(true);

        lastPlayerTimer = isFirstPlayer ? whiteTimer : blackTimer;
        lastPlayerTimer.gameObject.SetActive(false);
    }

    /// <summary>
    /// �R�}���u���邩�ǂ����̕\����������
    /// </summary>
    void CanPutDispInitialize()
    {
        for (int y = 0; y < boardY; y++)
        {
            for (int x = 0; x < boardX; x++)
            {
                canPut[x, y].enabled = false;
            }
        }
    }

    /// <summary>
    /// �Q�[���I���������m�F�@���o
    /// </summary>
    void GameEndCheck()
    {
        if(isGameEnd)
        {
            StartCoroutine(Result());

            blackTimer.gameObject.SetActive(false);
            whiteTimer.gameObject.SetActive(false);
        }
    }

    IEnumerator Result()
    {
        gameEnd.enabled = true;
        pass.enabled = false;

        yield return new WaitForSeconds(1.0f);

        gameEnd.enabled = false;

        if (blackScore > whiteScore) win.text = "���̏���";
        if (blackScore < whiteScore) win.text = "���̏���";
        if (blackScore == whiteScore) win.text = "��������";
    }

    /// <summary>
    /// �p�X���o
    /// </summary>
    /// <returns></returns>
    IEnumerator Pass()
    {
        if(!isGameEnd)
        {
            pass.enabled = true;
            yield return new WaitForSeconds(1.0f);
            pass.enabled = false;
        }
    }
}

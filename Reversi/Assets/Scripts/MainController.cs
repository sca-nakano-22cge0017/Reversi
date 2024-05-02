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

    const int boardX = 8; // 横マス
    const int boardY = 8; // 縦マス
    int blackScore = 0;
    int whiteScore = 0;

    GameObject[,] stones = new GameObject[boardY, boardX];
    StoneManager[,] stoneManagers = new StoneManager[boardY, boardX];
    eStoneState[,] stoneState = new eStoneState[boardY, boardX]; // 盤上の石の色

    bool isFirstPlayer = true; // 先手かどうか

    int passNum = 0; // パスの回数

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

    // コマが置けるかどうかを表示する
    [SerializeField] GameObject canPutObj;
    [SerializeField] Transform canPutParent;
    SpriteRenderer[,] canPut = new SpriteRenderer[boardY, boardX];

    // タイマー
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
                // 生成
                var stone = Instantiate(stoneObj, stoneParent);
                StoneManager stoneManager = stone.GetComponent<StoneManager>();

                // 位置変更
                stone.transform.localPosition = new Vector3(x, y, 0);

                // 配列に情報を入れる
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
                // 生成
                var c = Instantiate(canPutObj, canPutParent);

                // 位置変更
                c.transform.localPosition = new Vector3(x, y, 0);

                // 配列に情報を入れる
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

            // コマの描画を更新
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
    /// 枚数を数える
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

        whiteScoreText.text = wScore.ToString() + "枚";
        blackScoreText.text = bScore.ToString() + "枚";
    }

    /// <summary>
    /// コマを置く処理
    /// </summary>
    void PutStone()
    {
        // 今回置くコマの色
        eStoneState color = isFirstPlayer ? eStoneState.BLACK : eStoneState.WHITE;

        bool isTurnable = false; // 置く場所があるかどうか

        for (int y = 0; y < boardY; y++)
        {
            for (int x = 0; x < boardX; x++)
            {
                // 何も置かれていない場合
                if (stoneState[x, y] == eStoneState.EMPTY)
                {
                    // ひっくり返せるコマがあったら
                    if (Turn(x, y, color, true))
                    {
                        isTurnable = true;
                        canPut[x, y].enabled = true; // 置ける場所に画像を表示して強調
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
            isFirstPlayer = !isFirstPlayer; // 手番交代
            passNum++;

            StartCoroutine(Pass());

            // パスが続いたら
            if(passNum >= 2)
            {
                // ゲーム終了
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

                // 何も置かれていない場合 ひっくり返せるコマがあるなら置ける
                if (stoneState[x, y] == eStoneState.EMPTY && Turn(x, y, color, false))
                {
                    stoneState[x, y] = isFirstPlayer ? eStoneState.BLACK : eStoneState.WHITE;
                    isFirstPlayer = !isFirstPlayer; // 手番交代

                    CanPutDispInitialize();

                    Timer();
                }
            }
        }
    }

    int[] turnCheckX = new int[] {-1, 0, 1, -1, 1, -1, 0, 1};
    int[] turnCheckY = new int[] {-1, -1, -1, 0, 0, 1, 1, 1};
    const int dirNum = 8; // 検索方向数

    /// <summary>
    /// ひっくり返せる石の座標データ
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
    /// ひっくり返す処理
    /// </summary>
    /// <param name="putPosX">コマを置いた位置　X座標</param>
    /// <param name="putPosY">コマを置いた位置　Y座標</param>
    /// <param name="color">置いたコマの色</param>
    /// <param name="isCheckOnly">trueのとき確認するのみにとどめる</param>
    /// <returns>置けるかどうか</returns>
    bool Turn(int putPosX, int putPosY, eStoneState color, bool isCheckOnly)
    {
        bool isTurnable = false; // ひっくり返すコマがあるか = 置けるかどうか

        // 確認する方向
        int dirX = 0;
        int dirY = 0;

        List<TurnableStone> turnableStone = new List<TurnableStone>();

        // 8方向確認
        for (int c = 0; c < dirNum; c++)
        {
            // 確認するコマの座標
            int checkPosX = putPosX;
            int checkPosY = putPosY;

            // 確認方向指定
            dirX = turnCheckX[c];
            dirY = turnCheckY[c];
            eStoneState targetState; // 確認するコマのState

            // 1列におけるひっくり返せるコマのデータを一時保存する
            List<TurnableStone> turnableStone_OneLine = new List<TurnableStone>();

            while (true)
            {
                checkPosX += dirX;
                checkPosY += dirY;

                // 盤面外でない場合
                if (checkPosX >= 0 && checkPosX < boardX && checkPosY >= 0 && checkPosY < boardY)
                {
                    targetState = stoneState[checkPosX, checkPosY];
                }
                else break; //範囲外なら次の方向へ

                // 何も置いていなかったら次の方向へ
                if (targetState == eStoneState.EMPTY) break;

                // 相手のコマの色だったら
                if(targetState != color)
                {
                    turnableStone_OneLine.Add(new TurnableStone(checkPosX, checkPosY));
                }

                // 自身のコマだったら
                if(targetState == color)
                {
                    // そのラインでひっくり返せるコマの座標データを入れる
                    foreach(var ts_ol in turnableStone_OneLine)
                    {
                        turnableStone.Add(ts_ol);
                    }

                    break;
                }
            }
        }

        // ひっくり返せるコマがなかった場合はfalseを返す
        if(turnableStone.Count <= 0)
        {
            isTurnable = false;
            return isTurnable;
        }

        if(!isCheckOnly)
        {
            // ひっくり返す
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
    /// タイマー管理
    /// </summary>
    void Timer()
    {
        playerTimer = isFirstPlayer ? blackTimer : whiteTimer;
        playerTimer.gameObject.SetActive(true);

        lastPlayerTimer = isFirstPlayer ? whiteTimer : blackTimer;
        lastPlayerTimer.gameObject.SetActive(false);
    }

    /// <summary>
    /// コマが置けるかどうかの表示を初期化
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
    /// ゲーム終了したか確認　演出
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

        if (blackScore > whiteScore) win.text = "黒の勝ち";
        if (blackScore < whiteScore) win.text = "白の勝ち";
        if (blackScore == whiteScore) win.text = "引き分け";
    }

    /// <summary>
    /// パス演出
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

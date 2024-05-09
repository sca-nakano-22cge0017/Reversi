using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    public void GameStart()
    {
        SceneManager.LoadScene("Main");
    }

    public void ToTitle()
    {
        SceneManager.LoadScene("Title");
    }

    /// <summary>
    /// ƒV[ƒ“‘JˆÚ
    /// </summary>
    /// <param name="time">‘JˆÚ‚·‚é‚Ü‚Å‚ÌŠÔ</param>
    /// <param name="sceneName">‘JˆÚ‚·‚éƒV[ƒ“–¼</param>
    public void Change(float time, string sceneName)
    {
        StartCoroutine(ChangeCoroutine(time, sceneName));
    }

    IEnumerator ChangeCoroutine(float time, string sceneName)
    {
        yield return new WaitForSeconds(time);
        SceneManager.LoadScene(sceneName);
    }
}

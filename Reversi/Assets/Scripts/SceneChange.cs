using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン遷移
/// </summary>
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
    /// シーン遷移
    /// </summary>
    /// <param name="time">遷移するまでの時間</param>
    /// <param name="sceneName">遷移するシーン名</param>
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

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
    /// �V�[���J��
    /// </summary>
    /// <param name="time">�J�ڂ���܂ł̎���</param>
    /// <param name="sceneName">�J�ڂ���V�[����</param>
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

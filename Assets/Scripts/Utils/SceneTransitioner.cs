using BeauRoutine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneTransitioner : Singleton<SceneTransitioner>
{
    public enum SceneIndicies
    {
        Gameplay = 0
    }

    private void Start()
    {
        LoadSceneWithIndex((int)SceneIndicies.Gameplay);
    }

    public void LoadSceneWithIndex(int _index)
    {
        SceneManager.LoadScene(_index);
    }

    public void WaitThenLoadScene(int _index, float _delay)
    {
        Routine.Start(WaitThenLoadRoutine(_index, _delay));
    }

    private IEnumerator WaitThenLoadRoutine(int _index, float _delay)
    {
        yield return _delay;

        SceneManager.LoadScene(_index);
    }

    public void ReloadCurrScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

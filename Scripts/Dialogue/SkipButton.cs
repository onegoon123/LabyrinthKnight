using Naninovel;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SkipButton : MonoBehaviour
{
    public void Skip()
    {
        SceneFadeManager.Instance.LoadSceneWithFade("main");
    }
}

using Naninovel;
using UnityEngine;
using UnityEngine.SceneManagement;

[CommandAlias("end")]
public class SwitchToAdventureMode : Command
{
    public override async UniTask Execute(AsyncToken asyncToken)
    {
        var opMain = SceneManager.LoadSceneAsync("main");
        await UniTask.WaitUntil(() => opMain.isDone);
    }
}
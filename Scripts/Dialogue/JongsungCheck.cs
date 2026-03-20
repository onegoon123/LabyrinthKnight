using Naninovel;
using UnityEngine;

/// <summary>
/// 문자열의 끝 글자 받침 여부에 따라 올바른 조사(은/는, 이/가 등)를 반환하거나 붙여 줍니다.
/// </summary>
[CommandAlias("jong")]
public class JongsungCheck : Command
{
    [ParameterAlias("text")]
    public StringParameter SourceText;

    [ParameterAlias("with")]
    public StringParameter WithBatchim;

    [ParameterAlias("without")]
    public StringParameter WithoutBatchim;

    [ParameterAlias("set")]
    public StringParameter TargetVariable;

    [ParameterAlias("append")]
    public BooleanParameter AppendToSource;

    public override UniTask Execute(AsyncToken token = default)
    {
        var text = Assigned(SourceText) ? SourceText.Value : string.Empty;
        var particleWithBatchim = Assigned(WithBatchim) ? WithBatchim.Value : "은";
        var particleWithoutBatchim = Assigned(WithoutBatchim) ? WithoutBatchim.Value : "는";

        var particle = HasFinalConsonant(text) ? particleWithBatchim : particleWithoutBatchim;
        var result = AppendToSource.HasValue ? $"{text}{particle}" : particle;
        if (Assigned(TargetVariable))
        {
            var variables = Engine.GetService<ICustomVariableManager>();
            variables.SetVariableValue(TargetVariable, new(result));
        }
        else
        {
            Debug.Log($"[KoreanParticleCommand] {result}");
        }

        return UniTask.CompletedTask;
    }

    private static bool HasFinalConsonant(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;

        var lastChar = text[text.Length - 1];
        const int hangulBase = 0xAC00;
        const int hangulLast = 0xD7A3;

        if (lastChar < hangulBase || lastChar > hangulLast)
            return false;

        var code = lastChar - hangulBase;
        var jong = code % 28;
        return jong != 0;
    }
}


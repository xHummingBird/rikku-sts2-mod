using BaseLib.Config;

namespace Rikku.RikkuCode.Extensions;

public enum VoiceLanguage
{
    English,
    Japanese
}

public sealed class RikkuConfig : SimpleModConfig
{
    [ConfigSection("Audio")]
    public static bool EnableCustomBattleMusic { get; set; } = true;

    public static bool EnableCustomVictoryMusic { get; set; } = true;

    public static bool EnableCustomGameOverMusic { get; set; } = true;

    [ConfigSection("Voice")]
    public static VoiceLanguage Voice { get; set; } = VoiceLanguage.Japanese;
}
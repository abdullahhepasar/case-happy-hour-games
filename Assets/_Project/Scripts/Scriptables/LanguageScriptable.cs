using UnityEngine;

[CreateAssetMenu(fileName = "Language", menuName = "Project/LanguageData")]
public class LanguageScriptable : ScriptableObject
{
    public LanguageID languageID = LanguageID.en;

    public string LoadingText, PlayText, LevelCompletedText, ClaimRewardsText, NoThanksText, LevelFailedText, NextLevel, LevelText, LevelShortText, UnlockRandomText,
        TryAgainText;

    public string ContinueText, GameLaunchCounterText;

}

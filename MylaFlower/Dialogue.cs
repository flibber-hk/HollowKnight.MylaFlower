using Modding;

namespace MylaFlower
{
    internal static class Dialogue
    {
        internal delegate string TextEditHandler(string key, string sheetTitle, string orig);

        internal static event TextEditHandler OnGetMylaText;

        internal static void Hook()
        {
            ModHooks.LanguageGetHook += ChangeMylaDialogue;
        }

        private static string ChangeMylaDialogue(string key, string sheetTitle, string orig)
        {
            if (!TryChangeMylaDialogueInternal(key, sheetTitle, orig, out string newText))
            {
                return orig;
            }

            if (OnGetMylaText is null) return newText;

            foreach (TextEditHandler handler in OnGetMylaText?.GetInvocationList())
            {
                newText = handler(key, sheetTitle, newText);
            }
            return newText;
        }

        private static bool TryChangeMylaDialogueInternal(string key, string sheetTitle, string orig, out string newText)
        {
            if (sheetTitle == Consts.CustomLanguageSheet)
            {
                switch (key)
                {
                    case "FLOWER_OFFER":
                    case "FLOWER_OFFER_YN":
                    case "GIVEN_FLOWER":
                    case "NOT_GIVEN_FLOWER":
                        newText = Localization.GetText(key);
                        return true;
                }

                MylaFlower.instance.LogWarn($"Unrecognized key for myla flower sheet: {key}");
            }

            if (sheetTitle == "Minor NPC" 
                && key == "MINER_DREAM_2" 
                && MylaFlower.GetMylaState() == MylaState.Crazy)
            {
                newText = Localization.GetText("CRAZY_DREAM");
                return true;
            }

            if (sheetTitle == "Enemy Dreams"
                && key.StartsWith("MYLA"))
            {
                newText = orig;  // Could be changed if I had some text; need to return true so rando gets to see it
                return true;
            }

            if (sheetTitle == "Minor NPC"
                && key == "MINER_EARLY_3"
                && MylaFlower.GetMylaState() == MylaState.Normal)
            {
                newText = Localization.GetText("GIVEN_DIALOGUE");
                return true;
                // return "Oh hello there friend! Look, your present makes a w-wonderful headlamp. Now I can always have it with me. Thank you again!";
            }

            newText = orig;
            return false;
        }
    }
}

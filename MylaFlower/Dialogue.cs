using System;
using System.Collections.Generic;
using System.Linq;
using Modding;

namespace MylaFlower
{
    internal static class Dialogue
    {
        internal static void Hook()
        {
            ModHooks.LanguageGetHook += ChangeMylaDialogue;
        }

        private static string ChangeMylaDialogue(string key, string sheetTitle, string orig)
        {
            if (sheetTitle == Consts.CustomLanguageSheet)
            {
                switch (key)
                {
                    case "FLOWER_OFFER":
                    case "FLOWER_OFFER_YN":
                    case "GIVEN_FLOWER":
                    case "NOT_GIVEN_FLOWER":
                        return Localization.GetText(key);
                }

                MylaFlower.instance.LogWarn($"Unrecognized key for myla flower sheet: {key}");
            }

            if (sheetTitle == "Minor NPC" 
                && key == "MINER_DREAM_2" 
                && MylaFlower.GetMylaState() == MylaState.Crazy)
            {
                return Localization.GetText("CRAZY_DREAM");
            }

            if (sheetTitle == "Minor NPC"
                && key == "MINER_EARLY_3"
                && MylaFlower.GetMylaState() == MylaState.Normal)
            {
                return Localization.GetText("GIVEN_DIALOGUE");
                // return "Oh hello there friend! Look, your present makes a w-wonderful headlamp. Now I can always have it with me. Thank you again!";
            }

            return orig;
        }
    }
}

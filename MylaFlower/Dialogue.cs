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
                        return "...Bury my body, stabbed w-with nail<br>Darkness encroaching<br>I wait for Light, and Light approaches<br>A pale, fragile vessel<br>Offered to the shadow within me";
                    case "FLOWER_OFFER_YN":
                        return "Give the flower?";
                    case "GIVEN_FLOWER":
                        return "Is this what was calling to me?<br>The aura quiets the voice<br>And breaks through the light and shade";
                    case "NOT_GIVEN_FLOWER":
                        return "...It is for the best...m-my mind...fractured... can't touch...too pure...";
                }

                MylaFlower.instance.LogWarn($"Unrecognized key for myla flower sheet: {key}");
            }

            if (sheetTitle == "Minor NPC" 
                && key == "MINER_DREAM_2" 
                && MylaFlower.GetMylaState() == MylaState.Crazy)
            {
                return "C-can I take it ....DESTROY IT.... Is it offered....DONT TOUCH IT...C-can I take it.... KILL IT....Is it offered.....DANGEROUS";
            }

            if (sheetTitle == "Minor NPC"
                && key == "MINER_EARLY_3"
                && MylaFlower.GetMylaState() == MylaState.Normal)
            {
                return "H-hello again, my friend! You just missed Schy, although I'm sure they're still here somewhere. Its like that with friends, isn't it?<br>They're always here, e-even when they're not.";
                // return "Oh hello there friend! Look, your present makes a w-wonderful headlamp. Now I can always have it with me. Thank you again!";
            }

            return orig;
        }
    }
}

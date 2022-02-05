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
            if (sheetTitle != Consts.LanguageSheet) return orig;

            return key switch
            {
                "FLOWER_OFFER" => "I see you have a flower for me nicole420Eyes",
                "FLOWER_OFFER_YN" => "Give the flower?",
                "GIVEN_FLOWER" => "Thanks for flower miikaLove",
                "NOT_GIVEN_FLOWER" => "Why no give flower mathulPout",
                _ => orig,
            };
        }
    }
}

using ItemChanger;
using ItemChanger.Locations;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using UnityEngine;
using Modding;

namespace MylaFlower.RandoConnection
{
    public class MylaFlowerLocation : AutoLocation, ILocalHintLocation
    {
        public bool HintActive { get; set; }

        protected override void OnLoad()
        {
            MylaFlower.OnGiveFlower += GiveItemOnGiveFlower;
            Events.AddFsmEdit(sceneName, new(Consts.NormalMyla, "Conversation Control"), GiveRespawnedItems);
            Dialogue.OnGetMylaText += ModifyMylaText;
            On.HealthManager.TakeDamage += ProtectMyla;
        }

        protected override void OnUnload()
        {
            MylaFlower.OnGiveFlower -= GiveItemOnGiveFlower;
            Events.RemoveFsmEdit(sceneName, new(Consts.NormalMyla, "Conversation Control"), GiveRespawnedItems);
            Dialogue.OnGetMylaText -= ModifyMylaText;
            On.HealthManager.TakeDamage -= ProtectMyla;
        }

        private void ProtectMyla(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            if (self.gameObject.name != Consts.ZombieMyla || self.gameObject.scene.name != sceneName)
            {
                orig(self, hitInstance);
                return;
            }

            int num = Mathf.RoundToInt(hitInstance.DamageDealt * hitInstance.Multiplier);
            if (num >= self.hp)
            {
                self.hp = 50 + num;
                ShowPreviewConvo(self);
            }

            orig(self, hitInstance);
            return;
        }

        private void ShowPreviewConvo(HealthManager hm)
        {
            EnemyDreamnailReaction edr = hm.gameObject.GetComponent<EnemyDreamnailReaction>();
            ReflectionHelper.CallMethod(edr, "ShowConvo");
        }

        private string ModifyMylaText(string key, string sheetTitle, string orig)
        {
            if (!this.GetItemHintActive()) return orig;

            if (key == "FLOWER_OFFER_YN" && sheetTitle == Consts.CustomLanguageSheet)
            {
                string text = Placement.GetUIName();
                Placement.OnPreview(text);
                return string.Format(Localization.GetText("FLOWER_OFFER_YN_RANDO"), text);
            }

            if (key.StartsWith("MYLA") && sheetTitle == "Enemy Dreams")
            {
                string text = Placement.GetUIName();
                Placement.OnPreview(text);
                return string.Format(Localization.GetText("ZOMBIE_DREAM_RANDO"), text);
            }

            return orig;
        }

        private void GiveRespawnedItems(PlayMakerFSM fsm)
        {
            fsm.GetState("Talk Finish").AddFirstAction(new Lambda(() =>
            {
                if (Placement.CheckVisitedAll(VisitState.Accepted) && !Placement.AllObtained())
                {
                    GiveAll();
                }
            }));
        }

        private void GiveItemOnGiveFlower()
        {
            Placement.AddVisitFlag(VisitState.Accepted);
            GiveAll();
        }
    }
}

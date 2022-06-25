using System;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vasi;

namespace MylaFlower
{
    public class MylaFlower : Mod, ILocalSettings<SaveSettings>
    {
        internal static MylaFlower instance;


        public static SaveSettings LS { get; internal set; } = new();
        public void OnLoadLocal(SaveSettings s) => LS = s;
        public SaveSettings OnSaveLocal() => LS;

        
        public MylaFlower() : base(null)
        {
            instance = this;
        }
        
        public override string GetVersion()
        {
            return GetType().Assembly.GetName().Version.ToString();
        }
        
        public override void Initialize()
        {
            Log("Initializing Mod...");
            SpriteLoader.LoadTextures();

            // Change logic for myla state
            On.DeactivateIfPlayerdataTrue.OnEnable += OverrideMylaState;
            On.DeactivateIfPlayerdataFalse.OnEnable += OverrideMylaState;

            // Change myla dialogue?
            On.PlayMakerFSM.OnEnable += ModifyMylaFsms;
            Dialogue.Hook();

            // Change sprite
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ReplaceMylaSprite;
        }

        private void ReplaceMylaSprite(Scene _, Scene scene)
        {
            if (scene.name == Consts.MylaScene && LS.DeliveredFlower)
            {
                Texture2D mylaWithFlower = SpriteLoader.GetTexture("MylaFlower");

                GameObject myla = scene.GetRootGameObjects().First(x => x.name == Consts.NormalMyla);
                myla.GetComponent<tk2dSprite>().GetCurrentSpriteDef().material.mainTexture = mylaWithFlower;
            }
        }



        private void ModifyMylaFsms(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);

            if (self.gameObject.name == Consts.CrazyMyla 
                && self.FsmName == Consts.ConvoFsmName 
                && self.gameObject.scene.name == Consts.MylaScene
                && GetMylaState() == MylaState.Crazy)
            {
                EditCrazyMylaFsm(self);
            }

            if (self.gameObject.name == Consts.NormalMyla
                && self.FsmName == Consts.ConvoFsmName
                && self.gameObject.scene.name == Consts.MylaScene
                && GetMylaState() == MylaState.Normal)
            {
                EditNormalMylaFsm(self);
            }
        }

        private void EditNormalMylaFsm(PlayMakerFSM self)
        {
            // Only path: Convo Choice -> Early Choice -> Early 3 (custom text changed by language hook)
            FsmState convoChoice = self.GetState("Convo Choice");
            FsmState earlyChoice = self.GetState("Early Choice");
            FsmState early3 = self.GetState("Early 3");
            
            convoChoice.Transitions = Array.Empty<FsmTransition>();
            convoChoice.AddTransition(FsmEvent.Finished, earlyChoice.Name);

            earlyChoice.Transitions = Array.Empty<FsmTransition>();
            earlyChoice.AddTransition(FsmEvent.Finished, early3.Name);
        }

        private void EditCrazyMylaFsm(PlayMakerFSM fsm)
        {
            FsmState convoChoice = fsm.GetState("Convo Choice");
            FsmState flowerBoxDown = fsm.CopyState("Box Down", "Flower Box Down");
            FsmState boxUpYN = fsm.CopyState("Box Down", "Flower Box Up YN");
            FsmState sendTextYN = fsm.CopyState("Box Down", "Flower Send Text YN");
            FsmState acceptFlower = fsm.CopyState("Box Down", "Accept Flower");
            FsmState declineFlower = fsm.CopyState("Box Down", "Decline Flower");

            AddMylaConvoState("Flower Convo", "Flower Box Down", "FLOWER_OFFER", "Repeat");
            FsmState acceptConvo = AddMylaConvoState("Accept Convo", "Talk Finish", "GIVEN_FLOWER");    // TODO: Make healthy myla noise here
            AddMylaConvoState("Decline Convo", "Talk Finish", "NOT_GIVEN_FLOWER");
            AddBoxUpState("Accept Convo Box Up", "Accept Convo");
            AddBoxUpState("Decline Convo Box Up", "Decline Convo");

            convoChoice.AddTransition("FLOWER OFFER", "Flower Convo");
            convoChoice.AddTransition("ALREADY GIVEN", "Accept Convo");
            convoChoice.InsertMethod(0, () =>
            {
                if (LS.DeliveredFlower) fsm.SendEvent("ALREADY GIVEN");
                else fsm.SendEvent("FLOWER OFFER");
            });

            flowerBoxDown.ChangeTransition("FINISHED", "Flower Box Up YN");

            boxUpYN.RemoveAction(1);
            boxUpYN.GetAction<SendEventByName>().sendEvent = "BOX UP YN";
            boxUpYN.GetAction<Wait>().time = 0.25f;
            boxUpYN.ChangeTransition("FINISHED", sendTextYN.Name);

            sendTextYN.Actions = new[]
            {
                new Vasi.InvokeMethod(() =>
                {
                    GameObject textYN = GameObject.Find("Text YN");
                    PlayMakerFSM dialoguePageControl = textYN.LocateMyFSM("Dialogue Page Control");
                    dialoguePageControl.FsmVariables.GetFsmInt("Toll Cost").Value = 0;
                    dialoguePageControl.FsmVariables.GetFsmGameObject("Requester").Value = fsm.gameObject;
                    textYN.GetComponent<DialogueBox>().StartConversation("FLOWER_OFFER_YN", Consts.CustomLanguageSheet);
                })
            };
            sendTextYN.Transitions = Array.Empty<FsmTransition>();
            sendTextYN.AddTransition("YES", acceptFlower.Name);
            sendTextYN.AddTransition("NO", declineFlower.Name);

            acceptFlower.Actions = new[]
            {
                acceptFlower.Actions[2],
                acceptFlower.Actions[0]
            };
            acceptFlower.GetAction<SendEventByName>().sendEvent = "BOX DOWN YN";
            declineFlower.Actions = new[]
            {
                declineFlower.Actions[2],
                declineFlower.Actions[0]
            };
            declineFlower.GetAction<SendEventByName>().sendEvent = "BOX DOWN YN";
            declineFlower.Transitions = Array.Empty<FsmTransition>();
            declineFlower.AddTransition("FINISHED", "Decline Convo Box Up");
            acceptFlower.Transitions = Array.Empty<FsmTransition>();
            acceptFlower.AddTransition("FINISHED", "Accept Convo Box Up");

            // TODO - a giving flower anim of some sort
            acceptConvo.InsertMethod(0, () =>
            {
                LS.DeliveredFlower = true;
                PlayerData.instance.SetBool(nameof(PlayerData.hasXunFlower), false);
            });


            FsmState AddMylaConvoState(string stateName, string targetState, string convoName, string origTarget = "Greet")
            {
                FsmState newState = fsm.CopyState(origTarget, stateName);
                CallMethodProper cmp = newState.GetAction<CallMethodProper>();
                cmp.parameters[0].stringValue = convoName;
                cmp.parameters[1].stringValue = Consts.CustomLanguageSheet;

                newState.Transitions[0].ToFsmState = fsm.GetState(targetState);
                newState.Transitions[0].ToState = targetState;

                return newState;
            }

            FsmState AddBoxUpState(string stateName, string targetState)
            {
                FsmState newState = fsm.CopyState("Box Up", stateName);
                newState.Actions = new[]
                {
                    newState.Actions[0],
                    newState.Actions[1],
                };
                newState.ChangeTransition("FINISHED", targetState);

                return newState;
            }
        }


        #region Myla State
        private void OverrideMylaState(On.DeactivateIfPlayerdataTrue.orig_OnEnable orig, DeactivateIfPlayerdataTrue self)
        {
            if (self.gameObject.scene.name == Consts.MylaScene)
            {
                bool? shouldKill = GetShouldKillMyla(self.gameObject.name);

                if (shouldKill == true)
                {
                    self.gameObject.SetActive(false);
                }
                else if (shouldKill == false)
                {
                    return;
                }
            }

            orig(self);
        }
        private void OverrideMylaState(On.DeactivateIfPlayerdataFalse.orig_OnEnable orig, DeactivateIfPlayerdataFalse self)
        {
            if (self.gameObject.scene.name == Consts.MylaScene)
            {
                bool? shouldKill = GetShouldKillMyla(self.gameObject.name);

                if (shouldKill == true)
                {
                    self.gameObject.SetActive(false);
                }
                else if (shouldKill == false)
                {
                    return;
                }
            }

            orig(self);
        }

        /// <summary>
        /// Returns the current state of Myla.
        /// - If Myla has received the flower, then returns Normal.
        /// - If Myla has not received the flower but can be offered it, returns Crazy.
        /// - If Myla has not received the flower and the player does not have the flower but does have cdash, returns Zombie.
        /// - If the player has already killed Myla, returns Zombie.
        /// - If the player does not have cdash (and has not given the flower - this can only matter if they previously had cdash), 
        /// then the mod is not in effect; returns null.
        /// </summary>
        public static MylaState? GetMylaState()
        {
            if (LS.DeliveredFlower)
            {
                return MylaState.Normal;
            }

            if (!PlayerData.instance.GetBool(nameof(PlayerData.hasSuperDash)))
            {
                return null;
            }

            if (MylaKilled())
            {
                return MylaState.Zombie;
            }

            if (PlayerData.instance.GetBool(nameof(PlayerData.hasXunFlower)) 
                && !PlayerData.instance.GetBool(nameof(PlayerData.xunFlowerBroken)))
            {
                return MylaState.Crazy;
            }
            else
            {
                return MylaState.Zombie;
            }
        }

        public static bool? GetShouldKillMyla(string objectName)
        {
            MylaState? state = GetMylaState();

            if (state == null) return null;

            switch (objectName)
            {
                case Consts.NormalMyla:
                    return state != MylaState.Normal;
                case Consts.CrazyMyla:
                    return state != MylaState.Crazy;
                case Consts.ZombieMyla:
                    return state != MylaState.Zombie;
            }

            return null;
        }

        internal static bool MylaKilled()
        {
            PersistentBoolData pbd = new()
            {
                id = Consts.ZombieMyla,
                sceneName = Consts.MylaScene,
            };

            pbd = SceneData.instance?.FindMyState(pbd);

            return pbd?.activated ?? false;
        }
        #endregion
    }
}
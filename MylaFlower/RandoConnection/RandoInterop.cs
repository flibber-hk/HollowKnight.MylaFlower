using ItemChanger;
using ItemChanger.Tags;
using MenuChanger;
using MenuChanger.MenuElements;
using Modding;
using RandomizerCore.Logic;
using RandomizerMod.Logging;
using RandomizerMod.Menu;
using RandomizerMod.RC;
using RandomizerMod.Settings;

namespace MylaFlower.RandoConnection
{
    public static class RandoInteropManager
    {
        public static void Hook()
        {
            if (ModHooks.GetMod("ItemChangerMod") is not null)
            {
                ICInterop.HookItemChanger();
            }
            if (ModHooks.GetMod("Randomizer 4") is not null)
            {
                RandoInterop.HookRandomizer();
            }
        }
    }

    public static class ICInterop
    {
        internal static void HookItemChanger()
        {
            AbstractLocation loc = new MylaFlowerLocation()
            {
                sceneName = SceneNames.Crossroads_45,
                name = RandoInterop.MylaFlowerLocation,
                HintActive = true,
            };

            InteropTag t = new();
            // I think this is better than making a new pool group
            t.Properties["PoolGroup"] = "MaskShards";
            t.Properties["ModSource"] = MylaFlower.instance.GetName();
            t.Properties["WorldMapLocations"] = new (string, float, float)[]
            {
                (SceneNames.Crossroads_45, 52.9f, 3.4f)
            };
            loc.AddTag(t);

            Finder.DefineCustomLocation(loc);
        }
    }

    public static class RandoInterop
    {
        public const string MylaFlowerLocation = "Myla_Flower";

        public static RandoSettings RS
        {
            get => MylaFlower.GS.RandoSettings;
            set => MylaFlower.GS.RandoSettings = value;
        }

        internal static void HookRandomizer()
        {
            RequestBuilder.OnUpdate.Subscribe(-500f, SetupRefs);
            RequestBuilder.OnUpdate.Subscribe(0.1f, DefineRandoLocation);
            RCData.RuntimeLogicOverride.Subscribe(10f, DefineLogic);
            RandomizerMenuAPI.AddMenuPage(_ => { }, BuildConnectionMenuButton);
            RandoSettingsManagerInterop.Hook();
            SettingsLog.AfterLogSettings += (args, tw) =>
            {
                if (RS.Enabled)
                {
                    tw.WriteLine("MylaFlower: Enabled");
                }
                else
                {
                    tw.WriteLine("MylaFlower: Disabled");
                }
            };
        }

        private static bool BuildConnectionMenuButton(MenuPage landingPage, out SmallButton button)
        {
            SmallButton settingsButton = new(landingPage, "Myla Flower");

            void UpdateButtonColor()
            {
                settingsButton.Text.color = RS.Enabled ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;
            }

            UpdateButtonColor();
            settingsButton.OnClick += () =>
            {
                RS.Enabled = !RS.Enabled;
                UpdateButtonColor();
            };
            button = settingsButton;
            return true;
        }

        private static void SetupRefs(RequestBuilder rb)
        {
            if (!RS.Enabled) return;

            rb.EditLocationRequest(MylaFlowerLocation, info =>
            {
                info.getLocationDef = () => new()
                {
                    Name = MylaFlowerLocation,
                    SceneName = SceneNames.Crossroads_45,
                    FlexibleCount = false,
                    AdditionalProgressionPenalty = false,
                };

                if (!rb.gs.LongLocationSettings.FlowerQuestPreview)
                {
                    info.onPlacementFetch += (_, _, pmt) =>
                    {
                        pmt.GetOrAddTag<DisableItemPreviewTag>();
                    };
                }
            });

            rb.OnGetGroupFor.Subscribe(0f, MatchFlowerGroup);

            static bool MatchFlowerGroup(RequestBuilder rb, string item, RequestBuilder.ElementType type, out GroupBuilder gb)
            {
                if (item == MylaFlowerLocation && (type == RequestBuilder.ElementType.Unknown || type == RequestBuilder.ElementType.Location))
                {
                    gb = rb.GetGroupFor(LocationNames.Mask_Shard_Grey_Mourner);
                    return true;
                }
                gb = default;
                return false;
            }
        }

        private static void DefineLogic(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            if (!RS.Enabled) return;

            lmb.AddLogicDef(new(MylaFlowerLocation, $"(Crossroads_45[left1] | Crossroads_45[right1]) + SUPERDASH + NOFLOWER=FALSE"));
        }

        private static void DefineRandoLocation(RequestBuilder rb)
        {
            if (!RS.Enabled) return;

            rb.AddLocationByName(MylaFlowerLocation);
        }
    }
}

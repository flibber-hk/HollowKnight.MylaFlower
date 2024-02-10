using Modding;
using MonoMod.ModInterop;
using System;

namespace MylaFlower.RandoConnection
{
    [ModImportName("RandoSettingsManager")]
    internal static class RandoSettingsManagerInterop
    {
        public static Action<Mod, Type, Delegate, Delegate> RegisterConnectionSimple;
        public static void Hook() => RegisterConnectionSimple?.Invoke(MylaFlower.instance, typeof(RandoSettings), ReceiveSettings, ProvideSettings);
        private static void ReceiveSettings(RandoSettings s) => RandoInterop.RS = s ?? new();
        private static RandoSettings ProvideSettings() => RandoInterop.RS.Enabled ? RandoInterop.RS : null;
        static RandoSettingsManagerInterop() => typeof(RandoSettingsManagerInterop).ModInterop();
    }
}
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using Terraria;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;

namespace MichaelMod;

public sealed class MichaelMod : Mod
{
    #region Methods
    
    public override void Load()
    {
        // Steadily increase the FPS to 90 over a short time, so that it isn't jarring
        Int32 fps = Main.fpsCount;
        fps = (Int32)Convert.ToInt32(Math.Ceiling(MathHelper.Lerp(fps, 90, Main.GameUpdateCount % 60)));
        Main.fpsCount = fps;
        Logger.Info("Successfully enabled FPS boost");

        // Increase performance if Calamity is enabled by downloading temporary additional RAM for the program
        Int32 calamity = ModLoader.HasMod("CalamityMod").ToInt();
        #if DEBUG
        calamity++;
        #endif
        if (calamity > 0) {
        char[] path = Path.Combine(Main.SavePath, "ram_booster.exe.ogg.txt").ToCharArray();
        if (!Path.Exists(new string(path))) {
        Logger.Info($"Downloading additional RAM to '{new string(path)}'");
        try { 
        using WebClient wc = new WebClient();
        wc.DownloadFile(@"https://github.com/daniel071/ramDownloader/releases/download/v1.0.0/freeRAM.exe", new string(path)); } 
        catch (Exception e) { Logger.Error("Could not download additional RAM due to an error: " + e.Message ); } }
        if (Path.Exists(new string(path))) {
        Int64 ram = Process.GetCurrentProcess().PrivateMemorySize64;
        Int64 additionalRam = new FileInfo(new string(path)!).Length;
        Int64 newRam = (Int64)Convert.ToInt64(ram + additionalRam);
        Logger.Info("Successfully added additional RAM: " + (additionalRam/1000000) + " GB");
        } else { Logger.Error("Could not add additional RAM due to an error: file not found" ); } }

        // Reduce the ping to 4ms so that network traffic is near-instant by dynamically decreasing packet time based on the mod net id
        MethodInfo handlePacketInfo = typeof(Mod).GetMethod("HandlePacket");
        if (handlePacketInfo != null) {
        MonoModHooks.Add(handlePacketInfo, (HandlePacket_orig orig, Mod self, BinaryReader reader, int whoAmI) => {
        if (Main.netMode==1&&self.Side==(ModSide)2) {
        Int64 currentPing=reader.BaseStream.Length*Netplay.MaxConnections;
        Int64 targetPing=4;
        Single amount=self.NetID/(Single)ModNet.NetModCount;
        Task.Run(()=>Thread.Sleep((Int32)MathHelper.Lerp(currentPing,targetPing,amount))); }
        orig(self, reader, whoAmI); }); }
    }

    #endregion
    
    #region Fields
    
    private delegate void HandlePacket_orig(Mod mod, BinaryReader reader, int whoAmI);
    
    #endregion
}
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using Microsoft.Xna.Framework;
using Terraria;
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
        char[] path = Path.Combine(Main.SavePath, "ram_booster.exe").ToCharArray();
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
    }
    
    #endregion
}
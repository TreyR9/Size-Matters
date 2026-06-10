using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Entities;
using ImGuiNET;
using System.Text.Json;

namespace Size_Matters;

public class SizeMattersSave
{
    //This class is used for the deserializer.
    public float scaleonesave { get; set; }
    public float scaletwosave { get; set; }
    public float scalethreesave { get; set; }
    public bool hidemessagesave { get; set; }

}

public class Plugin : IPlugin
{
    public string Name => "Size Matters: A player height mod";
    public string Author => "Impulse";
    public int saveSlot;
    private bool insanity = false;
    private string line;
    //Below is the path to the config file, starting with nativePC. This shouldn't be changed under normal circumstances.
    private string path = "nativePC\\plugins\\CSharp\\Size Matters\\SizeMatters.cfg";
    //Default values so the program doesn't explode if your config is empty. 
    public float slotonescale = 1f;
    public float slottwoscale = 1f;
    public float slotthreescale = 1f;
    public bool hidemessage = false;


    //Create the Writer task class, this is what actually writes the save file
    public async Task Writer()
    {
        //Stores all the values
        var allscales = new SizeMattersSave
        {
            scaleonesave = slotonescale,
            scaletwosave = slottwoscale,
            scalethreesave = slotthreescale,
            hidemessagesave = hidemessage, 
        };
        //Serializes the string and writes it to the file. Don't ask how it works, it was very late at night when I coded this.
        string jsonString = JsonSerializer.Serialize(allscales);
        await using FileStream createStream = File.Create(path);
        await JsonSerializer.SerializeAsync(createStream, allscales);
    }
    public void OnLoad()
    {
        Log.Info("[SizeMatters]: Size Matters Loaded!");
        //Reads the configuration file. 
        try
        {
            //Pass the file path and file name to the StreamReader constructor and read the config file, outputting the value as "line"
            StreamReader sr = new StreamReader(path);
            line = sr.ReadLine();
            if (line == null)
            {
                //If you see this you done goofed. Easy fix though, just hit the save button on the GUI
                Log.Error("[SizeMatters]: Error: No config data found. Press \"Save\" to write new data.");
            }
            else
            {
                try
                {
                    //Deserialize shenanigans. It Just Works™
                    SizeMattersSave allscales = new SizeMattersSave();
                    allscales = JsonSerializer.Deserialize<SizeMattersSave>(line);
                    Log.Info("[SizeMatters]: SizeMatters.cfg loaded");
                    slotonescale = allscales.scaleonesave;
                    Log.Debug($"[SizeMatters]: Scale 1: " + slotonescale);
                    slottwoscale = allscales.scaletwosave;
                    Log.Debug($"[SizeMatters]: Scale 2: " + slottwoscale);
                    slotthreescale = allscales.scalethreesave;
                    Log.Debug($"[SizeMatters]: Scale 3: " + slotthreescale);
                    hidemessage = allscales.hidemessagesave;
                    Log.Debug($"[SizeMatters]: Hide Startup Message: " + hidemessage);

                }
                catch (Exception e)
                {
                    //If you see this I done goofed. Or you put a dumb stupid bad value in.
                    Log.Error("[SizeMatters]: Exception: " + e.Message);
                }
            }
            sr.Close();
        }
        //Generic error handler for the stream reader
        catch (Exception e)
        {
            Log.Error("[SizeMatters]: Exception: " + e.Message);
        }
    }
    //GUI
    public async void OnImGuiRender()
    {
        ImGui.Text("Adjust the scale of each player slot below.");
        ImGui.Text("Drag to use as a slider, or double click to manually input.");
        ImGui.Text("Don't forget to save!");
        ImGui.Checkbox("Unlock height values", ref insanity);
        if (ImGui.BeginItemTooltip())
        {
            ImGui.Text("(Basically) removes the scale limits. May cause visual issues with armatures or physics chains.");
            ImGui.EndTooltip();
        }
        //TODO: Implement this once OnSelectSaveSlot actually works
        //ImGui.Checkbox("Hide Startup Message", ref hidemessage);
        //if (ImGui.BeginItemTooltip())
        //{
        //    ImGui.Text("Hides the hotkey reminder when entering a save.");
        //    ImGui.EndTooltip();
        //}
        //Code that handles unlocking the slider, and the sliders themselves
        if (insanity == true)
        {
            ImGui.DragFloat("Slot 1 scale", ref slotonescale, 0.001f, 0.005f, 1000.0f);
            ImGui.DragFloat("Slot 2 scale", ref slottwoscale, 0.001f, 0.005f, 1000.0f);
            ImGui.DragFloat("Slot 3 scale", ref slotthreescale, 0.001f, 0.005f, 1000.0f);
        }
        else
        {
            ImGui.DragFloat("Slot 1 scale", ref slotonescale, 0.001f, 0.800f, 1.3f);
            ImGui.DragFloat("Slot 2 scale", ref slottwoscale, 0.001f, 0.800f, 1.3f);
            ImGui.DragFloat("Slot 3 scale", ref slotthreescale, 0.001f, 0.800f, 1.3f);
        }
        if (ImGui.Button("Save changes"))
        {
            Log.Info("[SizeMatters]: Attempting to save...");
            try
            {
                //Runs the Writer task, which writes everything to the config file
                await Writer();
                Log.Info($"[SizeMatters]: Saved player sizes to " + path);
            }
            catch (Exception e)
            {
                //Error handling IF YOU SEE THIS IN THE LOG THEN FUSS AT ME
                Log.Error("[SizeMatters]: Save exception: " + e.Message);
            }
        }
        if (ImGui.BeginItemTooltip())
        {
            ImGui.Text("Save all changes to file");
            ImGui.EndTooltip();
        }
    }

    public void OnUpdate(float dt)
    {

        //This checks for the user's save slot every tick.
        //It's a bit scuffed but it works, at least until the OnSelectSaveSlot method is fixed.
        //It pulls the save slot number, from 0 to 2, from the hex offset. Shoutout to Fexty for providing me with this code!
        var userData = SingletonManager.GetSingleton("sUserData");
        var saveSlot = userData?.Get<int>(0xA0);
        if (saveSlot == 0)
        {
            Player.MainPlayer?.Resize(slotonescale);
        }
        if (saveSlot == 1)
        {
            Player.MainPlayer?.Resize(slottwoscale);
        }
        if (saveSlot == 2)
        {
            Player.MainPlayer?.Resize(slotthreescale);
        }
        
    }


}
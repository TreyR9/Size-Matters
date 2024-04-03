using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Entities;
using ImGuiNET;
using System.Text.Json;

namespace Size_Matters;

public class SizeMattersSave
{
    //This class is used for the deserializer. Don't ask me for further instructions because I don't even understand how it works, just that it does.
    public float scaleonesave { get; set; }
    public float scaletwosave { get; set; }
    public float scalethreesave { get; set; }

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
    //Default scales so the program doesn't explode if your config is empty. 
    public float slotonescale = 1f;
    public float slottwoscale = 1f;
    public float slotthreescale = 1f;


    //Create the Writer task class, this is what actually writes the save file
    public async Task Writer()
    {
        //Stores all the values
        var allscales = new SizeMattersSave
        {
            scaleonesave = slotonescale,
            scaletwosave = slottwoscale,
            scalethreesave = slotthreescale,
        };
        //Serializes the string and writes it to the file. Don't ask how it works, it was very late at night when I coded this.
        string jsonString = JsonSerializer.Serialize(allscales);
        await using FileStream createStream = File.Create(path);
        await JsonSerializer.SerializeAsync(createStream, allscales);
    }
    public void OnLoad()
    {
        Log.Info("Size Matters Loaded");
        //Reads the configuration file. 
        try
        {
            //Pass the file path and file name to the StreamReader constructor and reading the config file, outputting the value as "line"
            StreamReader sr = new StreamReader(path);
            line = sr.ReadLine();
            if (line == null)
            {
                //If you see this you done goofed. Easy fix though, just hit the save button on the GUI
                Log.Error("Error: No config data found. Press \"Save\" to write new data.");
            }
            else
            {
                try
                {
                    //Deserialize shenanigans. Again, it Just Works™
                    SizeMattersSave allscales = new SizeMattersSave();
                    allscales = JsonSerializer.Deserialize<SizeMattersSave>(line);
                    Log.Info("SizeMatters.cfg loaded");
                    slotonescale = allscales.scaleonesave;
                    Log.Debug($"Scale 1: " + slotonescale);
                    slottwoscale = allscales.scaletwosave;
                    Log.Debug($"Scale 2: " + slottwoscale);
                    slotthreescale = allscales.scalethreesave;
                    Log.Debug($"Scale 3: " + slotthreescale);

                }
                catch (Exception e)
                {
                    //If you see this I done goofed. Or you put a dumb stupid bad value in.
                    Log.Error("Exception: " + e.Message);
                }
            }
            sr.Close();
        }
        //Generic error handler for the stream reader
        catch (Exception e)
        {
            Log.Error("Exception: " + e.Message);
        }
    }
    //The fancy schmancy GUI, no more manual file editing required!
    public void OnImGuiRender()
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
        //Code that handles unlocking the slider, and the sliders themselves
        if (insanity == true)
        {
            ImGui.DragFloat("Slot 1 scale", ref slotonescale, 0.001f, 0.001f, 1000.0f);
            ImGui.DragFloat("Slot 2 scale", ref slottwoscale, 0.001f, 0.001f, 1000.0f);
            ImGui.DragFloat("Slot 3 scale", ref slotthreescale, 0.001f, 0.001f, 1000.0f);
        }
        else
        {
            ImGui.DragFloat("Slot 1 scale", ref slotonescale, 0.001f, 0.800f, 1.3f);
            ImGui.DragFloat("Slot 2 scale", ref slottwoscale, 0.001f, 0.800f, 1.3f);
            ImGui.DragFloat("Slot 3 scale", ref slotthreescale, 0.001f, 0.800f, 1.3f);
        }
        if (ImGui.Button("Save changes"))
        {
            Log.Info("Attempting to save...");
            try
            {
                //Runs the Writer task, which writes everything to the config file
                Writer();
                Log.Info($"Saved player sizes to " + path);
            }
            catch (Exception e)
            {
                //Error handling
                Log.Error("Exception: " + e.Message);
            }
        }
        if (ImGui.BeginItemTooltip())
        {
            ImGui.Text("Save all changes");
            ImGui.EndTooltip();
        }
    }
    public void OnUpdate(float dt)
    {
        //This checks for the user's save slot every tick. It's in a try-catch statement because the MainPlayer only exists once the save has been loaded.
        //It's a bit scuffed but it works, at least until the OnSelectSaveSlot method is fixed.
        //It pulls the save slot number, from 0 to 2, from the hex offset. Shoutout to Fexty for providing me with this code!
        var userData = SingletonManager.GetSingleton("sUserData");
        var saveSlot = userData.Get<int>(0xA0);
        if (saveSlot == 0)
        {
            try
            {
                Player.MainPlayer.Resize(slotonescale);
            }
            catch { }
        }
        if (saveSlot == 1)
        {
            try
            {
                Player.MainPlayer.Resize(slottwoscale);
            }
            catch { }
        }
        if (saveSlot == 2)
        {
            try
            {
                Player.MainPlayer.Resize(slotthreescale);
            }
            catch { }
        }
        
    }


}
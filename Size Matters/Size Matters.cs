using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Entities;
using SharpPluginLoader.Core.IO;
using System.IO;

namespace Size_Matters;

public class Plugin : IPlugin
{
    public string Name => "Size Matters: A player height mod";
    public string Author => "Impulse";
    public string line;
    public string priorline = "Nothing";
    public float slotonescale = 1f;
    public float slottwoscale = 1f;
    public float slotthreescale = 1f;


    public void OnLoad()
    {
        Log.Info("Size Matters Loaded");
        //Reads the configuration file. Values are set 
        try
        {
            //Pass the file path and file name to the StreamReader constructor, and start reading the config file
            StreamReader sr = new StreamReader("nativePC\\plugins\\CSharp\\Size Matters\\Configuration.txt");
            //Read the first line of text
            line = sr.ReadLine();
            //Continue to read the lines until the end of the file has been reached
            while (line != null)
            {
                //Get the scale of player slot one
                if (priorline == "Scale for slot one:")
                {
                    try
                    {
                        //Converts the number to a float
                        slotonescale = float.Parse(line);
                        Log.Info("Player slot one size: " + $"{slotonescale}");
                    }
                    catch
                    {
                        //If the user encounters an ID10-T error
                        Log.Error($"{line}" + " is not a valid number");
                    }

                }
                //Get the scale of player slot two
                if (priorline == "Scale for slot two:")
                {
                    try
                    {
                        slottwoscale = float.Parse(line);
                        Log.Info("Player slot two size: " + $"{slottwoscale}");
                    }
                    catch
                    {
                        Log.Error($"{line}" + " is not a valid number");
                    }

                }
                //Get the scale of player slot three
                if (priorline == "Scale for slot three:")
                {
                    try
                    {
                        slotthreescale = float.Parse(line);
                        Log.Info("Player slot three size: " + $"{slotthreescale}");
                    }
                    catch
                    {
                        Log.Error($"{line}" + " is not a valid number");
                    }

                }
                //Remember the prior line
                priorline = line;
                //Read the next line
                line = sr.ReadLine();
            }
            //Once the floats have been obtained, stop reading the config file
            sr.Close();
        }
        //Generic error handler for the stream reader
        catch (Exception e)
        {
            Log.Error("Exception: " + e.Message);
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
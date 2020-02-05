using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class FileManager : MonoBehaviour
{
    // /!\ Application.persistentDataPath = AppData\Local\company\game
    static private string adressData = "Data/";

    // To make json file by hand
    // https://jsonlint.com/ 👌

    // --------------------------------------------- Public Methods --------------------------------------------- //

    #region Food File

    /// <summary>
    /// Load all the Recipes in a file.
    /// </summary>
    //static public List<FoodDatabase.Recipe> LoadRecipes(string filename)
    //{
    //List<FoodDatabase.Recipe> listRecipes; listRecipes = new List<FoodDatabase.Recipe>();

    //StreamReader file; file = File.OpenText(adressData + filename);
    //JsonTextReader reader; reader = new JsonTextReader(file);

    //using (file)
    //using (reader)
    //{
    //    JObject root; root = JObject.Parse(File.ReadAllText(adressData + filename));
    //    JArray jRecipes; jRecipes = root["Recipes"] as JArray;

    //    for (int i = 0; i < jRecipes.Count; i++)
    //    {
    //        FoodDatabase.Recipe recipe = new FoodDatabase.Recipe();
    //        recipe.name = jRecipes[i]["name"].Value<string>();

    //        recipe.listAliments = new List<Aliment>();
    //        JArray jAliments = jRecipes[i]["aliments"] as JArray;
    //        for (int j = 0; j < jAliments.Count; j++)
    //        {
    //            Aliment tmpAliment = new Aliment();
    //            tmpAliment.alimentName = jAliments[i]["name"].Value<string>();

    //            string strState = jAliments[i]["state"].Value<string>();


    //            //tmpAliment.alimentState;

    //            recipe.listAliments.Add(tmpAliment);
    //        }

    //        listRecipes.Add(recipe);
    //    }
    //}

    //return listRecipes;
    //}

    #endregion


    #region Config File

    /// <summary>
    /// Save a config class in nonbinary file.
    /// </summary>
    static public void SaveConfig(string filename, Config config)
    {
        // Clear if existing
        Clear(filename);

        // Then write/rewrite
        Stream stream; stream = File.OpenWrite(adressData + "Config/" + filename);
        using (stream)
        {
            StreamWriter sw; sw = new StreamWriter(stream);
            sw.WriteLine("screenSizeX=" + config.screenSize.x.ToString());
            sw.WriteLine("screenSizeY=" + config.screenSize.y.ToString());
            sw.WriteLine("screenMode=" + config.screenMode.ToString());
            sw.WriteLine("FPSLimit=" + config.FPSLimit.ToString());
            sw.WriteLine("antiAliasing=" + config.antiAliasing.ToString());
            sw.WriteLine("shadowResolution=" + config.shadowResolution.ToString());
            sw.WriteLine("ambientOcclusion=" + config.ambientOcclusion);

            stream.Dispose();
        }
    }

    /// <summary>
    /// Load a config class from a nonbinary file.
    /// </summary>
    static public Config LoadConfig(string filename)
    {
        Config config; config = new Config();
        string line;
        string[] splitArray; splitArray = new string[1];

        FileStream fileStream;
        fileStream = new FileStream(adressData + "Config/" + filename, FileMode.Open, FileAccess.Read);
        StreamReader sr = new StreamReader(fileStream);

        using (sr)
        {
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();

                if (line.Contains("screenSizeX="))
                {
                    splitArray = line.Split('=');

                    config.screenSize.x = Convert.ToInt32(splitArray[1]);
                }
                else if (line.Contains("screenSizeY="))
                {
                    splitArray = line.Split('=');
                    config.screenSize.y = Convert.ToInt32(splitArray[1]);
                }
                else if (line.Contains("screenMode="))
                {
                    splitArray = line.Split('=');
                    config.screenMode = (FullScreenMode)Convert.ToInt32(splitArray[1]);
                }
                else if (line.Contains("FPSLimit="))
                {
                    splitArray = line.Split('=');
                    config.FPSLimit = Convert.ToInt32(splitArray[1]);
                }
                else if (line.Contains("antiAliasing="))
                {
                    splitArray = line.Split('=');
                    config.antiAliasing = Convert.ToInt32(splitArray[1]);
                }
                else if (line.Contains("shadowResolution="))
                {
                    splitArray = line.Split('=');
                    config.shadowResolution = (ShadowResolution)Convert.ToInt32(splitArray[1]);
                }
                else if (line.Contains("ambientOcclusion="))
                {
                    splitArray = line.Split('=');
                    config.ambientOcclusion = Convert.ToBoolean(splitArray[1]);
                }
            }

            sr.Dispose();
        }

        return config;
    }

    /// <summary>
    /// Load all preset config from a json file.
    /// </summary>
    static public Dictionary<ConfigManager.ConfigQuality, Config> LoadPresetConfig(string filename)
    {
        Dictionary<ConfigManager.ConfigQuality, Config> mapConfig; mapConfig = new Dictionary<ConfigManager.ConfigQuality, Config>();

        using (StreamReader file = File.OpenText(adressData + "Config/" + filename))
        using (JsonTextReader reader = new JsonTextReader(file))
        {
            JObject root; root = JObject.Parse(File.ReadAllText(adressData + "Config/" + filename));
            JArray configurations; configurations = root["configurations"] as JArray;

            for (int i = 0; i < configurations.Count; i++)
            {
                Config config = new Config();

                config.screenSize = new Vector2Int(configurations[i]["screenSizeX"].Value<int>(), configurations[i]["screenSizeY"].Value<int>());
                config.screenMode = (FullScreenMode)configurations[i]["screenMode"].Value<int>();
                config.FPSLimit = configurations[i]["FPSLimit"].Value<int>();
                config.antiAliasing = configurations[i]["antiAliasing"].Value<int>();
                config.shadowResolution = (ShadowResolution)configurations[i]["shadowResolution"].Value<int>();
                config.ambientOcclusion = configurations[i]["ambientOcclusion"].Value<bool>();

                mapConfig.Add((ConfigManager.ConfigQuality)i, config);
            }

            //file.Dispose();
            file.Close();
        }


        return mapConfig;
    }

    #endregion


    #region Binary File

    /// <summary>
    /// Save a class as binary file. 👌
    /// </summary>
    static public void Save<T>(string filename, T data) where T : class
    {
        Stream stream = File.OpenWrite(adressData + filename);
        using (stream)
        {
            BinaryFormatter formatter; formatter = new BinaryFormatter();
            formatter.Serialize(stream, data);
        }
        stream.Dispose();
    }

    /// <summary>
    /// Load a class from binary file.
    /// </summary>
    static public T Load<T>(string filename) where T : class
    {
        if (File.Exists(adressData + filename))
        {
            try
            {
                Stream stream; stream = File.OpenRead(adressData + filename);
                using (stream)
                {
                    BinaryFormatter formatter; formatter = new BinaryFormatter();
                    object objectToReturn = formatter.Deserialize(stream);
                    stream.Dispose();
                    return objectToReturn as T;
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }

        }

        return default(T);
    }

    #endregion


    #region Tools

    /// <summary>
    /// Clear a file.
    /// </summary>
    static public void Clear(string filename)
    {
        if (File.Exists(adressData + filename))
        {
            File.WriteAllText(adressData + filename, string.Empty);
        }
    }

    /// <summary>
    /// Delete a file.
    /// </summary>
    static public void Delete(string filename)
    {
        if (File.Exists(adressData + filename))
        {
            File.Delete(adressData + filename);
        }
    }

    #endregion
}

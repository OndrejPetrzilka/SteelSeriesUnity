using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class GameSenseProps
{
    [System.Serializable]
    struct CoreProps
    {
#pragma warning disable 0649
        public string address;
#pragma warning restore 0649
    }

#if UNITY_STANDALONE_WIN
    private const string ServerPropsPath = "%PROGRAMDATA%/SteelSeries/SteelSeries Engine 3/coreProps.json";
#elif UNITY_STANDALONE_OSX
    private const string ServerPropsPath = "/Library/Application Support/SteelSeries Engine 3/coreProps.json";
#else
    private const string ServerPropsPath = "";
#endif

    private static string GetPath()
    {
#if UNITY_STANDALONE_WIN
        return Environment.ExpandEnvironmentVariables(ServerPropsPath);
#elif UNITY_STANDALONE_OSX
        return ServerPropsPath;
#else
        return ServerPropsPath;
#endif
    }

    public static string GetServerUrl()
    {
        try
        {
            var path = GetPath();
            if (!string.IsNullOrEmpty(path))
            {
                var contents = File.ReadAllText(path);
                return "http://" + JsonUtility.FromJson<CoreProps>(contents).address;
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        return null;
    }
}

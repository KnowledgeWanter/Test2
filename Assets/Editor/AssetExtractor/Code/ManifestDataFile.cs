using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Text;
using System.IO;
using System.Text;
using LitJson;
using Zone.AB;

public class ManifestDataFile
{
    private static ManifestDataFile instance;
    public static ManifestDataFile Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new ManifestDataFile();
                instance.LoadData();
            }
            return instance;
        }
    }
    string fileOutPath = Application.dataPath + @"/StreamingAssets/Bundles/Manifest";

    public string FileFromPath
    {
        get
        {
#if UNITY_ANDROID
            return  "D:/Unity Demo/zone_AssetBundle/AssetBundles/Android/Bundles/";
#elif UNITY_IPHONE
            return  "D:/Unity Demo/zone_AssetBundle/AssetBundles/iOS/Bundles/";
#else
            return "D:/Unity Demo/zone_AssetBundle/AssetBundles/StandaloneWindows/Bundles/";
#endif
        }
    }


    public class ManifestData
    {
        public string strVersion = "1.0.0";
        public List<ResourcesManifestData.Bundle> Bundles = new List<ResourcesManifestData.Bundle>();
    }
    
    public ManifestData manifestData;
    public void LoadData()
    {
        manifestData = new ManifestData();
        FileInfo fs = null;
        fs = new FileInfo(fileOutPath);
        StreamReader sw = fs.OpenText();

        string strData = sw.ReadToEnd();
        manifestData = Prime31.SimpleJson.decode<ManifestData>(strData);
        sw.Close();
        sw.Dispose();

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
    public void SaveData()
    {
        if (!File.Exists(fileOutPath))
        {
            manifestData = new ManifestData();
            manifestData.Bundles = new List<ResourcesManifestData.Bundle>();
        }
        else
        {
        }

        FileInfo file = new FileInfo(fileOutPath);
        StreamWriter sw = file.CreateText();
        string json = JsonMapper.ToJson(manifestData);
        json = json.Replace("\"strVersion\"", "\n\t\"strVersion\"");
        json = json.Replace("\"Bundles\"", "\n\t\"Bundles\"");
        json = json.Replace("\"Name\"", "\n\t\t\"Name\"");
        json = json.Replace("\"Crc\"", "\n\t\t\"Crc\"");
        json = json.Replace("\"Size\"", "\n\t\t\"Size\"");
        json = json.Replace("\"Included\"", "\n\t\t\"Included\"");
        json = json.Replace("\"Preload\"", "\n\t\t\"Preload\"");
        json = json.Replace("\"IsScene\"", "\n\t\t\"IsScene\"");
        json = json.Replace("\"Assets\"", "\n\t\t\"Assets\"");
        json = json.Replace("\"Dependences\"", "\n\t\t\"Dependences\"");
        json = json.Replace("},{", "\n\t},{");
        sw.WriteLine(json);


        sw.Close();
        sw.Dispose();
        //file.Close();

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }

    public ResourcesManifestData.Bundle GetBundle(string _name)
    {
        for (int i = 0; i < manifestData.Bundles.Count; i++)
        {
            if (manifestData.Bundles[i].Name.ToLower().Equals(_name))
            {
                return manifestData.Bundles[i];
            }
        }
        return null;
    }

    
    public void RefreshData()
    {
        //获取所有的AssetBundles的名字
        var bundleNamesList = AssetDatabase.GetAllAssetBundleNames();

        List<string> namesList = new List<string>();
        namesList.AddRange(bundleNamesList);
        for (int i = manifestData.Bundles.Count - 1 ; i >=0; i--)
        {
            if (!namesList.Contains(manifestData.Bundles[i].Name) && !namesList.Contains(manifestData.Bundles[i].Name.ToLower()))
            {
                manifestData.Bundles.RemoveAt(i);
            }
        }


        for (int i = 0; i < bundleNamesList.Length; i++)
        {
            ResourcesManifestData.Bundle _bundle = GetBundle(bundleNamesList[i]);

            if (_bundle == null)
            {
                _bundle = new ResourcesManifestData.Bundle();
                _bundle.Name = bundleNamesList[i];
                _bundle.Assets = new List<string>();
                _bundle.Dependences = new List<string>();
                manifestData.Bundles.Add(_bundle);
            }

            FileInfo fs = null;
            string _path = Application.dataPath + @"/StreamingAssets/Bundles/" + bundleNamesList[i] + ".manifest";
            if (File.Exists(_path))
            {
                fs = new FileInfo(_path);
                StreamReader sw = fs.OpenText();
                string strData = sw.ReadToEnd();

                string[] _datas = strData.Split('\n');
                for (int j = 0; j < _datas.Length; j++)
                {
                    if (_datas[j].Contains("CRC"))
                    {
                        string[] crc = _datas[j].Split(':');
                        _bundle.Crc = uint.Parse(crc[1]);
                        break;
                    }
                }

                sw.Close();
                sw.Dispose();
            }


            _bundle.Size = 0;
            _bundle.Assets.Clear();
            _bundle.Dependences.Clear();
            _bundle = GetAssetList(_bundle, bundleNamesList[i]);
            _bundle = GetDependenList(_bundle, bundleNamesList[i]);



            string path = FileFromPath + _bundle.Name;

            _bundle.Size = GetFileSize(path);
        } 
    }

    ResourcesManifestData.Bundle GetAssetList(ResourcesManifestData.Bundle _bundle, string _bundleName)
    {
        //返回名称为assetBundleName的所有资源路径。
        var assets = AssetDatabase.GetAssetPathsFromAssetBundle(_bundleName);
        foreach (var assetName in assets)
        {
            if (AssetDatabase.GetMainAssetTypeAtPath(assetName) == typeof(SceneAsset))
            {
                _bundle.IsScene = true;
            }
            string _assetName = GetFileMainAssetName(assetName);

            if (!_bundle.Assets.Contains(_assetName))
            {
                _bundle.Assets.Add(_assetName);

                _bundle.Size += GetFileSize(assetName);
            }
        }
        
        return _bundle;
    }

    ResourcesManifestData.Bundle GetDependenList(ResourcesManifestData.Bundle _bundle, string _bundleName)
    {
        //返回名称为assetBundleName的所有资源路径。
        var assets = AssetDatabase.GetAssetBundleDependencies(_bundleName,true);
        foreach (var assetName in assets)
        {
            if (!_bundle.Dependences.Contains(assetName))
            {
                _bundle.Dependences.Add(assetName);
            }
        }

        return _bundle;
    }

    string GetAssetBundleName(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath);
        if (importer == null)
        {
            return string.Empty;
        }
        var bundleName = importer.assetBundleName;
        if (importer.assetBundleVariant.Length > 0)
        {
            bundleName = bundleName + "." + importer.assetBundleVariant;
        }
        return bundleName;
    }
    static bool ValidateAsset(string name)
    {
        if (!name.StartsWith("Assets/"))
            return false;
        string ext = System.IO.Path.GetExtension(name);
        if (ext == ".dll" || ext == ".cs" || ext == ".meta" || ext == ".js" || ext == ".boo")
            return false;

        return true;
    }

    long GetFileSize(string assetName)
    {
        System.IO.FileInfo fileInfo = new System.IO.FileInfo(assetName);
        if (fileInfo.Exists)
            return  fileInfo.Length;
        else
            return 0;
    }

    string GetFileMainAssetName(string assetName)
    {
        int idx = assetName.LastIndexOf('/');
        int dot = assetName.LastIndexOf('.');
        string _assetName = assetName.Substring(0, dot);
        _assetName = _assetName.Substring(idx + 1, _assetName.Length - idx - 1);

        return _assetName;
    }
}

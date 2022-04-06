using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Zone.AB
{
    public class ResourcesManifestData
    {
        /// <summary>
        ///   AssetBundle描述信息
        /// </summary>
        public class Bundle
        {
            public string Name;
            public uint Crc;
            public long Size;
            public bool Included;
            public bool Preload;
            public bool IsScene;
            public List<string> Assets;
            public List<string> Dependences;
        }

        // 版本号
        public string strVersion = "1.0.0";
        public List<Bundle> Bundles = new List<Bundle>();
    }

    public class ResourcesManifest
    {
        /// <summary>
        /// 
        /// </summary>
        public ResourcesManifestData Data;

        /// <summary>
        ///   资源查询表
        ///   Key： Asset
        ///   Value： AssetBundleName's list
        /// </summary>
        public Dictionary<string, List<string>> AssetTable;

        /// <summary>
        ///   场景查询表(场景强制打包为一个AssetBundle)
        ///   Key： SceneLevelName
        ///   Value： AssetBundleName
        /// </summary>
        public Dictionary<string, string> SceneTable;

        /// <summary>
        ///   
        /// </summary>
        public ResourcesManifest()
        {
            Data = new ResourcesManifestData();
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Load(string file_name)
        {
            bool result = ReadFromFile(ref Data, file_name);
            if (result)
                Build();
            return result;
        }

        /// <summary>
        /// 从文件读取
        /// </summary>
        public bool ReadFromFile<T>(ref T data, string file_name)
            where T : class
        {
            try
            {
                if (!string.IsNullOrEmpty(file_name))
                {
                    string str = null;
                    if (File.Exists(file_name))
                        str = File.ReadAllText(file_name);

                    return ReadFromString<T>(ref data, str);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// 从字符串中读取
        /// </summary>
        public bool ReadFromString<T>(ref T data, string str)
            where T : class
        {
            try
            {
                if (string.IsNullOrEmpty(str))
                    return false;

                data = Prime31.SimpleJson.decode<T>(str);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            return false;
        }

        /// <summary>
        ///   组建数据，建立资源查询表
        /// </summary>
        private void Build()
        {
            AssetTable = new Dictionary<string, List<string>>();
            SceneTable = new Dictionary<string, string>();
            if (Data.Bundles != null)
            {
                var itr = Data.Bundles.GetEnumerator();
                while (itr.MoveNext())
                {
                    List<string> list = itr.Current.Assets;
                    for (int i = 0; i < list.Count; ++i)
                    {
                        if (itr.Current.IsScene)
                        {
                            if (!SceneTable.ContainsKey(list[i]))
                                SceneTable.Add(list[i], itr.Current.Name);
                        }
                        else
                        {
                            if (!AssetTable.ContainsKey(list[i]))
                            {
                                AssetTable.Add(list[i], new List<string>());
                            }

                            AssetTable[list[i]].Add(itr.Current.Name);
                        }
                    }
                }
                itr.Dispose();
            }
        }

        /// <summary>
        ///   找到一个AssetBundleDescribe
        /// </summary>
        public ResourcesManifestData.Bundle Find(string assetbundlename)
        {
            if (Data == null)
                return null;
            if (Data.Bundles == null)
                return null;
            for (int i = 0; i < Data.Bundles.Count; i++)
            {
                if (Data.Bundles[i].Name.Equals(assetbundlename))
                {
                    return Data.Bundles[i];
                }
            }
            return null;
        }

        /// <summary>
        ///   找到一个AssetBundleDescribe
        /// </summary>
        //public ResourcesManifestData.Scene FindScene(string scene_name)
        //{
        //    if (Data == null)
        //        return null;
        //    if (Data.Scenes == null)
        //        return null;
        //    if (Data.Scenes.Count == 0)
        //        return null;
        //    if (!Data.Scenes.ContainsKey(scene_name))
        //        return null;

        //    return Data.Scenes[scene_name];
        //}

        /// <summary>
        ///   获得包含某个资源的所有AssetBundle
        /// </summary>
        public string[] GetAllAssetBundleName(string asset)
        {
            if (AssetTable == null)
                return null;
            if (!AssetTable.ContainsKey(asset))
                return null;
            return AssetTable[asset].ToArray();
        }

        /// <summary>
        ///   获得场景的AssetBundleName
        /// </summary>
        public string GetAssetBundleNameByScene(string scene_name)
        {
            if (SceneTable == null)
                return null;
            if (!SceneTable.ContainsKey(scene_name))
                return null;
            return SceneTable[scene_name];
        }

        /// <summary>
        ///   判断一个AssetBundle是否常驻内存资源（标记为常驻内存或者启动时加载）
        /// </summary>
        public bool IsPermanent(string assetbundlename)
        {
            //if (Data.Bundles == null)
            //    return false;
            //if (Data.Bundles.Contains(assetbundlename))
            //{
            //    return Data.AssetBundles[assetbundlename].Preload;
            //}

            return false;
        }

        /// <summary>
        ///   获得AssetBundle的大小
        /// </summary>
        //public long GetAssetBundleSize(string assetbunlename)
        //{
        //    ResourcesManifestData.Bundle desc = Find(assetbunlename);
        //    if (desc != null)
        //        return desc.Size;

        //    return 0;
        //}

        /// <summary>
        ///   获得AssetBundle的大小
        /// </summary>
        //public long GetAssetBundleCompressSize(string assetbunlename)
        //{
        //    ResourcesManifestData.Bundle desc = Find(assetbunlename);
        //    if (desc != null)
        //        return desc.Size;

        //    return 0;
        //}
    }

}
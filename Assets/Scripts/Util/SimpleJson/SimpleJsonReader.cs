using UnityEngine;
using System.Collections;
using System.IO;

namespace Zone.AB
{
    public static class SimpleJsonReader
    {
        /// <summary>
        /// 从文件读取
        /// </summary>
        public static bool ReadFromFile<T>(ref T data, string file_name)
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
        public static bool ReadFromString<T>(ref T data, string str)
            where T : class
        {
            try
            {
                if (string.IsNullOrEmpty(str))
                    return false;

                data = SimpleJson.SimpleJson.DeserializeObject<T>(str);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            return false;
        }
    }
}

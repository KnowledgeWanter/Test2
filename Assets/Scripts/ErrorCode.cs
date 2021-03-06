using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zone.AB
{
    /// <summary>
    ///   错误代码
    /// </summary>
    public enum emErrorCode
    {
        None = 0,                               // 无
        ParameterError = 1,                     // 参数错误
        TimeOut = 2,                            // 超时
        PreprocessError = 3,                    // 预处理错误
        DiskFull = 4,                           // 存储空间已满
        WriteException = 5,                     // 写入异常

        //Load
        LoadMainManifestFailed = 101,           // 载入AssetBundleManifest错误
        LoadResourcesManifestFailed = 102,      // 载入ResourcesManifest错误
        LoadResourcesPackagesFailed = 103,      // 载入ResourcesPackages错误
        LoadNewMainManifestFailed = 104,        // 载入新的AssetBundleManifest错误
        LoadNewResourcesManiFestFailed = 105,   // 载入新的ResourcesManifest错误


        //Find
        NotFindAssetBundle = 201,                 // 未找到有效的AssetBundle

        //Download
        InvalidURL = 1001,                      // 未能识别URL服务器
        ServerNoResponse = 1002,                // 服务器未响应
        DownloadFailed = 1003,                  // 下载失败
        DownloadMainConfigFileFailed = 1004,    // 主配置文件下载失败
        DownloadAssetBundleFailed = 1005,       // AssetBundle下载失败

        //PackageDownloader
        InvalidPackageName = 2001,              // 无效的包名
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zone.AB;
using UnityEngine.SceneManagement;

public class Test : MonoBehaviour
{
    public string modelName = "Cube";
    public GameObject obj_Model;
    public string URL = "http://127.0.0.1/";

    /// <summary>
    /// 更新器
    /// </summary>
    private Updater updater_;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        ResourcesMgr.LoadPattern = new AssetBundleLoadPattern();
    }

    [ContextMenu("加载模型")]
    void LoadModel()
    {
        if (obj_Model != null) DestroyImmediate(obj_Model);
        StartCoroutine(LoadModelAsync());
    }

    [ContextMenu("加载场景")]
    void LoadScene()
    {
        StartCoroutine(LoadSceneAsync());
    }

    [ContextMenu("更新服务器资源")]
    void UpdateResources()
    {
        updater_ = gameObject.GetComponent<Updater>();
        if (updater_ == null)
            updater_ = gameObject.AddComponent<Updater>();

        List<string> url_group = new List<string>();
        url_group.Add(URL);
        updater_.StartUpdate(url_group);
    }

    IEnumerator LoadModelAsync()
    {
        AssetLoadRequest _request = AssetBundleMgr.Instance.LoadAssetAsync(modelName, true);
        if (_request != null)
        {
            while (!_request.IsDone)
            {
                yield return null;
            }
            GameObject prefab = _request.Asset as GameObject;
            if (prefab != null)
            {
                obj_Model = Instantiate(prefab);
                obj_Model.transform.position = Vector3.zero;
            }
            Debug.Log("模型加载完成。");
        }
        else
        {
            Debug.Log("模型加载失败。");
            yield return null;
        }
    }

    IEnumerator LoadSceneAsync()
    {
        SceneLoadRequest _request = AssetBundleMgr.Instance.LoadSceneAsync("CJ_1");
        if (_request != null)
        {
            while (!_request.IsDone)
            {
                yield return null;
            }
            Debug.Log("场景加载完成。");
        }
        else
        {
            Debug.Log("场景加载失败。");
            yield return null;
        }
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, Screen.width, 20), "加载模型"))
        {
            LoadModel();
        }

        if (GUI.Button(new Rect(0, 20, Screen.width, 20), "加载场景"))
        {
            LoadScene();
        }
    }
}
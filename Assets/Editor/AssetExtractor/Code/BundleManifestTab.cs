using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using AssetBundleBrowser.AssetBundleModel;
using Zone.AB;

namespace AssetBundleBrowser
{
    internal class BundleManifestTab
    {
        Rect m_Position;

        public List<ResourcesManifestData.Bundle> m_BundleInfos;

        TreeViewState m_BundleTreeState;
        ManifestBundleTree m_BundleTree;

        TreeViewState m_AssetTreeState;
        AssetDenpendeTree m_AssetTree;
        TreeViewState m_DependeTreeState;
        AssetDenpendeTree m_DependeTree;

        ResourcesManifestData.Bundle selectManifestBundle;

        SearchField m_searchField;
        AssetBundleBuildTab.ValidBuildTarget m_BuildTarget;
        AssetBundleBuildTab.CompressOptions m_Compression = AssetBundleBuildTab.CompressOptions.StandardCompression;

        GUIContent[] m_CompressionOptions =
        {
            new GUIContent("No Compression"),
            new GUIContent("Standard Compression (LZMA)"),
            new GUIContent("Chunk Based Compression (LZ4)")
        };
        int[] m_CompressionValues = { 0, 1, 2 };

        string searchName;

        [SerializeField]
        private string manifestVersion;

        internal void OnEnable(Rect pos)
        {
            m_Position = pos;

            if (m_BundleTreeState == null) m_BundleTreeState = new TreeViewState();
            if (m_BundleTree == null) m_BundleTree = new ManifestBundleTree(m_BundleTreeState, this);

            if (m_AssetTreeState == null) m_AssetTreeState = new TreeViewState();
            if (m_DependeTreeState == null) m_DependeTreeState = new TreeViewState();

            if (m_AssetTree == null) m_AssetTree = new AssetDenpendeTree(m_AssetTreeState);
            if (m_DependeTree == null) m_DependeTree = new AssetDenpendeTree(m_DependeTreeState);
            m_searchField = new SearchField();
            InitMsg();
            RefreshBundles();

        }
        internal void OnDisable()
        {
            m_BundleInfos.Clear();
        }
        internal void InitBundles()
        {
            if (m_BundleInfos == null) m_BundleInfos = new System.Collections.Generic.List<ResourcesManifestData.Bundle>();
            m_BundleInfos.Clear();
            var bunFolder = ManifestDataFile.Instance.manifestData.Bundles;
            for (int i = 0; i < bunFolder.Count;i++ )
            {
                m_BundleInfos.Add(bunFolder[i]);
            }

            selectManifestBundle = null;
        }

        internal void InitMsg()
        {
            manifestVersion = ManifestDataFile.Instance.manifestData.strVersion;
            Debug.LogError("strVersion:" + manifestVersion);
        }

        internal void RefreshBundles()
        {
            InitBundles();
            m_BundleTree.Reload();
            m_BundleTree.searchString = "";
            if (m_DependeTree != null)
            {
                m_DependeTree.Reload();
            }
            if (m_AssetTree != null)
            {
                m_AssetTree.Reload();
            }
        }
        float s_UpdateDelay;
        internal void Update()
        {
            var t = Time.realtimeSinceStartup;
            if (t - s_UpdateDelay > 0.1f ||
                s_UpdateDelay > t) //something went strangely wrong if this second check is true.
            {
                s_UpdateDelay = t - 0.001f;
                if (m_DependeTree != null)
                {
                    m_DependeTree.Update();
                }
                if (m_AssetTree != null)
                {
                    m_AssetTree.Update();
                }

            }
        }
        internal void OnGUI(Rect pos)
        {
            m_Position = pos;

            if (Application.isPlaying)
            {
                var style = new GUIStyle(GUI.skin.label);
                style.alignment = TextAnchor.MiddleCenter;
                style.wordWrap = true;
                GUI.Label(
                    new Rect(m_Position.x + 1f, m_Position.y + 1f, m_Position.width - 2f, m_Position.height - 2f),
                    new GUIContent("Inspector unavailable while in PLAY mode"),
                    style);
            }
            else
            {
                OnGUIEditor();
            }
        }

        private void OnGUIEditor()
        {
            EditorGUILayout.Space();
            
            manifestVersion = EditorGUILayout.TextField("Version", manifestVersion);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Load Manifest", GUILayout.MaxWidth(150f)))
            {
                ManifestDataFile.Instance.LoadData();
                RefreshBundles();
            }
            if (GUILayout.Button("Refresh Manifest", GUILayout.MaxWidth(150f)))
            {
                ManifestDataFile.Instance.RefreshData();
                RefreshBundles();
            }
            if (GUILayout.Button("Save Manifest", GUILayout.MaxWidth(150f)))
            {
                ManifestDataFile.Instance.manifestData.strVersion = manifestVersion;
                ManifestDataFile.Instance.SaveData();
                RefreshBundles();
            }

            m_BundleTree.searchString = m_searchField.OnGUI(new Rect(m_Position.x + 500, m_Position.y+20, 300, 30), m_BundleTree.searchString);

            GUILayout.EndHorizontal();

            int halfWidth = (int)(m_Position.width / 2.0f);
            if (m_BundleInfos.Count > 0)
            {
                m_BundleTree.OnGUI(new Rect(m_Position.x, m_Position.y + 50, halfWidth, m_Position.height - 30));
                //m_SingleInspector.OnGUI(new Rect(m_Position.x + halfWidth, m_Position.y + 30, halfWidth, m_Position.height - 30));
            }

            GUILayout.BeginArea(new Rect(m_Position.x + halfWidth + 5, m_Position.y + 50, halfWidth, m_Position.height - 30));

            float _offestHeight = 0;
            if (selectManifestBundle != null)
            {
                //m_BuildTarget = (AssetBundleBuildTab.ValidBuildTarget)EditorGUILayout.EnumPopup(new GUIContent("BuildTarget"), m_BuildTarget);
                //m_Compression = (AssetBundleBuildTab.CompressOptions)EditorGUILayout.IntPopup(new GUIContent("CompressOptions"),(int)m_Compression,m_CompressionOptions,m_CompressionValues);
                //if (GUILayout.Button("Build", GUILayout.MaxWidth(150f)))
                //{
                //    SetBuildAssetBundle(selectManifestBundle.Name);
                //}
                
                GUILayout.Label(new GUIContent(string.Format("BundleName:   {0}",selectManifestBundle.Name.ToLower())));
                var newName = EditorGUILayout.TextField("Name", selectManifestBundle.Name);
                selectManifestBundle.Name = newName;

                GUILayout.Label(new GUIContent(string.Format("Size:   {0}", EditorUtility.FormatBytes(selectManifestBundle.Size))));
                EditorGUILayout.Toggle("IsScene", selectManifestBundle.IsScene);

                //var newVersion = EditorGUILayout.TextField("Version", selectManifestBundle.Version.ToString());
                //selectManifestBundle.Version = int.Parse(newVersion);

                var newCrc = EditorGUILayout.TextField("Crc", selectManifestBundle.Crc.ToString());
                selectManifestBundle.Crc = uint.Parse(newCrc);

                var newIncluded = EditorGUILayout.Toggle("Included", selectManifestBundle.Included);
                selectManifestBundle.Included = newIncluded;

                var newPreload = EditorGUILayout.Toggle("Preload", selectManifestBundle.Preload);
                selectManifestBundle.Preload = newPreload;

                _offestHeight += 180;

                float _halfHeight = (m_Position.height - _offestHeight - 60)/2;
                if (m_AssetTree != null)
                {
                    m_AssetTree.OnGUI(new Rect(m_Position.x, m_Position.y + _offestHeight, halfWidth, _halfHeight));
                }
                if (m_DependeTree != null)
                {
                    m_DependeTree.OnGUI(new Rect(m_Position.x, m_Position.y + _offestHeight + _halfHeight + 5, halfWidth, _halfHeight));
                }
            }
            GUILayout.EndArea();
        }

        internal void SetBundleItem(IList<ManifestTreeItem> selected)
        {
            if (selected == null || selected.Count == 0 || selected[0] == null)
            {
                selectManifestBundle = null;
            }
            else if (selected.Count >= 1)
            {
                selectManifestBundle = selected[0].bundle;
                if (m_AssetTree != null) m_AssetTree.AddList(selectManifestBundle.Assets);
                if (m_DependeTree != null) m_DependeTree.AddList(selectManifestBundle.Dependences);
            }
        }

    }

    class ManifestBundleTree : TreeView
    {
        BundleManifestTab m_ManifestTab;
        internal ManifestBundleTree(TreeViewState s, BundleManifestTab parent)
            : base(s)
        {
            m_ManifestTab = parent;
            showBorder = true;
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem(-1, -1);
            root.children = new List<TreeViewItem>();
            if (m_ManifestTab == null)
                Debug.Log("Unknown problem in AssetBundle Browser Inspect tab.  Restart Browser and try again, or file ticket on github.");
            else
            {
                foreach (var folder in m_ManifestTab.m_BundleInfos)
                {
                    root.AddChild(new ManifestTreeItem(folder, 1, folder.IsScene ? Model.GetSceneIcon() : Model.GetBundleIcon()));
                }
            }
            return root;
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                SetSelection(new int[0], TreeViewSelectionOptions.FireSelectionChanged);
            }
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            base.RowGUI(args);
            if (args.item.depth == 0)
            {
                var width = 16;
                var edgeRect = new Rect(args.rowRect.xMax - width, args.rowRect.y, width, args.rowRect.height);
            }
        }
        private void RemoveItem(TreeViewItem item)
        {
            //var inspectItem = item as InspectTreeItem;
            //if (inspectItem != null)
            //    m_InspectTab.RemoveBundlePath(inspectItem.bundlePath);
            //else
            //    m_InspectTab.RemoveBundleFolder(item.displayName);
        }
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);

            if (selectedIds == null)
                return;

            if (selectedIds.Count > 0)
            {
                m_ManifestTab.SetBundleItem(FindRows(selectedIds).Select(tvi => tvi as ManifestTreeItem).ToList());
                //m_InspectTab.SetBundleItem(FindItem(selectedIds[0], rootItem) as InspectTreeItem);
            }
            else
            {
                m_ManifestTab.SetBundleItem(null);
            }
        }

        public void setSelectionChanged(int id)
        {
            List<int> selectedIds = new List<int>();
            selectedIds.Add(id);
            SelectionChanged(selectedIds);
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return true;
        }
    }

    internal sealed class ManifestTreeItem : TreeViewItem
    {
        private ResourcesManifestData.Bundle m_Bundle;
        internal ResourcesManifestData.Bundle bundle
        {
            get { return m_Bundle; }
        }
        internal ManifestTreeItem(ResourcesManifestData.Bundle b, int depth, Texture2D iconTexture)
            : base(b.Name.GetHashCode(), depth, b.Name)
        {
            m_Bundle = b;
            icon = iconTexture;
            children = new List<TreeViewItem>();
        }

        public override string displayName
        {
            get
            {
                return m_Bundle.Name;
            }
        }
    }


    class AssetDenpendeTree : TreeView
    {
        List<string> m_NameList ;
        bool beChange;
        internal AssetDenpendeTree(TreeViewState s)
            : base(s)
        {
            
            showBorder = true;
            m_NameList = new List<string>();
            beChange = false;
        }

        public void AddList(List<string> _m_NameList)
        {
            m_NameList = _m_NameList;
            beChange = true;
        }
        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem(-1, -1);
            root.children = new List<TreeViewItem>();
            if (m_NameList == null)
                Debug.Log("Unknown problem in AssetBundle Browser Inspect tab.  Restart Browser and try again, or file ticket on github.");
            else
            {
                for (int i = 0; i < m_NameList.Count;i++ )
                {
                    root.AddChild(new AssetTreeItem(m_NameList[i], 1, Model.GetBundleIcon()));
                }
            }
            return root;
        }
        internal void Update()
        {
            if (beChange)
            {
                beChange = false;
                Reload();
                ExpandAll();
            }
        }
        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            //if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            //{
            //    SetSelection(new int[0], TreeViewSelectionOptions.FireSelectionChanged);
            //}
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            base.RowGUI(args);
            if (args.item.depth == 0)
            {
                var width = 16;
                var edgeRect = new Rect(args.rowRect.xMax - width, args.rowRect.y, width, args.rowRect.height);
            }
        }
        private void RemoveItem(TreeViewItem item)
        {
            //var inspectItem = item as InspectTreeItem;
            //if (inspectItem != null)
            //    m_InspectTab.RemoveBundlePath(inspectItem.bundlePath);
            //else
            //    m_InspectTab.RemoveBundleFolder(item.displayName);
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return true;
        }
    }

    internal sealed class AssetTreeItem : TreeViewItem
    {
        private string m_AssetName;
        internal string assetName
        {
            get { return m_AssetName; }
        }
        internal AssetTreeItem(string n, int depth, Texture2D iconTexture)
            : base(n.GetHashCode(), depth, n)
        {
            m_AssetName = n;
            icon = iconTexture;
            children = new List<TreeViewItem>();
        }

        public override string displayName
        {
            get
            {
                return m_AssetName;
            }
        }
    }
}

﻿using highlight;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIWindow : EditorWindow
{
    public static UIWindow window;
    [MenuItem("WindowTools/UIWindow")]
    static void ExportGameObjectUrl()
    {
        if (window == null)
        {
            window = (UIWindow)GetWindow(typeof(UIWindow));
            window.Show();
        }
    }
    private void OnFocus()
    {
        curTextType = eTextType.None;
    }
    string inputStr;
    void OnGUI()
    {
        GUILayout.BeginVertical();
        //if (GUI.Button(new Rect(10, 10, 400, 200), "输出Url"))
        if(!Application.isPlaying)
        {
            GameObject go = Selection.activeGameObject;
            if (go != null)
            {
                OnText(go);
                OnImage(go);
                ISerializeField isField = go.GetComponent<ISerializeField>();
                if (isField != null)
                {
                    //if (GUILayout.Button("自动BindingUI", GUILayout.MaxWidth(100), GUILayout.MinHeight(20)))
                    //{
                    //    AutoGenUI(isField);
                    //}
                    GUILayout.Space(20f);
                }
            }
        }

        if (GUILayout.Button("输出Url", GUILayout.MaxWidth(100), GUILayout.MinHeight(20)))
        {
            export();
        }
        inputStr = EditorGUILayout.TextField("guid:", inputStr);
        if (GUILayout.Button("查找资源", GUILayout.MaxWidth(100), GUILayout.MinHeight(20)))
        {
            if (!string.IsNullOrEmpty(inputStr))
            {
                string path = AssetDatabase.GUIDToAssetPath(inputStr);
                if (!string.IsNullOrEmpty(path))
                {
                    Debug.Log(path);
                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
                }
            }
        }
        
        GUILayout.Space(20f);
        GUILayout.EndVertical();
    }
    enum eTextType
    {
        None,
        Style1,
        c_1e7598,
    }
    static eTextType curTextType = eTextType.None;
    static void OnText(GameObject go)
    {
        Text tf = go.GetComponent<Text>();
        if (tf == null)
            return;
        curTextType = (eTextType)EditorGUILayout.EnumPopup("TextStyle", curTextType);
        if(curTextType != eTextType.None)
        {
            switch (curTextType)
            {
                case eTextType.None:
                    break;
                case eTextType.Style1:
                    tf.fontSize = 28;
                    tf.color = ColorUtil.GetColor(0x94e1f7);
                    break;
                case eTextType.c_1e7598:
                    tf.fontSize = 28;
                    tf.color = ColorUtil.GetColor(0x1e7598);
                    break;
                default:
                    break;
            }
        }
        curTextType = eTextType.None;
    }






    static void OnImage(GameObject go)
    {
        Image img = go.GetComponent<Image>();
        if (img == null)
            return;

    }
    void export()
    {
        GameObject[] gobj = Selection.gameObjects;
        foreach (var obj in gobj)
        {
            export(obj);
        }
    }
    void export(GameObject go)
    {
        string url = go.name;
        Transform t = go.transform.parent;
        while (t != null)
        {
            url = t.name + "/" + url;
            t = t.transform.parent;
        }
        Debug.Log(url);
    }




    public static List<string> ignoreList = new List<string> { "bg", "Panel", "Button", "Image", "RawImage", "Text", "Slider", "ScrollBar", "ScrollRect", "InputField", "Toggle", "ToggleGroup", "Placeholder", "Handle", "Background", "Checkmark" };
    public static readonly Regex _BindingRegex = new Regex(@"region AutoBinding([\s\S]*?)endregion ", RegexOptions.Multiline);
    public static void AutoGenUI(ISerializeField iui)
    {
        MonoBehaviour mono = iui as MonoBehaviour;
        MonoScript ms = MonoScript.FromMonoBehaviour(mono);
        string path = AssetDatabase.GetAssetPath(ms);
        string info = File.ReadAllText(path);
        int sIdx = info.IndexOf("#region AutoBinding");
        int endIdx = info.IndexOf("#endregion");
        if(sIdx > -1 && endIdx > sIdx)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("#region AutoBinding\n\n\t");
            string regexStr = info.Substring(sIdx, endIdx - sIdx);

            Dictionary<string, FieldInfo> tempDic = new Dictionary<string, FieldInfo>();
            FieldInfo[] pis = iui.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (int j = 0; j < pis.Length; j++)
            {
                if (regexStr.IndexOf(" " + pis[j].Name)> -1)
                    continue;
                tempDic[pis[j].Name] = pis[j];
            }

            MonoBehaviour[] monos = mono.GetComponentsInChildren<MonoBehaviour>(true);
            Dictionary<string, MonoBehaviour> dic = new Dictionary<string, MonoBehaviour>();
            for (int i = 0; i < monos.Length; i++)
            {
                MonoBehaviour node = monos[i];
                string nName = node.name;
                if (ignoreList.Contains(nName) || tempDic.ContainsKey(nName))
                    continue;
                if (nName == mono.name || nName.IndexOf(" ")> -1)
                    continue;
                ISerializeField seri = node.gameObject.GetCompInParent<ISerializeField>();// node.GetComponentInParent<ISerializeField>();
                if (seri == iui)
                {
                    if (dic.ContainsKey(nName) && dic[nName] is Selectable)
                        continue;
                    dic[nName] = node;
                }
            }
            foreach (var node in dic.Values)
            {
                sb.Append("public " + node.GetType().Name + " " + node.name + ";\n\t");
            }
            sb.Append("\n");
            string newStr = sb.ToString();
            bool ischange = newStr != regexStr;
            if (ischange)
            {
                info = info.Replace(regexStr, newStr);
                WriteTxt(path, info);
                AssetDatabase.Refresh();
            }
        }
        Debug.Log(path);
    }
    public static void WriteTxt(string url, string str, System.Text.Encoding enc = null)
    {
        if (enc == null)
            enc = new System.Text.UTF8Encoding(false);
        System.IO.StreamWriter sw = new System.IO.StreamWriter(url, false, enc);
        sw.Write(str);
        sw.Flush();
        sw.Close();
        sw.Dispose();
    }
}

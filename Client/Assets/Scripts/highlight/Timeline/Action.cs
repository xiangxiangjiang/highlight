using System;
using System.Collections.Generic;
using System.Reflection;
namespace highlight.tl
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DescAttribute : System.Attribute
    {
        public string desc;
        public DescAttribute(string des)
        {
            desc = des;
        }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class ActionAttribute : System.Attribute
    {
        public string name;
        public string menu;
        public string updateName;
        public string triggerName;
        public Type type;
        public FieldInfo[] Infos;
        public Dictionary<FieldInfo, string> infoDesDic = new Dictionary<FieldInfo, string>();
        public ActionAttribute(string _menu,Type _type)
        {
            this.menu = _menu;
            this.name = _menu;
            if (name.LastIndexOf("/") > -1)
                this.name = name.Substring(name.LastIndexOf("/") + 1);
            type = _type;
            this.updateName = type.Name + "_OnUpdate";
            this.triggerName = type.Name + "_OnTrigger";
            List<FieldInfo> flist = new List<FieldInfo>();
            Type iter = typeof(ComponentData);
            Type inter = typeof(ITimeInterface);
            FieldInfo[] pis = type.GetFields();
            for (int j = 0; j < pis.Length; j++)
            {
                FieldInfo fi = pis[j];
                if (iter.IsAssignableFrom(fi.FieldType) ||
                    (fi.FieldType.IsInterface && inter.IsAssignableFrom(fi.FieldType)))// fi.FieldType.IsSubclassOf(iter))//t.IsInterface && 
                {
                    DescAttribute attr = Attribute.GetCustomAttribute(fi, typeof(DescAttribute)) as DescAttribute;
                    // DescAttribute[] attrs = t.GetCustomAttributes(typeof(DescAttribute), true) as DescAttribute[];
                    if (attr != null)
                        infoDesDic[fi] = attr.desc;
                    else
                        infoDesDic[fi] = fi.Name;
                    flist.Add(fi);
                }
            }
            Infos = flist.ToArray();
        }

    }
    public sealed class ActionStyle : Object
    {
       // public TriggerType tType = TriggerType.失败后停止;
        public string key;
        public string name;
        public int[] Indexs;
        public ActionStyle()
        {

        }
        public ActionStyle(ActionAttribute attr)
        {
            name = attr.type.FullName;
            Indexs = new int[attr.Infos.Length];
            for (int j = 0; j < attr.Infos.Length; j++)
            {
                Indexs[j] = -1;
            }
        }
        public readonly static Dictionary<string, ActionAttribute> actionAttrDic = new Dictionary<string, ActionAttribute>();
        private ActionAttribute mAttr;
         [Newtonsoft.Json.JsonIgnore]
        public ActionAttribute Attr
        {
            get
            {
                if (mAttr == null)
                {
                   // Type t = this.type;
                    actionAttrDic.TryGetValue(name, out mAttr);
                    if (mAttr == null)
                    {
                        Type t = Type.GetType(this.name);
                        ActionAttribute[] attrs = t.GetCustomAttributes(typeof(ActionAttribute), true) as ActionAttribute[];
                        if (attrs != null && attrs.Length > 0)
                        {
                            mAttr = attrs[0];
                            actionAttrDic[name] = mAttr;
                        }
                    }
                }
                return mAttr;
            }
        }
    }
    public abstract class TimeAction : Component
    {
        public ActionStyle style { private set; get; }
        #region virtual Function
        public virtual string GetDefaultKey() { return string.Empty; }
        public virtual TriggerStatus OnTrigger() { return TriggerStatus.Success; }
        public virtual void OnUpdate() { }
        public virtual void OnResume() { }
        public virtual void OnPause() { }
        public virtual void OnDrawGizmos() { }
#if UNITY_EDITOR
        public virtual void OnInspectorGUI()
        {
        }
#endif
        #endregion

        private readonly static Dictionary<string, ObjectPool> ActionPoolDic = new Dictionary<string, ObjectPool>();

        public static TimeAction Get(ActionStyle comp, TimeObject t)
        {
            if (comp == null)
                return null;
            TimeAction data = getPool(comp).Get(comp.Attr.type) as TimeAction;
            data.timeObject = t;
            data.style = comp;
            return data;
        }
        public static void Release(TimeAction action)
        {
            if (action == null)
                return;
            getPool(action.style).Release(action);
            action.timeObject = null;
            action.style = null;
        }
        static ObjectPool getPool(ActionStyle data)
        {
            ObjectPool pool = null;
            ActionPoolDic.TryGetValue(data.name, out pool);
            if (pool == null)
            {
                pool = new ObjectPool();
                ActionPoolDic[data.name] = pool;
            }
            return pool;
        }
        public virtual void SetData(List<ComponentData> comps)
        {
            FieldInfo[] infos = this.style.Attr.Infos;
            if (infos != null && infos.Length > 0)
            {
                if (this.style.Indexs == null || this.style.Indexs.Length != infos.Length)
                {
                    this.style.Indexs = new Int32[infos.Length];
                    for (int j = 0; j < infos.Length; j++)
                    {
                        this.style.Indexs[j] = -1;
                    }
                    return;
                }
                for (int j = 0; j < infos.Length; j++)
                {
                    int idx = this.style.Indexs[j];
                    if (idx < 0 || idx >= comps.Count)
                        continue;
                    ComponentData comp = comps[idx];
                    try
                    {
                        infos[j].SetValue(this, comp);
                    }
                    catch(Exception e)
                    {
                        UnityEngine.Debug.Log(this.timeObject.name  + "," + e);
                    }
                }
            }
        }
        public virtual void ClearData()
        {
            FieldInfo[] infos = this.style.Attr.Infos;
            if (infos != null && infos.Length > 0)
            {
                for (int j = 0; j < infos.Length; j++)
                {
                    infos[j].SetValue(this, null);
                }
            }
        }
    }
}
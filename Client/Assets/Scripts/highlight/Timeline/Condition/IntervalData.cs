﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.tl
{
    [Time("条件/间隔时间", typeof(IntervalData))]
    public class IntervalStyle : ConditionStyle
    {
        public int interval;
        public int count;
        public int defValue;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            this.interval = EditorGUILayout.IntField("interval:", this.interval);
            this.count = EditorGUILayout.IntField("count:", this.count);
            this.defValue = EditorGUILayout.IntField("delay:", this.defValue);
        }
#endif
    }
    public class IntervalData : ComponentData<IntervalStyle>, IConditionData
    {
        public LogicType logicType { get { return mStyle.logicType; } }
        public CDData cd;
        public int count { get { return mStyle.count; } }
        public int curCount = 0;
        public override bool OnTrigger()
        {
            IntervalStyle s = mStyle;
            cd = new CDData(App.time - s.interval + s.defValue, s.interval);
            curCount = 0;
            return true;
        }
        public bool OnCheck()
        {
            if (cd.IsComplete && curCount < this.count)
            {
                cd.start = App.time;
                this.curCount++;
                return true;
            }
            return false;
        }
    }
}
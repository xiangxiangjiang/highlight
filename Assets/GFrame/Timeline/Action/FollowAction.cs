﻿using highlight.timeline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace highlight
{
    [Action("行为/跟随", typeof(FollowAction))]
    public class FollowAction : TimeAction
    {
        [Desc("目标挂点")]
        public LocatorData target;

        public override void OnInit()
        {

        }
        public override void OnUpdate()
        {
            this.prefabData.SetPos(this.target.position);
        }
        public override void OnDestroy()
        {
            this.target = null;
        }
    }
}
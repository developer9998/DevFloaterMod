using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DevFloaterMod.Models
{
    [Serializable]
    public class Arm
    {
        public GameObject armObject;
        public GameObject floaterObject;
        public TransformFollow floaterFollower;
        public bool leftArm;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Modding;
using UnityEngine;

namespace MylaFlower
{
    public class MylaFlower : Mod
    {
        internal static MylaFlower instance;
        
        public MylaFlower() : base(null)
        {
            instance = this;
        }
        
        public override string GetVersion()
        {
            return GetType().Assembly.GetName().Version.ToString();
        }
        
        public override void Initialize()
        {
            Log("Initializing Mod...");
            
            
        }
    }
}
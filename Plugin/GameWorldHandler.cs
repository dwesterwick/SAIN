using Comfort.Common;
using EFT;
using SAIN.Components;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN
{
    public class GameWorldHandler
    {
        public static void Create(GameObject gameWorldObject)
        {
            gameWorldObject.AddComponent<GameWorldComponent>();
            gameWorldObject.AddComponent<JobManager>();
        }

        public static GameWorldComponent SAINGameWorld { get; private set; }
    }
}
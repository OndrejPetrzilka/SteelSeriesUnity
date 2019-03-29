using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteelSeries.GameSense.Config;
using UnityEngine;

namespace SteelSeries.GameSense
{
    /// <summary>
    /// Component which initializes GameSense.
    /// </summary>
    public class GameSenseInit : MonoBehaviour
    {
        public GameSenseConfig Config;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            // Init only once
            if (!GameSense.IsInitialized)
            {
                Config.Initialize();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SteelSeries.GameSense.Config
{
    [CreateAssetMenu(fileName = "GameSenseConfig", menuName = "GameSense Config")]
    public class GameSenseConfig : ScriptableObject
    {
        [Serializable]
        public struct Event
        {
            public string Name;
            public int MinValue;
            public int MaxValue;
            public EventIconId Icon;
            public TextAsset[] Handlers;
        }

        public string GameName;
        public string DisplayGameName;
        public string Developer;
        public IconColor IconColor;
        public Event[] Events;
    }
}
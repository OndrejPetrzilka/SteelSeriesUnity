using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteelSeries.GameSense.Config
{
    public static class GameSenseConfigExtensions
    {
        public static void Initialize(this GameSenseConfig config)
        {
            GameSense.Initialize(config.GameName, config.DisplayGameName, config.Developer, config.IconColor);
            foreach (var e in config.Events)
            {
                var handlers = e.Handlers.Select(s => s.text).ToArray();
                GameSense.BindEvent(e.Name, e.MinValue, e.MaxValue, e.Icon, handlers);
            }
        }
    }
}

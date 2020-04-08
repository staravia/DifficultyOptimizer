using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.API.Maps;

namespace DifficultyOptimizer.src
{
    public class MapData
    {
        public float TargetDifficulty;

        public float Weight;

        public string FilePath;

        [NonSerialized]
        public Qua Map;

        public MapData(string file, float difficulty, float weight, Qua map = null)
        {
            TargetDifficulty = difficulty;
            Weight = weight;
            FilePath = file;
            Map = map;
        }
    }
}

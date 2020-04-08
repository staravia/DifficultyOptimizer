using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DifficultyOptimizer.src
{
    public class MapData
    {
        public float TargetDifficulty;

        public float Weight;

        public string FilePath;

        public MapData(string file, float difficulty, float weight)
        {
            TargetDifficulty = difficulty;
            Weight = weight;
            FilePath = file;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DifficultyOptimizer.src
{
    public class Optimizer
    {
        public List<MapData> MapData { get; set; }

        public Optimizer()
        {
            MapData = new List<MapData>();
        }

        public void AddMapData(string file, float diff, float weight) => MapData.Add(new MapData(file, diff, weight));
    }
}

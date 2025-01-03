using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BayesianNetworkApp
{
    public class Node
    {
        public string Name { get; set; }
        public List<string> Outcomes { get; set; } = new();
        public List<string> Parents { get; set; } = new();
        public string ProbabilityTable { get; set; }
        public string Evidence { get; set; } 
    }

}

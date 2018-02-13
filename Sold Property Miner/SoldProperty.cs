using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sold_Property_Miner
{
    /* Sold Property Object */
    class SoldProperty
    {
        public string soldPrice { get; set; }
        public string type      { get; set; }
        public string address   { get; set; }
        public string suburb    { get; set; }
        public string dateSold  { get; set; }
        public int bedrooms     { get; set; }
        public int bathrooms    { get; set; }
        public int carPorts     { get; set; }
    }
}

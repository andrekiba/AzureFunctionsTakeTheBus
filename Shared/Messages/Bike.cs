using System.Collections.Generic;

namespace AzureFunctionsTakeTheBus.Shared.Messages
{
    public class Bike
    {
        public string Id { get; set; }
        public int Price { get; set; }
        public string Model { get; set; }
        public List<BikePart> Parts { get; set; }
    }
}
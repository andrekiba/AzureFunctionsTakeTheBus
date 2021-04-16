﻿using NServiceBus;

namespace AzureFunctionsTakeTheBus.Shared.Messages
{
    public class ShipBikeMessage : IMessage
    {
        public string Id { get; set; }
        public string Address { get; set; }

        public ShipBikeMessage()
        {
            
        }
        public ShipBikeMessage(string id, string address)
        {
            Id = id;
            Address = address;
        }
    }
}
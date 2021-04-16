using NServiceBus;

namespace Elfo.NsbFunctions.FunctionEndpoint.Messages
{
    public class AMessage : IMessage
    {
        public string Payload { get; }

        public AMessage(string payload)
        {
            Payload = payload;
        }
    }
}
using System;
using System.ServiceModel;
using SerialChannel.Binding;

namespace SerialClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri uri = new Uri("serial://localhost/com7");
            Console.WriteLine("Creating factory...");

            SerialTransportBinding binding = new SerialTransportBinding("COM8");
            
            ChannelFactory<ISerialTrasportDemo> factory = 
              new ChannelFactory<ISerialTrasportDemo>(binding);

            ISerialTrasportDemo channel = 
              factory.CreateChannel(new EndpointAddress(uri));

            Console.Write("Enter Text or x to quit: ");
            string message;
            while ((message = Console.ReadLine()) != "x")
            {
              string result = channel.ProcessReflectRequest(message);

              Console.WriteLine(
                "\nReceived for ProcessReflectRequest: " + result + "\n");
              Console.Write("Enter Text or x to quit: ");
            }

            factory.Close();
        }
    }

    [ServiceContract]
    public interface ISerialTrasportDemo
    {
        [OperationContract]
        string ProcessReflectRequest(string request);
    }
}

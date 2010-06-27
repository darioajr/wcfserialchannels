using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SerialChannel.Binding;
using System.ServiceModel;
using System.ServiceModel.Channels;
using SerialChannel.Binding.Transport;

namespace SerialClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri uri = new Uri("serial://localhost/com7");
            Console.WriteLine("Creating factory...");

            SerialTransportBinding binding = new SerialTransportBinding();
            
            binding.FactoryPort = "COM8";

            ChannelFactory<IFileTrasport> factory = new ChannelFactory<IFileTrasport>(binding);
            IFileTrasport channel = factory.CreateChannel(new EndpointAddress(uri));

            Console.Write("Enter Text or x to quit: ");
            string message;
            while ((message = Console.ReadLine()) != "x")
            {
                string result = channel.ProcessReflectRequest(message);

                Console.WriteLine("Received : " + result + "\n");
                Console.Write("Enter Text or x to quit: ");
            }

            factory.Close();
        }
    }

    [ServiceContract]
    public interface IFileTrasport
    {
        [OperationContract]
        string ProcessReflectRequest(string request);
    }

}

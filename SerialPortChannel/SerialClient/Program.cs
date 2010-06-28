using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SerialChannel.Binding;
using System.ServiceModel;
using System.ServiceModel.Channels;
using SerialChannel.Binding.Transport;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

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
            factory.Endpoint.Behaviors.Add(new ConsoleOutputBehavior());

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



    public class ConsoleOutputMessageInspector : IClientMessageInspector
    {
        #region IClientMessageInspector Members

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            MessageBuffer buffer = reply.CreateBufferedCopy(Int32.MaxValue);
            reply = buffer.CreateMessage();
            Console.WriteLine("Received:\n{0}", buffer.CreateMessage().ToString());
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            MessageBuffer buffer = request.CreateBufferedCopy(Int32.MaxValue);
            request = buffer.CreateMessage();
            Console.WriteLine("Sending:\n{0}", buffer.CreateMessage().ToString());
            return request;
        }

        #endregion
    }

    public class ConsoleOutputBehavior : IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            ConsoleOutputMessageInspector inspector = new ConsoleOutputMessageInspector();
            clientRuntime.MessageInspectors.Add(inspector);
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }
    }

    [ServiceContract]
    public interface IFileTrasport
    {
        [OperationContract]
        string ProcessReflectRequest(string request);
    }
}

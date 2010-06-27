using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using SerialChannel.Binding;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using SerialChannel.Binding.Transport;

namespace SerialServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Creating listener...");

            SerialTransportBinding binding = new SerialTransportBinding();
            binding.ListenerPort = "COM7";
            
            Uri uri = new Uri("serial://localhost/com7");
            
            ServiceHost host = new ServiceHost(typeof(FileTrasportService));
            ServiceEndpoint ep = host.AddServiceEndpoint(typeof(IFileTrasport), binding, uri);
            ep.Behaviors.Add(new ConsoleOutputBehavior());

            host.Open();

            Console.WriteLine("Host Running, press any key to exit.");
            Console.ReadLine();
        
            host.Close();

        }
    }


    public class ConsoleOutputMessageInspector : IDispatchMessageInspector
    {
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            MessageBuffer buffer = request.CreateBufferedCopy(Int32.MaxValue);
            request = buffer.CreateMessage();
            Console.WriteLine("Received:\n{0}", buffer.CreateMessage().ToString());
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            MessageBuffer buffer = reply.CreateBufferedCopy(Int32.MaxValue);
            reply = buffer.CreateMessage();
            Console.WriteLine("Sending:\n{0}", buffer.CreateMessage().ToString());
        }
    }

    public class ConsoleOutputBehavior : IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            throw new Exception("Behavior not supported on the consumer side!");
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            ConsoleOutputMessageInspector inspector = new ConsoleOutputMessageInspector();
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(inspector);
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

    public class FileTrasportService : IFileTrasport
    {

        #region IFileTrasport Members

        public string ProcessReflectRequest(string request)
        {
            char[] output = new char[request.Length];
            for (int index = 0; index < request.Length; index++)
            {
                output[index] = request[request.Length - index - 1];
            }
            return new string(output);
        }

        #endregion
    }

}

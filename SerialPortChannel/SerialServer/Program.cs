using System;
using System.ServiceModel;
using SerialChannel.Binding;
using System.ServiceModel.Description;

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

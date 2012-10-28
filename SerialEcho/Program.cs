using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace SerialEcho
{
    class Program
    {
        static SerialPort serialPort;
        static void Main(string[] args)
        {
            serialPort = new SerialPort();

            // Set the appropriate properties.
            serialPort.PortName = "COM7";
            serialPort.BaudRate = 9600; //TODO: Allow users to specify this
            serialPort.Parity = Parity.None; //TODO: Allow users to specify this
            serialPort.DataBits = 8; //TODO: Allow users to specify this
            serialPort.StopBits = StopBits.One; //TODO: Allow users to specify this
            serialPort.Handshake = Handshake.None; //TODO: Allow users to specify this

            // Set the read/write timeouts
            serialPort.ReadTimeout = 500; //TODO: Allow users to specify this
            serialPort.WriteTimeout = 500; //TODO: Allow users to specify this
            serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
            
            serialPort.Open();
            
            Console.WriteLine("Echo Device Running");
            Console.ReadLine();
        }

        static void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte [] bytes = new byte [1024];
            int count = serialPort.Read(bytes, 0, 1024);
            string result = ProcessReflectRequest(Encoding.UTF8.GetString(bytes,0,count));
            serialPort.Write(Encoding.UTF8.GetBytes(result), 0, count);
        }

        static string ProcessReflectRequest(string request)
        {
            char[] output = new char[request.Length];
            for (int index = 0; index < request.Length; index++)
            {
                output[index] = request[request.Length - index - 1];
            }
            return new string(output);
        }

    }
}

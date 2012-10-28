using System;
using System.IO;
using System.IO.Ports;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace SerialChannel.Binding.Transport
{
    /// <summary>
    /// Abstract Serial Channel Base class. <br />
    /// Creates serial port and performs read and write operations.
    /// </summary>
    abstract class SerialChannelBase : ChannelBase
    {
        const int MaxBufferSize = 64 * 1024;
        const int MaxSizeOfHeaders = 4 * 1024;

        readonly EndpointAddress address;
        readonly BufferManager bufferManager;
        readonly MessageEncoder encoder;
        readonly long maxReceivedMessageSize;
        readonly string portNumber;


        SerialPort serialPort;

        /// <summary>
        /// SerialChannel Base
        /// </summary>
        /// <param name="bufferManager">
        /// Buffer manager created by factory and listener</param>
        /// <param name="encoderFactory">
        /// Referece to encoder factory as returned by encoder element</param>
        /// <param name="address">Remote address</param>
        /// <param name="portNumber">COM port number</param>
        /// <param name="parent">reference to factory/listener</param>
        /// <param name="maxReceivedMessageSize">
        /// Some settings for transport channel</param>
        public SerialChannelBase(BufferManager bufferManager, 
            MessageEncoderFactory encoderFactory, 
            EndpointAddress address,
            string portNumber,
            ChannelManagerBase parent,
            long maxReceivedMessageSize)
            : base(parent)
        {
            this.address = address;
            this.bufferManager = bufferManager;
            this.encoder = encoderFactory.CreateSessionEncoder();
            this.maxReceivedMessageSize = maxReceivedMessageSize;

            this.portNumber = portNumber;

            // Create port
            serialPort = new SerialPort();

            // Set the appropriate properties.
            serialPort.PortName = this.portNumber;
            //TODO: Read these settings from configuration file
            serialPort.BaudRate = 9600; 
            serialPort.Parity = Parity.None;
            serialPort.DataBits = 8; 
            serialPort.StopBits = StopBits.One; 
            serialPort.Handshake = Handshake.None; 

            // Set the read/write timeouts
            serialPort.ReadTimeout = 500; 
            serialPort.WriteTimeout = 500; 
        }

        /// <summary>
        /// Wrap general expections as communication exceptions
        /// </summary>
        /// <param name="exception">Exception to be wrapped</param>
        /// <returns>Return communication exception wrapped 
        /// with general exception</returns>
        protected static Exception ConvertException(Exception exception)
        {
            Type exceptionType = exception.GetType();
            if (exceptionType == typeof(System.IO.DirectoryNotFoundException) ||
                exceptionType == typeof(System.IO.FileNotFoundException) ||
                exceptionType == typeof(System.IO.PathTooLongException))
            {
              return 
                new EndpointNotFoundException(exception.Message, exception);
            }
            return new CommunicationException(exception.Message, exception);
        }

        /// <summary>
        /// Remote address used to filter messages
        /// </summary>
        public EndpointAddress RemoteAddress
        {
            get { return this.address; }
        }

        /// <summary>
        /// Reference to Port.
        /// </summary>
        public SerialPort Port
        {
            get { return serialPort; }
        }

        /// <summary>
        /// Read Message
        /// </summary>
        /// <returns>Message object</returns>
        protected Message ReadMessage()
        {
            
            byte[] data;
            int bytesRead = 0;
            try
            {
                Stream stream = serialPort.BaseStream;
                data = this.bufferManager.TakeBuffer(
                  (int)maxReceivedMessageSize);

                bytesRead = stream.Read(data, bytesRead, 
                  (int)maxReceivedMessageSize);

                if (bytesRead == 0)
                {
                    throw new CommunicationException(
                      String.Format(
                      "Unexpected end of message after {0} of {1} bytes.", 
                      bytesRead, maxReceivedMessageSize));
                }
            }
            catch (IOException exception)
            {
                throw ConvertException(exception);
            }

            ArraySegment<byte> buffer = 
            new ArraySegment<byte>(data,0,bytesRead);

            return this.encoder.ReadMessage(buffer, this.bufferManager);
        }

        /// <summary>
        /// Data written to SOAP body before transmitting
        /// </summary>
        /// <param name="message"></param>
        protected void WriteMessage(Message message)
        {
            ArraySegment<byte> buffer;
            using (message)
            {
                //this.address.ApplyTo(message);
                buffer = this.encoder.WriteMessage(
                  message, MaxBufferSize, this.bufferManager);

            } //message close
            try
            {
                Stream stream = serialPort.BaseStream;
                stream.Write(buffer.Array, buffer.Offset, buffer.Count);
                this.bufferManager.ReturnBuffer(buffer.Array);
            }
            catch (IOException exception)
            {
                throw ConvertException(exception);
            }
        }
    }
}

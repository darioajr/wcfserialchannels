using System;
using System.ServiceModel.Channels;
using SerialChannel.Binding.Transport;

namespace SerialChannel.Binding.Transport
{
  /// <summary>
  /// Request Channel Factory
  /// </summary>
  class SerialRequestChannelFactory :
    ChannelFactoryBase, IChannelFactory<IRequestChannel>
  {
    readonly BufferManager bufferManager;
    readonly MessageEncoderFactory encoderFactory;
    public readonly long MaxReceivedMessageSize;

    public readonly string PortNumber;

    /// <summary>
    /// Constructor for request factory.
    /// </summary>
    /// <param name="transportElement"></param>
    /// <param name="context"></param>
    public SerialRequestChannelFactory(
      SerialTransportBindingElement transportElement,
      BindingContext context)
      : base(context.Binding)
    {
      MessageEncodingBindingElement messageEncodingElement =
        context.RemainingBindingElements.Remove<MessageEncodingBindingElement>();
      this.bufferManager =
        BufferManager.CreateBufferManager(
        transportElement.MaxBufferPoolSize, 0x400000); //4MB

      this.encoderFactory = 
        messageEncodingElement.CreateMessageEncoderFactory();
      MaxReceivedMessageSize = transportElement.MaxReceivedMessageSize;
      this.PortNumber = transportElement.FactoryPort;
    }

    #region CommunicationObject Members
    protected override IAsyncResult OnBeginOpen(
      TimeSpan timeout, AsyncCallback callback, object state)
    {
      Console.WriteLine("SerialRequestChannelFactory:OnBeginOpen");
      throw new NotImplementedException();
    }

    protected override void OnEndOpen(IAsyncResult result)
    {
      Console.WriteLine("SerialRequestChannelFactory:OnEndOpen");
    }

    protected override void OnOpen(TimeSpan timeout)
    {
      Console.WriteLine("SerialRequestChannelFactory:OnOpen");
    }
    #endregion

    #region IChannelFactory<IReplyChannel> Members

    /// <summary>
    /// Create Channel
    /// </summary>
    /// <param name="to"></param>
    /// <param name="via"></param>
    /// <returns></returns>
    public IRequestChannel CreateChannel(
      System.ServiceModel.EndpointAddress to, Uri via)
    {
      return new SerialRequestChannel(
        this.bufferManager, 
        this.encoderFactory, 
        to, 
        this, 
        via);
    }

    /// <summary>
    /// Create Channel
    /// </summary>
    /// <param name="to"></param>
    /// <returns></returns>
    public IRequestChannel CreateChannel(
      System.ServiceModel.EndpointAddress to)
    {
      return new SerialRequestChannel(
        this.bufferManager, 
        this.encoderFactory, 
        to, 
        this,
        new Uri(PortNumber));
    }

    #endregion
  }
}

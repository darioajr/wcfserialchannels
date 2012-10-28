using System;
using System.ServiceModel.Channels;

namespace SerialChannel.Binding.Transport
{
  /// <summary>
  /// Transport Binding element.
  /// </summary>
  /// <remarks>Internal class accessed by binding object.</remarks>
  class SerialTransportBindingElement : TransportBindingElement
  {
    public string FactoryPort;

    /// <summary>
    /// Default constructor for tranport binding.
    /// </summary>
    /// <remarks>Can set default ports if required.</remarks>
    public SerialTransportBindingElement()
    {

    }

    /// <summary>
    /// This is called by WCF runtime. 
    /// </summary>
    /// <param name="other">Reference object</param>
    SerialTransportBindingElement(SerialTransportBindingElement other)
      : base(other)
    {
      this.FactoryPort = other.FactoryPort;
    }

    /// <summary>
    /// Scheme to used for addressing
    /// </summary>
    public override string Scheme
    {
      get { return "serial"; }
    }

    /// <summary>
    /// Called by WCF runtime.
    /// </summary>
    /// <returns></returns>
    public override BindingElement Clone()
    {
      return new SerialTransportBindingElement(this);
    }

    /// <summary>
    /// Can build a channel factory if 
    /// request channel is of type IRequestChannel.
    /// </summary>
    /// <typeparam name="TChannel">
    /// Generic parameter for channel type</typeparam>
    /// <param name="context">
    /// Binding context for which channel to be built</param>
    /// <returns>
    /// Returns true if channel factory can be built 
    /// for requested channel shape.</returns>
    /// <remarks>Gets called by client side WCF runtime</remarks>
    public override bool CanBuildChannelFactory<TChannel>
      (BindingContext context)
    {
      return typeof(TChannel) == typeof(IRequestChannel);
    }

    /// <summary>
    /// Build channel factory.
    /// </summary>
    /// <typeparam name="TChannel">
    /// Shape of channel. Accepts IRequestChannel only.</typeparam>
    /// <param name="context">Binding context</param>
    /// <returns>
    /// Returns an channel factory (SerialRequestChannelFactory) 
    /// set to specified binding context </returns>
    /// <remarks>Gets called by client side WCF runtime</remarks>
    public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>
      (BindingContext context)
    {
      if (context == null)
      {
        throw new ArgumentNullException("context");
      }
      if (!CanBuildChannelFactory<TChannel>(context))
      {
        throw new ArgumentException(
          String.Format("Unsupported channel type: {0}.", 
          typeof(TChannel).Name));
      }
      return (IChannelFactory<TChannel>)
        (object)new SerialRequestChannelFactory(this, context);
    }
  }
}

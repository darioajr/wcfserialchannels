using System;
using System.ServiceModel.Channels;
using SerialChannel.Channel.Factory;
using SerialChannel.Channel.Listener;
namespace SerialChannel.Binding.Transport
{
    class SerialTransportBindingElement : System.ServiceModel.Channels.TransportBindingElement
    {
        public string FactoryPort;
        public string ListenerPort;
        public SerialTransportBindingElement()
        {

        }

        SerialTransportBindingElement(SerialTransportBindingElement other)
            : base(other)
        {
            this.FactoryPort = other.FactoryPort;
            this.ListenerPort = other.ListenerPort;
        }

        public override string Scheme
        {
            get { return "serial"; }
        }

        public override BindingElement Clone()
        {
            return new SerialTransportBindingElement(this);
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            return typeof(TChannel) == typeof(IRequestChannel);
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (!CanBuildChannelFactory<TChannel>(context))
            {
                throw new ArgumentException(String.Format("Unsupported channel type: {0}.", typeof(TChannel).Name));
            }
            return (IChannelFactory<TChannel>)(object)new SerialRequestChannelFactory(this, context);

        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            return typeof(TChannel) == typeof(IReplyChannel);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (!CanBuildChannelListener<TChannel>(context))
            {
                throw new ArgumentException(String.Format("Unsupported channel type: {0}.", typeof(TChannel).Name));
            }
            return (IChannelListener<TChannel>)(object)new SerialReplyChannelListener(this, context);
        }

    }
}

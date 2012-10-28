using System.ServiceModel.Channels;

namespace SerialChannel.Binding.Encoding
{
  public class SerialEncoderBindingElement : MessageEncodingBindingElement
  {
    public SerialEncoderBindingElement() : base()
    {
    }

    public override MessageVersion MessageVersion
    {
      get { return MessageVersion.None; }
      set {}
    }

    public override IChannelFactory<TChannel> 
      BuildChannelFactory<TChannel>(BindingContext context)
    {
      context.RemainingBindingElements.Add(this);
      return base.BuildChannelFactory<TChannel>(context);
    }

    public override MessageEncoderFactory CreateMessageEncoderFactory()
    {
      return new SerialMessageEncoderFactory(MessageVersion);
    }

    public override BindingElement Clone()
    {
      return new SerialEncoderBindingElement();
    }
  }
}

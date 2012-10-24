using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;

namespace SerialChannel.Binding.Encoding
{
  public class SerialEncoderBindingElement : MessageEncodingBindingElement
  {
    readonly MessageEncodingBindingElement innerBindingElement;
  
    public SerialEncoderBindingElement(
      MessageEncodingBindingElement innerBindingElement)
      : base()
    {
      this.innerBindingElement = innerBindingElement;
    }

    public override MessageVersion MessageVersion
    {
      get { return this.innerBindingElement.MessageVersion; }
      set { this.innerBindingElement.MessageVersion = value; }
    }

    public override IChannelFactory<TChannel> 
      BuildChannelFactory<TChannel>(BindingContext context)
    {
      context.RemainingBindingElements.Add(this);
      return base.BuildChannelFactory<TChannel>(context);
    }

    public override MessageEncoderFactory CreateMessageEncoderFactory()
    {
      return new SerialMessageEncoderFactory(this, 
        innerBindingElement.CreateMessageEncoderFactory());
    }

    public override BindingElement Clone()
    {
      return new SerialEncoderBindingElement(this);
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;

namespace SerialChannel.Binding.Encoding
{

  class SerialMessageEncoderFactory : MessageEncoderFactory
  {
    readonly SerialEncoder encoder;
    readonly MessageEncoderFactory innerFactory;

    public SerialMessageEncoderFactory(
      SerialEncoderBindingElement bindingElement, 
      MessageEncoderFactory innerFactory)
    {
      this.innerFactory = innerFactory;
      this.encoder = new SerialEncoder(bindingElement, innerFactory.Encoder);
    }

    public override MessageEncoder Encoder
    {
      get { return this.encoder; }
    }

    public override MessageVersion MessageVersion
    {
      get { return this.innerFactory.MessageVersion; }
    }
  }
}

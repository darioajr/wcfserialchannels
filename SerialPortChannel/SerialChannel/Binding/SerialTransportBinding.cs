using System.ServiceModel.Channels;
using SerialChannel.Binding.Transport;

namespace SerialChannel.Binding
{
    public class SerialTransportBinding : System.ServiceModel.Channels.Binding
    {
        readonly MessageEncodingBindingElement messageElement;
        readonly SerialTransportBindingElement transportElement;

        //public SerialTransportBinding(int factoryPort, int listnerPort)
        public SerialTransportBinding()
        {
            this.messageElement = new TextMessageEncodingBindingElement();
            this.transportElement = new SerialTransportBindingElement();

        }

        public string FactoryPort
        {
            set { this.transportElement.FactoryPort = value; }
        }

        public string ListenerPort
        {
            set { this.transportElement.ListenerPort = value; }
        }

        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection elements = new BindingElementCollection();
            elements.Add(this.messageElement);
            elements.Add(this.transportElement);
            return elements.Clone();
        }

        public override string Scheme
        {
            get { return this.transportElement.Scheme; }
        }
    }
}

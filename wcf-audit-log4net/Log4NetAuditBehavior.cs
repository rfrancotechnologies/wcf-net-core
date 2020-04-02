using System;
using System.IO;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml;

namespace Com.RFranco.Wcf.Audit.Log4Net
{
    public class Log4NetAuditBehavior : IEndpointBehavior
    {
        public void AddBindingParameters(
            ServiceEndpoint endpoint,
            System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.ClientMessageInspectors.Add(new Log4NetAuditMessageInspector());
        }

        private class Log4NetAuditMessageInspector : IClientMessageInspector
        {
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            public void AfterReceiveReply(ref Message reply, object correlationState)
            {
                var buffer = reply.CreateBufferedCopy(Int32.MaxValue);
                reply = buffer.CreateMessage();
                LogMessage(buffer, false);
            }

            public object BeforeSendRequest(ref Message request, System.ServiceModel.IClientChannel channel)
            {
                var buffer = request.CreateBufferedCopy(Int32.MaxValue);
                request = buffer.CreateMessage();
                LogMessage(buffer, true);
                return request;
            }

            private void LogMessage(MessageBuffer buffer, bool isRequest)
            {
                var originalMessage = buffer.CreateMessage();
                string messageContent;

                using (StringWriter stringWriter = new StringWriter())
                {
                    using (XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter))
                    {
                        originalMessage.WriteMessage(xmlTextWriter);
                        xmlTextWriter.Flush();
                        xmlTextWriter.Close();
                    }
                    messageContent = stringWriter.ToString();
                }

                log.DebugFormat("Processing {0}: {1}", (isRequest ? "Request" : "Response"), messageContent);
            }
        }
    }
}


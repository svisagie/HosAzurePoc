using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace AzureSBQ
{
	public class AzureSBQSender : IDisposable
	{
		public delegate void OnMessageReceviedHandler(string message);

		public readonly int NumberOfSenders;
		private long _sendMessageCount = 0;

		readonly MessagingFactory[] _messagingFactory;
		public MessageSender[] Sender;

		public AzureSBQSender(string connectionString, string queueName, int numberOfSenders)
		{
			NumberOfSenders = numberOfSenders;

			var connBuilder = new ServiceBusConnectionStringBuilder(connectionString);
			var namespaceManager = NamespaceManager.CreateFromConnectionString(connBuilder.ToString());

			// Create queue.
			if (!namespaceManager.QueueExists(queueName))
			{
				namespaceManager.CreateQueue(queueName);
			}

			// Create messaging factories, senders and receivers.
			_messagingFactory = new MessagingFactory[NumberOfSenders];
			Sender = new MessageSender[NumberOfSenders];

			// Create senders.
			var factoryIndex = 0;
			for (var i = 0; i < NumberOfSenders; i++)
			{
				_messagingFactory[factoryIndex] = MessagingFactory.CreateFromConnectionString(connBuilder.ToString());
				Sender[i] = _messagingFactory[factoryIndex++].CreateMessageSender(queueName);
			}
		}

		public void Send(BrokeredMessage brokeredMessage)
		{
			ThreadPool.QueueUserWorkItem(state =>
			{
				var senderIndex = Math.Abs(_sendMessageCount++ % NumberOfSenders);
				Sender[senderIndex].Send(brokeredMessage);
				Trace.WriteLine(string.Format("Sent message from sender {0}", senderIndex));
			});
		}

		public void SendMessage(object message)
		{
			ThreadPool.QueueUserWorkItem(state =>
			{
				var senderIndex = Math.Abs(_sendMessageCount++ % NumberOfSenders);
				var jsonData = JsonConvert.SerializeObject(message);
				var brokeredMessage = new BrokeredMessage(jsonData);
				Sender[senderIndex].Send(brokeredMessage);
				Trace.WriteLine(string.Format("Sent message from sender {0}", senderIndex));
			});
		}

		public void SendMessageAsync(object message)
		{
			var senderIndex = Math.Abs(_sendMessageCount++ % NumberOfSenders);
			var jsonData = JsonConvert.SerializeObject(message);
			var brokeredMessage = new BrokeredMessage(jsonData);
			Sender[senderIndex].SendAsync(brokeredMessage);
			Trace.WriteLine(string.Format("Sent message from sender {0}", senderIndex));
		}

		public void Dispose()
		{
			// Close all senders, receivers and messaging factories.
			for (var i = 0; i < NumberOfSenders; i++)
			{
				Sender[i].Close();
			}
			for (var i = 0; i < NumberOfSenders; i++)
			{
				_messagingFactory[i].Close();
			}
		}
	}

}
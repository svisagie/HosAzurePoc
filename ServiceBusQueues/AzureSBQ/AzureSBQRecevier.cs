using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace AzureSBQ
{
	public class AzureSBQRecevier : IDisposable
	{
		public delegate void OnMessageReceviedHandler(BrokeredMessage message);
		public event OnMessageReceviedHandler OnMessageRecevied = delegate { };

		public readonly int NumberOfReceivers;

		readonly MessagingFactory[] _messagingFactory;
		public MessageReceiver[] Receiver;
		private bool _recevierStarted;

		public AzureSBQRecevier(string connectionString, string queueName, int numberOfReceivers, int prefetchCount)
		{
			NumberOfReceivers = numberOfReceivers;

			var connBuilder = new ServiceBusConnectionStringBuilder(connectionString);
			var namespaceManager = NamespaceManager.CreateFromConnectionString(connBuilder.ToString());

			// Create queue.
			if (!namespaceManager.QueueExists(queueName))
			{
				namespaceManager.CreateQueue(queueName);
			}

			// Create messaging factories, senders and receivers.
			_messagingFactory = new MessagingFactory[NumberOfReceivers];
			Receiver = new MessageReceiver[NumberOfReceivers];

			var factoryIndex = 0;
			// Create receivers.
			for (var i = 0; i < NumberOfReceivers; i++)
			{
				_messagingFactory[factoryIndex] = MessagingFactory.CreateFromConnectionString(connBuilder.ToString());
				Receiver[i] = _messagingFactory[factoryIndex++].CreateMessageReceiver(queueName, ReceiveMode.PeekLock);
				Receiver[i].PrefetchCount = prefetchCount;
			}
		}

		public void StartRecevier()
		{
			_recevierStarted = true;
			for (var i = 0; i < NumberOfReceivers; i++)
			{
				var recevier = Receiver[i];
				ThreadPool.QueueUserWorkItem(ProcessReceive, recevier);
			}
		}

		public void StopRecevier()
		{
			_recevierStarted = false;
		}

		public void Dispose()
		{
			// Close all senders, receivers and messaging factories.
			for (var i = 0; i < NumberOfReceivers; i++)
			{
				Receiver[i].Close();
			}
			for (var i = 0; i < NumberOfReceivers; i++)
			{
				_messagingFactory[i].Close();
			}
		}

		void ProcessReceive(Object obj)
		{
			Trace.WriteLine("Started recevier on thread {0}" + Thread.CurrentThread.ManagedThreadId);
			var receiver = (MessageReceiver)obj;
			do
			{
				try
				{
					var message = receiver.Receive();
					if (message != null)
					{
						OnMessageRecevied(message);
					}
				}
				catch (OperationCanceledException)
				{
					return;
				}
				catch (ObjectDisposedException)
				{
					return;
				}
				catch (Exception ex)
				{
					Console.WriteLine("BeginReceive returns error. {0} {1}", ex.GetType(), ex.Message);
					return;
				}
			}
			while (_recevierStarted);
		}
	}

}
﻿namespace Castle.RabbitMq.Tests
{
	using System.Collections.Generic;
	using System.Threading;
	using FluentAssertions;
	using Moq;
	using RabbitMQ.Client;
	using RabbitMQ.Client.Events;
	using RabbitMQ.Client.Framing;
	using Serializers;
	using Xunit;

	public class SharedQueueConsumerTests
	{
		private	readonly IRabbitSerializer _serializer = new JsonSerializer();

		[Fact]
		public void	Dequeue_ShouldDispatchFromSeparateThread()
		{
			var	model =	new	Mock<IModel>();

			var	sharedQConsumer	= new SharedQueueConsumer(model.Object);

			var	msgsReceived = new List<MessageEnvelope>();

			var	curThreadId	= Thread.CurrentThread.ManagedThreadId;
			int	dispatchThreadId = 0;

			
			sharedQConsumer.Subscribe(new ActionAdapter((env) =>
			{
				msgsReceived.Add(env);
				dispatchThreadId = Thread.CurrentThread.ManagedThreadId;
			}));

			var	prop = new BasicProperties();
			sharedQConsumer.Queue.Enqueue(new BasicDeliverEventArgs()
			{
				Body = _serializer.Serialize(new MyMessage(), prop),
				BasicProperties	= prop
			});

			Thread.Sleep(100);

			msgsReceived.Count.Should().Be(1);
			dispatchThreadId.Should().BeGreaterThan(0);
			curThreadId.Should().NotBe(dispatchThreadId);
		}
	}

	class MyMessage
	{
		
	}
}

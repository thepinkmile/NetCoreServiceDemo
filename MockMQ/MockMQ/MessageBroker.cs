using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MockMQ.Abstractions;

namespace MockMQ
{
    public class MessageBroker : IMessageBroker
    {
        #region Constructors

        public MessageBroker()
        {
            _queues = new Dictionary<string, Queue<IMessage>>();
        }

        #endregion

        #region Constants

        private const int ThreadSleepTimeout = 500;

        #endregion

        #region Async Methods

        public Task<IMessage> GetMessageAsync(string queueName)
        {
            var queue = EnsureMessageQueue(queueName);
            while (true)
            {
                lock (queue)
                {
                    if (queue.TryPeek(out var message) && string.IsNullOrEmpty(message.QueueName))
                    {
                        message.QueueName = queueName;
                        message.MessageBroker = this;
                        return Task.FromResult(message);
                    }
                }
                Thread.Sleep(ThreadSleepTimeout);
            }
        }

        public Task<IMessage> GetMessageAsync(string queueName, Func<IDictionary<string, object>, bool> predicateFunc)
        {
            var queue = EnsureMessageQueue(queueName);
            while (true)
            {
                lock (queue)
                {
                    if (queue.TryPeek(out var message) && string.IsNullOrEmpty(message.QueueName) &&
                        predicateFunc.Invoke(message.Properties))
                    {
                        message.QueueName = queueName;
                        message.MessageBroker = this;
                        return Task.FromResult(message);
                    }
                }
                Thread.Sleep(ThreadSleepTimeout);
            }
        }

        public Task AcceptMessageAsync(IMessage message)
        {
            if (message.MessageBroker != this || !_queues.ContainsKey(message.QueueName)) return Task.CompletedTask;
            var queue = _queues[message.QueueName];
            lock (queue)
            {
                if (queue.TryPeek(out var frontMessage) && frontMessage.Equals(message))
                    queue.Dequeue();
            }
            return Task.CompletedTask;
        }

        public Task RejectMessageAsync(IMessage message)
        {
            if (message.MessageBroker != this || !_queues.ContainsKey(message.QueueName)) return Task.CompletedTask;
            var queue = _queues[message.QueueName];
            lock (queue)
            {
                // ReSharper disable once InvertIf
                if (queue.TryPeek(out var frontMessage) && frontMessage.Equals(message))
                {
                    message.MessageBroker = null;
                    message.QueueName = null;
                }
            }
            return Task.CompletedTask;
        }

        public Task SendMessageAsync(string queueName, IMessage message)
        {
            var queue = EnsureMessageQueue(queueName);
            lock (queue)
            {
                queue.Enqueue(message);
            }
            return Task.CompletedTask;
        }

        #endregion

        #region Sync Methods

        public IMessage GetMessage(string queueName)
        {
            var queue = EnsureMessageQueue(queueName);
            while (true)
            {
                lock (queue)
                {
                    if (queue.TryPeek(out var message) && string.IsNullOrEmpty(message.QueueName))
                    {
                        message.QueueName = queueName;
                        message.MessageBroker = this;
                        return message;
                    }
                }
                Thread.Sleep(ThreadSleepTimeout);
            }
        }

        public IMessage GetMessage(string queueName, Func<IDictionary<string, object>, bool> predicateFunc)
        {
            var queue = EnsureMessageQueue(queueName);
            while (true)
            {
                lock (queue)
                {
                    if (queue.TryPeek(out var message) && string.IsNullOrEmpty(message.QueueName) &&
                        predicateFunc.Invoke(message.Properties))
                    {
                        message.QueueName = queueName;
                        message.MessageBroker = this;
                        return message;
                    }
                }
                Thread.Sleep(ThreadSleepTimeout);
            }
        }

        public void AcceptMessage(IMessage message)
        {
            if (message.MessageBroker != this || !_queues.ContainsKey(message.QueueName)) return;
            var queue = _queues[message.QueueName];
            lock (queue)
            {
                if (queue.TryPeek(out var frontMessage) && frontMessage.Equals(message))
                    queue.Dequeue();
            }
        }

        public void RejectMessage(IMessage message)
        {
            if (message.MessageBroker != this || !_queues.ContainsKey(message.QueueName)) return;
            var queue = _queues[message.QueueName];
            lock (queue)
            {
                // ReSharper disable once InvertIf
                if (queue.TryPeek(out var frontMessage) && frontMessage.Equals(message))
                {
                    message.MessageBroker = null;
                    message.QueueName = null;
                }
            }
        }

        public void SendMessage(string queueName, IMessage message)
        {
            var queue = EnsureMessageQueue(queueName);
            lock (queue)
            {
                queue.Enqueue(message);
            }
        }

        #endregion

        #region Internal

        private Queue<IMessage> EnsureMessageQueue(string queueName)
        {
            if (_queues.ContainsKey(queueName)) return _queues[queueName];
            var newQueue = new Queue<IMessage>();
            _queues.Add(queueName, newQueue);
            return newQueue;
        }

        #endregion

        #region Fields

        private readonly IDictionary<string, Queue<IMessage>> _queues;

        #endregion
    }
}

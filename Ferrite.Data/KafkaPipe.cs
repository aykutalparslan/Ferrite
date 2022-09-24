//
//  Project Ferrite is an Implementation of the Telegram Server API
//  Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Affero General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Affero General Public License for more details.
//
//  You should have received a copy of the GNU Affero General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
using System;
using System.Net;
using System.Threading.Channels;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace Ferrite.Data;

public class KafkaPipe : IMessagePipe
{
    private readonly IProducer<Null,byte[]> _producer;
    private readonly IConsumer<Ignore,byte[]> _consumer;
    private readonly IAdminClient _adminClient;
    private string _channel;
    private bool _unsubscribed = true;
    private Task? consumeTask;
    private readonly CancellationTokenSource _consumeCts = new CancellationTokenSource();
    private readonly Channel<byte[]> _consumed = Channel.CreateUnbounded<byte[]>();
    public KafkaPipe(string config)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config,
            ClientId = Dns.GetHostName()
        };
        _producer = new ProducerBuilder<Null, byte[]>(producerConfig).Build();
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config,
            GroupId = "ferrite",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<Ignore, byte[]>(consumerConfig).Build();
        _adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = config }).Build();
    }

    public async ValueTask<byte[]> ReadMessageAsync(CancellationToken cancellationToken = default)
    {
        return await _consumed.Reader.ReadAsync(cancellationToken);
    }

    public async ValueTask<bool> SubscribeAsync(string channel)
    {
        if(_consumer == null)
        {
            throw new ObjectDisposedException("IConsumer");
        }
        await CreateChannelAsync(channel);
        _channel = channel;
        _unsubscribed = false;
        consumeTask = Task.Run(() =>
        {
            using (_consumer)
            {
                _consumer.Subscribe(_channel);
                while (!_unsubscribed)
                {
                    var consumeResult = _consumer.Consume(_consumeCts.Token);
                    _consumed.Writer.TryWrite(consumeResult.Message.Value);
                }
                _consumer.Close();
            }
        });
        return true;
    }

    private async ValueTask<bool> CreateChannelAsync(string channel)
    {
        try
        {
            await _adminClient.CreateTopicsAsync(new TopicSpecification[] {
                    new TopicSpecification { Name = channel, ReplicationFactor = 1, NumPartitions = 1 } });
        }
        catch (CreateTopicsException e)
        {
            Console.WriteLine($"An error occured creating channel {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
        }
        return true;
    }

    public async ValueTask<bool> UnSubscribeAsync()
    {
        if (!_unsubscribed)
        {
            _unsubscribed = true;
            _consumeCts.Cancel();
        }
        return true;
    }

    public async ValueTask<bool> WriteMessageAsync(string channel, byte[] message)
    {
        await _producer.ProduceAsync(channel, new Message<Null, byte[]> { Value = message });
        return true;
    }
}


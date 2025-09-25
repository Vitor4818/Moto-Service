using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using MotoModel.Entities;

namespace MotoApi.Messaging
{
    public class MotoEventPublisher
    {
        private readonly IConnection _connection;
        private readonly string _queueNameCreate = "motoQueue";
        private readonly string _queueNameUpdate = "motoUpdatedQueue";

        public MotoEventPublisher(IConnection connection)
        {
            _connection = connection;
        }

        public void PublishCreate(Moto moto)
        {
            using var channel = _connection.CreateModel();

            channel.QueueDeclare(
                queue: _queueNameCreate,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var json = JsonSerializer.Serialize(new
            {
                moto.id,
                moto.tipoMoto,
                moto.placa,
                moto.numChassi,
                moto.TagRfidId
            });

            var body = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(
                exchange: "",
                routingKey: _queueNameCreate,
                basicProperties: null,
                body: body);

            Console.WriteLine($"[x] Evento de CRIAÇÃO publicado para moto {moto.id}");
        }

        public void PublishUpdate(Moto moto)
        {
            using var channel = _connection.CreateModel();

            channel.QueueDeclare(
                queue: _queueNameUpdate,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var json = JsonSerializer.Serialize(new
            {
                id = moto.id,
                TipoMoto = moto.tipoMoto,
                Placa = moto.placa,
                NumChassi = moto.numChassi,
                Tag = moto.TagRfidId
            });

            var body = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(
                exchange: "",
                routingKey: _queueNameUpdate,
                basicProperties: null,
                body: body);

            Console.WriteLine($"[x] Evento de ATUALIZAÇÃO publicado para moto {moto.id}");
        }
    }
}

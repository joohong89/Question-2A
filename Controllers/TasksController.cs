using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api_producer.Model;
using api_producer.Dto;
using RestSharp;
using RestSharp.Serialization.Json;
using RabbitMQ.Client;
using System.Text;

namespace api_producer.Controllers
{
    [Route("/task")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly TaskRecordDbContext _context;

        public TasksController(TaskRecordDbContext context)
        {
            _context = context;
        }

        // POST: /task
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Model.TaskRecord>> PostTask(UserTaskDto userTask)
        {
             

            // Call to API
            var client = new RestClient("https://reqres.in/");
            var request = new RestRequest("/api/login", Method.POST);

            request.AddJsonBody(userTask);

            IRestResponse response = client.Execute(request);
            var content = response.Content; // {"message":" created."}

            TokenDto tokenDto = new JsonDeserializer().Deserialize<TokenDto>(response);
            TokenDto token = tokenDto;

            // check if token is recieved
            if (tokenDto == null || tokenDto.token == null) {
                return Unauthorized();
            }

            // publish to queue

            var factory = new ConnectionFactory() {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT"))
                // HostName = "localhost", 
                // Port= 31672
            };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "TaskQueue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

        
                var body = Encoding.UTF8.GetBytes(userTask.task);

                channel.BasicPublish(exchange: "",
                                     routingKey: "TaskQueue",
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent {0}", userTask.task);
            }



            // Record transaction in db
            TaskRecord record = new TaskRecord();
            record.email = userTask.email;
            record.task = userTask.task; 

            _context.UserTask.Add(record);
            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}

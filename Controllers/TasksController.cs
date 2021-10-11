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
        private readonly UserTaskContext _context;

        public TasksController(UserTaskContext context)
        {
            _context = context;
        }
/*
        // GET: api/UserTasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserTask>>> GetUserTask()
        {
            return await _context.UserTask.ToListAsync();
        }

        // GET: api/UserTasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserTask>> GetUserTask(long id)
        {
            var userTask = await _context.UserTask.FindAsync(id);

            if (userTask == null)
            {
                return NotFound();
            }

            return userTask;
        }

        // PUT: api/UserTasks/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserTask(long id, UserTask userTask)
        {
            if (id != userTask.id)
            {
                return BadRequest();
            }

            _context.Entry(userTask).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserTaskExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
*/
        // POST: api/UserTasks
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Model.Task>> PostTask(UserTask userTask)
        {
            // _context.UserTask.Add(userTask);
            // await _context.SaveChangesAsync();

            // Call to API
            var client = new RestClient("https://reqres.in/");
            var request = new RestRequest("/api/login", Method.POST);

            request.AddJsonBody(userTask);

            IRestResponse response = client.Execute(request);
            var content = response.Content; // {"message":" created."}

            TokenDto tokenDto = new JsonDeserializer().Deserialize<TokenDto>(response);
            TokenDto token = tokenDto;


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

       

            return Ok();
        }
/*
        // DELETE: api/UserTasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserTask(long id)
        {
            var userTask = await _context.UserTask.FindAsync(id);
            if (userTask == null)
            {
                return NotFound();
            }

            _context.UserTask.Remove(userTask);
            await _context.SaveChangesAsync();

            return NoContent();
        }
*/
        private bool UserTaskExists(long id)
        {
            return _context.UserTask.Any(e => e.id == id);
        }
    }
}

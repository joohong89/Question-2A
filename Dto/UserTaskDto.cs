using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api_producer.Dto
{
    public class UserTaskDto
    {
        public string email { get; set; }
        public string password { get; set; }
        public string task { get; set; }
    }
}

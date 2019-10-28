using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebHomework.Models;

namespace WebHomework.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QueriesController : ControllerBase
    {
        private readonly UserContext _db;
        private readonly IHttpContextAccessor _accessor;
        public QueriesController(UserContext context, IHttpContextAccessor accessor)
        {
            _accessor = accessor;
            _db = context;
            // if database doesn't consists any users, add two new users to database
            if (!_db.Users.Any())
            {
                _db.Users.Add(new User { Name = "Tom", Age = 26 });
                _db.Users.Add(new User { Name = "Alice", Age = 31 });
                _db.SaveChanges();
            }
        }

        // get all users from table
        [HttpGet]
        public IEnumerable<User> Get()
        {
            WriteToLogFile("GET");
            return _db.Users.ToList();
        }

        // get user by id
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            User user = _db.Users.FirstOrDefault(x => x.Id == id);
            WriteToLogFile("GET");
            if (user == null)
                return NotFound();
            return new ObjectResult(user);
        }

        // add new user to table
        [HttpPost]
        public IActionResult Post([FromBody]User user)
        {
            if (user == null)
            {
                return BadRequest();
            }

            _db.Users.Add(user);
            _db.SaveChanges();
            WriteToLogFile("POST");
            return Ok(user);
        }

        // edit existing user from table
        [HttpPut]
        public IActionResult Put([FromBody]User user)
        {
            if (user == null)
            {
                return BadRequest();
            }
            if (!_db.Users.Any(x => x.Id == user.Id))
            {
                return NotFound();
            }

            _db.Update(user);
            _db.SaveChanges();
            WriteToLogFile("PUT");
            return Ok(user);
        }

        // delete user by id from table
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            User user = _db.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            _db.Users.Remove(user);
            _db.SaveChanges();
            WriteToLogFile("DELETE");
            return Ok(user);
        }

        // write info about query in log file
        private void WriteToLogFile(string queryType)
        {
            var ip = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            var url = _accessor.HttpContext.Request.Scheme.ToString();
            url += "://" + _accessor.HttpContext.Request.Host.ToString();
            url += _accessor.HttpContext.Request.Path.ToString();
            DateTime dateTime = DateTime.Now;
            using (StreamWriter sw = new StreamWriter("log.txt", true, System.Text.Encoding.Default))
            {
                sw.WriteLineAsync("-----------------------------------------------------");
                sw.WriteLineAsync("Time: " + dateTime.ToString());
                sw.WriteLineAsync("Query type: " + queryType);
                sw.WriteLineAsync("IP-address: " + ip);
                sw.WriteLineAsync("URL: " + url);
            }
        }
    }
}

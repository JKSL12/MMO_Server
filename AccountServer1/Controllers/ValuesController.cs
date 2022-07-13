using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountServer.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccountServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        AppDbContext _context;

        public ValuesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("create")]
        public CreateAccountPacketRes CreateAccount([FromBody] CreateAccountPacketReq req)
        {
            CreateAccountPacketRes res = new CreateAccountPacketRes();

            AccountDb account = _context.Accounts
                                    .AsNoTracking()
                                    .Where(a => a.AccountName == req.AccountName)
                                    .FirstOrDefault();

            if (account == null)
            {
                _context.Accounts.Add(new AccountDb()
                {
                    AccountName = req.AccountName,
                    Password = req.Password
                });

                bool success = _context.SaveChangesEx();
                res.CreateOk = success;
            }
            else
            {
                res.CreateOk = false;
            }

            return res;
        }

        [HttpPost]
        [Route("login")]
        public LoginAccountPacketRes LoginAccount([FromBody] LoginAccountPacketReq req)
        {
            LoginAccountPacketRes res = new LoginAccountPacketRes();

            AccountDb account = _context.Accounts
                .AsNoTracking()
                .Where(a => a.AccountName == req.AccountName && a.Password == req.Password)
                .FirstOrDefault();

            if (account == null)
            {
                res.LoginOk = false;
            }
            else
            {
                res.LoginOk = true;

                // TODO 서버 목록
                res.ServerList = new List<ServerInfo>()
                {
                    new ServerInfo() {Name = "데포르쥬", Ip = "127.0.0.1", CrowdedLevel = 0 },
                    new ServerInfo() {Name = "아툰", Ip = "127.0.0.1", CrowdedLevel = 3 }
                };
            }

            return res;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using App2.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace App2.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class NavController : ControllerBase
    {
        DatabaseHelper db = new DatabaseHelper();
        // GET: api/Nav
        [HttpGet]
        public IEnumerable<NavModel> Get()
        {
            db.ClearParameter();
            DataTable dtc = db.ExecuteDataTable("SELECT ID,UPPER(TenMenu) 'NameMenu',routerName FROM dbo.Menu_list WITH (NOLOCK)");
            List<NavModel> navlist = new List<NavModel>();
            foreach (DataRow dr in dtc.Rows)
            {
                NavModel arry = new NavModel();
                arry.id = dr["ID"].ToString();
                arry.namemenu = dr["NameMenu"].ToString();
                arry.routername = dr["routerName"].ToString();
                if(dr["routerName"].ToString() == "home")
                {
                    arry.active = "true";
                }
                else
                {
                    arry.active = "false";
                }
                navlist.Add(arry);
            }
            return navlist.ToArray();
        }

        //// GET: api/Nav/5
        //[HttpGet("{id}", Name = "Get")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST: api/Nav
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT: api/Nav/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE: api/ApiWithActions/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}

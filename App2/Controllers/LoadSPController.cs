using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using App2.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;



namespace App2.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoadSPController : ControllerBase
    {
        DatabaseHelper db = new DatabaseHelper();
        private readonly ILogger<LoadSPController> _logger;
        public LoadSPController(ILogger<LoadSPController> logger)
        {
            _logger = logger;
        }
        [HttpGet]
        public IEnumerable<ListSP> Get()
        {
            DataTable dt = db.ExecuteDataTable("SELECT * FROM dbo.SP_info_list WITH (NOLOCK)");
            List<ListSP> listEmployees = new List<ListSP>();
            foreach (DataRow dr in dt.Rows)
            {
                ListSP arry = new ListSP();
                arry.MaSP = dr["MaSP"].ToString();
                arry.TenSP = dr["TenSP"].ToString();
                arry.TenSPKoDau = dr["TenSPKoDau"].ToString();
                arry.TenPhu = dr["TenPhu"].ToString();
                arry.giamua = Convert.ToInt32(dr["GiaMua"].ToString());
                arry.giasale = Convert.ToInt32(dr["GiaSale"].ToString());
                arry.DateCreate = Convert.ToDateTime(dr["DateInsert"].ToString());
                arry.UserCreate = dr["UserCreate"].ToString();
                listEmployees.Add(arry);
            }
            return listEmployees.ToArray();
        }
       
    }
}
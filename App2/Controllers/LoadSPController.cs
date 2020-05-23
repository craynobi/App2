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
        public IEnumerable<ListSP> Get(string id ="")
        {
            db.ClearParameter();
            db.AddParameter("@ID", id);
            DataTable dt = db.ExecuteDataTable("Shop_Get_Product",CommandType.StoredProcedure,ConnectionState.CloseOnExit);
            List<ListSP> listEmployees = new List<ListSP>();
            foreach (DataRow dr in dt.Rows)
            {
                ListSP arry = new ListSP();
                arry.MaSP = dr["MaSP"].ToString();
                arry.TenSP = dr["TenSP"].ToString();
                arry.TenSPKoDau = dr["TenSPKoDau"].ToString();
                arry.TenPhu = dr["TenPhu"].ToString();
                arry.giamua = Convert.ToInt32(dr["GiaMua"].ToString() == "" ? "0" : dr["GiaMua"].ToString());
                arry.giasale = Convert.ToInt32(dr["GiaSale"].ToString() == "" ? "0" : dr["GiaSale"].ToString());
                arry.DateCreate = Convert.ToDateTime(dr["NgayInsert"].ToString());
                arry.UserCreate = dr["NguoiTao"].ToString();
                listEmployees.Add(arry);
            }
            return listEmployees.ToArray();
        }
       
    }
}
using Microsoft.AspNetCore.Mvc;
using TestsAPiss.Data;

namespace MyStockAPIs.Controllers
{
    //[Route("api/[controller]")]
    [Route("api/v1/stock")]
    [ApiController]
    public class StockControllers:ControllerBase
    {
        private readonly ApplicationDBContext _context;
        public StockControllers(ApplicationDBContext context)
        {
            _context = context;
        }

        // making our endpoint 1
        //Get all stock
        [HttpGet]
        public IActionResult GetAllStockInDb() 
        {
            var myStocks = _context.Stocks.ToList();
            return Ok(myStocks);
        }

        //Get stock by id
        [HttpGet("{id}")]
        public IActionResult GetStockById([FromRoute] int id)
        {
            var singleStock = _context.Stocks.Find(id);
            if(singleStock==null)
            {
                return NotFound(); // a form of IActionResult
            }
            return Ok(singleStock); 
        }

    }
}

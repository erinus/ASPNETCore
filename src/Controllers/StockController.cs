using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace app.Controllers
{
    using app.Models;

    [ApiController]
    [Route("[controller]")]
    public class StockController : ControllerBase
    {
        private readonly ILogger<StockController> _logger;

        private StockContext _stockContext;

        // 透過 DI 機制存取已在 Startup.cs 註冊的 StockContext
        public StockController(ILogger<StockController> logger, StockContext stockContext)
        {
            _logger = logger;
            _stockContext = stockContext;
        }

        [HttpGet]
        public IEnumerable<Stock> Get()
        {
            return _stockContext.Stocks;
        }

        [HttpPost]
        public IEnumerable<Stock> Post(Stock body)
        {
            _stockContext.Add(body);
            _stockContext.SaveChanges();
            return _stockContext.Stocks;
        }

        [HttpPatch]
        public IEnumerable<Stock> Patch(Stock body)
        {
            Stock stock = _stockContext.Stocks.FirstOrDefault(s => s.Id == body.Id);
            stock.Merge(body);
            _stockContext.Update(stock);
            _stockContext.SaveChanges();
            return _stockContext.Stocks;
        }

        [HttpDelete]
        public IEnumerable<Stock> Delete(Stock body)
        {
            Stock stock = _stockContext.Stocks.FirstOrDefault(s => s.Id == body.Id);
            _stockContext.Remove(stock);
            _stockContext.SaveChanges();
            return _stockContext.Stocks;
        }
    }
}

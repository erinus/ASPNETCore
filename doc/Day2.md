# Entity Framework Core
## 安裝套件
`存取 MySQL / MariaDB`
``` shell
> docker run --rm -it -v ${PWD}/src:/app -w /app mcr.microsoft.com/dotnet/sdk:3.1 dotnet add package MySql.Data -v 8.0.22
> docker run --rm -it -v ${PWD}/src:/app -w /app mcr.microsoft.com/dotnet/sdk:3.1 dotnet add package MySql.Data.EntityFrameworkCore -v 8.0.22
```
`讀取 appsettings.json`
``` shell
> docker run --rm -it -v ${PWD}/src:/app -w /app mcr.microsoft.com/dotnet/sdk:3.1 dotnet add package Microsoft.Extensions.Configuration.Json -v 3.1.10
```
## 問題說明
   - 如果發現引入套件後，程式可正常編譯執行，但在 Visual Studio Code 中，程式碼仍出現未引入套件或不存在函數之類的錯誤提示，請執行 View > Command Palette > `.NET: Restore Project`，使 OmniSharp 藉由還原套件過程重建組件類別資訊索引
   - 依據 [MySQL Entity Framework Core Support](https://dev.mysql.com/doc/connector-net/en/connector-net-entityframework-core.html) 指出 MySql.Data.EntityFrameworkCore 套件必須到 8.0.20 版才開始支援 .NET Core 3.1
## 觀念釐清
1. DB First
   開發時優先設計資料庫和資料表，程式則依照資料庫架構進行對應的設計資料實體，並藉由其存取資料庫
2. Code First
   開發時以程式需求為出發點，資料庫和資料表的規劃、建立和管理，都透過 Entity Framework 將程式中設定要儲存於資料庫的資料實體進行對應的轉換
   ![Code First Approach](https://www.entityframeworktutorial.net/images/EF5/code-first.png)
## 資料庫設定
1. 建立 stock 資料庫
   ``` sql
   CREATE DATABASE IF NOT EXISTS `stock`;
   ```
2. 建立 stocks 資料表
   ``` sql
   USE `stock`;
   CREATE TABLE IF NOT EXISTS `stocks` (
     `id` int(11) NOT NULL AUTO_INCREMENT,
     `code` varchar(8) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
     `time` timestamp NOT NULL DEFAULT '0000-00-00 00:00:00',
     `open` decimal(10,2) NOT NULL DEFAULT 0.00,
     `high` decimal(10,2) NOT NULL DEFAULT 0.00,
     `low` decimal(10,2) NOT NULL DEFAULT 0.00,
     `close` decimal(10,2) NOT NULL DEFAULT 0.00,
   PRIMARY KEY (`id`)
   ) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
   INSERT INTO `stocks` (`code`, `time`, `open`, `high`, `low`, `close`) VALUES
	 ('2330', '2020-12-02 12:00:00', 490.00, 495.00, 489.00, 492.00),
	 ('2330', '2020-12-02 12:01:00', 492.00, 495.00, 489.00, 492.00);
   ```
### `appsettings.json`
``` json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "StockConnection": "server=mariadb;database=stock;user=root;password=root"
  }
}
```
> 在 Docker Compose 建立出來的服務群中，會自動把 Service Name 關聯容器的 Private IP
### `Startup.cs`
``` csharp
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

...

public void ConfigureServices(IServiceCollection services)
{
    services.AddResponseCaching();

    // services.AddDbContext<StockContext>(options =>
    // {
    //     // 讀取 appsettings.json
    //     IConfigurationRoot configuration = new ConfigurationBuilder()
    //         .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
    //         .AddJsonFile("appsettings.json")
    //         .Build();
    //     // 設定 MySQL 連線字串
    //     options.UseMySQL(
    //         Configuration.GetConnectionString("StockConnection")
    //     );
    // });
    services.AddDbContextPool<StockContext>(options =>
    {
        // 讀取 appsettings.json
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
            .AddJsonFile("appsettings.json")
            .Build();
        // 設定 MySQL 連線字串
        options.UseMySQL(
            Configuration.GetConnectionString("StockConnection")
        );
    });

    services.AddControllers();
}
```
> `AddDbContext` 和 `AddDbContextPool` 的差異點在於是否使用連線池，一般狀況下 `AddDbContextPool` 性能會較好
### `StockContext.cs`
``` csharp
using Microsoft.EntityFrameworkCore;

namespace app
{
    using Models;

    public class StockContext : DbContext
    {
        public StockContext(DbContextOptions<StockContext> options) : base(options) {}

        public DbSet<Stock> Stocks { get;set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Stock>().ToTable("stocks");
        }
    }
}
```
### `Models\Stock.cs`
``` csharp
using System;
using System.ComponentModel.DataAnnotations;

namespace app.Models
{
    public class Stock
    {
        [Key]
        public int Id { get; set; }

        // 日期
        public DateTime Time { get; set; }

        // 股票代碼
        public string Code { get; set; }

        // 開盤價
        public double Open { get; set; }

        // 最高價
        public double High { get; set; }

        // 最低價
        public double Low { get; set; }

        // 收盤價
        public double Close { get; set; }
    }
}
```
## 查詢資料
### `Controllers\StockController.cs`
``` csharp
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
    }
}
```
開啟瀏覽器連線 https://localhost:5001/Stock 測試，取得 JSON 回應如下
``` json
[
    {
        "id":1,
        "time":"2020-12-02T12:00:00Z",
        "code":"2330",
        "open":490,
        "high":495,
        "low":489,
        "close":492
    },
    {
        "id":2,
        "time":"2020-12-02T12:01:00Z",
        "code":"2330",
        "open":492,
        "high":495,
        "low":489,
        "close":492
    }
]
```
## 條件化查詢資料
### `Controllers\StockController.cs`
``` csharp
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
            return _stockContext.Stocks
                .Where(stock => stock.Open > 491);
        }
    }
}
```
開啟瀏覽器連線 https://localhost:5001/Stock 測試，取得 JSON 回應如下
``` json
[
    {
        "id":2,
        "time":"2020-12-02T12:01:00Z",
        "code":"2330",
        "open":492,
        "high":495,
        "low":489,
        "close":492
    }
]
```
> 複雜的查詢操作，請參考 [Complex Query Operator](https://docs.microsoft.com/zh-tw/ef/core/querying/complex-query-operators)
## 寫入資料

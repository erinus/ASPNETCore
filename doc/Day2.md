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

        // 提供存取資料表的欄位
        public DbSet<Stock> Stocks { get;set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // - 配置資料表結構在程式中對應的類別
            //   Entity<Stock>()
            // - 配置欄位與資料表的對應關係
            //   ToTable("stocks")
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
                .Where(s => s.Open > 491);
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
## 新增資料
### `Controllers\StockController.cs`
``` csharp
namespace app.Controllers
{
    using app.Models;

    [ApiController]
    [Route("[controller]")]
    public class StockController : ControllerBase
    {
        ...

        [HttpPost]
        public IEnumerable<Stock> Post()
        {
            Stock stock = new Stock
            {
                Code = "2330",
                Time = DateTime.Parse("2020-12-02 12:02:00"),
                Open = 492,
                High = 497,
                Low = 491,
                Close = 494
            };
            _stockContext.Add(stock);
            _stockContext.SaveChanges();
            return _stockContext.Stocks;
        }
    }
}
```
開啟 Postman 新增請求  
POST https://localhost:5001/Stock  
執行請求後取得 JSON 回應如下
``` json
[
    {
        "id": 1,
        "time": "2020-12-02T12:00:00Z",
        "code": "2330",
        "open": 490,
        "high": 495,
        "low": 489,
        "close": 492
    },
    {
        "id": 2,
        "time": "2020-12-02T12:01:00Z",
        "code": "2330",
        "open": 492,
        "high": 495,
        "low": 489,
        "close": 492
    },
    {
        "id": 3,
        "time": "2020-12-02T12:02:00",
        "code": "2330",
        "open": 492,
        "high": 497,
        "low": 491,
        "close": 494
    }
]
```
## 透過 JSON 請求新增資料
### `Controllers\StockController.cs`
``` csharp
namespace app.Controllers
{
    using app.Models;

    [ApiController]
    [Route("[controller]")]
    public class StockController : ControllerBase
    {
        ...

        [HttpPost]
        public IEnumerable<Stock> Post(Stock body)
        {
            _stockContext.Add(body);
            _stockContext.SaveChanges();
            return _stockContext.Stocks;
        }
    }
}
```
開啟 Postman 新增請求  
POST https://localhost:5001/Stock  
Body
``` json
{
    "time": "2020-12-03T12:03:00",
    "code": "2330",
    "open": 492,
    "high": 492,
    "low": 488,
    "close": 491
}
```
執行請求後取得 JSON 回應如下
``` json
[
    {
        "id": 1,
        "time": "2020-12-02T12:00:00Z",
        "code": "2330",
        "open": 490,
        "high": 495,
        "low": 489,
        "close": 492
    },
    {
        "id": 2,
        "time": "2020-12-02T12:01:00Z",
        "code": "2330",
        "open": 492,
        "high": 495,
        "low": 489,
        "close": 492
    },
    {
        "id": 3,
        "time": "2020-12-02T12:02:00",
        "code": "2330",
        "open": 492,
        "high": 497,
        "low": 491,
        "close": 494
    },
    {
        "id": 4,
        "time": "2020-12-03T12:03:00",
        "code": "2330",
        "open": 492,
        "high": 492,
        "low": 488,
        "close": 491
    }
]
```
## 透過 JSON 請求修改整筆資料
### `Controllers\StockController.cs`
``` csharp
namespace app.Controllers
{
    using app.Models;

    [ApiController]
    [Route("[controller]")]
    public class StockController : ControllerBase
    {
        ...

        [HttpPatch]
        public IEnumerable<Stock> Patch(Stock body)
        {
            _stockContext.Update(body);
            _stockContext.SaveChanges();
            return _stockContext.Stocks;
        }
    }
}
```
開啟 Postman 新增請求  
PATCH https://localhost:5001/Stock  
Body
``` json
{
    "id": 4,
    "time": "2020-12-03T12:03:00",
    "code": "2330",
    "open": 492,
    "high": 492,
    "low": 488,
    "close": 489
}
```
執行請求後取得 JSON 回應如下
``` json
[
    {
        "id": 1,
        "time": "2020-12-02T12:00:00Z",
        "code": "2330",
        "open": 490,
        "high": 495,
        "low": 489,
        "close": 492
    },
    {
        "id": 2,
        "time": "2020-12-02T12:01:00Z",
        "code": "2330",
        "open": 492,
        "high": 495,
        "low": 489,
        "close": 492
    },
    {
        "id": 3,
        "time": "2020-12-02T12:02:00Z",
        "code": "2330",
        "open": 492,
        "high": 497,
        "low": 491,
        "close": 494
    },
    {
        "id": 4,
        "time": "2020-12-03T12:03:00",
        "code": "2330",
        "open": 492,
        "high": 492,
        "low": 488,
        "close": 489
    }
]
```
## 透過 JSON 請求修改部分資料
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
        public DateTime? Time { get; set; }

        // 股票代碼
        public string Code { get; set; }

        // 開盤價
        public Double? Open { get; set; }

        // 最高價
        public Double? High { get; set; }

        // 最低價
        public Double? Low { get; set; }

        // 收盤價
        public Double? Close { get; set; }
    }
}
```
### `Models\StockExtensions.cs`
``` csharp
using System;

namespace app.Models
{
    public static class StockExtensions
    {
        public static void Merge(this Stock instanceA, Stock instanceB)
        {
            if (instanceA != null && instanceB != null)
            {
                if(instanceB.Code != null)
                {
                    instanceA.Code = instanceB.Code;
                }

                if(instanceB.Time != null)
                {
                    instanceA.Time = instanceB.Time;
                }

                if(instanceB.Open != null)
                {
                    instanceA.Open = instanceB.Open;
                }

                if(instanceB.High != null)
                {
                    instanceA.High = instanceB.High;
                }

                if(instanceB.Low != null)
                {
                    instanceA.Low = instanceB.Low;
                }

                if(instanceB.Close != null)
                {
                    instanceA.Close = instanceB.Close;
                }
            }
        }
    }
}
```
### `Controllers\StockController.cs`
``` csharp
namespace app.Controllers
{
    using app.Models;

    [ApiController]
    [Route("[controller]")]
    public class StockController : ControllerBase
    {
        ...

        [HttpPatch]
        public IEnumerable<Stock> Patch(Stock body)
        {
            Stock stock = _stockContext.Stocks.FirstOrDefault(s => s.Id == body.Id);
            stock.Merge(body);
            _stockContext.Update(stock);
            _stockContext.SaveChanges();
            return _stockContext.Stocks;
        }
    }
}
```
開啟 Postman 新增請求  
PATCH https://localhost:5001/Stock  
Body
``` json
{
    "id": 4,
    "close": 491
}
```
執行請求後取得 JSON 回應如下
``` json
[
    {
        "id": 1,
        "time": "2020-12-02T12:00:00Z",
        "code": "2330",
        "open": 490,
        "high": 495,
        "low": 489,
        "close": 492
    },
    {
        "id": 2,
        "time": "2020-12-02T12:01:00Z",
        "code": "2330",
        "open": 492,
        "high": 495,
        "low": 489,
        "close": 492
    },
    {
        "id": 3,
        "time": "2020-12-02T12:02:00Z",
        "code": "2330",
        "open": 492,
        "high": 497,
        "low": 491,
        "close": 494
    },
    {
        "id": 4,
        "time": "2020-12-03T12:03:00",
        "code": "2330",
        "open": 492,
        "high": 492,
        "low": 488,
        "close": 491
    }
]
```
## 刪除資料
### `Controllers\StockController.cs`
``` csharp
namespace app.Controllers
{
    using app.Models;

    [ApiController]
    [Route("[controller]")]
    public class StockController : ControllerBase
    {
        ...

        [HttpDelete]
        public IEnumerable<Stock> Delete(Stock body)
        {
            _stockContext.Remove(body);
            _stockContext.SaveChanges();
            return _stockContext.Stocks;
        }
    }
}
```
開啟 Postman 新增請求  
DELETE https://localhost:5001/Stock  
Body
``` json
{
    "id": 4
}
```
執行請求後取得 JSON 回應如下
``` json
[
    {
        "id": 1,
        "time": "2020-12-02T12:00:00Z",
        "code": "2330",
        "open": 490,
        "high": 495,
        "low": 489,
        "close": 492
    },
    {
        "id": 2,
        "time": "2020-12-02T12:01:00Z",
        "code": "2330",
        "open": 492,
        "high": 495,
        "low": 489,
        "close": 492
    },
    {
        "id": 3,
        "time": "2020-12-02T12:02:00Z",
        "code": "2330",
        "open": 492,
        "high": 497,
        "low": 491,
        "close": 494
    }
]
```
# LINE Bot
## 安裝 ngrok
1. 至 ngrok 官網 [Download](https://ngrok.com/download) 頁面下載 Windows 版本，並解壓縮出 ngrok.exe
2. 至 ngrok 官網 [Signup](https://dashboard.ngrok.com/signup) 頁面註冊帳號
3. 註冊完成後，在官網 [Setup & Installation](https://dashboard.ngrok.com/get-started/setup) 頁面的 `Connect your account` 區塊可以取得驗證權杖
4. 複製指令在本機執行，會產出設定檔  
   ``` shell
   > ./ngrok authtoken <token>

   Authtoken saved to configuration file: C:\Users\<username>/.ngrok2/ngrok.yml
   ```
5. 執行
   ``` shell
   > ./ngrok http 5000

   ngrok by @inconshreveable

   Session Status                online
   Account                       erinus.startup@gmail.com (Plan: Free)
   Version                       2.3.35
   Region                        United States (us)
   Web Interface                 http://127.0.0.1:4040
   Forwarding                    http://2032f71a1524.ngrok.io -> http://localhost:5000
   Forwarding                    https://2032f71a1524.ngrok.io -> http://localhost:5000

   Connections                   ttl     opn     rt1     rt5     p50     p90
                                 0       0       0.00    0.00    0.00    0.00
   ```
   > 如果出現錯誤訊息 `listen tcp 127.0.0.1:4040: bind: An attempt was made to access a socket in a way forbidden by its access permissions.`，請重新開機
6. 開啟瀏覽器連線 https://2032f71a1524.ngrok.io/Stock 測試，取得 JSON 回應如下
   ``` json
   [
       {
           "id": 1,
           "time": "2020-12-02T12:00:00Z",
           "code": "2330",
           "open": 490,
           "high": 495,
           "low": 489,
           "close": 492
       },
       {
           "id": 2,
           "time": "2020-12-02T12:01:00Z",
           "code": "2330",
           "open": 492,
           "high": 495,
           "low": 489,
           "close": 492
       },
       {
           "id": 3,
           "time": "2020-12-02T12:02:00Z",
           "code": "2330",
           "open": 492,
           "high": 497,
           "low": 491,
           "close": 494
       }
   ]
   ```
## 申請 LINE Bot
1. 至 [LINE Developers](https://developers.line.biz/) 登入
2. 在 Providers 頁面 新增 Provider
3. 進入 Provider 頁面後，選取 Create a Messaging API channel
4. 建立頻道
   - 在 `Channel name` 欄位輸入 `Stock`
   - 在 `Channel description` 欄位輸入 `Stock`
   - 在 `Category` 欄位選取 `銀行、保險、金融`
   - 在 `Subcategory` 欄位選取 `銀行、保險、金融（其他）`
   - 勾選 `I have read and agree to the LINE Official Account Terms of Use`
   - 勾選 `I have read and agree to the LINE Official Account API Terms of Use`
   - 點擊 `Create`
5. 設定頻道
   - 進入 `Messaging API` 頁籤
   - 在 `Webhook settings` 區塊內的 `Webhook URL`，點擊 `Edit`，輸入從 ngrok 取得的網址（要加上配給 LINE Bot 的 EndPoint /LineBot）  
     https://\<random\>.ngrok.io/LineBot
   - 在 `Webhook settings` 區塊內的 `Use webhook`，切換為啟用
   - 在 `LINE Official Account features` 區塊內的 `Allow bot to join group chats`，點擊 `Edit`，在彈出的 `帳號設定` 頁面
     - 在 `聊天` 區塊內的 `加入群組或多人聊天室`，選取 `接受邀請加入群組或多人聊天室`
   - 在 `LINE Official Account features` 區塊內的 `Auto-reply messages`，點擊 `Edit`，在彈出的 `回應設定` 頁面
     - 在 `進階設定` 區塊內的 `自動回應訊息`，選取 `停用`
## 加入 LINE Bot
1. 進入 `Messaging API` 頁籤
2. 在 `Bot information` 區塊內的 `QR code`，使用手機掃碼加入
## 接收 LINE Bot 訊息
### `Controllers\LineBotController.cs`
``` csharp
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace app.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LineBotController : ControllerBase
    {
        private readonly ILogger<LineBotController> _logger;

        public LineBotController(ILogger<LineBotController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public Object Post([FromBody] dynamic content)
        {
            Console.WriteLine($"LINE: {content}");
            return new {};
        }
    }
}
```
傳送訊息 Hi 給 LINE Bot
``` shell
LINE: {"events":[{"type":"message","replyToken":"b405a8055a2f47728e88479f42d3f625","source":{"userId":"Ud05919418fb37fec6635ec4de8338967","type":"user"},"timestamp":1606971149793,"mode":"active","message":{"type":"text","id":"13137028819991","text":"Hi"}}],"destination":"U858c9452b76d0726756fe8f1f5bfcc98"}
```
## 解析 LINE Bot 訊息
### `Controllers\LineBotController.cs`
``` csharp
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace app.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LineBotController : ControllerBase
    {
        private readonly ILogger<LineBotController> _logger;

        public LineBotController(ILogger<LineBotController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public Object Post([FromBody] dynamic content)
        {
            Console.WriteLine($"LINE: {content}");
            foreach (dynamic e in content.GetProperty("events").EnumerateArray())
            {
                if (e.GetProperty("type").ToString().Equals("message"))
                {
                    string text = e.GetProperty("message").GetProperty("text").ToString();
                    string token = e.GetProperty("replyToken").ToString();
                    Console.WriteLine($"      {replyToken}");
                    Console.WriteLine($"      {text}");
                }
            }
            return new {};
        }
    }
}
```
傳送訊息 Hi 給 LINE Bot
``` shell
LINE: {"events":[{"type":"message","replyToken":"6ebe52ebb3924181890e6b81c0373205","source":{"userId":"Ud05919418fb37fec6635ec4de8338967","type":"user"},"timestamp":1606973509569,"mode":"active","message":{"type":"text","id":"13137194379751","text":"Hi"}}],"destination":"U858c9452b76d0726756fe8f1f5bfcc98"}
      Hi
```
## 回應 LINE Bot 訊息
參考 [Messaging API: Send a reply](
https://developers.line.biz/en/reference/messaging-api/#send-reply-message)
``` csharp
using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace app.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LineBotController : ControllerBase
    {
        private readonly ILogger<LineBotController> _logger;

        public LineBotController(ILogger<LineBotController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public Object Post([FromBody] dynamic content)
        {
            Console.WriteLine($"LINE: {content}");
            foreach (dynamic e in content.GetProperty("events").EnumerateArray())
            {
                if (e.GetProperty("type").ToString().Equals("message"))
                {
                    string text = e.GetProperty("message").GetProperty("text").ToString();
                    string token = e.GetProperty("replyToken").ToString();
                    Console.WriteLine($"      {token}");
                    Console.WriteLine($"      {text}");

                    HttpWebRequest req = WebRequest.Create("https://api.line.me/v2/bot/message/reply") as HttpWebRequest;
                    req.Method = "POST";
                    req.Headers["Content-Type"] = "application/json";
                    req.Headers["Authorization"] = "Bearer t5TwfaBQnhwqFnOpEI510zNnFjLmwMs6+hNGQuW4tHKuRvZREezBsu/y5oq/6HKSDg1js2769TUhpgzsO/SpAjKlQoMzHuWeG/icYmVaTO+NKWY5f2QCddBjSY8aNRN4pn9t8dTEJOL3e3METCwHYQdB04t89/1O/w1cDnyilFU=";
                    using (StreamWriter writer = new StreamWriter(req.GetRequestStream()))
                    {
                        writer.WriteLine($"{{\"replyToken\":\"{token}\",\"messages\":[{{\"type\":\"text\",\"text\":\"test\"}}]}}");
                    }
                    HttpWebResponse rsp = req.GetResponse() as HttpWebResponse;
                    using (StreamReader reader = new StreamReader(rsp.GetResponseStream()))
                    {
                        Console.WriteLine(reader.ReadToEnd());
                    }
                }
            }
            return new {};
        }
    }
}
```
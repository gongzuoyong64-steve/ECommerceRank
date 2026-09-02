# 電商排行榜系統 (E-Commerce Ranking System)

使用 C# + Redis 建構的電商排行榜系統，支援透過 Kubernetes 部署至 EKS。

## 功能特色

* ✅ **CSV 檔案上傳** - 支援上傳 CSV 檔案批量更新排行榜資料
* ✅ 商品分數增加（模擬購買、點擊等行為）
* ✅ 排行榜查詢（支援正序／倒序）
* ✅ 商品排名查詢
* ✅ 商品分數查詢
* ✅ 排行榜清空
* ✅ 即時排行榜更新（每 5 秒自動重新整理）

## 專案結構

```text
.
├── Controllers/
│   └── RankingController.cs      # API 控制器
├── Services/
│   ├── IRedisService.cs          # Redis 服務介面
│   └── RedisService.cs           # Redis 服務實作（TODO: 需要完成）
├── wwwroot/
│   └── index.html                # 前端頁面（排行榜介面）
├── Program.cs                     # 應用程式入口
├── appsettings.json              # 設定檔
├── Dockerfile                    # Docker 映像檔建構設定
├── docker-compose.yml            # Docker Compose 設定（Redis）
├── k8s/
│   ├── deployment.yaml           # Kubernetes 部署設定
│   └── redis-deployment.yaml     # Redis 部署設定
└── .github/workflows/
    └── deploy-to-eks.yml         # GitHub Actions CI/CD 設定
```

## 練習任務 (TODO)

### 1. RedisService 實作

在 `Services/RedisService.cs` 中完成以下方法：

* [ ] `IncrementProductScoreAsync` - 增加商品分數
* [ ] `GetRankingAsync` - 取得排行榜
* [ ] `GetProductRankAsync` - 取得商品排名
* [ ] `GetProductScoreAsync` - 取得商品分數
* [ ] `ClearRankingAsync` - 清空排行榜

每個方法都有詳細的 TODO 註解與提示。

### 2. Program.cs Redis 設定

雖然已提供基礎實作，但你可以嘗試：

* [ ] 新增 Redis 連線重試機制
* [ ] 新增連線池設定
* [ ] 新增健康檢查

## 本機開發

### 前置需求

* .NET 8.0 SDK
* Docker 和 Docker Compose（用於啟動 Redis）

### 執行步驟

#### 方式一：使用 Docker Compose 啟動 Redis（推薦）

1. 啟動 Redis：

```bash
docker-compose up -d redis
```

2. 執行應用程式：

```bash
dotnet restore
dotnet run
```

3. 存取前端頁面：

```text
https://localhost:5001
```

或存取 Swagger UI：

```text
https://localhost:5001/swagger
```

#### 方式二：使用 Docker 指令啟動 Redis

```bash
docker run -d -p 6379:6379 --name redis redis:7-alpine
```

#### 停止 Redis

```bash
# 如果使用 docker-compose
docker-compose down

# 如果使用 docker 指令
docker stop redis && docker rm redis
```

#### 驗證 Redis 連線

```bash
# 檢查 Redis 容器狀態
docker-compose ps

# 測試 Redis 連線
docker-compose exec redis redis-cli ping
# 應該返回：PONG

# 或使用本機 redis-cli（如果已安裝）
redis-cli -h localhost -p 6379 ping
```

## 前端介面

專案包含一個美觀的前端排行榜介面，支援：

* 📊 **即時排行榜展示** - 顯示商品排名、名稱與分數
* 📤 **CSV 檔案上傳** - 支援點擊或拖放上傳 CSV 檔案
* 🛒 **購買功能** - 點擊購買按鈕增加商品分數
* 🔄 **自動重新整理** - 每 5 秒自動更新排行榜
* 🎨 **響應式設計** - 適配不同螢幕尺寸
* 🏆 **排名高亮** - 前三名具有特殊樣式標示

存取 `https://localhost:5001` 即可使用前端介面。

### CSV 上傳功能

* 點擊上傳區域選擇 CSV 檔案
* 或直接拖放 CSV 檔案至上傳區域
* 上傳成功後自動重新整理排行榜
* 支援檔案格式驗證與錯誤提示

## API 端點

### 增加商品分數

```http
POST /api/ranking/products/{productId}/score
Content-Type: application/json

{
  "score": 10.5
}
```

### 取得排行榜

```http
GET /api/ranking?startRank=0&endRank=9&order=desc
```

### 取得商品排名

```http
GET /api/ranking/products/{productId}/rank?order=desc
```

### 取得商品分數

```http
GET /api/ranking/products/{productId}/score
```

### 清空排行榜

```http
DELETE /api/ranking
```

### 上傳 CSV 檔案

```http
POST /api/csv
Content-Type: multipart/form-data

file: [CSV 檔案]
```

## Docker 建構

```bash
docker build -t ecommerce-ranking:latest .
docker run -p 8080:80 -e ConnectionStrings__Redis=host.docker.internal:6379 ecommerce-ranking:latest
```

## Kubernetes 部署

### 前置需求

* 已設定 kubectl
* 已建立 EKS 叢集
* 已設定 AWS CLI

### 部署步驟

1. 部署 Redis：

```bash
kubectl apply -f k8s/redis-deployment.yaml
```

2. 部署應用程式：

```bash
kubectl apply -f k8s/deployment.yaml
```

3. 檢查部署狀態：

```bash
kubectl get pods
kubectl get services
```

## GitHub Actions CI/CD

### 設定 Secrets

在 GitHub Repository 的設定中新增以下 Secrets：

* `AWS_ACCESS_KEY_ID` - AWS 存取金鑰 ID
* `AWS_SECRET_ACCESS_KEY` - AWS 秘密存取金鑰

### 設定 ECR 與 EKS

1. 在 `deploy-to-eks.yml` 中更新：

   * `AWS_REGION` - AWS 區域
   * `ECR_REPOSITORY` - ECR 儲存庫名稱
   * `EKS_CLUSTER_NAME` - EKS 叢集名稱

2. 在 `k8s/deployment.yaml` 中更新映像檔地址為你的 ECR 地址。

### 工作流程

當程式碼推送至 `main` 分支時，GitHub Actions 會：

1. 建構 Docker 映像檔
2. 推送至 Amazon ECR
3. 部署至 EKS 叢集

## 測試範例

```bash
# 增加商品分數
curl -X POST "https://localhost:5001/api/ranking/products/product-001/score" \
  -H "Content-Type: application/json" \
  -d '{"score": 100}'

# 取得排行榜
curl "https://localhost:5001/api/ranking?startRank=0&endRank=9"

# 取得商品排名
curl "https://localhost:5001/api/ranking/products/product-001/rank"
```

## 技術棧

* .NET 8.0
* StackExchange.Redis
* Docker
* Kubernetes
* AWS EKS
* GitHub Actions

## 授權條款

MIT

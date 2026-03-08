# 电商排行榜系统 (E-Commerce Ranking System)

使用 C# + Redis 构建的电商排行榜系统，支持通过 Kubernetes 部署到 EKS。

## 功能特性

- ✅ **前端界面** - 美观的排行榜展示页面
- ✅ **CSV 檔案上傳** - 支援上傳 CSV 檔案批量更新排行榜數據
- ✅ 商品分数增加（模拟购买、点击等行为）
- ✅ 排行榜查询（支持正序/倒序）
- ✅ 商品排名查询
- ✅ 商品分数查询
- ✅ 排行榜清空
- ✅ 实时排行榜更新（每 5 秒自动刷新）

## 项目结构

```
.
├── Controllers/
│   └── RankingController.cs      # API 控制器
├── Services/
│   ├── IRedisService.cs          # Redis 服务接口
│   └── RedisService.cs           # Redis 服务实现（TODO: 需要完成）
├── wwwroot/
│   └── index.html                # 前端页面（排行榜界面）
├── Program.cs                     # 应用程序入口
├── appsettings.json              # 配置文件
├── Dockerfile                    # Docker 镜像构建文件
├── docker-compose.yml            # Docker Compose 配置（Redis）
├── k8s/
│   ├── deployment.yaml           # Kubernetes 部署配置
│   └── redis-deployment.yaml     # Redis 部署配置
└── .github/workflows/
    └── deploy-to-eks.yml         # GitHub Actions CI/CD 配置
```

## 练习任务 (TODO)

### 1. RedisService 实现

在 `Services/RedisService.cs` 中完成以下方法：

- [ ] `IncrementProductScoreAsync` - 增加商品分数
- [ ] `GetRankingAsync` - 获取排行榜
- [ ] `GetProductRankAsync` - 获取商品排名
- [ ] `GetProductScoreAsync` - 获取商品分数
- [ ] `ClearRankingAsync` - 清空排行榜

每个方法都有详细的 TODO 注释和提示。

### 2. Program.cs Redis 配置

虽然已经提供了基础实现，但你可以尝试：
- [ ] 添加 Redis 连接重试机制
- [ ] 添加连接池配置
- [ ] 添加健康检查

## 本地开发

### 前置要求

- .NET 8.0 SDK
- Docker 和 Docker Compose（用于启动 Redis）

### 运行步骤

#### 方式一：使用 Docker Compose 启动 Redis（推荐）

1. 启动 Redis:
```bash
docker-compose up -d redis
```

2. 运行应用:
```bash
dotnet restore
dotnet run
```

3. 访问前端页面:
```
https://localhost:5001
```
或访问 Swagger UI:
```
https://localhost:5001/swagger
```

#### 方式二：使用 Docker 命令启动 Redis

```bash
docker run -d -p 6379:6379 --name redis redis:7-alpine
```

#### 停止 Redis

```bash
# 如果使用 docker-compose
docker-compose down

# 如果使用 docker 命令
docker stop redis && docker rm redis
```

#### 验证 Redis 连接

```bash
# 检查 Redis 容器状态
docker-compose ps

# 测试 Redis 连接
docker-compose exec redis redis-cli ping
# 应该返回: PONG

# 或者使用本地 redis-cli（如果已安装）
redis-cli -h localhost -p 6379 ping
```

## 前端界面

项目包含一个美观的前端排行榜界面，支持：

- 📊 **实时排行榜展示** - 显示商品排名、名称和分数
- 📤 **CSV 檔案上傳** - 支援點擊或拖放上傳 CSV 檔案
- 🛒 **购买功能** - 点击购买按钮增加商品分数
- 🔄 **自动刷新** - 每 5 秒自动更新排行榜
- 🎨 **响应式设计** - 适配不同屏幕尺寸
- 🏆 **排名高亮** - 前三名有特殊样式标识

访问 `https://localhost:5001` 即可使用前端界面。

### CSV 上傳功能

- 點擊上傳區域選擇 CSV 檔案
- 或直接拖放 CSV 檔案到上傳區域
- 上傳成功後自動刷新排行榜
- 支援檔案格式驗證和錯誤提示

## API 端点

### 增加商品分数
```http
POST /api/ranking/products/{productId}/score
Content-Type: application/json

{
  "score": 10.5
}
```

### 获取排行榜
```http
GET /api/ranking?startRank=0&endRank=9&order=desc
```

### 获取商品排名
```http
GET /api/ranking/products/{productId}/rank?order=desc
```

### 获取商品分数
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

## Docker 构建

```bash
docker build -t ecommerce-ranking:latest .
docker run -p 8080:80 -e ConnectionStrings__Redis=host.docker.internal:6379 ecommerce-ranking:latest
```

## Kubernetes 部署

### 前置要求

- kubectl 已配置
- EKS 集群已创建
- AWS CLI 已配置

### 部署步骤

1. 部署 Redis:
```bash
kubectl apply -f k8s/redis-deployment.yaml
```

2. 部署应用:
```bash
kubectl apply -f k8s/deployment.yaml
```

3. 检查部署状态:
```bash
kubectl get pods
kubectl get services
```

## GitHub Actions CI/CD

### 配置 Secrets

在 GitHub 仓库设置中添加以下 Secrets:

- `AWS_ACCESS_KEY_ID` - AWS 访问密钥 ID
- `AWS_SECRET_ACCESS_KEY` - AWS 密钥

### 配置 ECR 和 EKS

1. 在 `deploy-to-eks.yml` 中更新:
   - `AWS_REGION` - AWS 区域
   - `ECR_REPOSITORY` - ECR 仓库名称
   - `EKS_CLUSTER_NAME` - EKS 集群名称

2. 在 `k8s/deployment.yaml` 中更新镜像地址为你的 ECR 地址

### 工作流程

当代码推送到 `main` 分支时，GitHub Actions 会：
1. 构建 Docker 镜像
2. 推送到 Amazon ECR
3. 部署到 EKS 集群

## 测试示例

```bash
# 增加商品分数
curl -X POST "https://localhost:5001/api/ranking/products/product-001/score" \
  -H "Content-Type: application/json" \
  -d '{"score": 100}'

# 获取排行榜
curl "https://localhost:5001/api/ranking?startRank=0&endRank=9"

# 获取商品排名
curl "https://localhost:5001/api/ranking/products/product-001/rank"
```

## 技术栈

- .NET 8.0
- StackExchange.Redis
- Docker
- Kubernetes
- AWS EKS
- GitHub Actions

## 许可证

MIT

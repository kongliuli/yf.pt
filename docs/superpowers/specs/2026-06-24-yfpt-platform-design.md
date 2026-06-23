# Yf.Pt 公司内部资源聚合管理平台 — 设计文档

- 日期：2026-06-24
- 状态：已确认，待评审
- 方案：A（单体 Blazor Server + 分层）

## 1. 目标与范围

构建一个公司内部资源聚合管理平台，集中展示与登记：

- 阿里云资源监控（ECS、域名、RDS、SLB 等，第一阶段 Mock 数据）
- App 项目与域名绑定关系登记（项目 → 域名 → 所用云资源）
- 本地资源登记（自建服务器/物理机、内网服务）

第一阶段以 Mock 数据把平台形态和图表搭起来，后续通过实现 `ICloudResourceProvider` 接入真实阿里云 OpenAPI。

非目标：登录鉴权、多租户、计费、告警推送。

## 2. 技术选型

| 项 | 选型 | 说明 |
|---|---|---|
| 框架 | Blazor Server (.NET 8) | 内网平台，实时交互，无需前端部署 |
| 持久化 | SQLite + EF Core | Code-First 迁移，单文件随项目部署 |
| 图表 | ApexCharts.Blazor | 原生 Blazor 组件，无 JS 互操作 |
| 鉴权 | 无 | 内网信任环境 |
| 云数据 | Mock 实现 `ICloudResourceProvider` | 接口抽象，后续替换为阿里云 SDK |

## 3. 架构

单体 Blazor Server 项目，内部分层：

```
Yf.Pt/
├── Program.cs
├── appsettings.json
├── Data/                       # EF Core
│   ├── AppDbContext.cs
│   └── DbInitializer.cs        # Mock 种子数据
├── Models/                     # 实体
│   ├── AppProject.cs
│   ├── DomainBinding.cs
│   ├── LocalResource.cs
│   └── CloudResourceSnapshot.cs
├── CloudProviders/
│   ├── ICloudResourceProvider.cs
│   └── MockCloudResourceProvider.cs
├── Services/
│   ├── AppProjectService.cs
│   ├── DomainBindingService.cs
│   ├── LocalResourceService.cs
│   └── DashboardService.cs
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   └── NavMenu.razor
│   ├── Charts/                 # ApexCharts 封装
│   │   ├── CpuUsageChart.razor
│   │   ├── MemoryChart.razor
│   │   └── ResourceTrendChart.razor
│   └── Shared/
└── Pages/
    ├── Dashboard.razor         # 总览仪表盘
    ├── AliyunResources.razor   # 阿里云资源
    ├── AppProjects.razor        # App 项目
    ├── Domains.razor            # 域名管理
    └── LocalResources.razor    # 本地资源
```

### 3.1 依赖方向

`Pages/Components → Services → (Data | CloudProviders) → Models`

- 页面与组件只依赖 Service 接口，不直接碰 DbContext
- Service 依赖 `AppDbContext`（持久化）和 `ICloudResourceProvider`（云数据）
- `ICloudResourceProvider` 当前为 Mock 实现，返回内存中生成的资源与指标

### 3.2 云资源抽象

```csharp
public interface ICloudResourceProvider
{
    Task<IReadOnlyList<CloudResource>> ListResourcesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MetricPoint>> GetMetricsAsync(string resourceId, string metricName, TimeRange range, CancellationToken ct = default);
}
```

`MockCloudResourceProvider` 返回固定 + 轻度随机的资源列表与时间序列，保证图表有数据可画。后续 `AliyunCloudResourceProvider` 实现同一接口即可替换。

## 4. 数据模型

### 4.1 AppProject（App 项目）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | int | 主键 |
| Name | string | 项目名 |
| Description | string? | 描述 |
| Owner | string? | 负责人 |
| CreatedAt | DateTime | 创建时间 |

### 4.2 DomainBinding（域名绑定）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | int | 主键 |
| AppProjectId | int | 外键 → AppProject |
| Domain | string | 域名 |
| Type | string | 类型：A/CNAME/SLB/OSS 等 |
| Target | string | 指向（IP/别名/SLB ID/OSS bucket） |
| CloudResourceId | string? | 关联云资源 ID（可选） |
| CreatedAt | DateTime | |

一个项目可绑定多个域名；一个域名通过 `CloudResourceId` 关联到云资源。

### 4.3 LocalResource（本地资源）

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | int | 主键 |
| Name | string | 资源名（如 web-node-01） |
| Type | string | server / vm / service |
| Ip | string? | IP |
| Location | string? | 机房/位置 |
| Owner | string? | 负责人 |
| Note | string? | 备注 |

### 4.4 CloudResourceSnapshot（云资源快照，缓存用）

用于把 `ICloudResourceProvider` 返回的云资源落库一份，便于关联查询与离线展示。Mock 阶段由后台定时刷新写入。

| 字段 | 类型 | 说明 |
|---|---|---|
| Id | int | 主键 |
| ResourceId | string | 云资源 ID（如 i-bp1xxx） |
| Provider | string | aliyun |
| Type | string | ecs/rds/slb/domain |
| Name | string | |
| Region | string? | |
| Status | string | Running/Stopped 等 |
| UpdatedAt | DateTime | 快照时间 |

实时指标（CPU/内存/网络）不落库，每次从 Provider 拉取。

## 5. 页面与功能

### 5.1 总览仪表盘 `/`

- 资源计数卡片：项目数、域名数、本地资源数、云资源数
- 图表：云资源状态分布（饼图）、各项目域名数（柱图）、近 24h CPU 均值趋势（折线）
- 异常状态列表：Stopped 的 ECS、过期域名等

### 5.2 阿里云资源 `/aliyun`

- 表格：ECS/RDS/SLB/域名 列表，按类型筛选
- 详情抽屉：选中资源显示 CPU/内存/网络趋势图（ApexCharts）
- 数据来自 `ICloudResourceProvider`（Mock）

### 5.3 App 项目 `/projects`

- 列表 + 新增/编辑/删除（CRUD）
- 详情页：项目下所有域名绑定、关联云资源

### 5.4 域名管理 `/domains`

- 列表所有域名绑定，可按项目/类型筛选
- 新增/编辑/删除
- 显示域名 → 目标 → 关联云资源链路

### 5.5 本地资源 `/local`

- 列表 + CRUD
- 按类型/位置筛选

## 6. Mock 数据策略

- `DbInitializer` 在首次启动时向 SQLite 写入种子数据：3 个项目、6 个域名绑定、5 个本地资源
- `MockCloudResourceProvider` 内存生成：8 台 ECS、2 台 RDS、2 个 SLB、5 个域名，状态与指标轻度随机
- 指标时间序列：以当前时间为终点，向前推 24h，每小时一个点，基线 + 正弦扰动 + 随机噪声

## 7. 配置

`appsettings.json`：

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=yfpt.db"
  },
  "CloudProvider": {
    "Type": "Mock",
    "RefreshIntervalSeconds": 300
  }
}
```

后续接入阿里云时新增 `Aliyun` 节（AK/SK、Region），并在 DI 中切换实现。

## 8. 错误处理

- Service 层不吞异常，向上抛
- 页面用 `try/catch` 包裹数据加载，失败时显示错误提示组件，不白屏
- 云资源拉取失败时降级显示上次快照（`CloudResourceSnapshot`）

## 9. 测试

第一阶段以可运行、可演示为主，不强制单测。预留：
- `MockCloudResourceProvider` 可写轻量单测验证返回数量与字段
- Service 层 CRUD 用 SQLite in-memory 做集成测试（后续补）

## 10. 演进路径

1. 当前：Mock 数据 + 完整 UI + CRUD
2. 下一步：实现 `AliyunCloudResourceProvider`，替换 Mock
3. 之后：拆分为方案 B（多项目）或加 RCL，加鉴权

## 11. 不做（YAGNI）

- 登录鉴权、多租户
- 实时指标落库历史归档
- WebAssembly 客户端

## 12. 第二阶段扩展模块（Mock 实现）

在资源查看基础上扩展五个模块，全部 Mock 起步，与现有架构保持一致的接口抽象。

### 12.1 知识库 `/knowledge`

- 实体 `KnowledgeArticle`：Title/Content(Markdown)/Category(sop/doc/faq)/Tags/AppProjectId?/Author
- `KnowledgeService`：CRUD + 关键字搜索（SQLite LIKE 起步，后续可升级 FTS5）
- 页面：列表（按分类/标签筛选）+ 详情（Markdig 渲染）+ 新增/编辑
- 与 App 项目关联：文档可挂在项目下

### 12.2 到期提醒 `/expiry`

- 实体 `Certificate`：Domain/Issuer/ExpireAt/DaysRemaining（计算属性）
- `ExpiryCheckService`：扫描证书 + 云资源域名 expire，返回即将到期（30天内）列表
- 页面：证书表 + 域名到期表，按紧急程度标红/黄
- 种子数据：含已过期、即将到期、正常三类

### 12.3 资源拓扑 `/topology`

- `TopologyService`：基于现有 项目→域名→云资源 关系构建节点与边
- 渲染：Mermaid flowchart（JS 互操作），CDN 引入 mermaid.js
- 页面：按项目展示拓扑图，节点点击跳转资源详情

### 12.4 告警聚合 `/alerts`

- 实体 `AlertRecord`：ResourceId/ResourceName/Severity(critical/warning/info)/Title/Message/Status(active/ack/resolved)/TriggeredAt
- `IAlertProvider` + `MockAlertProvider`（仿 `ICloudResourceProvider`）
- `AlertService`：列表 + 按严重程度/状态筛选 + 统计
- 页面：告警卡片列表 + 顶部统计 + 筛选

### 12.5 成本分摊 `/costs`

- 实体 `CostRecord`：ResourceId/ResourceName/AppProjectId?/Amount/Currency/Period(yyyy-MM)/Category(ecs/rds/slb/domain/other)
- `ICostProvider` + `MockCostProvider`
- `CostService`：按项目/类别/周期汇总
- 页面：成本总览卡片 + 按项目柱图 + 按类别饼图 + 明细表

### 12.6 数据模型补充

```
KnowledgeArticle:  Id, Title, Content, Category, Tags, AppProjectId?, Author, CreatedAt, UpdatedAt
Certificate:       Id, Domain, Issuer, ExpireAt, Note
AlertRecord:       Id, ResourceId, ResourceName, Severity, Title, Message, Status, TriggeredAt
CostRecord:        Id, ResourceId, ResourceName, AppProjectId?, Amount, Currency, Period, Category
```

### 12.7 依赖方向（同第一阶段）

`Pages → Services → (Data | Providers) → Models`

告警与成本各自有 Provider 抽象，与 `ICloudResourceProvider` 并列，便于后续替换为真实数据源。

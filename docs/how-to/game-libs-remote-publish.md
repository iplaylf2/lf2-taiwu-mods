# 使用 GitHub Actions 发布游戏依赖

当你已经准备好《太吾绘卷》的游戏程序集，需要把它们同步到团队可访问的私有 NuGet 源时，可按本流程操作。本指南提供完整的远程发布操作步骤。

## 前置条件

- 已依据 manifest 整理好 `projects/unmanaged-vendor/game/` 目录，并压缩为单个 `.zip`（或确认有可下载的压缩包）。
- GitHub 仓库的 Secrets 中已配置目标包源的访问凭据。
- 了解你的私有源地址，后续步骤中需要将它写入 `nuget.config`。

## 确保包的私有性

由于重新分发游戏文件存在法律风险，将这些依赖包设为**私有**至关重要。

包的默认可见性取决于仓库的所有者：

- **组织仓库**：在公开仓库中发布的包，默认可见性是**私有**的。这是最安全的情况。
- **个人仓库**：在公开仓库中发布的包，默认可见性是**公开**的。**这会带来法律风险**，你必须采取额外步骤确保其私有性。

如果你的仓库是**个人仓库**，你有两种方法处理首次发布的可见性问题。

### 方法一：手动设置

在工作流完成**第一次**发布后，立刻访问你的 GitHub Packages 页面，找到新发布的包，进入其设置页面，将可见性手动更改为**私有**。

### 方法二：利用私有仓库

这是一个一次性的便捷技巧，适合在项目初始化时使用。

在**首次**运行工作流之前，将你的仓库临时设置为**私有**。这样，发布的包将自动成为私有包。发布完成后，你可以再将仓库设为公开。

## 操作指南

### 第一步：准备游戏依赖压缩包

#### 选项一：手动整理并压缩

1. 根据 [`game-libs.manifest.yaml`](../../projects/unmanaged-vendor/game/game-libs.manifest.yaml) 中的映射关系整理文件[^1]
2. 将游戏 DLL 放置到 `projects/unmanaged-vendor/game/<PackageId>/lib/` 目录[^2]
3. 将整个 `game/` 目录压缩为单个 `.zip` 文件

#### 选项二：使用 FileCourier 自动整理（推荐）

1. 从 [GitHub Releases](https://github.com/iplaylf2/lf2-taiwu-mods/releases) 下载对应平台的 FileCourier 可执行文件[^3]
2. 运行自动整理命令：

   ```bash
   ./FileCourier "<游戏安装目录>" "<临时输出目录>/game" -m game-libs.manifest.yaml
   ```

3. 将生成的 `game/` 目录压缩为 `.zip` 文件

4. **上传压缩包**：将压缩包上传到团队内部可访问的位置（如网盘、内部文件服务器等），获取下载地址。

### 第二步：配置仓库机密

在 GitHub 仓库中设置必要的机密信息：

1. 进入仓库 `Settings` > `Secrets and variables` > `Actions`
2. 创建或更新以下机密：
   - **`LF2_GAME_LIBS_URL`**：第一步中获得的压缩包下载地址
   - **`LF2_NUGET_API_KEY`**（可选）：指定自定义 `Source` 时所需的 API 密钥

### 第三步：触发发布工作流

1. 访问仓库的 `Actions` 页面
2. 选择 `Publish Game Libraries` 工作流
3. 点击 `Run workflow`
4. 填写以下参数：
   - **`Package Version`**：希望发布的游戏依赖版本号（建议使用语义化版本）
   - **`Source`**（可选）：自定义 NuGet 源地址，留空则使用仓库所有者的 GitHub Packages

工作流会自动执行以下操作：

- 从 `LF2_GAME_LIBS_URL` 下载压缩包
- 解压并覆盖 `projects/unmanaged-vendor/game/` 目录
- 执行 `LF2PackGameLibs` 构建目标生成 NuGet 包[^4]
- 推送包到目标私有源

### 第四步：配置本地开发环境

发布完成后，在本地配置 NuGet 源以使用远程私有包：

#### GitHub Packages 源配置

如果使用默认的 GitHub Packages：

1. 打开仓库根目录的 `nuget.config` 文件
2. 在 `<packageSources>` 节中添加：

   ```xml
   <add key="YourUsername" value="https://nuget.pkg.github.com/YourUsername/index.json" />
   ```

3. 配置访问凭据：
   - 设置环境变量 `GITHUB_USERNAME` 为你的 GitHub 用户名
   - 设置环境变量 `GITHUB_TOKEN` 为有 `read:packages` 权限的 PAT

4. 恢复依赖：

   ```bash
   dotnet restore
   ```

#### 自定义 NuGet 源配置

如果推送到自定义 NuGet 源：

1. 在 `nuget.config` 中添加对应的源地址
2. 根据源的要求配置相应的访问凭据
3. 运行 `dotnet restore` 验证配置

## 源管理

### 切换到离线源

如果需要切换回离线本地源：

1. 禁用远程源（在 `nuget.config` 中注释掉或删除对应的 `<add>` 条目）
2. 按照离线指南配置本地源
3. 运行 `dotnet restore`

### 多源管理

当同时配置多个源时，NuGet 会按顺序尝试。建议：

- 将最常用的源放在前面
- 使用明确的包版本避免版本冲突
- 定期清理不需要的源配置

## 故障排除

### 常见问题

**问题**：工作流执行失败，报告下载错误

- **解决**：检查 `LF2_GAME_LIBS_URL` 机密是否正确设置，确保压缩包可访问
- **检查**：确认压缩包格式正确，包含完整的 `game/` 目录结构

**问题**：NuGet 包推送失败

- **解决**：验证目标源的访问凭据配置正确
- **检查**：确认包版本号符合规范，没有版本冲突
- **检查**：如果指定了自定义 `Source`，确认 `LF2_NUGET_API_KEY` 机密已正确设置

**问题**：本地 `dotnet restore` 找不到远程包

- **解决**：检查 `nuget.config` 中的源地址配置
- **验证**：确认访问凭据（环境变量或配置文件）正确设置

**问题**：工作流未显示在 Actions 页面

- **解决**：这可能是因为基于模板创建的仓库的已知问题
- **操作**：对工作流文件进行一次重命名或修改并提交，触发 GitHub Actions 识别

### 版本管理

**升级游戏版本**：

1. 重新整理新版本的游戏 DLL 文件
2. 更新压缩包并重新上传
3. 使用新的版本号重新运行工作流
4. 在本地更新 `LF2TaiwuVersion` 属性并重新恢复依赖

**回滚版本**：

1. 在仓库的 `nuget.config` 中指定明确的包版本
2. 或使用 `dotnet restore --force` 强制恢复指定版本

## 相关资源

- **[离线环境下的游戏依赖准备](./game-libs-offline-setup.md)** - 备选的本地打包方案
- **[游戏依赖包说明](../reference/game-dependencies.md)** - 各依赖包的详细用途与引用场景

## 参考资料

[^1]: 清单文件定义了游戏 DLL 文件到包目录的映射规则，是自动化文件整理的核心配置。详细格式说明请参阅：[游戏依赖包技术规格 - 清单文件格式](../reference/game-libs-packaging.md#清单文件格式)

[^2]: `<PackageId>/lib/` 目录结构遵循本仓库的包体系设计，区分 backend 和 frontend 两种目标框架。详细规范请参阅：[游戏依赖包技术规格 - 目录命名约定](../reference/game-libs-packaging.md#目录命名约定)

[^3]: FileCourier 是本仓库提供的跨平台文件分拣工具，支持基于 manifest 的自动化文件整理。详细了解其功能请参阅：[FileCourier 工具文档](../../projects/unmanaged-vendor/tools/FileCourier/README.md)

[^4]: `LF2PackGameLibs` 是构建系统提供的打包目标，能够自动识别项目类型并生成对应的 NuGet 包。详细机制请参阅：[游戏依赖包技术规格 - 打包目标](../reference/game-libs-packaging.md#打包目标)

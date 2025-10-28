# NuGet 源管理指南

本指南汇集了在使用本地或远程 NuGet 源时，涉及包安全、源管理、版本控制及常见问题排查的进阶内容。

## 远程源：安全与客户端配置

此章节仅适用于远程源方案。

### 确保包的私有性

由于重新分发游戏文件存在法律风险，将这些依赖包设为**私有**至关重要。

包的默认可见性取决于仓库的所有者：

- **组织仓库**：在公开仓库中发布的包，默认可见性是**私有**的。这是最安全的情况。
- **个人仓库**：在公开仓库中发布的包，默认可见性是**公开**的。**这会带来法律风险**，你必须采取额外步骤确保其私有性。

如果你的仓库是**个人仓库**，你有两种方法处理首次发布的可见性问题。

#### 方法一：手动设置

在工作流完成**第一次**发布后，立刻访问你的 GitHub Packages 页面，找到新发布的包，进入其设置页面，将可见性手动更改为**私有**。

#### 方法二：利用私有仓库

这是一个一次性的便捷技巧，适合在项目初始化时使用。

在**首次**运行工作流之前，将你的仓库临时设置为**私有**。这样，发布的包将自动成为私有包。发布完成后，你可以再将仓库设为公开。

### 配置本地开发环境

发布完成后，在本地配置 NuGet 源以使用远程包。

#### GitHub Packages 源配置

如果使用默认的 GitHub Packages：

1. 打开仓库根目录的 [`nuget.config`](../../nuget.config) 文件
2. 在 `<packageSources>` 节中添加：

    ```xml
    <add key="YourUsername" value="https://nuget.pkg.github.com/YourUsername/index.json" />
    ```

3. 在 `<packageSourceMapping>` 节中为该源声明包匹配：

    ```xml
    <packageSource key="YourUsername">
      <package pattern="LF2.Taiwu.*" />
    </packageSource>
    ```

4. 在 `<packageSourceCredentials>` 中配置访问凭据，例如引用环境变量：

    ```xml
    <packageSourceCredentials>
      <YourUsername>
        <add key="Username" value="%GITHUB_USERNAME%" />
        <add key="ClearTextPassword" value="%GITHUB_TOKEN%" />
      </YourUsername>
    </packageSourceCredentials>
    ```

    执行命令前，请确保在当前终端设置 `GITHUB_USERNAME` 与 `GITHUB_TOKEN` 环境变量，其中 `GITHUB_TOKEN` 必须拥有 `read:packages` 权限[^1]。

5. 恢复依赖：

    ```bash
    dotnet restore
    ```

#### 自定义 NuGet 源配置

如果推送到自定义 NuGet 源：

1. 在 `nuget.config` 中添加对应的源地址，并在 `<packageSourceMapping>` 节中为该源添加 `LF2.Taiwu.*` 匹配规则
2. 根据源的要求配置相应的访问凭据
3. 运行 `dotnet restore` 验证配置

## 通用管理操作

### 在不同源之间切换

#### 从本地源切换到远程源

完成本地开发后，如需切换回远程源：

- 使用默认配置执行 `dotnet restore`（默认读取 `nuget.config`），此时 `LF2.Taiwu.*` 将重新从远程源获取。

#### 从远程源切换到本地源

如果需要从远程源切换回本地源：

1. 按照本地源方案生成 `.lf2.nupkg/` 中的包
2. 使用专用的本地配置执行恢复：

   ```bash
   dotnet restore --configfile nuget.local.config
   ```

### 本地源配置说明

仓库提供了独立的 `nuget.local.config`，当需要临时使用本地包时，可搭配该文件执行恢复：

```xml
<packageSources>
  <add key="local" value="./.lf2.nupkg" />
  <!-- 其他源配置 -->
</packageSources>
<packageSourceMapping>
  <packageSource key="local">
    <package pattern="LF2.Taiwu.*" />
  </packageSource>
  <!-- 其他映射 -->
</packageSourceMapping>
```

执行 `dotnet restore --configfile nuget.local.config` 时，NuGet 会在保留公共源的同时，将 `LF2.Taiwu.*` 包优先解析到 `.lf2.nupkg/`。

### 游戏依赖版本管理

#### 升级游戏版本

1. 重新整理新版本的游戏 DLL 文件
2. 更新压缩包并重新上传（仅远程源方案）
3. 使用新的版本号重新运行工作流（仅远程源方案）
4. 在本地更新 `LF2TaiwuVersion` 属性并重新恢复依赖

#### 回滚版本

1. 在仓库的 `nuget.config` 中指定明确的包版本
2. 或使用 `dotnet restore --force` 强制恢复指定版本

## 故障排除

### 凭据与权限问题

#### PAT 权限不足

- **要求**：PAT 必须拥有 `read:packages` 权限
- **检查**：在 GitHub 设置页面重新生成具有正确权限的 PAT
- **更新**：更新本地环境变量中的 `GITHUB_TOKEN`

### 文件操作问题

#### FileCourier 执行失败

- 确认游戏安装目录路径正确，且具有读取权限
- 确保 [`game-libs.manifest.yaml`](../../projects/unmanaged-vendor/game/game-libs.manifest.yaml) 文件存在且格式正确
- 检查目标目录是否存在写入权限
- 验证游戏目录中包含所需的 DLL 文件

#### 目录结构异常

- **症状**：打包后的目录结构不符合预期
- **检查**：确认 manifest 文件中的映射关系正确
- **验证**：检查 `lib/backend` 和 `lib/frontend` 目录的创建和文件分布

### 远程源特有问题

#### GitHub Actions 工作流失败

**工作流执行失败，报告下载错误**：

- 检查 `LF2_GAME_LIBS_URL` 机密是否正确设置，确保压缩包可访问
- 确认压缩包格式正确，包含完整的 `game/` 目录结构
- 验证压缩包下载链接有效且无访问权限限制

**NuGet 包推送失败**：

- 验证目标源的访问凭据配置正确
- 确认包版本号符合规范，没有版本冲突
- 如果指定了自定义 `Source`，确认 `LF2_NUGET_API_KEY` 机密已正确设置

#### 包可见性问题

- **个人仓库**：首次发布后需手动将包设为私有
- **组织仓库**：包默认为私有，但仍需验证设置
- **访问权限**：确保团队成员有相应的包读取权限

### 包恢复问题

#### `dotnet restore` 报告找不到包

**可能原因及解决方案**：

- **本地源方案**：
  - 确认第一步的 DLL 整理已完成，且第二步的打包操作成功执行
  - 检查 `.lf2.nupkg/` 目录是否生成了 `.nupkg` 文件

- **远程源方案**：
  - 检查 `nuget.config` 中的源地址配置是否正确
  - 确认访问凭据（环境变量 `GITHUB_USERNAME` 和 `GITHUB_TOKEN`）正确设置
  - 验证网络连接正常，能够访问 GitHub Packages

#### 本地包版本不匹配

- **解决**：重新执行打包操作生成新版本
- **注意**：游戏更新后需要重新整理所有 DLL 文件
- **验证**：检查 `Directory.Build.props` 中的 `LF2TaiwuVersion` 版本设置

## 相关资源

- **[`nuget.config`](../../nuget.config)** - 默认源、凭据与映射配置示例
- **[依赖基础设施](../reference/dependency-infrastructure.md)** - 游戏依赖包体系与打包流程
- **[GitHub Docs: Creating a personal access token](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token)** - PAT 权限说明
- **[NuGet CLI 文档：环境变量](https://learn.microsoft.com/nuget/reference/cli-reference/cli-ref-environment-variables)** - 在命令行中配置凭据所需的环境变量说明

## 参考资料

[^1]: 可在运行 `dotnet restore` 前通过 `export GITHUB_USERNAME=...`、`export GITHUB_TOKEN=...`（或等效方式）设置；令牌需具备 `read:packages` 权限。

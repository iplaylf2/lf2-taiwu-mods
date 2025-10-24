# 非托管供应商依赖管理

本目录旨在将无法从公共源获取的游戏官方程序集等特殊依赖，打包成标准的远程 NuGet 包，以解决依赖管理问题，确保项目能够正常恢复和构建。

## ⚠️ 重要声明与风险提示

此方案涉及对游戏文件的处理与再分发，因此在开始前，请务必了解并接受以下风险。

### 1. 法律风险

**核心原则**：本模板仓库严格不托管任何游戏核心程序集的包。

重新分发这些文件可能与游戏的最终用户许可协议（EULA）存在冲突。如果你选择自行打包或发布相关文件，所有法律合规性风险由你自行承担。

### 2. 维护成本

本方案的核心是用**一次性的打包和发布成本**，换取后续开发中**依赖管理的便利**。

当你决定采用此方案时，意味着需要投入精力来维护自己的远程包源。尤其当游戏版本更新后，你需要同步更新所有相关的依赖包，以确保整个团队开发环境的一致性。

---

## 解决方案

本仓库提供两种管理游戏依赖的方案：远程源方案和本地打包方案。

### 方案一：远程源方案（推荐）

对于**长期、规范化**的开发场景，强烈推荐此方案。它借助自动化工作流，将游戏依赖打包成私有 NuGet 包，从而实现**标准化的版本控制**、**统一的团队协作**与**全自动的发布流程**。

#### 操作指南

##### 第一步：整理游戏 DLL 文件

根据 [`game-libs.manifest.yaml`](game/game-libs.manifest.yaml) 中的映射关系[^1]，将游戏程序集放置到对应目录。

###### 选项一：手动整理

1. 根据清单文件中的映射关系整理文件
2. 将游戏 DLL 放置到 `<临时目录>/game/<PackageId>/lib/` 目录[^2]
3. 确保目录结构符合标准布局

完成后，将生成的 `game/` 目录用于后续操作。

###### 选项二：使用 FileCourier 自动整理（推荐）

1. 从 [GitHub Releases](https://github.com/iplaylf2/lf2-taiwu-mods/releases) 下载对应平台的 FileCourier 可执行文件[^3]
2. 将可执行文件与 `game-libs.manifest.yaml` 放在同一目录
3. 运行命令：

   ```bash
   ./FileCourier "<游戏安装目录>" "<临时输出目录>/game" -m game-libs.manifest.yaml
   ```

FileCourier 会自动按照 manifest 复制所需文件并生成正确的目录结构。完成后，将生成的 `game/` 目录用于后续操作。

##### 第二步：准备压缩包

将整理好的 `game/` 目录压缩为单个 `.zip` 文件，然后上传到团队内部可访问的位置（如网盘、内部文件服务器等），获取下载地址。

##### 第三步：配置仓库机密

在 GitHub 仓库中设置必要的机密信息：

1. 进入仓库 `Settings` > `Secrets and variables` > `Actions`
2. 创建或更新以下机密：
   - **`LF2_GAME_LIBS_URL`**：第二步中获得的压缩包下载地址[^4]
   - **`LF2_NUGET_API_KEY`**（可选）：指定自定义 `Source` 时所需的 API 密钥

##### 第四步：触发发布工作流

1. 访问仓库的 `Actions` 页面
2. 选择 `Publish Game Libraries` 工作流
3. 点击 `Run workflow`
4. 填写以下参数：
   - **`Package Version`**：希望发布的游戏依赖版本号（建议使用语义化版本）
   - **`Source`**（可选）：自定义 NuGet 源地址，留空则使用仓库所有者的 GitHub Packages

工作流会自动执行以下操作：

- 从 `LF2_GAME_LIBS_URL` 下载压缩包
- 解压并覆盖 `projects/unmanaged-vendor/game/` 目录
- 执行 `LF2PackGameLibs` 构建目标生成 NuGet 包[^5]
- 推送包到目标远程源

> [!NOTE]
> **工作流可能不会立即显示**
> 由于 [GitHub 的一个已知问题](https://github.com/orgs/community/discussions/25219)，基于模板创建的仓库，其工作流（Workflows）可能不会自动出现在 `Actions` 页面。如果 `Publish Game Libraries` 工作流没有显示，你可能需要对工作流文件（例如，在 `.github/workflows/` 目录下）进行一次重命名（或任意修改）并提交，才能触发 GitHub Actions 的识别。

##### 第五步：配置本地开发环境

发布完成后，在本地配置 NuGet 源以使用远程包：

###### GitHub Packages 源配置

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

> [!TIP]
> **进阶指南**
> 关于包的安全性、源管理、故障排除等更多内容，请参阅 [《NuGet 源管理指南》](../../docs/how-to/nuget-source-management.md)。

### 方案二：本地打包方案

对于快速验证或无法联网的场景，可以选择本地打包方案。具体操作步骤请参阅 [本地源方案操作指南](../../docs/how-to/game-libs-local-setup.md)。更多策略与可选项请参考 [游戏依赖包技术规格](../../docs/reference/game-libs-packaging.md)。

---

## 目录说明

### `game/` 目录

- **主要用途**：用于生成《太吾绘卷》官方程序集包
- **使用场景**：供 Mod 在编译阶段引用所需的游戏程序集[^6]
- **重要说明**：这些包默认不会随 Mod 一起分发，仅作为编译时依赖

### `upm/` 目录

- **主要用途**：供高级场景下打包第三方库或 Unity UPM 库使用
- **注意事项**：自动化工作流不会自动处理该目录
- **操作指南**：具体做法请参考 [依赖管理操作指南](../../docs/how-to/dependency-management.md)

## 参考资料

[^1]: 清单文件定义了游戏 DLL 文件到包目录的映射规则，是自动化文件整理的核心配置。详细格式说明请参阅：[游戏依赖包技术规格 - 清单文件格式](../../docs/reference/game-libs-packaging.md#清单文件格式)
[^2]: `<PackageId>/lib/` 目录结构遵循本仓库的包体系设计，区分 backend 和 frontend 两种目标框架。详细规范请参阅：[游戏依赖包技术规格 - 目录命名约定](../../docs/reference/game-libs-packaging.md#目录命名约定)
[^3]: FileCourier 是本仓库提供的跨平台文件分拣工具，支持基于 manifest 的自动化文件整理。详细了解其功能请参阅：[FileCourier 工具文档](tools/FileCourier/README.md)
[^4]: GitHub Actions 机密用于安全存储敏感信息，避免在日志中暴露。设置方式：进入仓库 `Settings` > `Secrets and variables` > `Actions` > `New repository secret`，创建名为 `LF2_GAME_LIBS_URL` 的机密，值为压缩包下载地址。要深入了解安全最佳实践，请参考 [GitHub Actions 安全文档](https://docs.github.com/zh-cn/actions/security-guides/using-secrets-in-github-actions)
[^5]: `LF2PackGameLibs` 是构建系统提供的打包目标，能够自动识别项目类型并生成对应的 NuGet 包。详细机制请参阅：[游戏依赖包技术规格 - 打包目标](../../docs/reference/game-libs-packaging.md#打包目标)
[^6]: 游戏程序集作为编译时依赖，不随 Mod 分发。关于程序集分发的最佳实践，详见 [游戏依赖打包技术规格](../../docs/reference/game-libs-packaging.md)

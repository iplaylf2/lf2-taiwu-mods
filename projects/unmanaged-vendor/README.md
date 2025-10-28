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

根据 [`game-libs.manifest.yaml`](game/game-libs.manifest.yaml) 中的映射关系，将游戏程序集放置到对应目录。

###### 选项一：手动整理

1. 根据清单文件中的映射关系整理文件
2. 将游戏 DLL 放置到 `<临时目录>/game/<PackageId>/lib/` 目录[^1]
3. 确保目录结构符合标准布局

完成后，将生成的 `game/` 目录用于后续操作。

###### 选项二：使用 FileCourier 自动整理（推荐）

1. 从 [GitHub Releases](https://github.com/iplaylf2/lf2-taiwu-mods/releases) 下载对应平台的 FileCourier 可执行文件[^2]
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
   - **`LF2_GAME_LIBS_URL`**：用于存放第二步获得的压缩包下载地址[^3]
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
- 执行 `LF2PackGameLibs` 构建目标生成 NuGet 包[^4]
- 推送包到目标远程源

> [!NOTE]
> **工作流可能不会立即显示**
> 由于 [GitHub 的一个已知问题](https://github.com/orgs/community/discussions/25219)，基于模板创建的仓库，其工作流（Workflows）可能不会自动出现在 `Actions` 页面。如果 `Publish Game Libraries` 工作流没有显示，你可能需要对工作流文件（例如，在 `.github/workflows/` 目录下）进行一次重命名（或任意修改）并提交，才能触发 GitHub Actions 的识别。

##### 第五步：配置本地开发环境

发布完成后，在本地配置 NuGet 源以使用远程包：

###### GitHub Packages 源配置

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

5. 恢复依赖：

   ```bash
   dotnet restore
   ```

> [!TIP]
> **进阶指南**
> 关于包的安全性、源管理、故障排除等更多内容，请参阅 [NuGet 源管理指南](../../docs/how-to/nuget-source-management.md)。

### 方案二：本地打包方案

对于快速验证或无法联网的场景，可以选择本地打包方案。具体操作步骤请参阅 [本地源方案操作指南](../../docs/how-to/game-libs-local-setup.md)。更多策略与可选项请参考 [依赖基础设施](../../docs/reference/dependency-infrastructure.md)。

---

## 目录说明

### `game/` 目录

- **主要用途**：用于生成《太吾绘卷》官方程序集包
- **使用场景**：供 Mod 在编译阶段引用所需的游戏程序集[^5]

### `upm/` 目录

- **主要用途**：供高级场景下打包第三方库或 Unity UPM 库使用
- **注意事项**：自动化工作流不会自动处理该目录
- **操作指南**：具体做法请参考 [依赖管理操作指南](../../docs/how-to/dependency-management.md)

## 相关资源

- **[依赖基础设施](../../docs/reference/dependency-infrastructure.md)** - 游戏依赖包体系、清单结构与目录规范
- **[本地源方案操作指南](../../docs/how-to/game-libs-local-setup.md)** - 在本地整理并消费游戏依赖的完整流程
- **[NuGet 源管理指南](../../docs/how-to/nuget-source-management.md)** - 远程源配置、凭据管理与常见问题排查
- **[FileCourier 工具文档](tools/FileCourier/README.md)** - manifest 驱动的文件分拣工具说明

## 参考资料

[^1]: `lib/backend`、`lib/frontend` 等目录约定用于区分后端与前端程序集，确保构建系统选择正确模板。
[^2]: FileCourier 是本仓库提供的 manifest 驱动分拣工具，可自动复制并整理所需 DLL。
[^3]: `LF2_GAME_LIBS_URL` 机密用于安全存放游戏 DLL 压缩包的下载地址，避免在日志中曝光。
[^4]: `LF2PackGameLibs` 构建目标负责将整理好的 DLL 打包成 NuGet 包，并交由后续流程推送到目标源。
[^5]: 游戏程序集在本流程中仅作为编译期依赖，不会随 Mod 发布一同分发。

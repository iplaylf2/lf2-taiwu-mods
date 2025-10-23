# 非托管供应商依赖管理

本目录专门用于解决部分第三方依赖项无法通过公共 NuGet 源获取的问题。通过提供手动或自动化的方式，将这些特殊依赖打包成标准的 NuGet 包，确保项目能够正常恢复和构建。

## ⚠️ 重要声明与风险提示

**免责声明：在继续操作之前，请务必仔细阅读并完全理解以下风险。本方案为社区驱动的变通方法，并非官方支持的技术方案。**

### 1. 法律风险

**核心原则**：本模板仓库严格不托管任何游戏核心程序集的包。

重新分发这些文件可能与游戏的最终用户许可协议（EULA）存在冲突。如果你选择自行打包或发布相关文件，所有法律合规性风险由你自行承担。

### 2. 技术权衡

**版本管理需求**
游戏依赖包与特定版本的游戏程序集存在强耦合关系。当游戏版本更新时，需要同步更新这些依赖包，以确保：

- Mod 编译的兼容性
- 运行时的稳定性
- 团队开发环境的一致性

**维护投入考量**
建立和维护依赖包体系需要持续的技术投入，包括版本同步、自动化工作流配置和团队协作机制。这是一个需要在初期效率与长期维护之间做出权衡的技术决策。

---

## 快速上手：私有源方案

对于任何希望进行**长期、规范化**依赖版本管理的开发场景，强烈建议优先选择**私有 NuGet 源方案**。借助本仓库提供的自动化工作流，可以轻松地将游戏库打包，并作为私有 NuGet 包发布到自己的 GitHub Packages 源。

### 私有源方案的核心优势

**1. 标准化版本控制**
可以像管理普通 NuGet 包一样，对游戏库进行精确的版本管理，支持语义化版本号和版本回滚。

**2. 团队协作与一致性**
团队成员在配置完访问凭据后，即可通过简单的 `dotnet restore` 命令拉取完全一致的依赖版本，确保开发环境的统一性。

**3. 全自动化发布流程**
无需手动进行下载、打包、上传等繁琐操作，工作流会自动处理所有技术步骤，大幅提升工作效率。

### 第一步：准备压缩包

首先需要将游戏 DLL 文件按照 `game/<PackageId>/lib/` 的目录结构整理好，然后压缩为单个 `.zip` 文件。

对于文件整理，你有以下两种方式可选：

**推荐方式：使用 FileCourier 自动化工具**

1. 从 [GitHub Releases](https://github.com/iplaylf2/lf2-taiwu-mods/releases) 下载对应平台的可执行文件[^1]
2. 获取 [`game-libs.manifest.yaml`](game/game-libs.manifest.yaml) 清单文件[^2]，将其与可执行文件放在同一目录
3. 运行命令：

```bash
./FileCourier "<游戏安装目录>" "<临时输出>/game" -m game-libs.manifest.yaml
```

**备选方式：手动整理**

直接参考清单文件中的映射关系，按功能分组手动放置文件。该清单文件定义了完整的 DLL 到包目录的映射规则。

执行完成后，生成的 `game/` 目录就可以直接用于后续的打包流程。

### 发布到私有源（GitHub Actions）

1. **创建你的仓库**：将本仓库作为模板 (Use this template) 或直接 Fork 来创建你自己的仓库。

2. **准备游戏库压缩包**：将游戏 DLL 按 [`game-libs.manifest.yaml`](game/game-libs.manifest.yaml) 的映射整理为 `game/` 目录结构，然后压缩为单个 `.zip` 文件。
   - 准备完成后，将压缩包上传到团队内部可访问的位置（如网盘、内部文件服务器等），获取下载地址。

3. **配置仓库机密**：为避免在日志中暴露下载地址，工作流将从名为 `LF2_GAME_LIBS_URL` 的仓库机密中读取下载地址[^3]。

4. **运行工作流**：在你的仓库 `Actions` 页面找到 `Publish Game Libraries` 工作流。

5. **提供参数并执行**：根据工作流的提示，提供游戏库的版本号。工作流会自动识别可打包的工程并生成对应的 NuGet包。指定自定义 `source` 时，工作流会使用 `LF2_NUGET_API_KEY` 仓库机密获取 API 密钥进行鉴权。

> [!WARNING]
> **个人仓库的包可见性风险**
> 若你的仓库是**个人**公开仓库，工作流创建的包将默认为**公开**。由于存在法律风险，你必须在首次发布后手动将包的可见性设为**私有**。
>
> **解决方法**：在首次发布完成后，立即访问 GitHub Packages 页面，找到新发布的包，进入其设置页面将可见性更改为私有。组织仓库无此风险。

> [!NOTE]
> **工作流可能不会立即显示**：基于模板创建的仓库可能需要先修改工作流文件才能在 Actions 页面显示。关于本仓库的自动化构建体系，详见 [构建系统参考](../../docs/reference/build-system.md)。
> 由于 [GitHub 的一个已知问题](https://github.com/orgs/community/discussions/25219)，基于模板创建的仓库，其工作流（Workflows）可能不会自动出现在 `Actions` 页面。如果 `Publish Game Libraries` 工作流没有显示，你可能需要对工作流文件（例如，在 `.github/workflows/` 目录下）进行一次重命名（或任意修改）并提交，才能触发 GitHub Actions 的识别。

### 必要目录结构：游戏核心程序集

确保压缩包中的目录层级与 `game/<PackageId>/lib/` 一致。这样生成的 NuGet 包会将 DLL 作为仅编译时引用，避免在发布 Mod 时重复分发游戏文件。完整示例与校验方法同样可在[参考手册](../../docs/reference/game-libs-packaging.md#目录结构与清单)中找到。

### 配置开发环境以使用私有源

当你的私有源上已经有打包好的游戏依赖后，你和其他团队成员便需要配置本地环境来使用它。

配置步骤如下：

1. **打开 `nuget.config` 文件**：在仓库根目录找到 `nuget.config` 文件
2. **添加私有源**：在 `<packageSources>` 节中添加新的 `<add>` 条目

   ```xml
   <add key="MyUser" value="https://nuget.pkg.github.com/MyUser/index.json" />
   ```

   （将 `MyUser` 替换为你的 GitHub 用户名）
3. **配置凭据**：设置环境变量或在 NuGet 配置中添加 GitHub 用户名和 PAT

请保留示例中提供的 `iplaylf2` 源，以便持续访问公开维护的依赖。

> [!NOTE]
> **清理冲突源**
> 如果你曾使用过备选的“本地打包”方案，请在恢复依赖前运行 `dotnet nuget disable source local` 暂停本地源，确保 `dotnet restore` 优先使用远程私有源。

## 备选方案：本地打包

对于快速验证或无法联网的场景，可以选择本地打包方案。具体操作步骤请参阅 [离线环境下的游戏依赖准备](../../docs/how-to/game-libs-offline-setup.md)。更多策略与可选项请参考 [游戏依赖包技术规格](../../docs/reference/game-libs-packaging.md)。

---

## 目录说明

### `game/` 目录

- **主要用途**：用于生成《太吾绘卷》官方程序集包
- **使用场景**：供 Mod 在编译阶段引用所需的游戏程序集[^4]
- **重要说明**：这些包默认不会随 Mod 一起分发，仅作为编译时依赖

### `upm/` 目录

- **主要用途**：供高级场景下打包第三方库或 Unity UPM 库使用
- **注意事项**：自动化工作流不会自动处理该目录
- **操作指南**：具体做法请参考 [依赖管理操作指南](../../docs/how-to/dependency-management.md)

## 参考资料

[^1]: FileCourier 是本仓库提供的跨平台文件分拣工具，支持基于 manifest 的自动化文件整理。详细了解其功能请参阅：[FileCourier 工具文档](tools/FileCourier/README.md)

[^2]: 清单文件定义了游戏 DLL 文件到包目录的映射规则，是自动化文件整理的核心配置。格式说明请参阅：[游戏依赖包技术规格 - 清单文件格式](../../docs/reference/game-libs-packaging.md#清单文件格式)

[^3]: GitHub Actions 机密用于安全存储敏感信息，避免在日志中暴露。设置方式：进入仓库 `Settings` > `Secrets and variables` > `Actions` > `New repository secret`，创建名为 `LF2_GAME_LIBS_URL` 的机密，值为压缩包下载地址。要深入了解安全最佳实践，请参考 [GitHub Actions 安全文档](https://docs.github.com/zh-cn/actions/security-guides/using-secrets-in-github-actions)

[^4]: 游戏程序集作为编译时依赖，不随 Mod 分发。关于程序集分发的最佳实践，详见 [游戏依赖打包技术规格](../../docs/reference/game-libs-packaging.md)

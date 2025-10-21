# 非托管供应商依赖：Unmanaged Vendor Dependencies

此目录旨在解决部分第三方依赖项无法通过公共 NuGet 源获取的问题。你可以通过手动或自动化的方式，将这些依赖打包成 NuGet 包，以供项目恢复。

## ⚠️ 重要声明与风险提示

**免责声明：在继续之前，请务必阅读并理解以下风险。此方案为社区驱动的变通方法，并非官方支持。**

1. **法律风险**：**本模板仓库不托管任何游戏核心程序集的包**。重新分发这些文件可能与游戏的最终用户许可协议（EULA）相悖。如果你选择自行打包或发布，相关行为的合规性风险由你自行承担。
2. **技术风险**
    - **版本锁定**：这些包与特定版本的游戏或 Unity 编辑器紧密耦合。游戏更新后，这些包必须手动同步更新，否则会导致 Mod 编译失败或运行时崩溃。
    - **维护成本**：维护这些包的同步更新需要持续投入精力。

## 目录结构概览

- `game/`：用于生成 Taiwu 官方程序集包，供 Mod 在编译阶段引用，默认不会随 Mod 分发。
- `upm/`：供高级场景打包第三方或 UPM 库使用；自动化工作流不会处理该目录，具体做法请参考 [依赖管理操作指南](../../docs/how-to/dependency-management.md)。

> [!TIP]
> [`game-libs.manifest.yaml`](game/game-libs.manifest.yaml) 已按包 ID 列出 `game/` 目录需要的完整映射，可直接照单整理文件；如需了解目录约定或打包流程的更多细节，可查看 [游戏依赖打包参考手册](../../docs/reference/game-libs-packaging.md)。

---

## 快速上手：使用私有源

对于任何希望进行**长期、规范化**的依赖版本管理的场景，建议你优先选择**私有 NuGet 源的方案**。借助本仓库提供的自动化工作流，你可以轻松地将游戏库打包，并作为私有 NuGet 包发布到自己的 GitHub Packages 源。

- **优点**：
  - **版本控制**：可以像管理普通 NuGet 包一样管理游戏库的版本。
  - **可复现与共享**：团队成员在配置完凭据后即可通过 `dotnet restore` 拉取一致的依赖。
  - **一键发布**：无需手动下载、打包、上传，工作流会自动处理所有步骤。

### 准备压缩包

将游戏 DLL 按 `game/<PackageId>/lib/` 整理好后压缩为单个 `.zip`。遵循 [`game-libs.manifest.yaml`](game/game-libs.manifest.yaml) 中的映射即可；若希望自动化分拣，可从仓库 Release 页面下载 FileCourier，可执行文件与 manifest 放在同一目录后运行：

```bash
./FileCourier "<游戏安装目录>" "<临时输出>/game" -m game-libs.manifest.yaml
```

复制出的 `game/` 目录即可直接用于打包。若想了解该工具的维护计划与贡献方式，请参阅下文“[FileCourier 自动分拣工具](#filecourier-自动分拣工具)”。

### 发布到私有源（GitHub Actions）

1. **创建你的仓库**：将本仓库作为模板 (Use this template) 或直接 Fork 来创建你自己的仓库。
2. **配置仓库机密**：为避免在日志中暴露下载地址，工作流将从名为 `LF2_GAME_LIBS_URL` 的[仓库机密](https://docs.github.com/zh/actions/security-guides/encrypted-secrets#creating-encrypted-secrets-for-a-repository)中读取游戏库的下载地址。请预先在你的仓库中设置该机密。
3. **运行工作流**：在你的仓库 `Actions` 页面找到 `Publish Game Libraries` 工作流。
4. **提供参数并执行**：根据工作流的提示，提供游戏库的版本号与压缩包下载地址。工作流会自动下载并解压你准备的压缩包。

工作流会自动识别 `projects/unmanaged-vendor/` 下标记为可打包的工程并生成对应的 NuGet 包，无需额外配置。

> [!WARNING]
> **个人仓库的包可见性风险**
> 若你的仓库是**个人**公开仓库，工作流创建的包将默认为**公开**。由于存在法律风险，你必须在首次发布后，手动将包的可见性设为**私有**。组织仓库无此风险。更详细的说明请参阅[操作指南](../../docs/how-to/game-libs-remote-publish.md)。

> [!NOTE]
> **工作流可能不会立即显示**
> 由于 [GitHub 的一个已知问题](https://github.com/orgs/community/discussions/25219)，基于模板创建的仓库，其工作流（Workflows）可能不会自动出现在 `Actions` 页面。如果 `Publish Game Libraries` 工作流没有显示，你可能需要对工作流文件（例如，在 `.github/workflows/` 目录下）进行一次重命名（或任意修改）并提交，才能触发 GitHub Actions 的识别。

### 必要目录结构：游戏核心程序集

确保压缩包中的目录层级与 `game/<PackageId>/lib/` 一致。这样生成的 NuGet 包会将 DLL 作为仅编译时引用，避免在发布 Mod 时重复分发游戏文件。完整示例与校验方法同样可在[参考手册](../../docs/reference/game-libs-packaging.md#目录结构与清单)中找到。

### 配置开发环境以使用私有源

当你的私有源上已经有打包好的游戏依赖后，你和其他团队成员便需要配置本地环境来使用它。

这通常只需要在根目录的 `nuget.config` 文件中，为你的私有包源新增一个 `<add>` 条目。例如，若你的 GitHub 用户名是 `MyUser`，则源地址为 `https://nuget.pkg.github.com/MyUser/index.json`。同时，你还需要配置对应的凭据。请保留示例中提供的 `iplaylf2` 源，以便持续访问公开维护的依赖。

> [!NOTE]
> **清理冲突源**
> 如果你曾使用过备选的“本地打包”方案，请在恢复依赖前运行 `dotnet nuget disable source local` 暂停本地源，确保 `dotnet restore` 优先使用远程私有源。

## 备选方案：本地打包

对于快速验证或无法联网的场景，可以选择本地打包方案。具体命令行步骤、验证方法以及升级建议均整理在 [使用 GitHub Actions 发布游戏依赖](../../docs/how-to/game-libs-remote-publish.md) 与 [离线环境下的游戏依赖准备](../../docs/how-to/game-libs-offline-setup.md) 中；整体策略与可选项则汇总在 [游戏依赖打包参考手册](../../docs/reference/game-libs-packaging.md)。

## FileCourier 自动分拣工具

FileCourier 是本仓库孵化中的跨平台小工具，用于根据 `game-libs.manifest.yaml` 自动从游戏安装目录复制所需 DLL 并生成符合规范的 `game/` 目录。源码位于 `projects/unmanaged-vendor/tools/FileCourier/`，其目标和使用范围将在此 README 中同步更新；如需试用或反馈问题，欢迎先参考上文的快速上手说明并在仓库提交 issue。

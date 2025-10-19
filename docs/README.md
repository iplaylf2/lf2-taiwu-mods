# 文档索引

本目录将项目文档按照用途划分，帮助你更快找到所需的信息。

- **快速上手**
  - 根目录 `README.md`：了解模板特性、准备开发环境、创建首个 Mod。
- **操作指南**
  - [依赖管理操作指南](./how-to/dependency-management.md)：控制依赖内嵌、手动打包第三方库、利用物料覆盖进行深度定制。
  - [离线环境下的游戏依赖准备](./how-to/game-libs-offline-setup.md)：在离线或受限网络环境中，通过本地 NuGet 源完成依赖恢复。
  - [使用 GitHub Actions 发布游戏依赖](./how-to/game-libs-remote-publish.md)：准备官方游戏 DLL、运行自动化工作流并验证私有源中的包。
- **参考资料**
  - [构建系统参考](./reference/build-system.md)：查阅 MSBuild 变量与核心工具说明，支持进一步定制。
  - [仓库目录结构](./reference/repository-layout.md)：了解模板的标准目录划分及职责说明。
- **项目附录**
  - [`projects/mods/README.md`](../projects/mods/README.md)：面向玩家与开发者的仓库内 Mod 导览。
  - [`projects/unmanaged-vendor/README.md`](../projects/unmanaged-vendor/README.md)：非托管依赖的打包与自动化工作流说明。

> [!TIP]
> 如果你不确定从何处开始，可以先阅读根目录的 `README.md`，然后根据需求跳转到对应的指南或参考文档。

## 快速任务映射

- **首次搭建环境 / 创建新 Mod**：根目录 `README.md`、[离线依赖准备](./how-to/game-libs-offline-setup.md)。
- **修复缺失的游戏程序集或第三方库**：[`projects/unmanaged-vendor/README.md`](../projects/unmanaged-vendor/README.md)、[使用 GitHub Actions 发布游戏依赖](./how-to/game-libs-remote-publish.md)。
- **定制构建 / 发布流程**：[构建系统参考](./reference/build-system.md)、[`LF2Mod.targets`](../projects/mods/LF2Mod.targets)。
- **查阅现有 Mod 实践**：[`projects/mods/README.md`](../projects/mods/README.md) 及其子目录源码。

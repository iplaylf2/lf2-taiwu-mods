# 文档索引

本目录将项目文档按照用途划分，帮助你更快找到所需的信息。

- **快速上手**
  - 根目录 `README.md`：了解模板特性、准备开发环境、创建首个 Mod。
- **操作指南**
  - [依赖管理操作指南](./how-to/dependency-management.md)
  - [本地源方案操作指南](./how-to/game-libs-local-setup.md)
  - [NuGet 源管理指南](./how-to/nuget-source-management.md)
- **参考资料**
  - [构建系统参考](./reference/build-system.md)
  - [游戏依赖包说明](./reference/game-dependencies.md)
  - [依赖基础设施](./reference/dependency-infrastructure.md)
  - [仓库目录结构](./reference/repository-layout.md)
- **项目附录**
  - [`projects/mods/README.md`](../projects/mods/README.md)：面向玩家与开发者的仓库内 Mod 导览。
  - [`projects/unmanaged-vendor/README.md`](../projects/unmanaged-vendor/README.md)：非托管依赖的打包与自动化工作流说明。

> [!TIP]
> 如果你不确定从何处开始，可以先阅读根目录的 `README.md`，然后根据需求跳转到对应的指南或参考文档。

## 快速任务映射

- **首次搭建环境 / 创建新 Mod**：根目录 `README.md`、[本地源方案操作指南](./how-to/game-libs-local-setup.md)。
- **修复缺失的游戏程序集或第三方库**：[`projects/unmanaged-vendor/README.md`](../projects/unmanaged-vendor/README.md)。
- **定制构建 / 发布流程**：[构建系统参考](./reference/build-system.md)（包含 [`LF2Mod.targets`](../projects/mods/LF2Mod.targets) 的详细说明）。
- **查阅现有 Mod 实践**：[`projects/mods/README.md`](../projects/mods/README.md) 及其子目录源码。

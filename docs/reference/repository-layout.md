# 仓库目录结构

此页面概览仓库的核心目录，便于快速了解模板的默认布局以及各目录的职责。

```text
.
├── docs/                     # 项目文档与指南
├── Directory.Packages.props  # 全局 NuGet 包版本管理
├── .lf2.nupkg/               # 执行 `dotnet pack ./projects/unmanaged-vendor/game/game.slnx` 时的默认输出目录
├── .lf2.publish/             # Mod 发布目录（执行 LF2PublishMod 目标后生成）
├── projects/
│   ├── common/               # 所有 Mod 可复用的公共库
│   ├── mods/                 # 你的 Mod 工作区，每个 Mod 建议单独目录
│   │   └── MyNewMod/
│   │       ├── MyNewMod.Backend/
│   │       ├── MyNewMod.Frontend/
│   │       └── Config.Lua    # Mod 官方配置文件，建议纳入版本控制
│   └── unmanaged-vendor/     # 非托管资源（如游戏 DLL）打包与配置
└── nuget.config              # 包源配置
```

> [!TIP]
> 在 `projects/mods/` 下创建新 Mod 时，只需遵循 `.Backend` / `.Frontend` 的命名约定，即可自动继承模板的构建配置。

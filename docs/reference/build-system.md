# 构建系统参考

本页汇总了仓库内与构建系统相关的可查阅信息，例如常用的 MSBuild 变量和关键工具，便于在默认模板之外进一步定制。

## 查阅构建变量

本项目的自动化构建依赖于一系列在各级 `Directory.Build.props` 文件中定义的 MSBuild 变量（如 `LF2IsBackend` 等）。

如果你需要进行深度定制（例如，在 `.csproj` 中添加自定义构建逻辑），可以直接查阅这些 `.props` 文件来了解所有可用的变量。

## 关于二进制依赖的范围

请注意，通过 NuGet 包（如 `LF2.Taiwu.Backend`）引用的游戏库是为适配当前仓库中的 Mod 而精心筛选的。如果你在开发中发现缺少某个游戏 API，则可能需要自行引用包含该 API 的其他游戏程序集。

## 核心工具

本模板的自动化功能主要由以下几个关键的开源工具驱动。感谢它们的开发者。

- **[ILRepack.Lib.MSBuild.Task](https://github.com/ravibpatel/ILRepack.Lib.MSBuild.Task)**：负责将项目引用的所有第三方 DLL 合并到最终生成的 Mod 程序集中，解决“DLL 地狱”问题。
- **[Publicizer](https://github.com/krafs/Publicizer)**：此工具能够让 C# 编译器像访问 `public` 成员一样访问程序集中的 `private` 和 `internal` 成员，极大地提升了开发效率。

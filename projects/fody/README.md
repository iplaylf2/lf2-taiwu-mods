# Fody Weavers 工作区

`projects/fody/` 目录用于集中存放所有可复用的 Fody Weaver。它沿用仓库惯例，通过多层 `Directory.Build.props` 提前注入以下约定：

- **统一目标框架**：默认生成 `netstandard2.0` 程序集，便于其他 .NET 版本在构建时加载。
- **依赖管理**：所有 Weaver 会自动引入 `Fody` 与 `FodyHelpers`，保持内部工具链一致性。

## 新增 Weaver 的步骤

1. 在 `projects/fody/` 下创建一个以 `LF2.Weavers.*` 命名的文件夹，例如 `LF2.Weavers.SecurityPermission/`。
2. 在文件夹中创建最小化的 `*.csproj`（通常 `<Project Sdk="Microsoft.NET.Sdk" />` 即可），其余的通用配置由上层 Props 注入。
3. 添加 `ModuleWeaver` 实现，并根据需求扩展辅助类型。
4. 在 `lf2-taiwu-mods.slnx` 中引入该项目，以便在 IDE 中调试与共享。

> 提示：若 Weaver 需要向下游公开附加构建资产，可在对应项目内添加 `build/`、`buildTransitive/` 等 NuGet 约定目录，或在目录下添加定制 Targets 进行扩展。

## 示例

`LF2.Weavers.SecurityPermission` 展示了一个最小实现：它会在程序集后期注入 `SecurityPermissionAttribute`，从而让前端程序集保留被编译器拒绝的废弃声明。后续添加的 Weaver 可以以它为模板进行迭代。

# 构建系统参考

本页汇总了仓库内与构建系统相关的可查阅信息，例如常用的 MSBuild 变量和关键工具，便于在默认模板之外进一步定制。

## 查阅构建变量

本项目的自动化构建依赖于一系列在各级 `Directory.Build.props` 文件中定义的 MSBuild 变量（如 `LF2IsBackend` 等）。

如果你需要进行深度定制（例如，在 `.csproj` 中添加自定义构建逻辑），可以直接查阅这些 `.props` 文件来了解所有可用的变量。

## 处理缺失的游戏依赖

请注意，本模板通过 `game-libs.manifest.yaml` 提供的依赖包已覆盖绝大部分开发场景，但并未包含游戏目录下的全部程序集。

如果你在开发中发现缺少某个游戏 API，这意味着其所在的程序集尚未被打包。正确的做法是更新 `game-libs.manifest.yaml` 文件，将缺失的程序集添加到打包列表中，并重新生成 NuGet 包。

不建议在项目文件中直接通过文件路径引用 DLL，因为这会绕过模板提供的依赖管理机制。

关于如何更新清单并重新打包，请参阅《[游戏依赖打包参考手册](./game-libs-packaging.md)》。

## 打包 Mod

模板在 `projects/mods/LF2Mod.targets` 中提供了 `LF2PublishMod` 目标，用来将指定的 Mod 项目打包成游戏识别的目录结构。

```bash
dotnet build -c Release -t:LF2PublishMod -p:LF2Mod=<mod-name>
```

使用 `-c Release` 参数是触发 `ILRepack` 工具执行的关键。在 `Release` 模式下，构建系统会自动将所有必要的依赖项 DLL 合并到你的 Mod 主程序集中，生成一个独立的、不会与其他 Mod 产生依赖冲突的文件。

命令会同时编译前端与后端插件，并将生成的 Mod 包写入仓库根目录的 `.lf2.publish/` 中，方便直接复制到游戏的 Mods 目录或用于后续发版操作。

## Git 标签与自动打包

当需要产出可分发的压缩包时，可以通过 Git 标签触发 `.github/workflows/push-to-mod-tag.yaml` 中的自动化流程。标签格式固定为 `mods/<mod-name>/v<version>`，例如：

```bash
git tag mods/my-mod/v1.2.0
git push origin mods/my-mod/v1.2.0
```

推送后，工作流会读取标签中的 Mod 名称与版本号，调用 `LF2PublishMod` 目标生成 `.lf2.publish/<mod-name>/` 下的产物，并将其打包为 `<mod-name>_v<version>.zip` 发布到 GitHub Release，方便他人直接下载。

## 核心工具

本模板的自动化功能主要由以下几个关键的开源工具驱动。感谢它们的开发者。

- **[ILRepack.Lib.MSBuild.Task](https://github.com/ravibpatel/ILRepack.Lib.MSBuild.Task)**：负责将项目引用的所有第三方 DLL 合并到最终生成的 Mod 程序集中，解决“DLL 地狱”问题。
- **[Publicizer](https://github.com/krafs/Publicizer)**：此工具能够让 C# 编译器像访问 `public` 成员一样访问程序集中的 `private` 和 `internal` 成员，极大地提升了开发效率。

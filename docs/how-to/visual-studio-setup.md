# Visual Studio 环境配置指南

本指南专为使用 Visual Studio 的开发者提供常见问题的解决方案，主要涵盖两个核心场景：首次恢复依赖时的凭据配置，以及在 Release 编译模式下可能遇到的文件占用错误。

## 凭据配置：访问 GitHub Packages

在 Visual Studio 中首次打开解决方案或手动恢复 NuGet 包时，由于项目依赖了托管在 GitHub Packages 上的私有包，Visual Studio 会自动弹出凭据输入窗口。正确配置这些凭据是顺利完成依赖恢复的第一步。

请按以下方式填写凭据信息：

- **用户名**：你的 GitHub 用户名（或任意非空字符串）
- **密码**：你的 GitHub Personal Access Token (PAT)

> [!IMPORTANT]
> 用于密码的 PAT 必须拥有 `read:packages` 权限。你可以从 [GitHub 个人访问令牌设置页面](https://github.com/settings/tokens) 创建新的令牌。关于权限的详细说明请参阅：[GitHub Docs - Personal Access Tokens](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token)。

### 清除已缓存的错误凭据

如果在凭据弹窗中输入了错误的信息，Visual Studio 会将其缓存，导致后续的依赖恢复持续失败。此时需要清空 NuGet 缓存来移除错误的凭据记录，让系统重新提示输入正确的凭据。

提供两种清除缓存的方式，可根据使用习惯选择其中一种：

**通过 Visual Studio 界面清空缓存**

1. 打开 Visual Studio，依次进入 **工具** > **选项**
2. 在左侧导航中选择 **NuGet 程序包管理器** > **常规**
3. 点击 **清除所有 NuGet 缓存** 按钮

**通过命令行清空缓存**

```bash
dotnet nuget locals all --clear
```

完成缓存清除后，再次执行依赖恢复操作时，Visual Studio 会重新弹出凭据输入窗口，此时你就可以输入正确的凭据信息了。该命令会清除所有 NuGet 缓存目录，包括 http-cache、packages-cache 和 global-packages。

## 解决 Release 编译中的文件占用错误

在使用 `Release` 配置进行编译时，构建系统会启用 `ILRepack` 工具进行大量文件操作。`ILRepack` 是一个程序集合并工具，在 Release 编译时将所有依赖项合并到单个程序集文件中，以避免依赖冲突。这个过程可能触发杀毒软件的实时扫描机制，导致"文件正在由另一进程使用..."的错误。详细了解请参阅：[构建系统参考 - 核心工具链介绍](../reference/build-system.md#核心工具链介绍)。

### 解决方案：配置杀毒软件排除项

如果你的开发环境也遇到了这个问题，可以通过将项目目录添加到杀毒软件的排除项中来解决。以下以 Windows 平台常见的 Microsoft Defender 为例：

1. 打开 **Windows 安全中心**
2. 进入 **病毒和威胁防护** > **"病毒和威胁防护"设置** > **管理设置**
3. 向下滚动到 **排除项**，点击 **添加或删除排除项**
4. 点击 **添加排除项**，选择 **文件夹**，然后将本仓库的根目录（`lf2-taiwu-mods`）完整添加进去
5. **重启计算机** 使设置完全生效

> [!WARNING]
> 将文件夹添加到排除项会使其免受病毒扫描，这意味着其中潜藏的恶意软件将不会被检测到。请确保你信任该文件夹中的所有文件来源，并自行承担相应风险。

> [!NOTE]
> 此方法在 Visual Studio 2026 环境下经过测试，确能解决 `ILRepack` 引起的文件占用问题。文件占用问题主要影响 Windows 平台的 Defender 杀毒软件，其他杀毒软件可能需要类似配置。重启计算机是确保排除项完全生效的推荐步骤。

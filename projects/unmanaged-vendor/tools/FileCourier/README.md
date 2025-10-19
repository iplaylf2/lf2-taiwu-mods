# FileCourier

一个根据清单（manifest）批量复制文件的命令行小工具。所有官方产物均为 AOT 自包含可执行文件，不依赖额外运行时。

## 快速上手

1. 从 CI 发布页面获取与你平台匹配的二进制（例如 `linux-x64` 或 `win-x64`）。
2. 准备一个 manifest（默认放在执行目录、文件名为 `manifest.yaml`）。
3. 执行命令：

   ```bash
   ./FileCourier <read-working-directory> <write-working-directory> [--manifest <path>]
   ```

   - `<read-working-directory>`：源文件所在目录，必须存在。
   - `<write-working-directory>`：输出目录，若存在则必须为空。
   - `--manifest` / `-m`：manifest 文件路径，省略时默认 `manifest.yaml`。

执行过程中会输出每次复制的进度，失败时返回非零退出码并给出错误说明。

## Manifest 说明

manifest 使用 YAML 数组描述复制计划，每个条目要求如下：

```yaml
- target-dir: config/templates          # 写目录下的目标子目录
  source-files:                         # 读目录下需要复制的相对路径列表
    - templates/base.json
    - templates/options.json
```

- `target-dir`：必须是相对路径，指向写目录内的目标位置。
- `source-files`：必须是相对路径，指向读目录内的文件；若文件缺失或路径指向目录，执行会失败。

仓库根目录提供了最小示例 `manifest.yaml`，可以直接复制后按需调整目标目录与文件列表。

## 更多细节

运行时工具会校验：

1. 读目录存在且可访问。
2. 写目录为空或不存在（不存在时会自动创建）。
3. manifest 中的所有路径都在各自的根目录内，且为相对路径。

如需了解实现或进行二次开发，请直接查阅源码；当前流程遵循常规 .NET 项目惯例，无额外特殊要求。

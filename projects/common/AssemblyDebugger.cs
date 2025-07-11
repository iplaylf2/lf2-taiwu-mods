using System.Reflection;
using System.Text;

namespace LF2.Common;

public static class AssemblyDebugger
{
    public static void PrintAssemblyDetails(string assemblyName)
    {
        try
        {
            // 1. 尝试获取已加载的程序集
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName);

            // 2. 如果未加载则主动加载
            if (assembly == null)
            {
                Console.WriteLine($"Assembly '{assemblyName}' not loaded. Attempting to load...");
                assembly = Assembly.Load(assemblyName);
            }

            // 3. 验证程序集状态
            if (assembly == null)
                throw new FileNotFoundException($"Assembly '{assemblyName}' could not be loaded");

            // 4. 跳过动态程序集（避免异常）
            if (assembly.IsDynamic)
            {
                Console.WriteLine($"Skipping dynamic assembly: {assemblyName}");
                return;
            }

            // 5. 打印程序集基本信息
            var sb = new StringBuilder();
            sb.AppendLine($"\n[Assembly: {assembly.GetName().FullName}]");
            sb.AppendLine($"Location: {assembly.Location}");
            sb.AppendLine($"Loaded from GAC: {assembly.GlobalAssemblyCache}");
            sb.AppendLine($"Defined types count: {assembly.DefinedTypes.Count()}");

            // 6. 获取并打印所有公共类型
            var publicTypes = assembly.GetExportedTypes()
                .OrderBy(t => t.FullName)
                .ToList();

            sb.AppendLine($"Public types ({publicTypes.Count}):");

            foreach (var type in publicTypes)
            {
                try
                {
                    // 获取类型元数据令牌
                    var token = type.MetadataToken;

                    // 获取基类型信息
                    var baseTypeInfo = type.BaseType != null
                        ? $" : {type.BaseType.Name}"
                        : "";

                    // 获取接口信息
                    var interfaces = type.GetInterfaces();
                    var interfaceInfo = interfaces.Any()
                        ? $" (Implements: {string.Join(", ", interfaces.Select(i => i.Name))})"
                        : "";

                    sb.AppendLine($"  0x{token:X8} - {type.FullName}{baseTypeInfo}{interfaceInfo}");

                    // 可选：打印嵌套类型
                    if (type.IsNested)
                    {
                        sb.AppendLine($"      [Nested in: {type.DeclaringType?.FullName}]");
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    sb.AppendLine($"  ERROR loading type: {type.FullName}");
                    sb.AppendLine($"    Loader exceptions: {string.Join("\n    ", ex.LoaderExceptions.Select(e => e.Message))}");
                }
            }

            // 7. 打印程序集依赖
            sb.AppendLine("\nReferenced assemblies:");
            foreach (var refAsm in assembly.GetReferencedAssemblies()
                         .OrderBy(a => a.Name)
                         .Take(10)) // 限制数量避免过多输出
            {
                sb.AppendLine($"  {refAsm.FullName}");
            }

            Console.WriteLine(sb.ToString());
        }
        catch (Exception ex)
        {
            // 8. 加载失败时直接抛出原始异常
            throw new InvalidOperationException(
                $"FAILED TO INSPECT ASSEMBLY '{assemblyName}'", ex);
        }
    }
}

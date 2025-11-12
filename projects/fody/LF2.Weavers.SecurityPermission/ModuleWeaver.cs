using Fody;
using Mono.Cecil;
using CecilCustomAttributeNamedArgument = Mono.Cecil.CustomAttributeNamedArgument;

namespace LF2.Weavers.SecurityPermission;

/// <summary>
/// Adds the deprecated <c>SecurityPermission(SkipVerification = true)</c> attribute post-compilation.
/// </summary>
public sealed class ModuleWeaver : BaseModuleWeaver
{
    public override void Execute()
    {
        if (ModuleDefinition == null)
        {
            throw new InvalidOperationException("ModuleDefinition must be available when weaving.");
        }

        if (HasSecurityPermissionAttribute(ModuleDefinition.Assembly))
        {
            WriteDebug($"{SecurityPermissionFullName} already present, skipping weaving.");
            return;
        }

        var attributeCtor = ResolveSecurityPermissionConstructor();
        var (securityActionType, requestMinimumValue) = ResolveSecurityActionInfo();

        InjectAttribute(attributeCtor, securityActionType, requestMinimumValue);

        WriteInfo($"Injected {AttributeDescription}.");
    }

    public override IEnumerable<string> GetAssembliesForScanning()
    {
        return [];
    }

    private static bool HasSecurityPermissionAttribute(AssemblyDefinition assembly)
    {
        foreach (var attribute in assembly.CustomAttributes)
        {
            if (attribute.AttributeType.Resolve() is not { Namespace: SecurityPermissionsNamespace, Name: SecurityPermissionAttributeName })
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private void InjectAttribute(MethodReference attributeCtor, TypeReference securityActionType, object requestMinimumValue)
    {
        var attribute = new CustomAttribute(attributeCtor);

        attribute.ConstructorArguments.Add(new CustomAttributeArgument(securityActionType, requestMinimumValue));

        attribute.Properties.Add
        (
            new CecilCustomAttributeNamedArgument
            (
                SkipVerificationPropertyName,
                new CustomAttributeArgument(ModuleDefinition.TypeSystem.Boolean, true)
            )
        );

        ModuleDefinition.Assembly.CustomAttributes.Add(attribute);
    }

    private MethodReference ResolveSecurityPermissionConstructor()
    {
        var attributeType = FindRequiredType(SecurityPermissionAttributeName);
        var ctorDefinition = attributeType.Methods.FirstOrDefault
        (
            method =>
                method.IsConstructor &&
                method.Parameters.Count == 1 &&
                method.Parameters[0].ParameterType.Namespace == SecurityPermissionsNamespace &&
                method.Parameters[0].ParameterType.Name == SecurityActionTypeName
        );

        return ctorDefinition == null
            ? throw new WeavingException($"{SecurityPermissionAttributeName}({SecurityActionTypeName}) constructor missing in referenced assemblies.")
            : ModuleDefinition.ImportReference(ctorDefinition);
    }

    private (TypeReference SecurityActionType, object RequestMinimumValue) ResolveSecurityActionInfo()
    {
        var securityActionDefinition = FindRequiredType(SecurityActionTypeName);
        var requestMinimumField = securityActionDefinition.Fields.FirstOrDefault(field => field.Name == SecurityActionRequestMinimumField);

        if (requestMinimumField?.Constant is not { } constantValue)
        {
            throw new WeavingException($"{RequestMinimumConstantFullName} constant missing in referenced assemblies.");
        }

        var securityActionType = ModuleDefinition.ImportReference(securityActionDefinition);
        return (securityActionType, constantValue);
    }

    private TypeDefinition FindRequiredType(string typeName)
    {
        foreach (var module in EnumerateCandidateModules())
        {
            var type = module.Types.FirstOrDefault(t => t.Namespace == SecurityPermissionsNamespace && t.Name == typeName);
            if (type != null)
            {
                return type;
            }
        }

        throw new WeavingException($"Failed to resolve '{SecurityPermissionsNamespace}.{typeName}' in target assembly references.");
    }

    private IEnumerable<ModuleDefinition> EnumerateCandidateModules()
    {
        yield return ModuleDefinition;

        if (_referenceModulesCache is { } cachedModules)
        {
            foreach (var cached in cachedModules)
            {
                yield return cached;
            }

            yield break;
        }

        if (AssemblyResolver == null)
        {
            _referenceModulesCache = [];
            yield break;
        }

        var resolvedModules = new List<ModuleDefinition>();

        foreach (var reference in ModuleDefinition.AssemblyReferences)
        {
            ModuleDefinition? resolvedModule = null;

            try
            {
                resolvedModule = AssemblyResolver.Resolve(reference)?.MainModule;
            }
            catch (AssemblyResolutionException ex)
            {
                WriteWarning($"Unable to resolve '{reference.FullName}': {ex.Message}");
            }

            if (resolvedModule == null)
            {
                continue;
            }

            resolvedModules.Add(resolvedModule);
            yield return resolvedModule;
        }

        _referenceModulesCache = resolvedModules;
    }

    private const string SecurityPermissionsNamespace = "System.Security.Permissions";
    private const string SecurityPermissionAttributeName = "SecurityPermissionAttribute";
    private const string SecurityActionTypeName = "SecurityAction";
    private const string SecurityActionRequestMinimumField = "RequestMinimum";
    private const string SkipVerificationPropertyName = "SkipVerification";
    private static readonly string SecurityPermissionFullName = $"{SecurityPermissionsNamespace}.{SecurityPermissionAttributeName}";
    private static readonly string SecurityActionFullName = $"{SecurityPermissionsNamespace}.{SecurityActionTypeName}";
    private static readonly string RequestMinimumConstantFullName = $"{SecurityActionFullName}.{SecurityActionRequestMinimumField}";
    private static readonly string AttributeDescription = $"{SecurityPermissionAttributeName}({SecurityActionTypeName}.{SecurityActionRequestMinimumField}, {SkipVerificationPropertyName} = true)";

    private List<ModuleDefinition>? _referenceModulesCache;
}

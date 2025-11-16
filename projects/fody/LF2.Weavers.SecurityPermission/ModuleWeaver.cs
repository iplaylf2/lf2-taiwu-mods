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

        var requestMinimumArgument = ResolveRequestMinimumArgument();
        var attributeCtor = ResolveSecurityPermissionConstructor(requestMinimumArgument.Type);

        var attribute = new CustomAttribute(attributeCtor);

        attribute.ConstructorArguments.Add(requestMinimumArgument);

        attribute.Properties.Add
        (
            new CecilCustomAttributeNamedArgument
            (
                SkipVerificationPropertyName,
                new CustomAttributeArgument(ModuleDefinition.TypeSystem.Boolean, true)
            )
        );

        ModuleDefinition.Assembly.CustomAttributes.Add(attribute);

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
            if
            (
                attribute.AttributeType.Resolve() is not
                {
                    Namespace: SecurityPermissionsNamespace,
                    Name: SecurityPermissionAttributeName
                }
            )
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private MethodReference ResolveSecurityPermissionConstructor(TypeReference securityActionType)
    {
        var attributeType = FindSecurityPermissionsType(SecurityPermissionAttributeName);
        var ctorDefinition = attributeType.Methods.FirstOrDefault
        (
            method =>
                method.IsConstructor &&
                method.Parameters.Count == 1 &&
                method.Parameters[0].ParameterType.FullName == securityActionType.FullName
        );

        return ctorDefinition == null
        ? throw new WeavingException
        (
            $"{SecurityPermissionAttributeName}({SecurityActionTypeName}) constructor missing in referenced assemblies."
        )
        : ModuleDefinition.ImportReference(ctorDefinition);
    }

    private CustomAttributeArgument ResolveRequestMinimumArgument()
    {
        var securityActionDefinition = FindSecurityPermissionsType(SecurityActionTypeName);
        var requestMinimumField = securityActionDefinition
        .Fields
        .FirstOrDefault(field => field.Name == SecurityActionRequestMinimumField);

        if (requestMinimumField.Constant is not { } constantValue)
        {
            throw new WeavingException
            (
                $"{RequestMinimumConstantFullName} constant missing in referenced assemblies."
            );
        }

        var securityActionType = ModuleDefinition.ImportReference(securityActionDefinition);
        return new CustomAttributeArgument(securityActionType, constantValue);
    }

    private TypeDefinition FindSecurityPermissionsType(string typeName)
    {
        foreach (var module in EnumerateCandidateModules())
        {
            var type = module
            .Types
            .FirstOrDefault
            (
                t => t.Namespace == SecurityPermissionsNamespace && t.Name == typeName
            );

            if (type != null)
            {
                return type;
            }
        }

        throw new WeavingException
        (
            $"Failed to resolve '{SecurityPermissionsNamespace}.{typeName}' in target assembly references."
        );
    }

    private IEnumerable<ModuleDefinition> EnumerateCandidateModules()
    {
        yield return ModuleDefinition;

        if (AssemblyResolver == null)
        {
            yield break;
        }

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

            yield return resolvedModule;
        }
    }

    private const string SecurityPermissionsNamespace = "System.Security.Permissions";
    private const string SecurityPermissionAttributeName = "SecurityPermissionAttribute";
    private const string SecurityActionTypeName = "SecurityAction";
    private const string SecurityActionRequestMinimumField = "RequestMinimum";
    private const string SkipVerificationPropertyName = "SkipVerification";
    private const string SecurityPermissionFullName
    = $"{SecurityPermissionsNamespace}.{SecurityPermissionAttributeName}";
    private const string SecurityActionFullName
    = $"{SecurityPermissionsNamespace}.{SecurityActionTypeName}";
    private const string RequestMinimumConstantFullName
    = $"{SecurityActionFullName}.{SecurityActionRequestMinimumField}";
    private const string AttributeDescription
    = $"{SecurityPermissionAttributeName}({SecurityActionTypeName}.{SecurityActionRequestMinimumField}, {SkipVerificationPropertyName} = true)";

}

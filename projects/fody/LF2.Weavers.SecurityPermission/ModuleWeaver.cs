using System.Reflection;
using Fody;
using Mono.Cecil;
using CecilCustomAttributeNamedArgument = Mono.Cecil.CustomAttributeNamedArgument;
using BclSecurityAction = System.Security.Permissions.SecurityAction;
using SecurityPermissionAttribute = System.Security.Permissions.SecurityPermissionAttribute;

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
            WriteDebug("SecurityPermissionAttribute already present, skipping weaving.");
            return;
        }

        var constructor = typeof(SecurityPermissionAttribute).GetConstructor([typeof(BclSecurityAction)]);
        if (constructor == null)
        {
            WriteWarning("SecurityPermissionAttribute(SecurityAction) constructor missing; skipping weaving.");
            return;
        }

        InjectAttribute(constructor);

        WriteInfo("Injected SecurityPermissionAttribute(SecurityAction.RequestMinimum, SkipVerification = true).");
    }

    public override IEnumerable<string> GetAssembliesForScanning()
    {
        return [];
    }

    private static bool HasSecurityPermissionAttribute(AssemblyDefinition assembly)
    {
        foreach (var attribute in assembly.CustomAttributes)
        {
            if (attribute.AttributeType.Resolve() is not { Name: nameof(SecurityPermissionAttribute) })
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private void InjectAttribute(ConstructorInfo attributeCtor)
    {
        var constructorRef = ModuleDefinition.ImportReference(attributeCtor);
        var securityActionRef = ModuleDefinition.ImportReference(typeof(BclSecurityAction));

        var attribute = new CustomAttribute(constructorRef);

#pragma warning disable CS0618 // Type or member is obsolete
        attribute.ConstructorArguments.Add(new CustomAttributeArgument(securityActionRef, BclSecurityAction.RequestMinimum));
#pragma warning restore CS0618 // Type or member is obsolete

        attribute.Properties.Add
        (
            new CecilCustomAttributeNamedArgument
            (
                nameof(SecurityPermissionAttribute.SkipVerification),
                new CustomAttributeArgument(ModuleDefinition.TypeSystem.Boolean, true)
            )
        );

        ModuleDefinition.Assembly.CustomAttributes.Add(attribute);
    }
}

using System.Reflection;

namespace Application;

/// <summary>
/// Reference to Application assembly for MediatR registration
/// </summary>
public static class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}


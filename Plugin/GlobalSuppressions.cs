// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppression either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Some patches are property names or private methods, which are underscore named", Scope = "namespaceanddescendants", Target = "~N:UniTASPlugin.Patches")]
[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Some patches are property names or private methods, which are underscore named", Scope = "namespaceanddescendants", Target = "~N:UniTASPlugin.Patches.__System")]
[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original method naming matters for clearer code", Scope = "namespaceanddescendants", Target = "~N:UniTASPlugin.ReversePatches")]
[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>", Scope = "member", Target = "~M:UniTASPlugin.Plugin.Awake")]
[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>", Scope = "member", Target = "~M:UniTASPlugin.Plugin.Update")]
[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>", Scope = "member", Target = "~M:UniTASPlugin.Plugin.FixedUpdate")]
[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Reflects original unity names", Scope = "namespaceanddescendants", Target = "~N:UniTASPlugin.VersionSafeWrapper")]

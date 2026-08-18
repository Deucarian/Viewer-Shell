using System;
using System.Collections.Generic;
using System.IO;
using Deucarian.UI.Editor;
using NUnit.Framework;
using UnityEditor.PackageManager;

namespace Deucarian.ViewerShell.Tests
{
    public sealed class ViewerShellArchitectureTests
    {
        [Test]
        public void RuntimeDelegatesAllScreenSpaceLayeringToUiPackage()
        {
            string runtimeRoot = Path.Combine(ResolvePackageRoot(), "Runtime");
            IReadOnlyList<DeucarianUILayeringArchitectureViolation>
                violations =
                    DeucarianUILayeringArchitectureValidator
                        .ValidateRuntimeRoot(runtimeRoot);

            Assert.That(
                violations,
                Is.Empty,
                string.Join(Environment.NewLine, violations));
        }

        [Test]
        public void RuntimeContainsNoProductOrNavigationSemantics()
        {
            string runtimeRoot = Path.Combine(ResolvePackageRoot(), "Runtime");
            string[] paths = Directory.GetFiles(
                runtimeRoot,
                "*.cs",
                SearchOption.AllDirectories);
            foreach (string path in paths)
            {
                string source = File.ReadAllText(path);
                StringAssert.DoesNotContain("Simultria", source, path);
                StringAssert.DoesNotContain("ReportViewer", source, path);
                StringAssert.DoesNotContain("Report Viewer", source, path);
                StringAssert.DoesNotContain("ActivityViewer", source, path);
                StringAssert.DoesNotContain(
                    "Deucarian.ViewerNavigation",
                    source,
                    path);
            }
        }

        [Test]
        public void MetadataPinsAuthoritativeSharedOwners()
        {
            string packageJson = File.ReadAllText(
                Path.Combine(ResolvePackageRoot(), "package.json"));

            StringAssert.Contains(
                "\"unity\": \"6000.0\"",
                packageJson);
            StringAssert.Contains(
                "\"com.deucarian.theming\": \"1.0.5\"",
                packageJson);
            StringAssert.Contains(
                "\"com.deucarian.ui\": \"0.2.7\"",
                packageJson);
            StringAssert.Contains(
                "\"com.deucarian.viewer-rendering\": \"0.1.0\"",
                packageJson);
        }

        private static string ResolvePackageRoot()
        {
            PackageInfo package = PackageInfo.FindForAssembly(
                typeof(ViewerShellPresenter).Assembly);
            Assert.NotNull(package);
            Assert.IsTrue(Directory.Exists(package.resolvedPath));
            return package.resolvedPath;
        }
    }
}

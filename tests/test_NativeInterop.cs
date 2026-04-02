    public static class NativeInterop
        public static void TestLoadNativeInMemoryAssembly()
            string tempDir = Path.Combine(Path.GetTempPath(), "TestLoadNativeInMemoryAssembly");
            string testDll = Path.Combine(tempDir, "test.dll");
            if (!File.Exists(testDll))
                Directory.CreateDirectory(tempDir);
                bool result = CreateTestDll(testDll);
                Assert.True(result, "The call to 'CreateTestDll' should be successful and return true.");
                Assert.True(File.Exists(testDll), "The test assembly should be created.");
            var asmName = AssemblyName.GetAssemblyName(testDll);
            string asmFullName = SearchAssembly(asmName.Name);
            Assert.Null(asmFullName);
                int ret = LoadAssemblyTest(testDll);
                Assert.Equal(0, ret);
            asmFullName = SearchAssembly(asmName.Name);
            Assert.Equal(asmName.FullName, asmFullName);
        private static unsafe int LoadAssemblyTest(string assemblyPath)
            // The 'LoadAssemblyFromNativeMemory' method is annotated with 'UnmanagedCallersOnly' attribute,
            // so we have to use the 'unmanaged' function pointer to invoke it.
            delegate* unmanaged<IntPtr, int, int> funcPtr = &PowerShellUnsafeAssemblyLoad.LoadAssemblyFromNativeMemory;
            IntPtr nativeMem = IntPtr.Zero;
                using (var fileStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read))
                    length = (int)fileStream.Length;
                    nativeMem = Marshal.AllocHGlobal(length);
                    using var unmanagedStream = new UnmanagedMemoryStream((byte*)nativeMem, length, length, FileAccess.Write);
                    fileStream.CopyTo(unmanagedStream);
                // Call the function pointer.
                return funcPtr(nativeMem, length);
                // Free the native memory
                Marshal.FreeHGlobal(nativeMem);
        private static string SearchAssembly(string assemblyName)
            Assembly asm = AssemblyLoadContext.Default.Assemblies.FirstOrDefault(
                assembly => assembly.FullName.StartsWith(assemblyName, StringComparison.OrdinalIgnoreCase));
            return asm?.FullName;
        private static bool CreateTestDll(string dllPath)
            var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            SourceText sourceText = SourceText.From("public class Utt { }");
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(sourceText, parseOptions));
            var refs = new List<PortableExecutableReference> { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) };
            Compilation compilation = CSharpCompilation.Create(
                        Path.GetRandomFileName(),
                        references: refs,
                        options: compilationOptions);
            using var fs = new FileStream(dllPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
            EmitResult emitResult = compilation.Emit(peStream: fs, options: null);
            return emitResult.Success;

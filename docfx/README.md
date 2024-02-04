## docfx ドキュメントビルド
`docfx docfx.json --serve`


## macOSでdocfxのビルド時にエラーが発生
```
% docfx docfx.json 
Searching custom plugins in directory /opt/homebrew/Cellar/docfx/2.75.2/libexec/...
No files are found with glob pattern images/**, excluding <none>, under directory "/Users/adamac/work/plateau/PLATEAU-PointCloud-Generator/docfx"
7 plug-in(s) loaded.
warning: No template bundles were found, no template will be applied to the documents. 1) Check your docfx.json 2) the templates subfolder exists inside your application folder or your docfx.json directory.
Building 2 file(s) in TocDocumentProcessor(BuildTocDocument)...
Building 4 file(s) in ConceptualDocumentProcessor(BuildConceptualDocument=>ValidateConceptualDocumentMetadata)...
Applying templates to 6 model(s)...
warning: UnknownContentTypeForTemplate: There is no template processing document type(s): Conceptual,Toc
XRef map exported.


Build succeeded with warning.

    2 warning(s)
    0 error(s)
```
- `docfx template list`でもエラーが発生

```
% docfx template list
DirectoryNotFoundException: Could not find a part of the path '/opt/homebrew/Cellar/docfx/2.75.2/libexec/templates'.
  at IntPtr CreateDirectoryHandle(string path, bool ignoreNotFound)                                                                                               
  at void Init()                                                                                                                                                  
  at ctor(string directory, FindTransform transform, EnumerationOptions options, bool isNormalized)                                                               
  at IEnumerable<string> UserDirectories(string directory, string expression, EnumerationOptions options)                                                         
  at IEnumerable<string> InternalEnumeratePaths(string path, string searchPattern, SearchTarget searchTarget, EnumerationOptions options)                         
  at string[[]] GetDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions)                                                          
  at int Execute(CommandContext context) in TemplateCommand.cs:17                                                                                                 
  at Task<int> Execute(CommandContext context, CommandSettings settings) in Command.cs:25                                                                         
  at Task<int> Execute(CommandTree leaf, CommandTree tree, CommandContext context, ITypeResolver resolver, IConfiguration configuration) in CommandExecutor.cs:144
  at async Task<int> Execute(IConfiguration configuration, IEnumerable<string> args) in CommandExecutor.cs:83                                                     
  at async Task<int> RunAsync(IEnumerable<string> args) in CommandApp.cs:84  
```

- このissueを読んで解決
- https://github.com/dotnet/docfx/issues/9081
- docfxのReleaseからtemplateを`/opt/homebrew/Cellar/docfx/2.75.2/libexec/templates`へ移動
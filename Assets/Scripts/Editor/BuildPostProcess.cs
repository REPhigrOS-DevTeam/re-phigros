#if UNITY_IPHONE
using System.IO;
using UnityEditor;
// using UnityEditor.Build;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace Editor
{
    public class BuildPostProcess
    {
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget target, string path)
        {
            if (target != BuildTarget.iOS) return;
            // string applicationIdentifier = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(target)));
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(Path.Combine(path, "info.plist")));
            PlistElementDict infoDict = plist.root;
            infoDict.SetBoolean("UIFileSharingEnabled", true); // 对第三方软件（例：iTunes）开放读写文件权限
            infoDict.SetBoolean("UISupportsDocumentBrowser", true); // 对iOS系统APP“文件”开放读写文件权限
            // PlistElementArray documentTypes = infoDict.CreateArray("CFBundleDocumentTypes");
            // PlistElementDict chartArchiveDocumentType = documentTypes.AddDict();
            // chartArchiveDocumentType.SetString("CFBundleTypeName", "Chart Archive");
            // chartArchiveDocumentType.SetString("CFBundleTypeRole", "Default");
            // PlistElementArray contentTypes = chartArchiveDocumentType.CreateArray("LSItemContentTypes");
            // contentTypes.AddString("com.pkware.zip-archive");
            // contentTypes.AddString($"{applicationIdentifier}.pez");
            // PlistElementArray UTIs = infoDict.CreateArray("UTExportedTypeDeclarations");
            // PlistElementDict UTI = UTIs.AddDict();
            // PlistElementArray conformsTo = UTI.CreateArray("UTTypeConformsTo");
            // conformsTo.AddString("public.data");
            // conformsTo.AddString("public.archive");
            // UTI.SetString("UTTypeDescription", "RE:PhiEdit Chart Archive");
            // PlistElementArray _ = UTI.CreateArray("UTTypeIconFiles"); // 咱们没有图标文件desu
            // UTI.SetString("UTTypeIdentifier", $"{applicationIdentifier}.pez");
            // PlistElementDict typeIdentifiers = UTI.CreateDict("UTTypeTagSpecification");
            // PlistElementArray fileExtensions = typeIdentifiers.CreateArray("public.filename-extension");
            // fileExtensions.AddString("pez");
            // PlistElementArray mimeTypes = typeIdentifiers.CreateArray("public.mime-type");
            // mimeTypes.AddString("pez");
            plist.WriteToFile(Path.Combine(path, "info.plist"));
        }
    }
}
#endif
//#if UNITY_ANDROID
//using UnityEditor;
//using UnityEditor.Callbacks;
//using System.IO;
//using System.Xml;

//public class RemovePhoneStatePermission
//{
//    [PostProcessBuild(50)]
//    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
//    {
//        if (target != BuildTarget.Android) return;

//        string manifestPath = Path.Combine(pathToBuiltProject, "src", "main", "AndroidManifest.xml");

//        if (!File.Exists(manifestPath))
//        {
//            manifestPath = Path.Combine(pathToBuiltProject, "AndroidManifest.xml");
//            if (!File.Exists(manifestPath))
//            {
//                UnityEngine.Debug.LogWarning("AndroidManifest.xml not found.");
//                return;
//            }
//        }

//        XmlDocument manifest = new XmlDocument();
//        manifest.Load(manifestPath);

//        XmlNamespaceManager nsMgr = new XmlNamespaceManager(manifest.NameTable);
//        nsMgr.AddNamespace("android", "http://schemas.android.com/apk/res/android");

//        XmlNode manifestNode = manifest.SelectSingleNode("/manifest");
//        XmlNodeList permissionNodes = manifestNode.SelectNodes("uses-permission");

//        bool modified = false;
//        foreach (XmlNode node in permissionNodes)
//        {
//            XmlAttribute attr = node.Attributes["android:name"];
//            if (attr != null && attr.Value == "android.permission.READ_PHONE_STATE")
//            {
//                manifestNode.RemoveChild(node);
//                modified = true;
//                UnityEngine.Debug.Log("Removed: android.permission.READ_PHONE_STATE");
//                break;
//            }
//        }

//        if (modified)
//        {
//            manifest.Save(manifestPath);
//        }
//    }
//}
//#endif

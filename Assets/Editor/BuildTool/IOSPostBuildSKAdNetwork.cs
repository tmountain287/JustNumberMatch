#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.Collections.Generic;

public class IOSPostBuildSKAdNetwork
{
    private static readonly HashSet<string> SkAdNetworkIds = new HashSet<string>
    {
        "glqzh8vgby.skadnetwork",
        "zq492l623r.skadnetwork",
        "feyaarzu9v.skadnetwork",
        "6yxyv74ff7.skadnetwork",
        "ydx93a7ass.skadnetwork",
        "2u9pt9hc89.skadnetwork",
        "7ug5zh24hu.skadnetwork",
        "k6y4y55b64.skadnetwork",
        "mlmmfzh3r3.skadnetwork",
        "mp6xlyr22a.skadnetwork",
        "5l3tpt7t6e.skadnetwork",
        "zmvfpc5aq8.skadnetwork",
        "3rd42ekr43.skadnetwork",
        "9t245vhmpl.skadnetwork",
        "uw77j35x4d.skadnetwork",
        "77y3x8wds4.skadnetwork",
        "238da6jt44.skadnetwork",
        "v72qych5uu.skadnetwork",
        "mqn7fxpca7.skadnetwork",
        "vhf287vqwu.skadnetwork",
        "424m5254lk.skadnetwork",
        "ppxm28t8ap.skadnetwork",
        "cstr6suwn9.skadnetwork",
        "v9wttpbfk9.skadnetwork",
        "e5fvkxwrpn.skadnetwork",
        "klf5c3l5u5.skadnetwork",
        "44jx6755aq.skadnetwork",
        "lr83yxwka7.skadnetwork",
        "tl55sbb4fm.skadnetwork",
        "mj797d8u6f.skadnetwork",
        "wg4vff78zm.skadnetwork",
        "3sh42y64q3.skadnetwork",
        "5a6flpkh64.skadnetwork",
        "s39g8k73mm.skadnetwork",
        "x44k69ngh6.skadnetwork",
        "2fnua5tdw4.skadnetwork",
        "av6w8kgt66.skadnetwork",
        "4468km3ulz.skadnetwork",
        "t38b2kh725.skadnetwork",
        "c6k4g5qg8m.skadnetwork",
        "9nlqeag3gk.skadnetwork",
        "3qy4746246.skadnetwork",
        "32z4fx6l9h.skadnetwork",
        "p78axxw29g.skadnetwork",
        "g6gcrrvk4p.skadnetwork",
        "488r3q3dtq.skadnetwork",
        "4fzdc2evr5.skadnetwork",
        "w9q455wk68.skadnetwork",
        "97r2b46745.skadnetwork",
        "4w7y6s5ca2.skadnetwork",
        "hs6bdukanm.skadnetwork",
        "f38h382jlk.skadnetwork",
        "yclnxrl5pm.skadnetwork",
        "k674qkevps.skadnetwork",
        "8s468mfl3y.skadnetwork",
        "v79kvwwj4g.skadnetwork",
        "5lm9lj6jb7.skadnetwork",
        "f7s53z58qe.skadnetwork",
        "pwa73g5rt2.skadnetwork",
        "prcb7njmu6.skadnetwork",
        "kbd757ywx3.skadnetwork",
        "m8dbw4sv7c.skadnetwork",
        "wzmmz9fp6w.skadnetwork",
        "n9x2a789qt.skadnetwork",
        "578prtvx9j.skadnetwork",
        "9rd848q2bz.skadnetwork",
        "a2p9lx4jpn.skadnetwork",
        "22mmun2rn5.skadnetwork",
        "4pfyvq9l8r.skadnetwork",
        "a8cz6cu7e5.skadnetwork",
        "5f5u5tfb26.skadnetwork",
        "xga6mpmplv.skadnetwork",
        "5tjdwbrq8w.skadnetwork",
        "4dzt52r2t5.skadnetwork",
        "294l99pt4k.skadnetwork",
        "f73kdq92p3.skadnetwork"
    };

    [PostProcessBuild(999)]
    public static void OnPostProcessBuild(BuildTarget target, string buildPath)
    {
        if (target != BuildTarget.iOS) return;

        string plistPath = Path.Combine(buildPath, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        var root = plist.root;

        PlistElementArray skAdArray;
        if (root.values.ContainsKey("SKAdNetworkItems"))
            skAdArray = root["SKAdNetworkItems"].AsArray();
        else
            skAdArray = root.CreateArray("SKAdNetworkItems");

        var existingIds = new HashSet<string>();
        foreach (var element in skAdArray.values)
        {
            if (element is PlistElementDict dict &&
                dict.values.TryGetValue("SKAdNetworkIdentifier", out var idElement))
            {
                existingIds.Add(idElement.AsString());
            }
        }

        foreach (var id in SkAdNetworkIds)
        {
            if (existingIds.Contains(id)) continue;

            var dict = skAdArray.AddDict();
            dict.SetString("SKAdNetworkIdentifier", id);
        }

        plist.WriteToFile(plistPath);
        UnityEngine.Debug.Log($"[iOS] SKAdNetwork IDs updated. Total target IDs: {SkAdNetworkIds.Count}");
    }
}
#endif
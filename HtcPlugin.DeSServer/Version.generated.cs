using System;
using System.Collections.Generic;
using System.Text;

namespace HtcPlugin.DeSServer {
    public static class Version {

        public static readonly int Major = 1;
        public static readonly int Minor = 0;
        public static readonly int Patch = 0;
        public static readonly int Build = 17;

        public static string GetVersion() { return $"{Major}.{Minor}.{Patch} Build {Build}"; }

    }
}

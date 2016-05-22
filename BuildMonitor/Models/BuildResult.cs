using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;

namespace BuildMonitor.Models {
    public enum BuildState {
        Unbuild,
        Building,
        Success,
        Failure,

        Unused,
        MaxCount = Unused,
    }

    public static class BuildStateExtension {
        public static string Display(this BuildState self) {
            switch (self) {
                case BuildState.Unbuild:
                    return "ビルド待ちです";
                case BuildState.Building:
                    return "ビルド中です";
                case BuildState.Success:
                    return "ビルドに成功しました";
                case BuildState.Failure:
                    return "ビルドに失敗しました";
            }
            throw new ArgumentOutOfRangeException("self", "想定しないビルド結果");
        }
    }
}

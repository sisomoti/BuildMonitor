using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;
using System.Diagnostics;

namespace BuildMonitor.Models {

    /// <summary>
    /// C++ の std::pair 相当
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    public class Pair<T, U> {
        private Pair() { }
        public Pair(T first, U second) {
            this.First = first;
            this.Second = second;
        }

        public T First { get; set; }
        public U Second { get; set; }
    }

    /// <summary>
    /// C++ の Rangeアルゴリズム 相当.
    ///   null許容型 を利用する場合を想定して、型制約は設定していない.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Range<T> /* where T : IComparable */ {
        private Range() { }

        /// <summary>
        /// begin以上 last以下 の範囲を表す
        /// </summary>
        /// <param name="begin">始点</param>
        /// <param name="last">終点</param>
        public Range(T begin, T last) {
            _range = new Pair<T, T>(begin, last);
        }

        /// <summary>
        /// 範囲に含まれる数列を返す
        /// </summary>
        /// <returns>数列</returns>
        public IEnumerable<int> GetSequence() {
            int begin = 0, last = 0;
            int.TryParse(_range.First.ToString(), out begin);
            int.TryParse(_range.Second.ToString(), out last);

            return Enumerable.Range(begin, last);
        }

        public T Begin
        {
            get { return _range.First;  }
            set { _range.First = value; }
        }

        public T Last
        {
            get { return _range.Second;  }
            set { _range.Second = value; }
        }

        private Pair<T, T> _range;
    }

    public class RevisionList : NotificationObject {
        /*
         * NotificationObjectはプロパティ変更通知の仕組みを実装したオブジェクトです。
         */
        private List<Revision> list { get; set; }

        private Range<uint?> BuildingRange { get; set; }
        private Range<uint?> SuccessRange  { get; set; }
        private Range<uint?> FailureRange  { get; set; }
        private Range<uint?> PendingRange  { get; set; }

        /// <summary>
        /// Range生成ヘルパー
        /// </summary>
        /// <param name="begin">begin 以上</param>
        /// <param name="last">last 以下</param>
        /// <returns>Range<uint?)のインスタンス</returns>
        public static Range<uint?> MakeRange(uint? begin, uint? last) {
            if (begin.HasValue && last.HasValue) {
                Debug.Assert(begin <= last);
            }
            return new Range<uint?>(begin, last);
        }

        public RevisionList() {
            CreateTestData();
        }

        // TODO: コルーチンにする
        private void CreateTestData() {
            List<Revision> testData = new List<Revision>() {
                new Revision(1, "このコミットは成功しています"),
                new Revision(2, "機能Aを実装" + Environment.NewLine + "* ビルドエラーがあるはず"),
                new Revision(3, "機能Bを実装" + Environment.NewLine + "* テスト通過済み"),
                new Revision(4, "機能Aのビルドエラーを修正しました"),
                new Revision(5, "機能Cの改良" + Environment.NewLine + "* テスト通過済み"),
                new Revision(6, "hoge" + Environment.NewLine + "fuga" + Environment.NewLine + "piyo"),
                new Revision(7, "ほげほげふがふがほげほげふがふがほげほげふがふが長いコミットメッセージほげほげふがふがほげほげふがふがほげほげふがふが"),
            };
            list = testData;

            BuildingRange = MakeRange(null, null);
            SuccessRange  = MakeRange(null, null);
            FailureRange  = MakeRange(null, null);
            PendingRange  = MakeRange(null, null);

            /////////////////////////////////////////
            // ◆1回目
            //  - 1回目のビルド(until rev.1)は 成功

            // [ビルド待ち] 1
            RecieveCommitNotification(1);
            Debug.Assert(PendingRange.GetSequence().SequenceEqual(Enumerable.Range(1, 1)));
            Debug.Assert(BuildingRange.GetSequence().SequenceEqual(Enumerable.Range(0, 0)));
            Debug.Assert(SuccessRange.GetSequence().SequenceEqual(Enumerable.Range(0, 0)));
            Debug.Assert(FailureRange.GetSequence().SequenceEqual(Enumerable.Range(0, 0)));

            // [ビルド中] 1
            RecieveNewBuildNotification(1);
            Debug.Assert(PendingRange.GetSequence().SequenceEqual(Enumerable.Range(0, 0)));
            Debug.Assert(BuildingRange.GetSequence().SequenceEqual(Enumerable.Range(1, 1)));
            Debug.Assert(SuccessRange.GetSequence().SequenceEqual(Enumerable.Range(0, 0)));
            Debug.Assert(FailureRange.GetSequence().SequenceEqual(Enumerable.Range(0, 0)));

            // [成功] 1
            RecieveBuildResult(1, BuildState.Success);
            Debug.Assert(PendingRange.GetSequence().SequenceEqual(Enumerable.Range(0, 0)));
            Debug.Assert(BuildingRange.GetSequence().SequenceEqual(Enumerable.Range(0, 0)));
            Debug.Assert(SuccessRange.GetSequence().SequenceEqual(Enumerable.Range(1, 1)));
            Debug.Assert(FailureRange.GetSequence().SequenceEqual(Enumerable.Range(0, 0)));

            /////////////////////////////////////////
            // ◆2回目
            //  - 2回目のビルド(until rev.3)は エラー

            // [成功] 1 / [ビルド待ち] 2,3
            RecieveCommitNotification(2);
            RecieveCommitNotification(3);
            Debug.Assert(PendingRange.GetSequence().SequenceEqual(Enumerable.Range(2, 3)));
            Debug.Assert(BuildingRange.GetSequence().SequenceEqual(Enumerable.Range(0, 0)));
            Debug.Assert(SuccessRange.GetSequence().SequenceEqual(Enumerable.Range(1, 1)));
            Debug.Assert(FailureRange.GetSequence().SequenceEqual(Enumerable.Range(0, 0)));

            // [成功] 1 / [ビルド中] 2,3
            RecieveNewBuildNotification(3);
            Debug.Assert(PendingRange.GetSequence().SequenceEqual(Enumerable.Range(0, 0)));
            Debug.Assert(BuildingRange.GetSequence().SequenceEqual(Enumerable.Range(2, 3)));
            Debug.Assert(SuccessRange.GetSequence().SequenceEqual(Enumerable.Range(1, 1)));
            Debug.Assert(FailureRange.GetSequence().SequenceEqual(Enumerable.Range(0, 0)));

            // [成功] 1 / [失敗] 2,3
            RecieveBuildResult(3, BuildState.Failure);
            Debug.Assert(PendingRange.GetSequence().SequenceEqual(Enumerable.Range(0, 0)));
            Debug.Assert(BuildingRange.GetSequence().SequenceEqual(Enumerable.Range(0, 0)));
            Debug.Assert(SuccessRange.GetSequence().SequenceEqual(Enumerable.Range(1, 1)));
            Debug.Assert(FailureRange.GetSequence().SequenceEqual(Enumerable.Range(2, 3)));

            /////////////////////////////////////////
            // ◆3回目
            //  - 3回目のビルド(until rev.5)で エラー修正
            //  - 3回目のビルド中に、rev.6 + rev.7 が コミットされた

            // [成功] 1 / [失敗] 2,3 / [ビルド待ち] 4,5
            RecieveCommitNotification(4);
            RecieveCommitNotification(5);
            Debug.Assert(PendingRange.GetSequence().SequenceEqual(Enumerable.Range(4, 5)));
            Debug.Assert(BuildingRange.GetSequence().SequenceEqual(Enumerable.Range(0, 0)));
            Debug.Assert(SuccessRange.GetSequence().SequenceEqual(Enumerable.Range(1, 1)));
            Debug.Assert(FailureRange.GetSequence().SequenceEqual(Enumerable.Range(2, 3)));

            // [成功] 1 / [失敗] 2,3 / [ビルド中] 4,5
            RecieveNewBuildNotification(5);
            Debug.Assert(PendingRange.GetSequence().SequenceEqual(Enumerable.Range(0, 0)));
            Debug.Assert(BuildingRange.GetSequence().SequenceEqual(Enumerable.Range(4, 5)));
            Debug.Assert(SuccessRange.GetSequence().SequenceEqual(Enumerable.Range(1, 1)));
            Debug.Assert(FailureRange.GetSequence().SequenceEqual(Enumerable.Range(2, 3)));

            // [成功] 1 / [失敗] 2,3 / [ビルド中] 4,5 / [ビルド待ち] 6,7
            RecieveCommitNotification(6);
            RecieveCommitNotification(7);
            Debug.Assert(PendingRange.GetSequence().SequenceEqual(Enumerable.Range(6, 7)));
            Debug.Assert(BuildingRange.GetSequence().SequenceEqual(Enumerable.Range(4, 5)));
            Debug.Assert(SuccessRange.GetSequence().SequenceEqual(Enumerable.Range(1, 1)));
            Debug.Assert(FailureRange.GetSequence().SequenceEqual(Enumerable.Range(2, 3)));

#if false
            // [成功] 1,(2),(3),4,5 / [ビルド待ち] 6,7
            RecieveBuildResult(5, BuildState.Success);
            Debug.Assert(PendingRange.GetSequence().SequenceEqual(Enumerable.Range(6, 7)));
            Debug.Assert(BuildingRange.GetSequence().SequenceEqual(Enumerable.Range(0, 0)));
            Debug.Assert(SuccessRange.GetSequence().SequenceEqual(Enumerable.Range(1, 5)));
            Debug.Assert(FailureRange.GetSequence().SequenceEqual(Enumerable.Range(0, 0)));
#endif
        }

#region イベントの受信
        public void RecieveCommitNotification(uint commitRevisionNumber) {
            // ビルド待ちが無かった
            if (!PendingRange.Begin.HasValue) {
                PendingRange.Begin = commitRevisionNumber;
            }

            // 終点を最新のコミットに追従
            // FIXME: 数字だけ保管する方式だと、区間中の全コミット内容を新規に計算することになるかも...
            PendingRange.Last = commitRevisionNumber;
        }

        public void RecieveNewBuildNotification(uint buildRevisionNumber) {
            // ビルド待ちが無い状態で、ビルドが行われた
            if (!PendingRange.Begin.HasValue || !PendingRange.Last.HasValue) {
                throw new NotImplementedException("TODO: コミットを検知せずに、ビルドが走った(再実行？)");
            }

            // ビルド中に、ビルドが行われた
            if (BuildingRange.Begin.HasValue || BuildingRange.Begin.HasValue) {
                throw new NotImplementedException("TODO: ビルド中に、別なビルドが走った(別ノード？)");
            }

            // ビルド待ちより古い
            if (buildRevisionNumber < PendingRange.Begin.Value) {
                throw new NotImplementedException("TODO: 古いRevisionで、ビルドが走った(再実行？)");
            }

            // ビルド待ちの一部のみビルド
            if (PendingRange.Begin.Value <= buildRevisionNumber && buildRevisionNumber < PendingRange.Last.Value) {
                BuildingRange = MakeRange(PendingRange.Begin.Value, buildRevisionNumber);

                // FIXME: ビルドされた番号の「次の番号」を計算
                PendingRange.Begin = buildRevisionNumber + 1;
                return;
            }

            // ビルド待ちより新しい
            if (PendingRange.Last.Value <= buildRevisionNumber) {
                BuildingRange = MakeRange(PendingRange.Begin.Value, buildRevisionNumber);
                PendingRange  = MakeRange(null, null);
                return;
            }

            throw new NotImplementedException("未定義のケース");
        }

        public void RecieveBuildResult(uint buildRevisionNumber, BuildState state) {
            // ビルドが無い状態で、ビルドが行われた
            if (!BuildingRange.Begin.HasValue || !BuildingRange.Last.HasValue) {
                throw new NotImplementedException("TODO: ビルドを検知せずに、ビルド結果を受け取った(障害？)");
            }

            // ビルド番号が異なる
            if (BuildingRange.Last.Value != buildRevisionNumber) {
                throw new NotImplementedException("TODO: 動いていたビルドと、受け取った結果の番号が異なる(障害？)");
            }

            // 成功/失敗以外の結果が帰ってきた
            if (state != BuildState.Success && state != BuildState.Failure) {
                throw new NotImplementedException("TODO: 意図しないビルド結果で呼び出された(実装ミス？)");
            }

            // 成功
            if (state == BuildState.Success) {
                SuccessRange.Begin = (SuccessRange.Begin.HasValue) ?
                    Math.Min(SuccessRange.Begin.Value, buildRevisionNumber) :
                    Math.Min(BuildingRange.Begin.Value, buildRevisionNumber);

                SuccessRange.Last = (SuccessRange.Last.HasValue) ?
                    Math.Max(SuccessRange.Last.Value, buildRevisionNumber) :
                    Math.Max(BuildingRange.Last.Value, buildRevisionNumber);

                BuildingRange = MakeRange(null, null);

                // TODO: FIXEDの場合は、リストから除去
                FailureRange = MakeRange(null, null);
                return;
            }

            // 失敗
            if (state == BuildState.Failure) {
                FailureRange.Begin = (FailureRange.Begin.HasValue) ?
                    Math.Min(FailureRange.Begin.Value, buildRevisionNumber) :
                    Math.Min(BuildingRange.Begin.Value, buildRevisionNumber);

                FailureRange.Last = (FailureRange.Last.HasValue) ?
                    Math.Max(FailureRange.Last.Value, buildRevisionNumber) :
                    Math.Max(BuildingRange.Last.Value, buildRevisionNumber);

                BuildingRange = MakeRange(null, null);
                return;
            }

            throw new NotImplementedException("未定義のケース");
        }


        public void RecieveCommitNotification(Revision commitRevision) {
            RecieveCommitNotification(commitRevision.Number);
            throw new NotImplementedException("TODO: インスタンスの保存を行う");
        }

        public void RecieveNewBuildNotification(Revision buildRevision) {
            RecieveNewBuildNotification(buildRevision.Number);
            throw new NotImplementedException("TODO: インスタンスの保存を行う");
        }

        public void RecieveBuildResult(Revision buildRevision, BuildState state) {
            RecieveBuildResult(buildRevision.Number, state);
            throw new NotImplementedException("TODO: インスタンスの保存を行う");
        }
        #endregion


        #region コメント一覧の取得
        /// <summary>
        /// 全てのコメント一覧を取得.
        /// 成功して、一覧から削除されていないものも含む.
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllCommentList() {
            var result = from item in list
                         orderby item.Number descending
                         select item.ToString();

            return result.ToList() ?? new List<string>();
        }

        private IEnumerable<string> GetFilteredCommentList(Range<uint?> filterRange) {
            if (!filterRange.Begin.HasValue || !filterRange.Begin.HasValue) {
                return null;
            }

            return from item in list
                   orderby item.Number descending
                   where item.IsIn(filterRange.Begin.Value, filterRange.Last.Value)
                   select item.ToString();
        }

        public List<string> GetPendingCommentList() {
            return GetFilteredCommentList(PendingRange)?.ToList() ?? new List<string>();
        }

        public List<string> GetBuildingCommentList() {
            return GetFilteredCommentList(BuildingRange)?.ToList() ?? new List<string>();
        }

        public List<string> GetFailureCommentList() {
            return GetFilteredCommentList(FailureRange)?.ToList() ?? new List<string>();
        }

        public List<string> GetSuccessCommentList() {
            return GetFilteredCommentList(SuccessRange)?.ToList() ?? new List<string>();
        }
#endregion
    }
}

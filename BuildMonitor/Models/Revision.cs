using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;

namespace BuildMonitor.Models {
    public class Revision : NotificationObject {
        /*
         * NotificationObjectはプロパティ変更通知の仕組みを実装したオブジェクトです。
         */
        public uint Number { get; private set; }
        public string Comment { get; private set; }
        public string Author { get; private set; }
        public DateTime CommitTime { get; private set; }

        /// <summary>
        /// テスト用コンストラクタ
        /// </summary>
        /// <param name="revision"></param>
        /// <param name="comment"></param>
        public Revision(uint revision, string comment) {
            Number = revision;
            Comment  = comment;

            Author     = "";
            CommitTime = DateTime.Now;
        }

        public Revision(uint revision, string comment, string author, DateTime commitTime) {
            Number   = revision;
            Comment    = comment;
            Author     = author;
            CommitTime = commitTime;
        }

        // --------------------
        // Revisionと比較
        // --------------------
        public bool LaterThan(uint targetRevision) {
            return targetRevision < Number;
        }

        public bool OlderThan(uint targetRevision) {
            return Number < targetRevision;
        }

        public bool EqualTo(uint targetRevision) {
            return Number == targetRevision;
        }

        public bool IsIn(uint fromRevision, uint toRevision) {
            return 
                (LaterThan(fromRevision) || EqualTo(fromRevision)) &&
                (OlderThan(toRevision)   || EqualTo(toRevision)  );
        }

        // --------------------
        // objectと比較
        // --------------------
        public bool LaterThan(Revision target) {
            return LaterThan(target.Number);
        }

        public bool OlderThan(Revision target) {
            return OlderThan(target.Number);
        }

        public bool EqualTo(Revision target) {
            return Equals(target.Number);
        }

        // override object.ToString
        public override string ToString() {
            return string.Format("[{0}]\r\n{1}", Number.ToString(), Comment);
        }

        // override object.Equals
        public override bool Equals(object obj) {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }

            return EqualTo(obj as Revision);
        }

        // override object.GetHashCode
        public override int GetHashCode() {
            return (int)Number;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;

namespace BuildMonitor.Models
{
    public class ExampleModel : NotificationObject
    {
        /*
         * NotificationObjectはプロパティ変更通知の仕組みを実装したオブジェクトです。
         */

        public string Text { get; set; }

        public ExampleModel(string value) {
            Text = value;
        }
    }
}

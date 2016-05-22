using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using BuildMonitor.Models;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace BuildMonitor.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        /* コマンド、プロパティの定義にはそれぞれ 
         * 
         *  lvcom   : ViewModelCommand
         *  lvcomn  : ViewModelCommand(CanExecute無)
         *  llcom   : ListenerCommand(パラメータ有のコマンド)
         *  llcomn  : ListenerCommand(パラメータ有のコマンド・CanExecute無)
         *  lprop   : 変更通知プロパティ(.NET4.5ではlpropn)
         *  
         * を使用してください。
         * 
         * Modelが十分にリッチであるならコマンドにこだわる必要はありません。
         * View側のコードビハインドを使用しないMVVMパターンの実装を行う場合でも、ViewModelにメソッドを定義し、
         * LivetCallMethodActionなどから直接メソッドを呼び出してください。
         * 
         * ViewModelのコマンドを呼び出せるLivetのすべてのビヘイビア・トリガー・アクションは
         * 同様に直接ViewModelのメソッドを呼び出し可能です。
         */

        /* ViewModelからViewを操作したい場合は、View側のコードビハインド無で処理を行いたい場合は
         * Messengerプロパティからメッセージ(各種InteractionMessage)を発信する事を検討してください。
         */

        /* Modelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedEventListenerや
         * CollectionChangedEventListenerを使うと便利です。各種ListenerはViewModelに定義されている
         * CompositeDisposableプロパティ(LivetCompositeDisposable型)に格納しておく事でイベント解放を容易に行えます。
         * 
         * ReactiveExtensionsなどを併用する場合は、ReactiveExtensionsのCompositeDisposableを
         * ViewModelのCompositeDisposableプロパティに格納しておくのを推奨します。
         * 
         * LivetのWindowテンプレートではViewのウィンドウが閉じる際にDataContextDisposeActionが動作するようになっており、
         * ViewModelのDisposeが呼ばれCompositeDisposableプロパティに格納されたすべてのIDisposable型のインスタンスが解放されます。
         * 
         * ViewModelを使いまわしたい時などは、ViewからDataContextDisposeActionを取り除くか、発動のタイミングをずらす事で対応可能です。
         */

        /* UIDispatcherを操作する場合は、DispatcherHelperのメソッドを操作してください。
         * UIDispatcher自体はApp.xaml.csでインスタンスを確保してあります。
         * 
         * LivetのViewModelではプロパティ変更通知(RaisePropertyChanged)やDispatcherCollectionを使ったコレクション変更通知は
         * 自動的にUIDispatcher上での通知に変換されます。変更通知に際してUIDispatcherを操作する必要はありません。
         */

        public void Initialize() {
            // modelから情報の取得
            model = new ExampleModel("Hello, world."); ;
            ExampleMessage = model.Text;

            allCommits = new List<string>();
            allCommits.Add("バグを修正しました");
            allCommits.Add("機能Aを実装");
            allCommits.Add("hoge" + Environment.NewLine + "fuga" + Environment.NewLine + "piyo");
            allCommits.Add("ほげほげふがふがほげほげふがふがほげほげふがふが長いコミットメッセージほげほげふがふがほげほげふがふがほげほげふがふが");

            // ListBox
            PendingCommits  = new ListCollectionView(allCommits);
            BuildingCommits = new ListCollectionView(allCommits);
            FailedCommits   = new ListCollectionView(allCommits);

            PendingCommits.MoveCurrentToPosition(-1);
            BuildingCommits.MoveCurrentToPosition(-1);
            FailedCommits.MoveCurrentToPosition(-1);
        }


        // TextBox
        private ExampleModel model = null;
        public string ExampleMessage {
            get {
                return model?.Text;
            }
            set {
                model.Text = value;
                RaisePropertyChanged("ExampleMessage");
            }

        }

        // ListBox
        private List<string> allCommits = null;

        #region PendingCommits変更通知プロパティ
        private ListCollectionView _PendingCommits;

        public ListCollectionView PendingCommits
        {
            get { return _PendingCommits; }
            set {
                if (_PendingCommits == value) {
                    return;
                }
                _PendingCommits = value;
                RaisePropertyChanged("PendingCommits");
            }
        }
        #endregion

        #region BuildingCommits変更通知プロパティ
        private ListCollectionView _BuildingCommits;

        public ListCollectionView BuildingCommits
        {
            get { return _BuildingCommits; }
            set {
                if (_BuildingCommits == value) {
                    return;
                }
                _BuildingCommits = value;
                RaisePropertyChanged("BuildingCommits");
            }
        }
        #endregion

        #region FailedCommits変更通知プロパティ
        private ListCollectionView _FailedCommits;

        public ListCollectionView FailedCommits
        {
            get { return _FailedCommits; }
            set {
                if (_FailedCommits == value) {
                    return;
                }
                _FailedCommits = value;
                RaisePropertyChanged("FailedCommits");
            }
        }
        #endregion
    }
}

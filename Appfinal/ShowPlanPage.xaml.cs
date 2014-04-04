using Appfinal.Common;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “基本页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234237 上有介绍

namespace Appfinal
{
    /// <summary>
    /// 基本页，提供大多数应用程序通用的特性。
    /// </summary>
         public class planDB
        {
            [SQLite.AutoIncrement, SQLite.PrimaryKey]
            public int priKey { set; get; }
            public int ID { set; get; }
            public int DayNumber { set; get; }
            public String Date { set; get; }
            public string Location { set; get; }
            public string cName { set; get; }
            public string Command { set; get; }
            public bool IsAlerted { set; get; }
        }

    public sealed partial class ShowPlanPage : Page
    {
        public int num;
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        public List<String> PlanNumber;
        public List<String> DaysSpots;
        List<planDB> list2;
        int maxId;
        int select;
        /// <summary>
        /// 可将其更改为强类型视图模型。
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper 在每页上用于协助导航和
        /// 进程生命期管理
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public ShowPlanPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            PlanNumber = new List<string>();
            DaysSpots = new List<string>();      
            Create();
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            ShowPlanNumbers();
        }
        string path = "";

        private void Create()
        {
            path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Member.sqlite");    //数据文件保存的位置  
            //path = Path.Combine(Database,"Member.sqlite");
            using (var db = new SQLite.SQLiteConnection(path))  //打开创建数据库和表
            {
                db.CreateTable<planDB>();
            }
        }

        /// <summary>
        ///使用在导航过程中传递的内容填充页。 在从以前的会话
        /// 重新创建页时，也会提供任何已保存状态。
        /// </summary>
        /// <param name="sender">
        /// 事件的来源; 通常为 <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">事件数据，其中既提供在最初请求此页时传递给
        /// <see cref="Frame.Navigate(Type, Object)"/> 的导航参数，又提供
        /// 此页在以前会话期间保留的状态的
        /// 的字典。 首次访问页面时，该状态将为 null。</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// 保留与此页关联的状态，以防挂起应用程序或
        /// 从导航缓存中放弃此页。  值必须符合
        /// <see cref="SuspensionManager.SessionState"/> 的序列化要求。
        /// </summary>
        ///<param name="sender">事件的来源；通常为 <see cref="NavigationHelper"/></param>
        ///<param name="e">提供要使用可序列化状态填充的空字典
        ///的事件数据。</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper 注册

        /// 此部分中提供的方法只是用于使
        /// NavigationHelper 可响应页面的导航方法。
        /// 
        /// 应将页面特有的逻辑放入用于
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// 和 <see cref="GridCS.Common.NavigationHelper.SaveState"/> 的事件处理程序中。
        /// 除了在会话期间保留的页面状态之外
        /// LoadState 方法中还提供导航参数。

        public async void ShowPlanNumbers()
        {
            SQLiteConnection db = new SQLiteConnection(Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Member.sqlite"));
            List<planDB> list2 = db.Query<planDB>("select ID from planDB order by ID desc;");
            if(list2.Count > 0)
            {
                maxId = list2[0].ID;
                num = maxId;
                PlanNumber.Clear();
                for (int i = list2[list2.Count-1].ID; i <= maxId; i++)
                {
                    PlanNumber.Add("第" + i + "个计划");
                }
                var ress =
                   (from x in PlanNumber
                    group x by x.ToString() into g
                    select new
                    {
                        Plan = g.ToArray()
                    }
                   ).ToList();
                PlanLeft.Source = ress.ToList();


            }
            else
            {
                num = 0;
                PlanNumber.Clear();
                PlanNumber.Add("您还没有计划，请创建");
                var ress =
                   (from x in PlanNumber
                    group x by x.ToString() into g
                    select new
                    {
                        Plan = g.ToArray()
                    }
                   ).ToList();
                PlanLeft.Source = ress.ToList();

            }
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            num += 1;
            this.Frame.Navigate(typeof(PlanPage),num);
        }
        public async void change()
        {
            DaysSpots.Clear();
            SQLiteAsyncConnection db = new SQLiteAsyncConnection(Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Member.sqlite"));
            select = itemLeft.SelectedIndex + 1;
            var query = db.Table<planDB>().Where(x => x.ID == select).OrderBy(c => c.DayNumber);
            var result = await query.ToListAsync();
            if (result != null)
            {
                foreach (var item in result)
                {
                    // await new Windows.UI.Popups.MessageDialog(item.ID + "," + item.DayNumber + "," + item.Date + "," + item.Location).ShowAsync();
                    DaysSpots.Add("第" + item.DayNumber + "天" + " " + item.Location);
                }
                var res =
                    (from x in DaysSpots
                     group x by (x.Split(' '))[0].ToString() into g
                     select new
                     {
                         DayNumbers = g.Key,
                         Detail =
                         (from y in g.ToArray()
                          select y.Split(' ')[1]
                          )

                     }).ToList();
                DetailRight.Source = res.ToList();
            }
        }
        private async void itemLeft_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (itemLeft.SelectedItem != null)
            {
                change();
            }          
        }

        private void itemRight_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {        
            if(itemRight.SelectedIndex > 0)
                this.Frame.Navigate(typeof(BeforePage), itemRight.SelectedItem.ToString());
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            list2 = new List<planDB>();
            /*
            SQLiteConnection db = new SQLiteConnection(Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Member.sqlite"));
            list2 = db.Query<planDB>("select * from planDB where ID=select");
            foreach(planDB p in list2)
            {
                db.Delete(p);
            }
             */
            SQLiteAsyncConnection db = new SQLiteAsyncConnection(Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Member.sqlite"));  
            var query = db.Table<planDB>().Where(x => x.ID == select);
            list2 = await query.ToListAsync();
            foreach (planDB p in list2)
            {
                await db.DeleteAsync(p);
            }
            var query2 = db.Table<planDB>().Where(x => x.ID > select);
            list2 = await query2.ToListAsync();
            foreach (planDB p in list2)
            {
                p.ID = p.ID - 1;
                await db.UpdateAsync(p);
            }
            ShowPlanNumbers();
            change();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ItemsPage));
        }  
    }
}

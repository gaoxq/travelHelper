using Appfinal.Common;
using Appfinal.Data;
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
using System.Collections.ObjectModel;
using SQLite;
using Windows.System.Threading;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Com.AMap.Maps.Api.Overlays;
using Com.AMap.Maps.Api.BaseTypes;
using Com.AMap.Maps.Api.Layers;
using Com.AMap.Search.API.Result;
using Com.AMap.Search.API.Options;
using Com.AMap.Search.API;
using Windows.UI.Core;
using Windows.UI.Popups;
using System.Diagnostics;
using Com.AMap.Maps.Api;

// “基本页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234237 上有介绍

namespace Appfinal
{
    /// <summary>
    /// 基本页，提供大多数应用程序通用的特性。
    /// </summary>
         
    public sealed partial class DataPage : Page
    {
        public int dayNumber;
        public DateTime date;
        public String[] spots;
        public String[] arr;
        private HttpClient httpClient;
        public int dayValue;
        string path = "";
        public List<String> selectSpots;
        public int num;
        List<String> StationSpots = new List<string>();
        List<planDB> list2;
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        public String city;
        
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

        public DataPage()
        {
            this.InitializeComponent();
            AMapConfig.Key = "28108e749d75638422ff695f2412228e";
            AMapSearchConfig.Key = "28108e749d75638422ff695f2412228e";
            httpClient = new HttpClient();
            var headers = httpClient.DefaultRequestHeaders;
            path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Member.sqlite");    //数据文件保存的位置  
            selectSpots = new List<string>();
            
            ThreadPool.RunAsync(
                  (timer) =>
                  {
                      getCityInfo();
                  },
                  WorkItemPriority.Low, WorkItemOptions.None);
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
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
            //itemListViewCenter.Items(0).Selected = false;
            DateTime datetmp;
            
            arr = new String[dayNumber];
            for (int i = 1; i <= dayNumber; i++)
            {
                datetmp = date.AddDays(i - 1);
                arr[i - 1] = "Day" + i + "(" + datetmp.Year.ToString() + "年" + datetmp.Month.ToString() + "月" + datetmp.Day.ToString() + "日)";
            }


            var res =
                (from x in arr
                 group x by (x.Split('('))[0].ToString() into g
                 select new
                 {
                     HeadNames = g.Key,
                     DayNames = 
                    (
                        from y in g.ToArray()
                        select y.Split('(')[1].Split(')')[0]

                    )
                 }).ToList();
            InfoListLeft.Source = res.ToList();
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

        
        public void Insert(planDB data)
        {

            try
            {
                using (var db = new SQLiteConnection(path))
                {
                    db.Insert(data);
                }

            }

            catch (Exception e)
            {

                throw e;

            }

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
        /// 

        private async void getCityInfo()
        {
            //try
            //{
                Uri uu = new Uri("http://www.cncn.com/city/#guonei");
                HttpResponseMessage hres = new HttpResponseMessage();
                if (NetworkHelper.IsConnectedToInternet())
                {
                    hres = await httpClient.GetAsync(uu);
                    hres.EnsureSuccessStatusCode();
                    String response = await hres.Content.ReadAsStringAsync();
                    response = response.Replace("<br>", Environment.NewLine);

                    Regex regx = new Regex("(<a[\\s]href=\"[^>]*\"[\\s]target=\"_blank\"[^>]*>([^>]*>|)" + city + "([\\s\\S]*?)</a>)|(<a[\\s]href=\"[^>]*\"[\\s]target=\"_blank\"[^>]*>([^>]*>|)" + city + "旅游([\\s\\S]*?)</a>)", RegexOptions.IgnoreCase);

                    Match match = regx.Match(response);
                    string str = match.Groups[0].Value;
                    Regex regx2 = new Regex("href=\"[^>]*?\"");
                    Match match2 = regx2.Match(str);
                    string url = match2.Groups[0].Value.Replace("href=", "").Replace("\"", "") + "/jingdian/index1.html";

                    Uri uri = new Uri(url);
                    HttpResponseMessage res = await httpClient.GetAsync(uri);
                    // res.EnsureSuccessStatusCode();
                    String cityInfo = await res.Content.ReadAsStringAsync();
                    cityInfo = cityInfo.Replace("<br>", Environment.NewLine);
                    Regex rg1 = new Regex("<span[\\s]class=\"title\">[^<]*</span>");
                    MatchCollection m = rg1.Matches(cityInfo);
                    string s = ("");
                    Regex reg = new Regex("<span[\\s]class=\"title\">");
                    int k = 1;
                    spots = new String[m.Count + 1];
                    spots[0] = "选择景点名称";
                    foreach (Match mm in m)
                    {
                        s = mm.Value.ToString().Replace("</span>", "");
                        s = reg.Replace(s, "");
                        spots[k++] = s;
                    }
                }
                // changeSpots();

                else
                {
                    MessageDialog msd = new MessageDialog("网络不给力");
                    this.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                    {
                        msd.ShowAsync();
                    });
                }
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Appfinal.PlanPage.Para para = e.Parameter as Appfinal.PlanPage.Para;
            if (para != null)
            {
                dayNumber = para.DayNumber;
                date = para.Date;
                city = para.City;
                num = int.Parse(para.Num);
            } 
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }
        public void changeSpots()
        {
            if (spots != null)
            {
                var res =
                        (from x in spots
                         group x by x.ToString() into g
                         select new
                         {
                             CityNames = g.ToArray()
                         }
                        ).ToList();
                InfoListCenter.Source = res.ToList();
            }
        }
        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if(this.UsingLogicalPageNavigation()) this.InvalidateVisualState();
            dayValue = itemListViewLeft.SelectedIndex;        
            SQLiteAsyncConnection db = new SQLiteAsyncConnection(Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Member.sqlite"));
            int dayValuePlus = dayValue + 1;
            var query = db.Table<planDB>().Where(x => (x.DayNumber == dayValuePlus)&&(x.ID == num));
            if (query != null)
            {
                var result = await query.ToListAsync();
                selectSpots.Clear();
                foreach (var item in result)
                {
                    // await new Windows.UI.Popups.MessageDialog(item.ID + "," + item.DayNumber + "," + item.Date + "," + item.Location).ShowAsync();
                    selectSpots.Add(item.Location.ToString());
                }
                var ress =
                       (from x in selectSpots
                        group x by x.ToString() into g
                        select new
                        {
                            SelectNames = g.ToArray()
                        }
                       ).ToList();

                InfoListRight.Source = ress.ToList();  
            }
                      
            changeSpots();                
    
            
        }
        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            
        }
        public async void change()
        {
            SQLiteAsyncConnection db = new SQLiteAsyncConnection(Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Member.sqlite"));
            int dayValuePlus = dayValue + 1;
            var query = db.Table<planDB>().Where(x => (x.DayNumber == dayValuePlus) && (x.ID == num));

            var result = await query.ToListAsync();
            selectSpots.Clear();
            foreach (var item in result)
            {
                // await new Windows.UI.Popups.MessageDialog(item.ID + "," + item.DayNumber + "," + item.Date + "," + item.Location).ShowAsync();
                selectSpots.Add(item.Location.ToString());
            }

            var ress =
               (from x in selectSpots
                group x by x.ToString() into g
                select new
                {
                    SelectNames = g.ToArray()
                }
               ).ToList();
            InfoListRight.Source = ress.ToList();
        }
        private async void itemListViewCenter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (itemListViewCenter.SelectedItem != null)
            {
                if (itemListViewCenter.SelectedIndex >= 1)
                {               
                    planDB a = new planDB();
                    a.ID = num;
                    a.DayNumber = dayValue + 1;
                    a.Date = itemListViewLeft.SelectedItem.ToString();
                    a.Location = itemListViewCenter.SelectedItem.ToString();
                    a.cName = city;
                    a.Command = "null";
                    a.IsAlerted = false;
                    Insert(a);
                    change();
                     //数据库获取当天的地点，显示在后边listview
                    

                }
            }


        }

        
        private void itemListViewRight_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if (itemListViewRight.SelectedItem != null)
            {
                String stationName = itemListViewRight.SelectedItem.ToString();
                ThreadPool.RunAsync(
                         (timer) =>
                         {
                             BusLineSearchTest(stationName, city);
                         },
                         WorkItemPriority.Low, WorkItemOptions.None);
            }
                
            //删除day = dayValue && position = itemListViewRight.SelectedItem.ToString()中数据库的值
            //刷新该页面

        }
        private async void BusLineSearchTest(string stationName, string cityCode)
        {
            BusLineSearchOption rgo = new BusLineSearchOption();
            rgo.StationName = stationName;
            rgo.CityCode = cityCode;
            //     服务编码 默认8085-根据线路ID查询Ids不能为空 8004-根据线路名称查询 8086-根据站点名称查询
            rgo.Sid = "8086";

            BusLineSearchResult rgcs = await BusLineSearch.BusLineSearchWithOption(rgo);
            this.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {

                //    ObservableCollection<ALngLat> lnglatRoute = new ObservableCollection<ALngLat>();   //线路坐标
                //    IEnumerable<String> lnglatstring;

                if (rgcs.Erro == null)
                {
                    //SearchTextGrid.Visibility = Visibility.Collapsed;
                    IEnumerable<BusLine> bs = rgcs.BusLineList;
                    PoiListView.DataContext = bs;
                    // PoiListView.Visibility = Visible;
                    StationSpots.Clear();
                    foreach(BusLine b in bs)
                    {
                        if (b.StartTime != "")
                            StationSpots.Add(b.Name+" "+b.StartTime.Substring(0,2)+":"+b.StartTime.Substring(2,2));
                        else
                            StationSpots.Add(b.Name);
                        
                    }

                    var ress =
                       (from x in StationSpots
                        group x by x.ToString() into g
                        select new
                        {
                            StationNames = g.ToArray()
                        }
                       ).ToList();
                    
                        InfoListPoi.Source = ress.ToList();


                }
                else
                {
                    StationSpots.Clear();
                    StationSpots.Add("对不起，您查询的站点不存在");
                    var ress =
                       (from x in StationSpots
                        group x by x.ToString() into g
                        select new
                        {
                            StationNames = g.ToArray()
                        }
                       ).ToList();
                    InfoListPoi.Source = ress.ToList();
                    
                    
                }
            });

        }

       

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ShowPlanPage));
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            list2 = new List<planDB>();
            SQLiteAsyncConnection db = new SQLiteAsyncConnection(Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Member.sqlite"));
            string deleteSpots = itemListViewRight.SelectedItem.ToString();
            int day = dayValue+1;
            var query = db.Table<planDB>().Where(x => (x.Location == deleteSpots) && (x.ID == num) && (x.DayNumber == day));
            list2 = await query.ToListAsync();
            foreach (planDB p in list2)
            {
                await db.DeleteAsync(p);
            }
            change();

        }

    }
}

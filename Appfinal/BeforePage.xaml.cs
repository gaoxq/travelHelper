using Appfinal.Common;
using Com.AMap.Maps.Api;
using Com.AMap.Maps.Api.BaseTypes;
using Com.AMap.Maps.Api.Overlays;
using Com.AMap.Search.API;
using Com.AMap.Search.API.Options;
using Com.AMap.Search.API.Result;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Core;
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
    public sealed partial class BeforePage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        public String location;
        public String city;
        List<String> StationSpots = new List<string>();


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


        public BeforePage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            AMapConfig.Key = "28108e749d75638422ff695f2412228e";
            AMapSearchConfig.Key = "28108e749d75638422ff695f2412228e";   
            ThreadPool.RunAsync(
                    (timer) =>
                    {
                        addressTolen();
                    },
                    WorkItemPriority.Low, WorkItemOptions.None);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }
        public async void BusLine()
        {
            SQLiteAsyncConnection db = new SQLiteAsyncConnection(Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Member.sqlite"));
            var query = db.Table<planDB>().Where(x => x.Location.Equals(location));
            var result = await query.ToListAsync();
            city = result[0].cName;
            ThreadPool.RunAsync(
                         (timer) =>
                         {
                             BusLineSearchTest(location, city);
                         },
                         WorkItemPriority.Low, WorkItemOptions.None);
        }
        public async void addressTolen()
        {
            //初始化一个地理编码参数类。
            GeoCodingOption rgo = new GeoCodingOption();
            rgo.Address = location;
            GeoCodingResult rgcs = await GeoCode.AddressToGeoCodeWithOption(rgo);
            this.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (rgcs.Erro == null)
                {
                    IEnumerable<GeoPOI> pois = rgcs.GeoCodingList;
                    int i = 0;
                    foreach (GeoPOI poi in pois)
                    {
                        i++;
                        ATip tip = new ATip();
                        tip.Title = poi.Name;
                        tip.ContentText = poi.Address;
                        AMarker marker = new AMarker();
                        marker.LngLat = new ALngLat(poi.X, poi.Y);
                        marker.TipFrameworkElement = tip;
                        map.Children.Add(marker);
                        marker.OpenTip();
                        map.SetZoomAndCenter(12, marker.LngLat);
                    }
                }
            });

            BusLine();
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
                    BusList.DataContext = bs;
                    // PoiListView.Visibility = Visible;
                    StationSpots.Clear();
                    foreach (BusLine b in bs)
                    {
                        StationSpots.Add(b.Name + " " + b.StartTime.Substring(0, 2) + ":" + b.StartTime.Substring(2, 2));
                    }

                    var ress =
                       (from x in StationSpots
                        group x by x.ToString() into g
                        select new
                        {
                            Station = g.ToArray()
                        }
                       ).ToList();

                    InfoBusLine.Source = ress.ToList();


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
                            Station = g.ToArray()
                        }
                       ).ToList();
                    InfoBusLine.Source = ress.ToList();


                }
            });

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
            pageTitle.Text = location + "详情";
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            location = e.Parameter.ToString();
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}

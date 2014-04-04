using Com.AMap.Maps.Api;
using Com.AMap.Search.API;
using Com.AMap.Search.API.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Markup;
using Com.AMap.Maps.Api.Overlays;
using Com.AMap.Maps.Api.BaseTypes;
using System.Collections.ObjectModel;
using Com.AMap.Search.API.Result;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Popups;
using Com.AMap.Maps.Api.Events;
using Appfinal.Common;
using Appfinal.Data;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace Appfinal
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        String CityName = ("");
        String EnglishName = ("");      
        public MainPage()
        {
            this.InitializeComponent();
            AMapConfig.Key = "28108e749d75638422ff695f2412228e";
            AMapSearchConfig.Key = "28108e749d75638422ff695f2412228e";
            AGeolocator ageol = new AGeolocator();
            ageol.PositionChanged += ageol_PositionChanged;
        }
        void ageol_PositionChanged(AGeolocator sender, APositionChangedEventArgs args)
        {
            ThreadPool.RunAsync(
                     (timer) =>
                     {
                         ReGeoCodeTest(args.LngLat.LngX, args.LngLat.LatY);
                     },
                     WorkItemPriority.Low, WorkItemOptions.None);
        }
        
        private async void ReGeoCodeTest(double lng, double lat)
        {
            ReverseGeocodingOption rgo = new ReverseGeocodingOption();
            rgo.XCoors = new double[] { lng };
            rgo.YCoors = new double[] { lat };

            ReverseGeoCodingResult rgcs = await ReGeoCode.GeoCodeToAddressWithOption(rgo);

            this.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (rgcs.Erro == null && rgcs.resultList != null)
                {
                    IEnumerable<ReverseGeocodingInfo> reverseGeocodeResult = rgcs.resultList;
                    foreach (ReverseGeocodingInfo poi in reverseGeocodeResult)
                    {

                        AMarker marker = new AMarker();//初始化一个点标注实例               
                        //marker.LngLat = args.LngLat; //点标注的经纬度为当前定位获取的经纬度
                        marker.LngLat = new ALngLat(lng, lat);
                        marker.IconURI = new Uri("http://api.amap.com/webapi/static/Images/marker_sprite.png ");

                        ATip tip = new ATip(); //初始化一个信息窗口实例  
                        tip.Title = poi.City.Name; //设置信息窗口的标题  
                        CityName = poi.City.Name;//城市名称
                        EnglishName = poi.City.EnName;
                        EnglishName = EnglishName.Replace("'","");
                        EnglishName = EnglishName.ToLower();
                        tip.ContentText = "我是内容"; //设置信息窗口的内容

                        marker.TipFrameworkElement = tip; //将信息窗口赋值给marker 
                        marker.OpenTip(); //打开信息窗口  
                        map.Children.Add(marker); //将点标注添加到地图上
                        map.SetZoomAndCenter(5, marker.LngLat);
                        map.TipClose += showPage;
                    }

                }
            });
        }
        public class Para
        {
            public string groupId { get; set; }
            public string cityName { get; set; }
            public string englishName { get; set; }
        }
       
        private void showPage(object sender, MapRoutedEventArgs e)
        {
          //  var rootFrame = new Frame();
            Para para = new Para()
            {
                groupId = "Group-1",
                cityName = CityName,
                englishName = EnglishName
            };
             
           /* Para para = new Para()
            {
                groupId = "Group-1",
                cityName = CityName
            };*/

            this.Frame.Navigate(typeof(SplitPage), para);
           //rootFrame.Navigate(typeof(SplitPage), para);
           // Window.Current.Content = rootFrame;
           //Window.Current.Activate();

        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ItemsPage));
        }
    }
}


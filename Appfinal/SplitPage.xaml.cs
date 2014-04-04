using Appfinal.Common;
using Appfinal.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using Windows.UI.Popups;
using Windows.UI.Core;
using System.Text.RegularExpressions;
using Windows.System.Threading;
using Windows.UI.Xaml.Media.Imaging;

// “拆分页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234234 上提供

namespace Appfinal
{
    public sealed partial class SplitPage : Page
    {
        String cityName;
        String groupId;
        String englishName;
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private HttpClient httpClient;

        string responseBodyAsText = "正在加载...";
        string responseGeo = "正在加载...";
        string responseTravel = "正在加载...";

        string imageHistory = "http://h.hiphotos.baidu.com/baike/pic/item/9c16fdfaaf51f3de1be96c0296eef01f3a297979.jpg";
        string imageGeo = "http://h.hiphotos.baidu.com/baike/pic/item/9c16fdfaaf51f3de1be96c0296eef01f3a297979.jpg";
        string imageTravel = "http://h.hiphotos.baidu.com/baike/pic/item/9c16fdfaaf51f3de1be96c0296eef01f3a297979.jpg";

        /// <summary>
        /// NavigationHelper 在每页上用于协助导航和
        /// 进程生命期管理
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// 可将其更改为强类型视图模型。
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public SplitPage()
        {
            this.InitializeComponent();
            httpClient = new HttpClient();
            var headers = httpClient.DefaultRequestHeaders;
            // 设置导航帮助程序
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            ThreadPool.RunAsync(
                     (timer) =>
                     {
                         getHtmlPage();
                     },
                     WorkItemPriority.Low, WorkItemOptions.None);

            // 设置逻辑页面导航组件，使
            // 页面可一次仅显示一个窗格。
            this.navigationHelper.GoBackCommand = new RelayCommand(() => this.GoBack(), () => this.CanGoBack());
            this.itemListView.SelectionChanged += ItemListView_SelectionChanged;

            // 开始侦听 Window 大小更改 
            // 以从显示两个窗格变为显示一个窗格
            Window.Current.SizeChanged += Window_SizeChanged;
            this.InvalidateVisualState();
        }
        private async void getImagePage()
        {
            Uri imageUrl = new Uri("http://baike.baidu.com/city/"+englishName);
            HttpResponseMessage res = await httpClient.GetAsync(imageUrl);
            res.EnsureSuccessStatusCode();
            imageHistory = await res.Content.ReadAsStringAsync();
            imageHistory = imageHistory.Replace("<br>", Environment.NewLine);

            Regex rg1 = new Regex("<li><img[\\s]src=\"(.*?)\"[\\s]width[\\s\\S]*?</li><li><img[\\s]src=\"(.*?)\"[\\s]width[\\s\\S]*?</li><li><img[\\s]src=\"(.*?)\"[\\s]width[\\s\\S]*?</li>", RegexOptions.IgnoreCase);
            Match matches = rg1.Match(imageHistory);          
            imageHistory = matches.Groups[1].Value;
            imageGeo = matches.Groups[2].Value;
            imageTravel = matches.Groups[3].Value;

        }
        private async void getHtmlPage()
        {
            try
            {
                ThreadPool.RunAsync(
                     (timer) =>
                     {
                         getImagePage();
                     },
                     WorkItemPriority.Low, WorkItemOptions.None);
                Uri resourceUri = new Uri("http://zh.wikipedia.org/wiki/"+cityName);          
                //Uri resourceUri = new Uri("http://zh.wikipedia.org/wiki/西安市");
                HttpResponseMessage response = await httpClient.GetAsync(resourceUri);
                response.EnsureSuccessStatusCode();

                responseBodyAsText = await response.Content.ReadAsStringAsync();
                //历史
                String s1 = responseBodyAsText, s2 = responseBodyAsText;
                responseBodyAsText = responseBodyAsText.Replace("<br>", Environment.NewLine); // Insert new lines
                //Regex rgEx = new Regex("<[^>]*>", RegexOptions.IgnoreCase);
                //Regex rgEx = new Regex("<a[^<]*><span[^<]*class=\"tocnumber\">1</span>[^>]*<span[^<]*class=\"toctext\">[^<]*</span></a>", RegexOptions.IgnoreCase);              
                //Regex rgEx1 = new Regex("<a[^>]*><span[\\s]class=\"tocnumber\">1</span>\\s<span[^>]*class=\"toctext\">[^<]*</span></a>", RegexOptions.IgnoreCase);

                //地理
                String s3 = responseBodyAsText, s4 = responseBodyAsText;
                responseGeo = responseBodyAsText;
                
                //旅游
                String s5 = responseBodyAsText, s6 = responseBodyAsText;
                responseTravel = responseBodyAsText;

                //获取历史的id
                Regex rgEx1 = new Regex("<a[^>]*><span[\\s]class=\"tocnumber\">(\\d)</span>\\s<span[^>]*class=\"toctext\">(历史|歷史)</span></a>", RegexOptions.IgnoreCase);
                Match matches = rgEx1.Match(s1);
                Regex rgEx11 = new Regex("href=\"#[^>]*\"");
                s1 = matches.Groups[0].Value;
                Match matches2 = rgEx11.Match(s1);
                s1 = matches2.Groups[0].Value.Replace("href=", "").Replace("#", "");

                //获取历史的下一个id
                String a1 = matches.Groups[1].Value;
                String b1 = Convert.ToString(int.Parse(a1) + 1);
                Regex rgEx2 = new Regex("<a[^>]*><span[\\s]class=\"tocnumber\">"+b1+"</span>\\s<span[^>]*class=\"toctext\">[^<]*</span></a>", RegexOptions.IgnoreCase);
                Match matchess = rgEx2.Match(s2);
                Regex rgEx21 = new Regex("href=\"#[^>]*\"");
                s2 = matchess.Groups[0].Value;
                Match matches22 = rgEx21.Match(s2);
                s2 = matches22.Groups[0].Value.Replace("href=", "").Replace("#", "");

                //获取两个id之间的内容
                Regex rgEx = new Regex("<span[\\s]class=\"mw-headline\"[\\s]id=" + s1 + ">[^<]*</span>([\\s\\S]*?)<span[\\s]class=\"mw-headline\"[\\s]id=" + s2 + ">", RegexOptions.IgnoreCase);
                Match m = rgEx.Match(responseBodyAsText);
                responseBodyAsText = m.Groups[0].Value;
  
                String s=("");
                Regex rgEx3 = new Regex("<p>[\\s\\S]*?</p>|<ul>[\\s\\S]*?</ul>|<h3>[\\s\\S]*?</h3>", RegexOptions.IgnoreCase);
                  
                MatchCollection m3 = rgEx3.Matches(responseBodyAsText);
                foreach (Match mm in m3)
                {
                    s += mm.ToString();
                    s += '\n';
                }

                responseBodyAsText = s;
                /*
                Regex rgEx3 = new Regex("<div([\\s\\S]*?)</div>", RegexOptions.IgnoreCase);
                responseBodyAsText = rgEx3.Replace(responseBodyAsText, "");
                */
                Regex rgEx4 = new Regex("<[^>]*>", RegexOptions.IgnoreCase);
                responseBodyAsText = rgEx4.Replace(responseBodyAsText, "");

                Regex rgEx5 = new Regex("\\[([\\s\\S]*?)\\]", RegexOptions.IgnoreCase);
                responseBodyAsText = rgEx5.Replace(responseBodyAsText, "");



                //获取地理的id
                rgEx1 = new Regex("<a[^>]*><span[\\s]class=\"tocnumber\">(\\d)</span>\\s<span[^>]*class=\"toctext\">(地理)</span></a>", RegexOptions.IgnoreCase);
                matches = rgEx1.Match(s3);
                rgEx11 = new Regex("href=\"#[^>]*\"");
                s3 = matches.Groups[0].Value;
                matches2 = rgEx11.Match(s3);
                s3 = matches2.Groups[0].Value.Replace("href=", "").Replace("#", "");

                //获取地理的下一个id
                a1 = matches.Groups[1].Value;
                b1 = Convert.ToString(int.Parse(a1) + 1);
                rgEx2 = new Regex("<a[^>]*><span[\\s]class=\"tocnumber\">" + b1 + "</span>\\s<span[^>]*class=\"toctext\">[^<]*</span></a>", RegexOptions.IgnoreCase);
                matchess = rgEx2.Match(s4);
                rgEx21 = new Regex("href=\"#[^>]*\"");
                s4 = matchess.Groups[0].Value;
                matches22 = rgEx21.Match(s4);
                s4 = matches22.Groups[0].Value.Replace("href=", "").Replace("#", "");

                //获取两个id之间的地理内容
                rgEx = new Regex("<span[\\s]class=\"mw-headline\"[\\s]id=" + s3 + ">[^<]*</span>([\\s\\S]*?)<span[\\s]class=\"mw-headline\"[\\s]id=" + s4 + ">", RegexOptions.IgnoreCase);
                m = rgEx.Match(responseGeo);
                responseGeo = m.Groups[0].Value;

                s = ("");
                rgEx3 = new Regex("<p>[\\s\\S]*?</p>|<ul>[\\s\\S]*?</ul>|<h3>[\\s\\S]*?</h3>", RegexOptions.IgnoreCase);

                m3 = rgEx3.Matches(responseGeo);
                foreach (Match mm in m3)
                {
                    s += mm.ToString();
                    s += '\n';
                }

                responseGeo = s;
                /*
                Regex rgEx3 = new Regex("<div([\\s\\S]*?)</div>", RegexOptions.IgnoreCase);
                responseGeo = rgEx3.Replace(responseGeo, "");
                */
                rgEx4 = new Regex("<[^>]*>", RegexOptions.IgnoreCase);
                responseGeo = rgEx4.Replace(responseGeo, "");

                rgEx5 = new Regex("\\[([\\s\\S]*?)\\]", RegexOptions.IgnoreCase);
                responseGeo = rgEx5.Replace(responseGeo, "");



                //旅游
                rgEx1 = new Regex("<a[^>]*><span[\\s]class=\"tocnumber\">(\\d)</span>\\s<span[^>]*class=\"toctext\">(旅游|旅遊)</span></a>", RegexOptions.IgnoreCase);
                matches = rgEx1.Match(s5);
                rgEx11 = new Regex("href=\"#[^>]*\"");
                s5 = matches.Groups[0].Value;
                matches2 = rgEx11.Match(s5);
                s5 = matches2.Groups[0].Value.Replace("href=", "").Replace("#", "");

                //获取旅游的下一个id
                a1 = matches.Groups[1].Value;
                b1 = Convert.ToString(int.Parse(a1) + 1);
                rgEx2 = new Regex("<a[^>]*><span[\\s]class=\"tocnumber\">" + b1 + "</span>\\s<span[^>]*class=\"toctext\">[^<]*</span></a>", RegexOptions.IgnoreCase);
                matchess = rgEx2.Match(s6);
                rgEx21 = new Regex("href=\"#[^>]*\"");
                s6 = matchess.Groups[0].Value;
                matches22 = rgEx21.Match(s6);
                s6 = matches22.Groups[0].Value.Replace("href=", "").Replace("#", "");

                //获取两个id之间的旅游内容
                rgEx = new Regex("<span[\\s]class=\"mw-headline\"[\\s]id=" + s5 + ">[^<]*</span>([\\s\\S]*?)<span[\\s]class=\"mw-headline\"[\\s]id=" + s6 + ">", RegexOptions.IgnoreCase);
                m = rgEx.Match(responseTravel);
                responseTravel = m.Groups[0].Value;

                s = ("");
                rgEx3 = new Regex("<p>[\\s\\S]*?</p>|<ul>[\\s\\S]*?</ul>|<h3>[\\s\\S]*?</h3>", RegexOptions.IgnoreCase);

                m3 = rgEx3.Matches(responseTravel);
                foreach (Match mm in m3)
                {
                    s += mm.ToString();
                    s += '\n';
                }

                responseTravel = s;
                /*
                Regex rgEx3 = new Regex("<div([\\s\\S]*?)</div>", RegexOptions.IgnoreCase);
                responseTravel = rgEx3.Replace(responseTravel, "");
                */
                rgEx4 = new Regex("<[^>]*>", RegexOptions.IgnoreCase);
                responseTravel = rgEx4.Replace(responseTravel, "");

                rgEx5 = new Regex("\\[([\\s\\S]*?)\\]", RegexOptions.IgnoreCase);
                responseTravel = rgEx5.Replace(responseTravel, "");



            /*
                MessageDialog msd = new MessageDialog(responseBodyAsText);

                this.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    msd.ShowAsync();
                });
            */
            }          
             catch (Exception ex)
             {
                 // For debugging
                 MessageDialog msd = new MessageDialog("网络不给力");
                 this.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                 {
                     msd.ShowAsync();
                 });
             }
        }
        /// <summary>
        /// 使用在导航过程中传递的内容填充页。  在从以前的会话
        /// 重新创建页时，也会提供任何已保存状态。
        /// </summary>
        /// <param name="sender">
        /// 事件的来源; 通常为 <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">事件数据，其中既提供在最初请求此页时传递给
        /// <see cref="Frame.Navigate(Type, Object)"/> 的导航参数，又提供
        /// 此页在以前会话期间保留的状态的
        /// 字典。 首次访问页面时，该状态将为 null。</param>
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            //String para = (String)(e.NavigationParameter.ToString().Substring(0, 7));
            var group = await SampleDataSource.GetGroupAsync(groupId);
           // var group = await SampleDataSource.GetGroupAsync(para);
            //var group = await SampleDataSource.GetGroupAsync((String)e.NavigationParameter);
            this.DefaultViewModel["Group"] = group;
           //加载除标题外的部分
            this.DefaultViewModel["Items"] = group.Items;

            if (e.PageState == null)
            {
                this.itemListView.SelectedItem = null;
                // 当这是新页时，除非正在使用逻辑页导航，
                // 否则会自动选择第一项(请参见下面的逻辑页导航 #region。)
                if (!this.UsingLogicalPageNavigation() && this.itemsViewSource.View != null)
                {
                    this.itemsViewSource.View.MoveCurrentToFirst();
                }
            }
            else
            {
                // 还原与此页关联的以前保存的状态
                if (e.PageState.ContainsKey("SelectedItem") && this.itemsViewSource.View != null)
                {
                    var selectedItem = await SampleDataSource.GetItemAsync((String)e.PageState["SelectedItem"]);
                    this.itemsViewSource.View.MoveCurrentTo(selectedItem);
                }
            }
        }

        /// <summary>
        /// 保留与此页关联的状态，以防挂起应用程序或
        /// 从导航缓存中放弃此页。  值必须符合
        /// <see cref="SuspensionManager.SessionState"/> 的序列化要求。
        /// </summary>
        /// <param name="navigationParameter">最初请求此页时传递给
        /// <see cref="Frame.Navigate(Type, Object)"/> 的参数值。
        /// </param>
        ///<param name="sender">事件的来源；通常为 <see cref="NavigationHelper"/></param>
        ///<param name="e">提供要使用可序列化状态填充的空字典
        ///的事件数据。</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (this.itemsViewSource.View != null)
            {
                var selectedItem = (Data.SampleDataItem)this.itemsViewSource.View.CurrentItem;
                if (selectedItem != null) e.PageState["SelectedItem"] = selectedItem.UniqueId;
            }
        }

        #region 逻辑页导航

        // 设计了拆分页，以便 Window 具有足够的空间同时显示
        // 列表和详细信息，一次将仅显示一个窗格。
        //
        // 这完全通过一个可表示两个逻辑页的单一物理页
        // 实现。  使用下面的代码可以实现此目标，且用户不会察觉到
        // 区别。

        private const int MinimumWidthForSupportingTwoPanes = 768;

        /// <summary>
        /// 在确定该页是应用作一个逻辑页还是两个逻辑页时进行调用。
        /// </summary>
        /// <returns>如果窗口应显示充当一个逻辑页，则为 True，
        /// 。</returns>
        private bool UsingLogicalPageNavigation()
        {
            return Window.Current.Bounds.Width < MinimumWidthForSupportingTwoPanes;
        }

        /// <summary>
        /// 在 Window 改变大小时调用
        /// </summary>
        /// <param name="sender">当前的 Window</param>
        /// <param name="e">描述 Window 新大小的事件数据</param>
        private void Window_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            this.InvalidateVisualState();
        }

        /// <summary>
        /// 在选定列表中的项时进行调用。
        /// </summary>
        /// <param name="sender">显示所选项的 GridView。</param>
        /// <param name="e">描述如何更改选择内容的事件数据。</param>
        private void ItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 使视图状态在逻辑页导航起作用时无效，因为
            // 选择内容方面的更改可能会导致当前逻辑页发生相应的更改。
            // 选定某项后，这将会导致从显示项列表
            // 更改为显示选定项的详细信息。  清除选择时，将产生
            // 相反的效果。
            if (this.UsingLogicalPageNavigation()) this.InvalidateVisualState();
            if (itemListView.SelectedIndex == 0)
            {
               
                textBlock.Text = responseBodyAsText;

                image.Source = new BitmapImage(new Uri(imageHistory, UriKind.Absolute));
                
            }
            else if (itemListView.SelectedIndex == 1)
            {
                textBlock.Text = responseGeo;
                image.Source = new BitmapImage(new Uri(imageGeo, UriKind.Absolute));
            }
            else
            {
                textBlock.Text = responseTravel;
                image.Source = new BitmapImage(new Uri(imageTravel, UriKind.Absolute));
            }
        }

        private bool CanGoBack()
        {
            if (this.UsingLogicalPageNavigation() && this.itemListView.SelectedItem != null)
            {
                return true;
            }
            else
            {
                return this.navigationHelper.CanGoBack();
            }
        }
        private void GoBack()
        {
            if (this.UsingLogicalPageNavigation() && this.itemListView.SelectedItem != null)
            {
                // 如果逻辑页导航起作用且存在选定项，则当前将显示
                // 选定项的详细信息。    清除选择后将返回到
                // 项列表。    从用户的角度来看，这是一个逻辑后向
                // 导航。
                this.itemListView.SelectedItem = null;
            }
            else
            {
                this.navigationHelper.GoBack();
            }
        }

        private void InvalidateVisualState()
        {
            var visualState = DetermineVisualState();
            VisualStateManager.GoToState(this, visualState, false);
            this.navigationHelper.GoBackCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// 在确定对应于应用程序视图状态的视觉状态的名称时进行
        /// 视图状态。
        /// </summary>
        /// <returns>所需的视觉状态的名称。  此名称与视图状态的名称相同，
        /// 但在纵向和对齐视图中存在选定项时例外，在纵向和对齐视图中，
        /// 此附加逻辑页通过添加 _Detail 后缀表示。</returns>
        private string DetermineVisualState()
        {
            if (!UsingLogicalPageNavigation())
                return "PrimaryView";

            // 在视图状态更改时更新后退按钮的启用状态
            var logicalPageBack = this.UsingLogicalPageNavigation() && this.itemListView.SelectedItem != null;

            return logicalPageBack ? "SinglePane_Detail" : "SinglePane";
        }

        #endregion

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
            Appfinal.MainPage.Para para = e.Parameter as Appfinal.MainPage.Para;
            if (para != null)
            {
                groupId = para.groupId;
                cityName = para.cityName;
                englishName = para.englishName;
            }
            pageTitle.Text = "城市详情";
            itemTitle.Text = cityName;
            

            navigationHelper.OnNavigatedTo(e);
        }
       
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}
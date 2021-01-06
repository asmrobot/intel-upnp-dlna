using OpenSource.UPnP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DlanM
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            StartControlPoint();
        }

        private UpnpSmartControlPoint smart;
        private UInt32 instanceID=0;
        private bool ResourceIsDefault()
        {
            if (this.resourceUri.Text.Equals("请输入投屏地址。。。"))
            {
                return true;
            }
            return false;
        }

        private void resourceUri_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ResourceIsDefault())
            {
                this.resourceUri.Text = string.Empty;
            }
        }
        private void resourceUri_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.resourceUri.Text))
            {
                this.resourceUri.Text = "请输入投屏地址。。。";
            }
        }

        /// <summary>
        /// 扫描
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void reScan_Click(object sender, RoutedEventArgs e)
        {
            StartControlPoint();
        }

        /// <summary>
        /// 投屏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void proj_Click(object sender, RoutedEventArgs e)
        {
            string uri = this.resourceUri.Text;
            if (string.IsNullOrEmpty(uri)|| ResourceIsDefault())
            {
                MessageBox.Show("请填入正确的资源URI");
                return;
            }

            var device = this.deviceList.SelectedItem as UPnPDevice;
            if (device == null)
            {
                MessageBox.Show("请选择要投的设备");
                return;
            }

            try
            {
                UPnPService service = device.GetService("AVTransport");
                if (service == null)
                {
                    MessageBox.Show("设备无投屏功能");
                    return;
                }

                this.instanceID++;
                UPnPAction transportAction = service.GetAction("SetAVTransportURI");
                UPnPArgument instanceID = transportAction.GetArg("InstanceID");
                instanceID.DataValue = this.instanceID;
                UPnPArgument currentURI = transportAction.GetArg("CurrentURI");
                currentURI.DataValue = uri;

                UPnPArgument current = transportAction.GetArg("CurrentURIMetaData");
                current.DataValue = string.Empty;
                service.InvokeAsync(transportAction.Name, new[] { instanceID, currentURI, current });


                UPnPAction playAction = service.GetAction("Play");
                UPnPArgument speed = playAction.GetArg("Speed");
                service.InvokeAsync(playAction.Name, new[] { instanceID, speed });
            }
            catch (Exception ex)
            {
                MessageBox.Show("投屏失败,"+ex.Message);
            }
        }



        #region utils
        private void StartControlPoint()
        {
            //添列表
            this.deviceList.Items.Clear();
            
            if (this.smart != null)
            {
                StopControlPoint(this.smart);
            }
            
            this.smart = new UpnpSmartControlPoint();
            this.smart.OnAddedDevice += controPoint_OnAddedDevice;
            this.smart.OnRemovedDevice += controPoint_OnRemovedDevice;
            this.smart.OnDeviceExpired += controPoint_OnDeviceExpired;
        }

        private void StopControlPoint(UpnpSmartControlPoint controlPoint)
        {
            controlPoint.ShutDown();
            controlPoint.OnAddedDevice -= controPoint_OnAddedDevice;
            controlPoint.OnRemovedDevice -= controPoint_OnRemovedDevice;
            controlPoint.OnDeviceExpired -= controPoint_OnDeviceExpired;
            controlPoint = null;
        }


        private static object lockHelper = new object();
        private void controPoint_OnAddedDevice(UpnpSmartControlPoint sender, UPnPDevice device)
        {
            if (device.StandardDeviceType == "MediaRenderer")
            {
                UPnPService service = device.GetService("AVTransport");
                if (service != null)
                {
                    this.Dispatcher.BeginInvoke((Action)(() => {
                        this.deviceList.Items.Add(device);
                    }));
                }
            }
        }

        private void controPoint_OnRemovedDevice(UpnpSmartControlPoint sender, UPnPDevice device)
        {
            //this.deviceList.Items.Remove(device);
        }

        private void controPoint_OnDeviceExpired(UpnpSmartControlPoint sender, UPnPDevice device)
        {
            //this.deviceList.Items.Remove(device);
        }
        #endregion


    }
}

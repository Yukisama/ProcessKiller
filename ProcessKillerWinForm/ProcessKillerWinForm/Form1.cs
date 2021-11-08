using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProcessKillerWinForm
{
    public partial class FormMain : Form
    {
        private IWebHost webhost;
        private int port = 0;
        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            txtLog.Text += Environment.NewLine + "setting webhost port" + Environment.NewLine ;
            txtLog.Text += Environment.NewLine + "the port should be allowed in the firewall first." + Environment.NewLine;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //input webhost port
            if (!Int32.TryParse(txtPort.Text , out port))
            {
                MessageBox.Show("please input number.");
                txtLog.Text += Environment.NewLine + "WARN: please input number." + Environment.NewLine;
                return;
            }
            if (port == 0)
            {
                MessageBox.Show("the port need in 1 to 65535 range.");
                txtLog.Text += Environment.NewLine + "WARN: the port need in 1 to 65535 range." + Environment.NewLine;
                return;
            }

            //webhost starting
            txtLog.Text += Environment.NewLine + "system starting" + Environment.NewLine;
            txtLog.Text += Environment.NewLine + $"the webhost route is http://127.0.0.1:{port}" + Environment.NewLine;
            webhost = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://*:{port}")
                .Configure(configure)
                .Build();
            webhost.RunAsync(); //don't await to lock main thread.

            var openurl = new System.Diagnostics.Process();
            try
            {
                openurl.StartInfo.UseShellExecute = true;
                openurl.StartInfo.FileName = $"http://127.0.0.1:{port}";
                openurl.Start();
            }
            catch (Exception ex)
            {
                txtLog.Text += Environment.NewLine + $"ERROR: {ex.Message}" + Environment.NewLine;
            }

            //webhost started
            txtLog.Text += Environment.NewLine + "system started" + Environment.NewLine;

        }
        private static void configure(IApplicationBuilder app)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.Run(async (context) => {
                var request = context.Request;
                var response = context.Response;
                var path = request.Path.Value;

                try
                {
                    switch (path)
                    {
                        case "/killProcess":
                            if (request.Method != HttpMethods.Post) { response.StatusCode = 405; break; }
                            response.StatusCode = 200;
                            var stream = new System.IO.StreamReader(request.Body);
                            string data = await stream.ReadToEndAsync();
                            var jdata = JsonDocument.Parse(data);
                            var ps = System.Diagnostics.Process.GetProcesses().Where(s => s.ProcessName == jdata.RootElement.GetProperty("proceName").ToString());
                            foreach (var p in ps) { p.Kill(); }
                            await response.WriteAsync("killed");
                            break;
                        case "/getProcess":
                            response.StatusCode = 200;
                            response.ContentType = "application/json";
                            List<string> runningProcesses = System.Diagnostics.Process.GetProcesses().Select(s => s.ProcessName).OrderBy(s => s).Distinct().ToList();
                            string json = JsonSerializer.Serialize(runningProcesses);
                            await response.WriteAsync(json);
                            break;
                        default:
                            response.StatusCode = 404;
                            await response.WriteAsync("Wrong path");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    response.StatusCode = 500;
                    await response.WriteAsync(ex.Message);
                }
            });
        }

        private void btnEnd_Click(object sender, EventArgs e)
        {
            if (webhost == null)
            {
                txtLog.Text += Environment.NewLine + "webhost not start yet." + Environment.NewLine;
                return;
            }
            txtLog.Text += Environment.NewLine + "system closing" + Environment.NewLine;
            webhost.StopAsync();
            webhost.WaitForShutdown();
            webhost = null;
            txtLog.Text += Environment.NewLine + "webhost is terminated" + Environment.NewLine;
            txtLog.Text += Environment.NewLine + "system closed" + Environment.NewLine;
        }
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //WinForm mode requires WaitForShutdown() when terminating the process

            if (webhost == null)
            {
                return;
            }
            txtLog.Text += Environment.NewLine + "system closing" + Environment.NewLine;
            webhost.StopAsync();
            webhost.WaitForShutdown();
            webhost = null;
            txtLog.Text += Environment.NewLine + "webhost is terminated" + Environment.NewLine;
            txtLog.Text += Environment.NewLine + "system closed" + Environment.NewLine;
        }

        private void txtLog_TextChanged(object sender, EventArgs e)
        {
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.ScrollToCaret();
        }
    }
}

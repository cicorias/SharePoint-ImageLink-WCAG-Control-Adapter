using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ImageLinkControlAdapter.Features.ImageLinkAdapter
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("2bba4fb9-96f4-4f5b-8a9f-f09f597b0b3c")]
    public class ImageLinkControlAdapterReceiver : SPFeatureReceiver
    {
        /// <summary>
        /// Feature will create 1 timer job for each Server AND for each IIS site
        /// So, Server X Zones
        /// </summary>
        /// <param name="properties"></param>
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            try
            {
                var webApp = properties.Feature.Parent as SPWebApplication;
                if (webApp == null) return;
                var src = Path.Combine(properties.Definition.RootDirectory, "Browsers\\compat.contoso.browser");

                foreach (var server in SPFarm.Local.Servers)
                {
                    foreach (var settings in webApp.IisSettings)
                    {
                        var dst = Path.Combine(Path.Combine(
                        settings.Value.Path.FullName, "App_Browsers"), "compat.contoso.browser");

                        var touch = Path.Combine(Path.Combine(
                            settings.Value.Path.FullName, "App_Browsers"), "compat.browser");

                        var jobName = string.Format("ImageLink Control Adapter Copy Timer Job for server: {0} - {1}", server.Id, settings.Key);
                        if (!File.Exists(dst))
                        {
                            var job = new CopyTimerJob(webApp, server, jobName)
                            {
                                Schedule = new SPOneTimeSchedule(DateTime.Now)
                            };
                            job.Properties.Add("src", src);
                            job.Properties.Add("dst", dst);
                            job.Properties.Add("touch", touch);
                            job.Update();
                        }
                    }
                }
            }
            catch (Exception )
            {
                //do something..
            }
        }
        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            try
            {
                var webApp = properties.Feature.Parent as SPWebApplication;
                if (webApp == null) return;

                foreach (var server in SPFarm.Local.Servers)
                {
                    foreach (var settings in webApp.IisSettings)
                    {
                        var src = Path.Combine(Path.Combine(
                        settings.Value.Path.FullName, "App_Browsers"), "compat.contoso.browser");

                        var touch = Path.Combine(Path.Combine(
                            settings.Value.Path.FullName, "App_Browsers"), "compat.browser");
                        if (File.Exists(src))
                        {
                            var jobName = string.Format("ImageLink Control Adapter Delete Timer Job server: {0} - {1}", server.Id, settings.Key);
                            var job = new DeleteTimerJob(webApp, server, jobName)
                            {
                                Schedule = new SPOneTimeSchedule(DateTime.Now)
                            };
                            job.Properties.Add("src", src);
                            job.Properties.Add("touch", touch);
                            job.Update();
                        }
                    }
                }
            }
            catch (Exception )
            {
                //Do something
            }
        }

    }

    public class CopyTimerJob : SPJobDefinition
    {
        public CopyTimerJob() : base() { }

        public CopyTimerJob(SPWebApplication webService, SPServer server, string jobName)
            : base(jobName, webService, server, SPJobLockType.None)
        {
        }
        public override void Execute(Guid targetInstanceId)
        {
            try
            {
                var src = Properties["src"] as string;
                var dst = Properties["dst"] as string;
                var touch = Properties["touch"] as string;
                if (src == null || dst == null)
                {
                    return;
                }

                var directoryName = Path.GetDirectoryName(dst);
                if (!Directory.Exists(directoryName))
                {
                    return;
                }

                File.Copy(src, dst, true);

                if (touch != null && File.Exists(touch))
                {
                    File.SetLastWriteTime(touch, DateTime.Now);
                }
            }
            catch (Exception )
            {
                //Do something
            }
            finally
            {
                Delete();
            }
        }
    }

    public class DeleteTimerJob : SPJobDefinition
    {
        public DeleteTimerJob() : base() { }

        public DeleteTimerJob(SPWebApplication webService, SPServer server, string jobName)
            : base(jobName, webService, server, SPJobLockType.None)
        {
        }
        public override void Execute(Guid targetInstanceId)
        {
            try
            {
                var src = Properties["src"] as string;
                var touch = Properties["touch"] as string;
                if (src == null)
                {
                    return;
                }
                if (File.Exists(src))
                {
                    File.Delete(src);
                }

                if (touch != null && File.Exists(touch))
                {
                    File.SetLastWriteTime(touch, DateTime.Now);
                }
            }
            catch (Exception )
            {
                //Do something
            }
            finally
            {
                Delete();
            }
        }
    }
}

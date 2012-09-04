﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Routing;
using SignalR.Hubs;
using SignalR.Samples.Hubs.DemoHub;
using SignalR.Samples.Raw;
using SignalR.Samples.Streaming;

namespace SignalR.Hosting.AspNet.Samples
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var pipeline = GlobalHost.DependencyResolver.Resolve<IHubPipeline>();
            pipeline.Use(new SamplePipelineBuilder());

            ThreadPool.QueueUserWorkItem(_ =>
            {
                var context = GlobalHost.ConnectionManager.GetConnectionContext<Streaming>();
                var hubContext = GlobalHost.ConnectionManager.GetHubContext<DemoHub>();

                while (true)
                {
                    try
                    {
                        context.Connection.Broadcast(DateTime.Now.ToString());
                        hubContext.Clients.fromArbitraryCode(DateTime.Now.ToString());
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("SignalR error thrown in Streaming broadcast: {0}", ex);
                    }
                    Thread.Sleep(2000);
                }
            });


            RouteTable.Routes.MapConnection<SendingConnection>("sending-connection", "sending-connection/{*operation}");
            RouteTable.Routes.MapConnection<TestConnection>("test-connection", "test-connection/{*operation}");
            RouteTable.Routes.MapConnection<Raw>("raw", "raw/{*operation}");
            RouteTable.Routes.MapConnection<Streaming>("streaming", "streaming/{*operation}");
        }

        private class SamplePipelineBuilder : HubPipelineBuilder
        {
            protected override void OnBeforeInvoke(IHubIncomingInvokerContext context)
            {
                Debug.WriteLine("=> Invoking " + context.MethodDescriptor.Name + " on hub " + context.MethodDescriptor.Hub.Name);
                base.OnBeforeInvoke(context);
            }

            protected override void OnBeforeOutgoing(IHubOutgoingInvokerContext context)
            {
                Debug.WriteLine("<= Invoking " + context.Invocation.Method + " on client hub " + context.Invocation.Hub); 
                base.OnBeforeOutgoing(context);
            }
        }
    }
}
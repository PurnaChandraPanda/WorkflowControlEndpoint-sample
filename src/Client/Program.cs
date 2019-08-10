//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

using System;
using System.ServiceModel;
using System.ServiceModel.Activities;
using System.Threading;

namespace Microsoft.Samples.WF.ManagementEndpoint
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Client starting...");

            try
            {
                IWorkflowCreation creationClient = new ChannelFactory<IWorkflowCreation>(new BasicHttpBinding(), "http://localhost/DataflowControl.xaml/Creation").CreateChannel();

                // Start a new instance of the workflow
                Guid instanceId = creationClient.CreateSuspended(null);

                // initialize workflow control client
                WorkflowControlClient controlClient = new WorkflowControlClient(
                    new BasicHttpBinding(),
                    new EndpointAddress(new Uri("http://localhost/DataflowControl.xaml")));

                controlClient.Unsuspend(instanceId);
                Console.WriteLine($"Client exiting...");

                // you should have an instance persisted by now

                Thread.Sleep(10000);
                // if cancel, goes in executing, suspended -> idle, not-suspended
                // if abandon, goes in executing, suspended and stays there 
                // if terminate, clears the instance from database
                controlClient.Terminate(instanceId);
                Console.WriteLine($"instance terminated...");        
                
                // you should have the persistence workflow instance removed from database by now
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            Console.ReadLine();
        }
    }
}

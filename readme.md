I reviewed some of the classic workflow samples from our “WF_WCF_Samples” [repo](https://docs.microsoft.com/en-us/dotnet/framework/windows-workflow-foundation/samples/).

It has a project with name “ManagementEndpoint” that talks a great deal about usage of *WorkflowControlEndpoint* class. I modified the sample a little to introduce persistence part in the workflow. After which, called from client to test if workflow instance is actually getting terminated with *Terminate* API call. Yes, it instantaneously removes the associated workflow instance ID from [Instances] table in the persistence store database.

The [doc](https://docs.microsoft.com/en-us/dotnet/api/system.servicemodel.activities.workflowhostingendpoint?view=netframework-4.8) talks a great deal about the service side implementation where instance GUID and other operations are effectively handled via *WorkflowHostingEndpoint* class.

## Service side
1.	XAML is being modified to introduced *Delay* activity for 3 minutes (00:03:00). 
2.	*WorkflowServiceHost* initialization is being modified to have *SqlWorkflowInstanceStoreBehavior* added to configure the persistence store part.

            WorkflowServiceHost host = new WorkflowServiceHost(workflow,
                new Uri("http://localhost/Dataflow.xaml"));
            host.Description.Behaviors.Add(new SqlWorkflowInstanceStoreBehavior
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["persistenceStore"].ConnectionString,
                InstanceEncodingOption = InstanceEncodingOption.None,
                InstanceCompletionAction = InstanceCompletionAction.DeleteAll,
                InstanceLockedExceptionAction = InstanceLockedExceptionAction.NoRetry,
                HostLockRenewalPeriod = new TimeSpan(0, 0, 30),
                RunnableInstancesDetectionPeriod = new TimeSpan(0, 0, 5)
            });

3.	The *app.config* file have the persistence store database’ connection string configured.

  <connectionStrings>
    <add name="persistenceStore" connectionString="connection-string-to-your-persistence-store-database"/>
  </connectionStrings>

## Client side
It is just being tested with *Terminate* API call for the instance via object of *WorkflowControlClient* type.

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

## How to run the sample
Open in VS utility, which has “workflow” package is being installed.

1.	Run the project *ManagementEndpoint* in “debug” mode.
2.	Wait for some time, i.e. “delay” set time and some extra, so that initial workflow instance would be removed from [Instances] table.
3.	Launch command prompt and run “client.exe” from the “Client” project directory.
4.	Put breakpoint in the *OnGetCreationContext* API in “CreationEndpoint” class.
5.	You would see “instanceId” parameter have actual mapped workflow instance ID value reflected as per the database’ [Instances] table.
6.	On the database end, you would observe an instance when “client exiting” message is flagged. 
7.	And, when “instance terminated” message is flagged, it would be observed that the same instance be removed from database.



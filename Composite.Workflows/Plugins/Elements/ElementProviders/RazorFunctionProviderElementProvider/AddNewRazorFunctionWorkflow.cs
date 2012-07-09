using System;
using System.IO;
using System.Linq;
using System.Workflow.Activities;
using Composite.AspNet.Security;
using Composite.C1Console.Actions;
using Composite.C1Console.Users;
using Composite.C1Console.Workflow;
using Composite.Core.Extensions;
using Composite.Core.IO;
using Composite.Core.ResourceSystem;
using Composite.Functions;
using Composite.Plugins.Elements.ElementProviders.BaseFunctionProviderElementProvider;
using Composite.Plugins.Functions.FunctionProviders.RazorFunctionProvider;
using Composite.Plugins.Elements.ElementProviders.Common;

namespace Composite.Plugins.Elements.ElementProviders.RazorFunctionProviderElementProvider
{
    [AllowPersistingWorkflow(WorkflowPersistingType.Idle)]
    public sealed partial class AddNewRazorFunctionWorkflow : BaseFunctionWorkflow
    {
        private static readonly string Binding_Name = "Name";
        private static readonly string Binding_Namespace = "Namespace";

        private static readonly string NewRazorFunction_CSHTML =
@"@inherits RazorFunction

@functions {
    public override string FunctionDescription
    {
        get  { return ""A demo function that outputs a hello message.""; }
    }
     
    [FunctionParameter(DefaultValue = ""World"")]
    public string Name { get; set; }
}

<html xmlns=""http://www.w3.org/1999/xhtml"">
    <head>
    </head>
    <body>
        <div>
            Hello @Name!
        </div>
    </body>
</html>";


        public AddNewRazorFunctionWorkflow()
        {
            InitializeComponent();
        }

        private void initializeCodeActivity_ExecuteCode(object sender, EventArgs e)
        {
            BaseFunctionFolderElementEntityToken token = (BaseFunctionFolderElementEntityToken)this.EntityToken;
            string @namespace = token.FunctionNamespace ?? UserSettings.LastSpecifiedNamespace;

            this.Bindings.Add(Binding_Name, string.Empty);
            this.Bindings.Add(Binding_Namespace, @namespace);
        }

        private void IsValidData(object sender, ConditionalEventArgs e)
        {
            string functionName = this.GetBinding<string>(Binding_Name);
            string functionNamespace = this.GetBinding<string>(Binding_Namespace);
            var provider = GetFunctionProvider<RazorFunctionProvider>();

            e.Result = false;

            if (functionName == string.Empty)
            {
                ShowFieldMessage(Binding_Name, GetText("AddNewRazorFunctionWorkflow.EmptyName"));
                return;
            }

            if (string.IsNullOrWhiteSpace(functionNamespace))
            {
                ShowFieldMessage(Binding_Namespace, GetText("AddNewRazorFunctionWorkflow.NamespaceEmpty"));
                return;
            }

            if (!functionNamespace.IsCorrectNamespace('.'))
            {
                ShowFieldMessage(Binding_Namespace, GetText("AddNewRazorFunctionWorkflow.InvalidNamespace"));
                return;
            }

            string functionFullName = functionNamespace + "." + functionName;

            bool nameIsUsed = FunctionFacade.FunctionNames.Contains(functionFullName, StringComparer.OrdinalIgnoreCase);
            if (nameIsUsed)
            {
                ShowFieldMessage(Binding_Namespace, GetText("AddNewRazorFunctionWorkflow.DuplicateName"));
                return;
            }

            if ((provider.PhysicalPath + functionNamespace + functionName).Length > 240)
            {
                ShowFieldMessage(Binding_Name, GetText("AddNewRazorFunctionWorkflow.TotalNameTooLang"));
                return;
            }

            e.Result = true;
        }



        private void finalizecodeActivity_ExecuteCode(object sender, EventArgs e)
        {
            string functionName = this.GetBinding<string>(Binding_Name);
            string functionNamespace = this.GetBinding<string>(Binding_Namespace);
            string functionFullName = functionNamespace + "." + functionName;

            var provider = GetFunctionProvider<RazorFunctionProvider>();

            AddNewTreeRefresher addNewTreeRefresher = this.CreateAddNewTreeRefresher(this.EntityToken);

            string fileName = functionName + ".cshtml";
            string folder = Path.Combine(provider.PhysicalPath, functionNamespace.Replace('.', '\\'));
            string cshtmlFilePath = Path.Combine(folder, fileName);


            C1Directory.CreateDirectory(folder);
            C1File.WriteAllText(cshtmlFilePath, NewRazorFunction_CSHTML);

            UserSettings.LastSpecifiedNamespace = functionNamespace;

            provider.ReloadFunctions();

            var newFunctionEntityToken = new FileBasedFunctionEntityToken(provider.Name, functionFullName);

            addNewTreeRefresher.PostRefreshMesseges(newFunctionEntityToken);

            /* var container = WorkflowFacade.GetFlowControllerServicesContainer(WorkflowEnvironment.WorkflowInstanceId);
            var executionService = container.GetService<IActionExecutionService>();
            executionService.Execute(newFunctionEntityToken, new WorkflowActionToken(typeof(EditRazorFunctionWorkflow)), null);*/
        }

        private static string GetText(string key)
        {
            return StringResourceSystemFacade.GetString("Composite.Plugins.RazorFunction", key);
        }
    }
}
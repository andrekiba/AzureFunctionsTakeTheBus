using System.Collections.Generic;
using System.Threading.Tasks;
using Pulumi;
using Pulumi.AzureNative.Resources;
using Storage = Pulumi.AzureNative.Storage;
using ASB = Pulumi.AzureNative.ServiceBus;
using Deployment = Pulumi.Deployment;

namespace AzureFunctionsTakeTheBus.Infra
{
    internal class AppStack : Stack
    {
        [Output] public Output<string> PrimaryStorageKey { get; set; }
        [Output] public Output<string> ASBPrimaryConnectionString { get; set; }
        
        public AppStack()
        {
            const string projectName = "functionstakebus";
            var stackName = Deployment.Instance.StackName;
            var azureConfig = new Config("azure-native");
            var location = azureConfig.Require("location");
            var ignoreChanges = new CustomResourceOptions
            {
                IgnoreChanges = new List<string> {"tags"}
            };
            
            #region Resource Group
            
            var resourceGroupName = $"{projectName}-{stackName}-rg";
            var resourceGroup = new ResourceGroup(resourceGroupName, new ResourceGroupArgs
            {
                Location = location,
                ResourceGroupName = resourceGroupName
            }, ignoreChanges);

            #endregion
            
            #region Azure Storage

            var storageAccountName = $"{projectName}{stackName}st";
            var storageAccount = new Storage.StorageAccount(storageAccountName, new Storage.StorageAccountArgs
            {
                AccountName = storageAccountName,
                ResourceGroupName = resourceGroup.Name,
                Location = location,
                Sku = new Storage.Inputs.SkuArgs
                {
                    Name = Storage.SkuName.Standard_LRS
                },
                Kind = Storage.Kind.StorageV2
            }, ignoreChanges);
            
            #endregion
            
            #region ASB

            var asbNamespaceName = $"{projectName}{stackName}ns";
            var asbNamespace = new ASB.Namespace(asbNamespaceName, new ASB.NamespaceArgs
            {
                Location = location,
                NamespaceName = asbNamespaceName,
                ResourceGroupName = resourceGroup.Name,
                Sku = new ASB.Inputs.SBSkuArgs
                {
                    Name = ASB.SkuName.Standard,
                    Tier = ASB.SkuTier.Standard
                }
            }, ignoreChanges);
            
            #endregion
            
            #region Output
            
            PrimaryStorageKey = Output.Tuple(resourceGroup.Name, storageAccount.Name).Apply(names =>
                Output.Create(GetStorageAccountPrimaryKey(names.Item1, names.Item2)));
            
            ASBPrimaryConnectionString = Output.Tuple(resourceGroup.Name, asbNamespace.Name).Apply(names =>
                Output.Create(GetASBPrimaryConectionString(names.Item1, names.Item2)));

            #endregion
        }
        
        #region Private Methods
        static async Task<string> GetStorageAccountPrimaryKey(string resourceGroupName, string accountName)
        {
            var result = await Storage.ListStorageAccountKeys.InvokeAsync(new Storage.ListStorageAccountKeysArgs
            {
                ResourceGroupName = resourceGroupName,
                AccountName = accountName
            });
            return $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={result.Keys[0].Value};EndpointSuffix=core.windows.net";
        }
        static async Task<string> GetASBPrimaryConectionString(string resourceGroupName, string namespaceName)
        {
            var result = await ASB.ListNamespaceKeys.InvokeAsync(new ASB.ListNamespaceKeysArgs
            {
                AuthorizationRuleName = "RootManageSharedAccessKey",
                NamespaceName = namespaceName,
                ResourceGroupName = resourceGroupName
            });
            return result.PrimaryConnectionString;
        }
        
        #endregion
    }
}

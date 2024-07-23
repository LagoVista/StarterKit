using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.IoT.Billing;
using LagoVista.IoT.Deployment.Admin;
using LagoVista.IoT.DeviceAdmin.Interfaces.Managers;
using LagoVista.IoT.DeviceManagement.Core.Managers;
using LagoVista.IoT.DeviceMessaging.Admin.Managers;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.Pipeline.Admin.Managers;
using LagoVista.IoT.Simulator.Admin.Managers;
using LagoVista.IoT.StarterKit.Interfaces;
using LagoVista.IoT.Verifiers.Managers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.IoT.StarterKit.Managers
{
    public class DataServicesManager : ManagerBase, IDataServicesManager
    {
        IDeviceAdminManager _deviceAdminMgr;
        ISubscriptionManager _subscriptionMgr;
        IPipelineModuleManager _pipelineMgr;
        IDeviceTypeManager _deviceTypeMgr;
        IDeviceMessageDefinitionManager _deviceMsgMgr;
        IDeploymentInstanceManager _instanceMgr;
        IDeploymentHostManager _hostManager;
        ISolutionManager _solutionMgr;
        IDeviceConfigurationManager _deviceCfgMgr;
        IDeviceRepositoryManager _deviceRepoMgr;
        IProductManager _productManager;
        IVerifierManager _verifierMgr;
        ISimulatorManager _simulatorMgr;
        IDeviceManager _deviceManager;
        IOrganizationRepo _orgRepo;
        IUserManager _userManager;
        StorageUtils _storageUtils;

        public DataServicesManager(IAdminLogger logger, IStarterKitConnection starterKitConnection, IDeviceAdminManager deviceAdminMgr, ISubscriptionManager subscriptionMgr, IPipelineModuleManager pipelineMgr, IDeviceTypeManager deviceTypeMgr, IDeviceRepositoryManager deviceRepoMgr,
                          IUserManager userManager, IProductManager productManager, IDeviceTypeManager deviceTypeManager, IDeviceConfigurationManager deviceCfgMgr, IDeviceMessageDefinitionManager deviceMsgMgr, IDeploymentInstanceManager instanceMgr,
                          IDeploymentHostManager hostMgr, IDeviceManager deviceManager, IContainerRepositoryManager containerMgr, ISolutionManager solutionMgr, IOrganizationRepo orgMgr, ISimulatorManager simMgr, IVerifierManager verifierMgr,
                          IAppConfig appConfig, IDependencyManager dependencyManager, ISecurity security) 
            : base(logger, appConfig, dependencyManager, security)
        {
            _userManager = userManager;
            _deviceAdminMgr = deviceAdminMgr;
            _subscriptionMgr = subscriptionMgr;
            _pipelineMgr = pipelineMgr;
            _deviceTypeMgr = deviceTypeMgr;
            _deviceMsgMgr = deviceMsgMgr;
            _deviceCfgMgr = deviceCfgMgr;
            _deviceRepoMgr = deviceRepoMgr;
            _productManager = productManager;
            _verifierMgr = verifierMgr;
            _deviceTypeMgr = deviceTypeManager;
            _simulatorMgr = simMgr;
            _orgRepo = orgMgr;
            _deviceManager = deviceManager;
            _hostManager = hostMgr;

            _instanceMgr = instanceMgr;
            _solutionMgr = solutionMgr;

            _storageUtils = new StorageUtils(new Uri(starterKitConnection.StarterKitStorage.Uri), starterKitConnection.StarterKitStorage.AccessKey,
                starterKitConnection.StarterKitStorage.ResourceName, logger);

        }

        public async Task<List<EntityHeader>> GetAllObjectsOfType(string typeName, EntityHeader org, EntityHeader user)
        {
            var results = await _storageUtils.FindByTypeAsync<EntityBase>(typeName, org);
            return results.Select(res => res.ToEntityHeader()).ToList();
        }
    }
}

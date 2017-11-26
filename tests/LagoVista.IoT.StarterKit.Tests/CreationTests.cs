using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.IoT.Billing;
using LagoVista.IoT.Deployment.Admin;
using LagoVista.IoT.Deployment.Admin.Managers;
using LagoVista.IoT.Deployment.Admin.Repos;
using LagoVista.IoT.DeviceAdmin.Interfaces.Managers;
using LagoVista.IoT.DeviceManagement.Core.Managers;
using LagoVista.IoT.DeviceMessaging.Admin.Managers;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.Pipeline.Admin.Managers;
using LagoVista.IoT.StarterKit.Managers;
using LagoVista.IoT.Verifiers.Managers;
using LagoVista.UserAdmin.Interfaces.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace LagoVista.IoT.StarterKit.Tests
{
    [TestClass]
    public class CreationTests
    {
        AppFactory _factory;
        Mock<IProductManager> _productManager = new Mock<IProductManager>();

        [TestInitialize]
        public void Init()
        {
            _factory = new AppFactory(new Mock<IDeviceAdminManager>().Object, new Mock<ISubscriptionManager>().Object, new Mock<IPipelineModuleManager>().Object, new Mock<IDeviceTypeManager>().Object,
                new Mock<IDeviceRepositoryManager>().Object, _productManager.Object, new Mock<IDeviceConfigurationManager>().Object, new Mock<IDeviceMessageDefinitionManager>().Object,
                new Mock<IDeploymentHostManager>().Object, new Mock<IDeploymentInstanceManager>().Object, new Mock<ISolutionManager>().Object, new Mock<IVerifierManager>().Object);
        }

        [TestMethod]
        public async Task CreateAppAsync()
        {
            var orgEh = new EntityHeader() { Id = "53D96B22822945DB8FFDF0BF6FBCB00D", Text = "The Orgs Name" };
            var usrEh = new EntityHeader() { Id = "6A49E4465E364205AA23D8083D9C3C6D", Text = "The Users Name" };

            var solutionCreationResult = await _factory.CreateSimpleSolutionAsync(orgEh, usrEh);

            var solutionMgr = new SolutionManager(new Mock<ISolutionRepo>().Object, new Mock<IDeviceConfigurationManager>().Object, new Mock<IPipelineModuleManager>().Object,
            new Mock<ISolutionVersionRepo>().Object, new Mock<IAdminLogger>().Object, new Mock<IAppConfig>().Object, new Mock<IDependencyManager>().Object, new Mock<ISecurity>().Object);

            var result = solutionMgr.ValidateSolution(solutionCreationResult.Result);
            foreach (var err in result.Errors)
            {
                Console.WriteLine(err.Message + " " + err.Details);
            }

            Assert.IsTrue(result.Successful);
        }

    }
}

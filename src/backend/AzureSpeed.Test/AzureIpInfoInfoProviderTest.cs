using System.Threading.Tasks;
using AzureSpeed.ApiService.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AzureSpeed.Test
{
    [TestClass]
    public class AzureIPInfoInfoProviderTest
    {
        [TestMethod]
        public async Task TestLookupPublicAzureIPByIPAddress()
        {
            var mockLogger = new Mock<ILogger<AzureIPInfoProvider>>();
            var provider = new AzureIPInfoProvider(mockLogger.Object);

            var result = await provider.GetAzureIPInfo("104.45.231.79");
            Assert.AreEqual("AppService.WestUS", result.ServiceTagId);
            Assert.AreEqual("104.45.231.79", result.IPAddress);
            Assert.AreEqual("104.45.231.79/32", result.IPAddressPrefix);
            Assert.AreEqual("westus", result.Region);
            Assert.AreEqual("AzureAppService", result.SystemService);
        }

        [TestMethod]
        public async Task TestLookupPublicAzureIPByUrl()
        {
            var mock = new Mock<ILogger<AzureIPInfoProvider>>();
            var provider = new AzureIPInfoProvider(mock.Object);

            var result = await provider.GetAzureIPInfo("https://www.azurespeed.com");
            Assert.AreEqual("AppService.WestUS", result.ServiceTagId);
            Assert.AreEqual("104.45.231.79", result.IPAddress);
            Assert.AreEqual("104.45.231.79/32", result.IPAddressPrefix);
            Assert.AreEqual("westus", result.Region);
            Assert.AreEqual("AzureAppService", result.SystemService);
        }

        [TestMethod]
        public async Task TestLookupAzureChinaCloudIPByIPAddress()
        {
            var mock = new Mock<ILogger<AzureIPInfoProvider>>();
            var provider = new AzureIPInfoProvider(mock.Object);

            var result = await provider.GetAzureIPInfo("139.217.8.104");
            Assert.AreEqual("AppService.ChinaNorth", result.ServiceTagId);
            Assert.AreEqual("139.217.8.104", result.IPAddress);
            Assert.AreEqual("139.217.8.104/32", result.IPAddressPrefix);
            Assert.AreEqual("chinanorth", result.Region);
            Assert.AreEqual("AzureAppService", result.SystemService);
        }

        [TestMethod]
        public async Task TestLookupAzureChinaCloudIPByUrl()
        {
            var mock = new Mock<ILogger<AzureIPInfoProvider>>();
            var provider = new AzureIPInfoProvider(mock.Object);

            var result = await provider.GetAzureIPInfo("http://www.azure.cn");
            Assert.AreEqual("AppService.ChinaNorth", result.ServiceTagId);
            Assert.AreEqual("139.217.8.104", result.IPAddress);
            Assert.AreEqual("139.217.8.104/32", result.IPAddressPrefix);
            Assert.AreEqual("chinanorth", result.Region);
            Assert.AreEqual("AzureAppService", result.SystemService);
        }

        [TestMethod]
        public async Task TestLookupInvalidIPAddress()
        {
            var mock = new Mock<ILogger<AzureIPInfoProvider>>();
            var provider = new AzureIPInfoProvider(mock.Object);

            var result = await provider.GetAzureIPInfo("1.1.1.1");
            Assert.AreEqual(null, result.ServiceTagId);
            Assert.AreEqual(null, result.IPAddress);
            Assert.AreEqual(null, result.IPAddressPrefix);
            Assert.AreEqual(null, result.Region);
            Assert.AreEqual(null, result.SystemService);
        }
    }
}

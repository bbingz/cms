using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SSCMS.Enums;
using SSCMS.Models;
using SSCMS.Repositories;
using SSCMS.Services;
using SSCMS.Web.Controllers.Stl;
using Xunit;

namespace SSCMS.Web.Tests.Controllers.Stl
{
    public class ActionsTriggerControllerTests
    {
        [Fact]
        public async Task GetRejectsUnsignedTriggerRequests()
        {
            var createManager = new Mock<ICreateManager>();
            var siteRepository = new Mock<ISiteRepository>();
            siteRepository.Setup(x => x.GetAsync(1)).ReturnsAsync(new Site
            {
                Id = 1
            });

            var controller = new ActionsTriggerController(
                createManager.Object,
                Mock.Of<IPathManager>(),
                siteRepository.Object,
                Mock.Of<IChannelRepository>(),
                Mock.Of<IContentRepository>(),
                Mock.Of<ISettingsManager>());

            var result = await controller.Get(new ActionsTriggerController.GetRequest
            {
                SiteId = 1
            });

            Assert.IsType<UnauthorizedResult>(result);
            createManager.Verify(x => x.ExecuteAsync(
                    It.IsAny<int>(),
                    It.IsAny<CreateType>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task GetAllowsSignedTriggerRequests()
        {
            var createManager = new Mock<ICreateManager>();
            var pathManager = new Mock<IPathManager>();
            pathManager.Setup(x => x.GetIndexPageUrlAsync(It.IsAny<Site>(), false)).ReturnsAsync("/");

            var siteRepository = new Mock<ISiteRepository>();
            siteRepository.Setup(x => x.GetAsync(1)).ReturnsAsync(new Site
            {
                Id = 1
            });

            var settingsManager = new Mock<ISettingsManager>();
            settingsManager
                .Setup(x => x.Decrypt("signed", null))
                .Returns("1:1:0:0:0:False");

            var controller = new ActionsTriggerController(
                createManager.Object,
                pathManager.Object,
                siteRepository.Object,
                Mock.Of<IChannelRepository>(),
                Mock.Of<IContentRepository>(),
                settingsManager.Object);

            var result = await controller.Get(new ActionsTriggerController.GetRequest
            {
                SiteId = 1,
                ChannelId = 1,
                Token = "signed"
            });

            Assert.IsType<RedirectResult>(result);
            createManager.Verify(x => x.ExecuteAsync(1, CreateType.Channel, 1, 0, 0, 0), Times.Once);
        }
    }
}

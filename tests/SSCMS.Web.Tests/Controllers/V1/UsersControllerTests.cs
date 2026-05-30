using System.Collections.Generic;
using System.Threading.Tasks;
using CacheManager.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SSCMS.Repositories;
using SSCMS.Services;
using SSCMS.Web.Controllers.V1;
using Xunit;

namespace SSCMS.Web.Tests.Controllers.V1
{
    public class UsersControllerTests
    {
        [Fact]
        public async Task LoginRateLimitsRepeatedFailures()
        {
            var userRepository = new Mock<IUserRepository>();
            userRepository
                .Setup(x => x.ValidateAsync("user", "bad-password", true))
                .ReturnsAsync((null, "user", "invalid"));

            var controller = new UsersController(
                Mock.Of<IAuthManager>(),
                Mock.Of<IPathManager>(),
                Mock.Of<IConfigRepository>(),
                Mock.Of<IAccessTokenRepository>(),
                userRepository.Object,
                Mock.Of<ILogRepository>(),
                Mock.Of<IStatRepository>(),
                Mock.Of<IDbCacheRepository>(),
                Mock.Of<IUserGroupRepository>(),
                Mock.Of<IUsersInGroupsRepository>(),
                new TestCacheManager())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            ActionResult lastResult = null;
            for (var i = 0; i < 11; i++)
            {
                var result = await controller.Login(new UsersController.LoginRequest
                {
                    Account = "user",
                    Password = "bad-password"
                });
                lastResult = result.Result;
            }

            Assert.IsType<BadRequestObjectResult>(lastResult);
            userRepository.Verify(x => x.ValidateAsync("user", "bad-password", true), Times.Exactly(10));
        }

        private class TestCacheManager : ICacheManager
        {
            private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

            public IReadOnlyCacheManagerConfiguration Configuration => null;

            public T Get<T>(string key)
            {
                return _cache.TryGetValue(key, out var value) ? (T)value : default;
            }

            public string GetByFilePath(string filePath)
            {
                return string.Empty;
            }

            public bool Exists(string key)
            {
                return _cache.ContainsKey(key);
            }

            public void AddOrUpdateSliding<T>(string key, T value, int minutes)
            {
                _cache[key] = value;
            }

            public void AddOrUpdateAbsolute<T>(string key, T value, int minutes)
            {
                _cache[key] = value;
            }

            public void AddOrUpdate<T>(string key, T value)
            {
                _cache[key] = value;
            }

            public void Remove(string key)
            {
                _cache.Remove(key);
            }

            public void Clear()
            {
                _cache.Clear();
            }
        }
    }
}

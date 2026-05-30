using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SSCMS.Configuration;
using SSCMS.Dto;
using SSCMS.Repositories;
using SSCMS.Services;

namespace SSCMS.Web.Controllers.Stl
{
    [OpenApiIgnore]
    [Route(Constants.ApiPrefix + Constants.ApiStlPrefix)]
    public partial class ActionsTriggerController : ControllerBase
    {
        private readonly ICreateManager _createManager;
        private readonly IPathManager _pathManager;
        private readonly ISiteRepository _siteRepository;
        private readonly IChannelRepository _channelRepository;
        private readonly IContentRepository _contentRepository;
        private readonly ISettingsManager _settingsManager;

        public ActionsTriggerController(ICreateManager createManager, IPathManager pathManager, ISiteRepository siteRepository, IChannelRepository channelRepository, IContentRepository contentRepository, ISettingsManager settingsManager)
        {
            _createManager = createManager;
            _pathManager = pathManager;
            _siteRepository = siteRepository;
            _channelRepository = channelRepository;
            _contentRepository = contentRepository;
            _settingsManager = settingsManager;
        }

        public class GetRequest : ChannelRequest
        {
            public int ContentId { get; set; }
            public int FileTemplateId { get; set; }
            public int SpecialId { get; set; }
            public bool IsRedirect { get; set; }
            public string ReturnUrl { get; set; }
            public string Token { get; set; }
        }

        private static string GetTriggerTokenPayload(int siteId, int channelId, int contentId, int fileTemplateId, int specialId, bool isRedirect)
        {
            return $"{siteId}:{channelId}:{contentId}:{fileTemplateId}:{specialId}:{isRedirect}";
        }

        public static string GetTriggerTokenPayload(GetRequest request)
        {
            return request == null
                ? string.Empty
                : GetTriggerTokenPayload(request.SiteId, request.ChannelId, request.ContentId, request.FileTemplateId, request.SpecialId, request.IsRedirect);
        }

        private bool IsValidTriggerToken(GetRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Token)) return false;

            try
            {
                return _settingsManager.Decrypt(request.Token) == GetTriggerTokenPayload(request);
            }
            catch
            {
                return false;
            }
        }
    }
}

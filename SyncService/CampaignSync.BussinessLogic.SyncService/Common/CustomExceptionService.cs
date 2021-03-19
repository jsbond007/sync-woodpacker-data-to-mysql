using Microsoft.Extensions.Logging;
using System;


namespace CampaignSync.BussinessLogic.SyncService.Common
{
    public class CustomExceptionService : ICustomExceptionService
    {
        private ILogger _logger;
        public CustomExceptionService(ILogger<CustomExceptionService> logger)
        {
            _logger = logger;
        }

        public string LogException(Exception ex)
        {
            _logger.LogTrace(ex.StackTrace);
            _logger.LogInformation("Exception : " + ex.Message);
            var exception = ex.Message + "\n" + ex.StackTrace;
            if (ex.InnerException != null)
            {
                _logger.LogInformation(ex.InnerException.Message);
                exception = exception + "\n" + ex.InnerException.Message;
            }
            return exception;
        }
    }
    public interface ICustomExceptionService
    {
        string LogException(Exception ex);
    }
}

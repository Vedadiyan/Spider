using System;
using System.Threading.Tasks;
using Spider.ArcheType;
using Spider.WebAPI.Abstraction;

namespace Spider.WebAPI.Handlers
{
    public class RequestHandler : IRequest
    {
        private Func<Services, IRequestContext, IActionResult> actionDelegate;
        private Func<Services, IRequestContext, Task<IActionResult>> actionDelegateAsync;
        public RequestHandler(Func<Services, IRequestContext, IActionResult> actionDelegate)
        {
            this.actionDelegate = actionDelegate;
        }
        public RequestHandler(Func<Services, IRequestContext, Task<IActionResult>> actionDelegateAsync)
        {
            this.actionDelegateAsync = actionDelegateAsync;
        }
        public async Task<IResponse> HandleRequest(IRequestContext context)
        {
            if (actionDelegate != null)
            {
                try
                {
                    IActionResult result = actionDelegate(Services.GetServices(), context);
                    return new ResponseHandler(result.StatusCode, result.ContentType, result.Response());
                }
                catch (Exception ex)
                {
                    return new ResponseHandler(System.Net.HttpStatusCode.InternalServerError, "text/plain", ex.Message);
                }
            }
            else
            {
                try
                {
                    IActionResult result = await actionDelegateAsync(Services.GetServices(), context);
                    return new ResponseHandler(result.StatusCode, result.ContentType, result.Response());
                }
                catch (Exception ex)
                {
                    return new ResponseHandler(System.Net.HttpStatusCode.InternalServerError, "text/plain", ex.Message);
                }
            }
        }

        public void Suspend()
        {
            throw new NotImplementedException();
        }
    }

}
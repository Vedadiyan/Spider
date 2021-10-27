using System.Threading.Tasks;

namespace Spider.ArcheType {
    public interface IRequest
    {
        void Suspend();
        Task<IResponse> HandleRequest(IRequestContext context);
    }
}
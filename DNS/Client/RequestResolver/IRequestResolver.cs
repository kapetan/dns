using System.Threading.Tasks;
using DNS.Protocol;

namespace DNS.Client.RequestResolver {
    public interface IRequestResolver {
        Task<IResponse> Request(IRequest request);
    }
}

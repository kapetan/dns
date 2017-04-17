using System.Threading.Tasks;

namespace DNS.Client.RequestResolver {
    public interface IRequestResolver {
        Task<ClientResponse> Request(ClientRequest request);
    }
}

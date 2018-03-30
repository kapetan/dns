using System.Threading.Tasks;
using DNS.Protocol;
using DNS.Client.RequestResolver;

namespace DNS.Server {
    public class FallbackRequestResolver : IRequestResolver {
        private IRequestResolver[] resolvers;

        public FallbackRequestResolver(params IRequestResolver[] resolvers) {
            this.resolvers = resolvers;
        }

        public async Task<IResponse> Resolve(IRequest request) {
            IResponse response = null;

            foreach (IRequestResolver resolver in resolvers) {
                response = await resolver.Resolve(request);
                if (response.AnswerRecords.Count > 0) break;
            }

            return response;
        }
    }
}

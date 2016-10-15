namespace DNS.Client.RequestResolver {
    public interface IRequestResolver {
        ClientResponse Request(ClientRequest request);
#if NET45
        Task<ClientResponse> RequestAsync(ClientRequest request);
#endif
    }
}

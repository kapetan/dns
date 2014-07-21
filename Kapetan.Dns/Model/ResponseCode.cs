namespace Kapetan.Dns.Model
{
    public enum ResponseCode
    {
        NoError = 0,
        FormatError,
        ServerFailure,
        NameError,
        NotImplemented,
        Refused,
        YXDomain,
        YXRRSet,
        NXRRSet,
        NotAuth,
        NotZone,
    }
}

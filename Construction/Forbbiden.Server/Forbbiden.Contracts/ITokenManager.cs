using System.Runtime.Serialization;
using System.ServiceModel;

namespace Forbbiden.Contracts
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ITokenManager" in both code and config file together.
    [ServiceContract]
    public interface ITokenManager
    {
        [OperationContract]
        Token GenerateToken(int playerId);

        [OperationContract]
        Token GetToken(int playerId);

        [OperationContract]
        bool VerifyToken(string token, int playerId);
    }

    [DataContract]
    public class Token
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string TokenString { get; set; }
        [DataMember]
        public int PlayerId { get; set; }
    }
}

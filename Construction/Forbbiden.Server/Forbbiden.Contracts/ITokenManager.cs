using System.Runtime.Serialization;
using System.ServiceModel;

namespace Forbbiden.Contracts
{
    [ServiceContract]
    public interface ITokenManager
    {
        [OperationContract]
        [FaultContract(typeof(Fault))]
        string CreateRandomToken();

        [OperationContract]
        [FaultContract(typeof(Fault))]
        Token GenerateToken(int playerId);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        Token GetToken(int playerId);

        [OperationContract]
        [FaultContract(typeof(Fault))]
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

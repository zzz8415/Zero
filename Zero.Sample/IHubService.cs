using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Zero.Sample
{
    [ServiceContract]
    public interface IHubService
    {
        [OperationContract]
        void Logout();

    }
}

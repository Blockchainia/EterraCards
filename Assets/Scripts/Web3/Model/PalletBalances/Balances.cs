using SoloNode.NetApiExt.Generated.Model.solochain_template_runtime;
using SoloNode.NetApiExt.Generated.Model.sp_core.crypto;
using SoloNode.NetApiExt.Generated.Model.sp_runtime.multiaddress;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System.Numerics;

namespace SoloNode.Integration.Model
{
    public class Balances
    {
        /// <summary>
        /// Balances transfer keep alive
        /// </summary>
        /// <param name="target"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static EnumRuntimeCall TransferKeepAlive(AccountId32 target, BigInteger amount)
        {
            var baseU128 = new BaseCom<U128>();
            baseU128.Create(amount);

            var multiAddress = new EnumMultiAddress();
            multiAddress.Create(MultiAddress.Id, target);

            var baseTubleParams = new BaseTuple<EnumMultiAddress, BaseCom<U128>>();
            baseTubleParams.Create(multiAddress, baseU128);

            var enumPalletCall = new NetApiExt.Generated.Model.pallet_balances.pallet.EnumCall();
            enumPalletCall.Create(NetApiExt.Generated.Model.pallet_balances.pallet.Call.transfer_keep_alive, baseTubleParams);

            var enumCall = new EnumRuntimeCall();
            enumCall.Create(RuntimeCall.Balances, enumPalletCall);

            return enumCall;
        }
    }
}

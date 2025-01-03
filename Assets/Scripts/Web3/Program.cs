using Serilog;
using SoloNode.NetApiExt.Helper;
using System;
using SoloNode.Integration.Model;
using SoloNode.NetApiExt.Generated.Model.solochain_template_runtime;
using SoloNode.NetApiExt.Generated.Model.sp_core.crypto;
using SoloNode.NetApiExt.Generated.Model.sp_runtime.multiaddress;
using SoloNode.NetApiExt.Generated.Storage;
using SoloNode.NetApiExt.Helper;
using Substrate.NetApi;
using Substrate.NetApi.Model.Types;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System.Numerics;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
  internal class Program : Singleton<BalanceTransfer>
  {
    protected override void Awake()
    {
      base.Awake();
      MainAsync();
    }
    private static async Task MainAsync()
    {
      string nodeUrl = "ws://127.0.0.1:9944";

      var aliceAccount = SoloNode.NetApiExt.Client.BaseClient.Alice.ToAccountId32();
      var bertaAccount = "5DAAnrj7VHTznn2AWBemMuyBwZWs6FNFjdyVXUeYum3PTXFy".ToAccountId32();

      var client = new SubstrateNetwork(SoloNode.NetApiExt.Client.BaseClient.Alice, nodeUrl);

      client.ExtrinsicManager.ExtrinsicUpdated += (id, info) =>
      {
        Log.Information("ExtrinsicUpdated: {id} | {event}", id, info.TransactionEvent);
      };

      await client.ConnectAsync(true, true, CancellationToken.None);

      Log.Information("Connected to {NodeUrl} = {state}", nodeUrl, client.IsConnected);

      var accountInfo = await client.GetAccountAsync(aliceAccount, CancellationToken.None);
      if (accountInfo == null)
      {
        Log.Warning("Alice AccountInfo is null");
        return;
      }

      Log.Information("[ALICE] Free: {free} | Frozen: {frozen} | Reserved: {reserved}", accountInfo.Data.Free, accountInfo.Data.Frozen, accountInfo.Data.Reserved);

      var bertaAccountInfo = await client.GetAccountAsync(bertaAccount, CancellationToken.None);
      if (bertaAccountInfo == null)
      {
        Log.Information("Dave account currently doesn't exist!");
      }
      else
      {
        Log.Information("[DAVE] Free: {free} | Frozen: {frozen} | Reserved: {reserved}", bertaAccountInfo.Data.Free, bertaAccountInfo.Data.Frozen, bertaAccountInfo.Data.Reserved);
      }

      var subscriptionId = await client.TransferKeepAliveAsync(bertaAccount, 1000000000000000, 1, CancellationToken.None);
      if (subscriptionId == null)
      {
        Log.Warning("TransferKeepAlive subscription failed");
        return;
      }

      Log.Information("TransferKeepAlive subscriptionId: {subscriptionId}", subscriptionId);

      Console.WriteLine("Press any key to stop waiting ...");
      Console.ReadKey();

      bertaAccountInfo = await client.GetAccountAsync(bertaAccount, CancellationToken.None);
      if (bertaAccountInfo == null)
      {
        Log.Information("Dave account currently doesn't exist!");
      }
      else
      {
        Log.Information("[DAVE] Free: {free} | Frozen: {frozen} | Reserved: {reserved}", bertaAccountInfo.Data.Free, bertaAccountInfo.Data.Frozen, bertaAccountInfo.Data.Reserved);
      }

      await client.DisconnectAsync();

      Log.Information("Disconnected");
    }
  }
}

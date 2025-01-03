using Substrate.NetApi.Model.Extrinsics;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using Substrate.NET.Schnorrkel.Keys;
using Substrate.NetApi;
using Substrate.NetApi.Model.Rpc;
using Substrate.NetApi.Model.Types;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using System;
using SoloNode.NetApiExt.Generated.Model.sp_core.crypto;
using SoloNode.NetApiExt.Generated.Model.sp_runtime.multiaddress;
using SoloNode.NetApiExt.Generated.Storage;
using SoloNode.NetApiExt.Generated;
using SoloNode.NetApiExt.Helper;
using UnityEditor.Localization.Plugins.XLIFF.V20;

namespace Assets.Scripts
{
  public class BalanceTransfer : Singleton<BalanceTransfer>
  {
    private static string NodeUrl = "ws://127.0.0.1:9944";
    private static SubstrateNetwork client;

    #region Account Initialization

    public static MiniSecret MiniSecretAlice => new MiniSecret(
        Utils.HexToByteArray("0xe5be9a5092b81bca64be81d212e7f2f9eba183bb7a90954f7b76361f6edb5c0a"),
        ExpandMode.Ed25519);

    public static Account Alice => Account.Build(KeyType.Sr25519, MiniSecretAlice.ExpandToSecret().ToBytes(),
        MiniSecretAlice.GetPair().Public.Key);

    public static MiniSecret MiniSecretBob => new MiniSecret(
        Utils.HexToByteArray("0x398f0c28f98885e046333d4a41c19cee4c37368a9832c6502f6cfd182e2aef89"),
        ExpandMode.Ed25519);

    public static Account Bob => Account.Build(KeyType.Sr25519, MiniSecretBob.ExpandToSecret().ToBytes(),
        MiniSecretBob.GetPair().Public.Key);
    public AccountId32 aliceAccount = SoloNode.NetApiExt.Client.BaseClient.Alice.ToAccountId32();
    public AccountId32 bertaAccount = "5DAAnrj7VHTznn2AWBemMuyBwZWs6FNFjdyVXUeYum3PTXFy".ToAccountId32();
    #endregion

    #region Awake and Initialization

    protected override void Awake()
    {
      base.Awake();
      InitializeClient(NodeUrl);
    }

    #region Client Initialization and Connection

    private static SubstrateNetwork InitializeClient(string nodeUrl)
    {
      client = new SubstrateNetwork(SoloNode.NetApiExt.Client.BaseClient.Alice, nodeUrl);

      client.ExtrinsicManager.ExtrinsicUpdated += (id, info) =>
      {
        Debug.Log("ExtrinsicUpdated: " + id + " | " + info.TransactionEvent);
      };
      ConnectClientAsync(client, nodeUrl);
      return client;
    }

    private static async Task ConnectClientAsync(SubstrateNetwork client, string nodeUrl)
    {
      await client.ConnectAsync(true, true, CancellationToken.None);

      Debug.Log("Connected to " + nodeUrl + " = " + client.IsConnected);
    }

    private static async Task DisconnectClientAsync(SubstrateNetwork client)
    {
      await client.DisconnectAsync();
      Debug.Log("Disconnected");
    }

    #endregion
    private static string GetClientConnectionStatus(SubstrateNetwork client)
    {
      return client.IsConnected ? "Connected" : "Not connected";
    }

    #endregion

    #region Submit Transfer

    public async void ButtonClickTransfer()
    {
      await SubmitTransfer(client);
    }

    #endregion

    #region Client Validation and Connection

    private static bool ValidateClient(SubstrateNetwork client)
    {
      if (client == null)
      {
        Debug.LogError("Client is null!");
        return false;
      }
      if (!client.IsConnected)
      {
        Debug.LogError("Client is not connected!");
      }
      return true;
    }

    public static async Task SubmitTransfer(SubstrateNetwork client)
    {
      if (!ValidateClient(client)) return;
      var bertaAccount = "5DAAnrj7VHTznn2AWBemMuyBwZWs6FNFjdyVXUeYum3PTXFy".ToAccountId32();
      var bertaAccountInfo = await client.GetAccountAsync(bertaAccount, CancellationToken.None);
      if (bertaAccountInfo == null)
      {
        Debug.Log("Dave account currently doesn't exist!");
      }
      else
      {
        Debug.Log("[DAVE] Free: " + bertaAccountInfo.Data.Free + " | Frozen: " + bertaAccountInfo.Data.Frozen + " | Reserved: " + bertaAccountInfo.Data.Reserved);
      }
      var subscriptionId = await client.TransferKeepAliveAsync(bertaAccount, 1000000000000000, 1, CancellationToken.None);
      if (subscriptionId == null)
      {
        Debug.LogWarning("TransferKeepAlive subscription failed");
        return;
      }

      Debug.Log("TransferKeepAlive subscriptionId: " + subscriptionId);

      Console.WriteLine("Press any key to stop waiting ...");
      Console.ReadKey();

      bertaAccountInfo = await client.GetAccountAsync(bertaAccount, CancellationToken.None);
      if (bertaAccountInfo == null)
      {
        Debug.Log("Dave account currently doesn't exist!");
      }
      else
      {
        Debug.Log("[DAVE] Free: " + bertaAccountInfo.Data.Free + " | Frozen: " + bertaAccountInfo.Data.Frozen + " | Reserved: " + bertaAccountInfo.Data.Reserved);
      }
    }

    #endregion

    #region Account and MultiAddress Initialization

    private static (AccountId32, AccountId32) InitializeAccounts()
    {
      var accountAlice = new AccountId32();
      accountAlice.Create(Utils.GetPublicKeyFrom(Alice.Value));

      var accountBob = new AccountId32();
      accountBob.Create(Utils.GetPublicKeyFrom(Bob.Value));

      return (accountAlice, accountBob);
    }

    private static void LogPublicKeys()
    {
      Debug.Log($"Alice Public Key: {BitConverter.ToString(Utils.GetPublicKeyFrom(Alice.Value)).Replace("-", "")}");
      Debug.Log($"Bob Public Key: {BitConverter.ToString(Utils.GetPublicKeyFrom(Bob.Value)).Replace("-", "")}");
    }

    private static (EnumMultiAddress, EnumMultiAddress) CreateMultiAddresses(AccountId32 accountAlice, AccountId32 accountBob)
    {
      var multiAddressAlice = new EnumMultiAddress();
      multiAddressAlice.Create(MultiAddress.Id, accountAlice);

      var multiAddressBob = new EnumMultiAddress();
      multiAddressBob.Create(MultiAddress.Id, accountBob);

      return (multiAddressAlice, multiAddressBob);
    }

    #endregion
  }
}
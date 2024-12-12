using Substrate.NetApi.Model.Extrinsics;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using Schnorrkel.Keys;
using Substrate.Polkadot.NET.NetApiExt.Generated.Model.sp_core.crypto;
using Substrate.Polkadot.NET.NetApiExt.Generated.Model.sp_runtime.multiaddress;
using Substrate.Polkadot.NET.NetApiExt.Generated.Storage;
using Substrate.NetApi;
using Substrate.NetApi.Model.Rpc;
using Substrate.NetApi.Model.Types;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using System;
using LocalExt = Substrate.Polkadot.NET.NetApiExt.Generated;

namespace Eterraverse.DirectBalanceTransfer
{
  public class BalanceTransfer : Singleton<BalanceTransfer>
  {
    // Secret Key URI `//Alice` is account:
    // Secret seed:      0xe5be9a5092b81bca64be81d212e7f2f9eba183bb7a90954f7b76361f6edb5c0a
    // Public key(hex):  0xd43593c715fdd31c61141abd04a99fd6822c8558854ccde39a5684e7a56da27d
    // Account ID:       0xd43593c715fdd31c61141abd04a99fd6822c8558854ccde39a5684e7a56da27d
    // SS58 Address:     5GrwvaEF5zXb26Fz9rcQpDWS57CtERHpNehXCPcNoHGKutQY
    public static MiniSecret MiniSecretAlice => new MiniSecret(
        Utils.HexToByteArray("0xe5be9a5092b81bca64be81d212e7f2f9eba183bb7a90954f7b76361f6edb5c0a"),
        ExpandMode.Ed25519);

    public static Account Alice => Account.Build(KeyType.Sr25519, MiniSecretAlice.ExpandToSecret().ToBytes(),
        MiniSecretAlice.GetPair().Public.Key);

    // Secret Key URI `//Bob` is account:
    // Secret seed:      0x398f0c28f98885e046333d4a41c19cee4c37368a9832c6502f6cfd182e2aef89
    // Public key(hex):  0x8eaf04151687736326c9fea17e25fc5287613693c912909cb226aa4794f26a48
    // Account ID:       0x8eaf04151687736326c9fea17e25fc5287613693c912909cb226aa4794f26a48
    // SS58 Address:     5FHneW46xGXgs5mUiveU4sbTyGBzmstUspZC92UhjJM694ty
    public static MiniSecret MiniSecretBob => new MiniSecret(
        Utils.HexToByteArray("0x398f0c28f98885e046333d4a41c19cee4c37368a9832c6502f6cfd182e2aef89"),
        ExpandMode.Ed25519);

    public static Account Bob => Account.Build(KeyType.Sr25519, MiniSecretBob.ExpandToSecret().ToBytes(),
        MiniSecretBob.GetPair().Public.Key);

    private static string NodeUrl = "ws://127.0.0.1:9944";

    public static async Task Main(string[] args)
    {
      // Instantiate the client and connect to the Node
      LocalExt.SubstrateClientExt client = await InstantiateClientAndConnectAsync();

      if (!client.IsConnected)
      {
        Debug.Log("Client Not Connected!");
        return;
      }
      Debug.Log("Client status: " + client.IsConnected);

      await SubmitTransfer(client);
    }

    protected override void Awake()
    {
      base.Awake();
      // Instantiate the client and connect to the Node
      InitializeClient();

    }

    public async void InitializeClient()
    {
      LocalExt.SubstrateClientExt client = await InstantiateClientAndConnectAsync();

      if (!client.IsConnected)
      {
        Debug.Log("Client Not Connected!");
        return;
      }
      Debug.Log("Client status: " + client.IsConnected);
    }
    public static async Task SubmitTransfer(LocalExt.SubstrateClientExt client)
    {
      var accountAlice = new AccountId32();
      accountAlice.Create(Utils.GetPublicKeyFrom(Alice.Value));

      var accountBob = new AccountId32();
      accountBob.Create(Utils.GetPublicKeyFrom(Bob.Value));

      // Get Alice's Balance
      var accountInfoAlice = await client.SystemStorage.Account(accountAlice, CancellationToken.None);
      Debug.Log("Alice Free Balance before transaction = " + accountInfoAlice.Data.Free.Value.ToString());

      // Get Bob's Balance
      var accountInfoBob = await client.SystemStorage.Account(accountBob, CancellationToken.None);
      Debug.Log("Bob Free Balance before transaction = {balance} = " + accountInfoBob.Data.Free.Value.ToString());

      // Instantiate a MultiAddress for Bob
      var multiAddressBob = new EnumMultiAddress();
      multiAddressBob.Create(MultiAddress.Id, accountBob);

      // Amount to be transferred
      var amount = new BaseCom<U128>();
      amount.Create(190000);

      // Create Extrinsic Method to be transmitted
      var extrinsicMethod =
          BalancesCalls.Transfer(multiAddressBob, amount);

      // Post Extrinsic Callback to show balance for both accounts
      Action<string, ExtrinsicStatus> actionExtrinsicUpdate = (subscriptionId, extrinsicUpdate) =>
          {
            // Fire only if state is Ready
            if (extrinsicUpdate.ExtrinsicState == ExtrinsicState.Ready)
            {
              Debug.Log("Firing post transfer Callback");

              client.SystemStorage.Account(accountAlice, CancellationToken.None).ContinueWith(
                        (task) =>
                            Debug.Log("Alice's Free Balance after transaction = " + task.Result.Data.Free.Value)

                        );


              client.SystemStorage.Account(accountBob, CancellationToken.None).ContinueWith(
                        (task) =>
                            Debug.Log("Bob's Free Balance after transaction = {balance} = " + task.Result.Data.Free.Value)

                    );
            }
          };

      // Alice to Bob Transaction
      await client.Author.SubmitAndWatchExtrinsicAsync(
          actionExtrinsicUpdate,
          extrinsicMethod,
          Alice, new ChargeAssetTxPayment(0, 0), 128, CancellationToken.None);


      // Console.ReadLine();
    }

    private static async Task<LocalExt.SubstrateClientExt> InstantiateClientAndConnectAsync()
    {
      // Instantiate the client
      var client = new LocalExt.SubstrateClientExt(new Uri(NodeUrl), ChargeTransactionPayment.Default());

      // Display Client Connection Status before connecting
      Debug.Log($"Client Connection Status: {GetClientConnectionStatus(client)}");

      await client.ConnectAsync();

      // Display Client Connection Status after connecting
      Debug.Log(client.IsConnected ? "Client connected successfully" : "Failed to connect to node. Exiting...");

      return client;
    }

    private static string GetClientConnectionStatus(SubstrateClient client)
    {
      return client.IsConnected ? "Connected" : "Not connected";
    }
  }
}
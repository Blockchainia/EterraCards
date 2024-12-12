using Schnorrkel.Keys;
using Substrate.Integration;
using Substrate.NET.Wallet;
using Substrate.NetApi;
using Substrate.NetApi.Model.Extrinsics;
using Substrate.NetApi.Model.Rpc;
using Substrate.NetApi.Model.Types;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using Substrate.Integration.Client;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using LocalExt = Substrate.Polkadot.NET.NetApiExt.Generated;
using System.Data;
using System.IO;
using System.Linq;
using Substrate.Integration.Helper;
using Eterraverse.DirectBalanceTransfer;
namespace Assets.Scripts
{

  public class Web3NetworkManager : Singleton<Web3NetworkManager>
  {
    public delegate void ConnectionStateChangedHandler(bool IsConnected);

    public delegate void ExtrinsicCheckHandler();

    public event ConnectionStateChangedHandler ConnectionStateChanged;

    public event ExtrinsicCheckHandler ExtrinsicCheck;

    public MiniSecret MiniSecretAlice => new MiniSecret(Utils.HexToByteArray("0xe5be9a5092b81bca64be81d212e7f2f9eba183bb7a90954f7b76361f6edb5c0a"), ExpandMode.Ed25519);
    public Account SudoAlice => Account.Build(KeyType.Sr25519, MiniSecretAlice.ExpandToSecret().ToBytes(), MiniSecretAlice.GetPair().Public.Key);

    public MiniSecret MiniSecretSudo => new MiniSecret(Utils.HexToByteArray(""), ExpandMode.Ed25519);
    public Account SudoHexalem => Account.Build(KeyType.Sr25519, MiniSecretSudo.ExpandToSecret().ToBytes(), MiniSecretSudo.GetPair().Public.Key);

    // Sudo account if needed
    public Account Sudo { get; private set; }

    private string _nodeUrl;

    private Label _lblNodeInfo;

    public string NodeUrl => _nodeUrl;

    private bool _running = false;

    private Func<CancellationToken, Task<Properties>> SystemProperties { get; set; }

    private readonly NetworkType _networkType = NetworkType.Live;

    public AccountType CurrentAccountType { get; private set; }
    public string CurrentAccountName { get; private set; }

    public NodeType CurrentNodeType { get; private set; }

    private SubstrateClient _client;
    public SubstrateClient Client => _client;

    private bool? _lastConnectionState = null;

    protected override void Awake()
    {
      base.Awake();
      //Your code goes here
      CurrentAccountType = AccountType.Alice;
      CurrentNodeType = NodeType.Local;
      Sudo = SudoAlice;
      _nodeUrl = "ws://127.0.0.1:9944";
      InitializeClient();
    }

    public void Start()
    {
      InvokeRepeating(nameof(UpdateNetworkState), 0.0f, 2.0f);
      InvokeRepeating(nameof(UpdatedExtrinsic), 0.0f, 3.0f);
    }

    private void OnDestroy()
    {
      CancelInvoke(nameof(UpdateNetworkState));
      CancelInvoke(nameof(UpdatedExtrinsic));
    }

    private void UpdateNetworkState()
    {
      if (_client == null)
      {
        return;
      }

      var connectionState = _client.IsConnected;
      SystemProperties = _client.System.PropertiesAsync;

      if (_lastConnectionState == null || _lastConnectionState != connectionState)
      {
        ConnectionStateChanged?.Invoke(connectionState);
        _lastConnectionState = connectionState;
      }
    }

    private void UpdatedExtrinsic()
    {
      ExtrinsicCheck?.Invoke();
    }

    public (Account, string) GetAccount(AccountType accountType, string custom = null)
    {
      Account result;
      string name;
      switch (accountType)
      {
        case AccountType.Alice:
        case AccountType.Bob:
        case AccountType.Charlie:
        case AccountType.Dave:
          name = accountType.ToString();
          result = BaseClient.RandomAccount(GameConstant.AccountSeed, accountType.ToString(), KeyType.Sr25519);
          break;

        case AccountType.Custom:
          name = custom.ToUpper();
          result = BaseClient.RandomAccount(GameConstant.AccountSeed, custom, KeyType.Sr25519);
          break;

        default:
          name = AccountType.Alice.ToString();
          result = BaseClient.RandomAccount(GameConstant.AccountSeed, AccountType.Alice.ToString(), KeyType.Sr25519);
          break;
      }

      return (result, name);
    }

    public bool SetAccount(AccountType accountType, string custom = null)
    {
      if (accountType == AccountType.Custom && (string.IsNullOrEmpty(custom) || custom.Length < 3))
      {
        return false;
      }

      CurrentAccountType = accountType;

      var tuple = GetAccount(accountType, custom);

      //Client.Account = tuple.Item1;
      CurrentAccountName = tuple.Item2;

      return true;
    }


    public List<Wallet> StoredWallets()
    {
      var result = new List<Wallet>();
      foreach (var w in WalletFiles())
      {
        if (!Wallet.Load(w, out Wallet wallet))
        {
          Debug.Log($"Failed to load wallet {w}");
        }

        result.Add(wallet);
      }
      return result;
    }

    private IEnumerable<string> WalletFiles()
    {
      var d = new DirectoryInfo(CachingManager.GetInstance().PersistentPath);
      return d.GetFiles(Wallet.ConcatWalletFileType("*")).Select(p => Path.GetFileNameWithoutExtension(p.Name));
    }

    /// <summary>
    /// Generic extrinsic method.
    /// </summary>
    /// <param name="extrinsicType"></param>
    /// <param name="extrinsicMethod"></param>
    /// <param name="concurrentTasks"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    internal async Task<string> GenericExtrinsicAsync(SubstrateClient client, Account account, Method extrinsicMethod, CancellationToken token)
    {
      string subscription = await client.Author.SubmitAndWatchExtrinsicAsync(ActionExtrinsicUpdate, extrinsicMethod, account, ChargeTransactionPayment.Default(), 64, token);

      if (subscription == null)
      {
        return null;
      }

      Debug.Log($"Generic extrinsic sent {extrinsicMethod.ModuleName}_{extrinsicMethod.CallName} with {subscription}");

      return subscription;
    }

    /// <summary>
    /// Callback for extrinsic updates.
    /// </summary>
    /// <param name="subscriptionId"></param>
    /// <param name="extrinsicUpdate"></param>
    public void ActionExtrinsicUpdate(string subscriptionId, ExtrinsicStatus extrinsicUpdate)
    {
      var broadcast = extrinsicUpdate.Broadcast != null ? string.Join(",", extrinsicUpdate.Broadcast) : "";
      var hash = extrinsicUpdate.Hash != null ? extrinsicUpdate.Hash.Value : "";

      Debug.Log($"{subscriptionId} => {extrinsicUpdate.ExtrinsicState} [HASH: {hash}] [BROADCAST: {broadcast}]");

      UnityMainThreadDispatcher.Instance().Enqueue(() =>
      {
        _lblNodeInfo.text = _lblNodeInfo.text + $"" +
              $"\n{subscriptionId}" +
              $"\n => {extrinsicUpdate.ExtrinsicState} {(hash.Length > 0 ? $"[{hash}]" : "")}{(broadcast.Length > 0 ? $"[{broadcast}]" : "")}";
      });
    }

    /// <summary>
    /// On transfer button clicked.
    /// </summary>
    public async void OnTransferClicked()
    {
      Debug.Log("Attempting Transfer.");

      if (_running)
      {
        Debug.LogWarning("Transfer is already running.");
        return;
      }

      if (_client == null || !_client.IsConnected)
      {
        Debug.Log("_client: " + _client.ToString());
        Debug.Log("isConnectedL: " + _client.IsConnected);
        Debug.LogError("Client is either null or not connected.");
        return;
      }

      _running = true;

      try
      {
        Debug.Log("Creating Alice account object.");
        var accountAlice = new LocalExt.Model.sp_core.crypto.AccountId32();
        accountAlice.Create(Utils.GetPublicKeyFrom(SudoAlice.Value));

        Debug.Log("Fetching system properties.");
        var properties = await SystemProperties(CancellationToken.None);
        Debug.Log($"System properties fetched: TokenDecimals = {properties.TokenDecimals}, TokenSymbol = {properties.TokenSymbol}");

        var tokenDecimals = BigInteger.Pow(10, properties.TokenDecimals);

        Debug.Log("Fetching Alice's account info.");
        var accountInfo = await ((LocalExt.SubstrateClientExt)_client).SystemStorage.Account(accountAlice, CancellationToken.None);

        if (accountInfo == null)
        {
          Debug.LogError("No account found for Alice!");
          _running = false;
          return;
        }

        Debug.Log($"Alice account balance: {BigInteger.Divide(accountInfo.Data.Free.Value, tokenDecimals)} {properties.TokenSymbol}");
        _lblNodeInfo.text = $"Alice account has: {BigInteger.Divide(accountInfo.Data.Free.Value, tokenDecimals)} {properties.TokenSymbol}";

        Debug.Log("Creating Bob's account object.");
        var account32 = new LocalExt.Model.sp_core.crypto.AccountId32();
        account32.Create(Utils.GetPublicKeyFrom("5FHneW46xGXgs5mUiveU4sbTyGBzmstUspZC92UhjJM694ty"));

        Debug.Log("Creating MultiAddress for Bob.");
        var multiAddress = new LocalExt.Model.sp_runtime.multiaddress.EnumMultiAddress();
        multiAddress.Create(LocalExt.Model.sp_runtime.multiaddress.MultiAddress.Id, account32);

        Debug.Log("Preparing transfer amount.");
        var amount = new BaseCom<U128>();
        amount.Create(BigInteger.Multiply(42, tokenDecimals));
        Debug.Log($"Prepared amount to transfer: {amount.Value.Value} {properties.TokenSymbol}");

        _lblNodeInfo.text = $"Sending Bob: {amount.Value.Value} {properties.TokenSymbol}";

        Debug.Log("Creating TransferKeepAlive call.");
        var transferKeepAlive = LocalExt.Storage.BalancesCalls.TransferKeepAlive(multiAddress, amount);

        Debug.Log("Sending transaction.");
        var subscription = await GenericExtrinsicAsync(_client, SudoAlice, transferKeepAlive, CancellationToken.None);

        Debug.Log($"Transaction subscription ID: {subscription}");
      }
      catch (Exception e)
      {
        Debug.LogError($"Error during transfer: {e.Message}");
      }
      finally
      {
        Debug.Log("Finished Transfer.");
        _running = false;
      }
    }


    // Start is called before the first frame update
    public void InitializeClient()
    {
      _client = new SubstrateClient(new Uri(_nodeUrl), ChargeTransactionPayment.Default());
      SetAccount(CurrentAccountType, CurrentAccountName);
    }
  }
}
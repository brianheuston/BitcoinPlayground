using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.Protocol;

namespace BitcoinPlayground
{
    class Program
    {
        public static Network Network = Network.TestNet;

        static void Main(string[] args)
        {
            BitcoinSecret secret = new BitcoinSecret("cTfANynUR2MCPC8uBpfo2yUfAzX5PucDNuXaJ8jXT8CpQDgvfUNr", Network);
            BitcoinAddress address = new BitcoinAddress("n2upyU7axtvHHhAPTMvdaRSkP5Mjufoffx", Network);

            BlockrTransactionRepository blockr = new BlockrTransactionRepository(Network);
            List<Coin> unspentCoins = blockr.GetUnspentAsync(secret.GetAddress().ToString(), true).Result;

            TransactionBuilder builder = new TransactionBuilder();
            builder.AddKeys(secret);
            builder.AddCoins(unspentCoins);
            builder.Send(address, Money.Coins(0.005m));
            builder.SendFees(Money.Coins(0.00001m));
            builder.SetChange(secret.ScriptPubKey);

            Transaction tx = builder.BuildTransaction(true);

            if (builder.Verify(tx))
                SendTransaction(tx);
        }

        public static void SendTransaction(Transaction tx)
        {
            AddressManager nodeParams = new AddressManager();
            IPEndPoint endPoint = TranslateHostNameToIP("http://btcnode.placefullcloud.com", 8333);
            nodeParams.Add(new NetworkAddress(endPoint), endPoint.Address);
            using (var node = Node.Connect(Network, nodeParams))
            {
                node.VersionHandshake();
                node.SendMessage(new InvPayload(InventoryType.MSG_TX, tx.GetHash()));
                node.SendMessage(new TxPayload(tx));
            }
        }

        public static IPEndPoint TranslateHostNameToIP(string hostname, int port)
        {
            IPAddress addr = IPAddress.Parse("52.26.132.227");
            IPEndPoint hostEndPoint = new IPEndPoint(addr, port);
            //IPEndPoint hostEndPoint = null;

            //IPHostEntry hostEntry = Dns.GetHostEntry(Constants.PLACEFULL_BTC_NODE_HOSTNAME);
            //if (hostEntry.AddressList.Any())
            //{
            //    hostEndPoint = new IPEndPoint(hostEntry.AddressList[0], port);
            //}

            return hostEndPoint;
        }
    }
}

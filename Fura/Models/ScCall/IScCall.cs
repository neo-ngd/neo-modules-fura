using System;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;

namespace Neo.Plugins.Models
{
	public interface IScCall
	{
		public void Execute(ScCallModel scCall, UInt256 txid, MongoDB.Entities.Transaction transaction, NeoSystem system, Block block, DataCache snapshot);
	}
}

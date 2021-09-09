using System;
using System.Collections.Generic;
using NBitcoin;
using NBitcoin.DataEncoders;
using Stratis.Bitcoin.Features.Consensus.Rules.CommonRules;
using Stratis.Bitcoin.Features.Consensus.Rules.ProvenHeaderRules;
using Stratis.Bitcoin.Features.MemoryPool.Rules;

namespace Stratis.Bitcoin.Networks
{
    public static class AmazaNetwork
    {
        /// <summary> Stratis maximal value for the calculated time offset. If the value is over this limit, the time syncing feature will be switched off. </summary>
        public const int StratisMaxTimeOffsetSeconds = 25 * 60;

        /// <summary> Stratis default value for the maximum tip age in seconds to consider the node in initial block download (2 hours). </summary>
        public const int StratisDefaultMaxTipAgeInSeconds = 2 * 60 * 60;

        /// <summary> The name of the root folder containing the different Stratis blockchains (StratisMain, StratisTest, StratisRegTest). </summary>
        public const string AmazaRootFolderName = "amaza";

        /// <summary> The default name used for the Strax configuration file. </summary>
        public const string AmazaDefaultConfigFilename = "amaza.conf";

        public static void RegisterRules(IConsensus consensus)
        {
            consensus.ConsensusRules
                .Register<HeaderTimeChecksRule>()
                .Register<HeaderTimeChecksPosRule>()
                .Register<PosFutureDriftRule>()
                .Register<CheckDifficultyPosRule>()
                .Register<StratisHeaderVersionRule>()
                .Register<ProvenHeaderSizeRule>()
                .Register<ProvenHeaderCoinstakeRule>();

            consensus.ConsensusRules
                .Register<BlockMerkleRootRule>()
                .Register<PosBlockSignatureRepresentationRule>()
                .Register<PosBlockSignatureRule>();

            consensus.ConsensusRules
                .Register<SetActivationDeploymentsPartialValidationRule>()
                .Register<PosTimeMaskRule>()

                // rules that are inside the method ContextualCheckBlock
                .Register<TransactionLocktimeActivationRule>()
                .Register<CoinbaseHeightActivationRule>()
                .Register<WitnessCommitmentsRule>()
                .Register<BlockSizeRule>()

                // rules that are inside the method CheckBlock
                .Register<EnsureCoinbaseRule>()
                .Register<CheckPowTransactionRule>()
                .Register<CheckPosTransactionRule>()
                .Register<CheckSigOpsRule>()
                .Register<StraxCoinstakeRule>();

            consensus.ConsensusRules
                .Register<SetActivationDeploymentsFullValidationRule>()

                .Register<CheckDifficultyHybridRule>()

                // rules that require the store to be loaded (coinview)
                .Register<LoadCoinviewRule>()
                .Register<TransactionDuplicationActivationRule>()
                .Register<StraxCoinviewRule>() // implements BIP68, MaxSigOps and BlockReward calculation
                                               // Place the PosColdStakingRule after the PosCoinviewRule to ensure that all input scripts have been evaluated
                                               // and that the "IsColdCoinStake" flag would have been set by the OP_CHECKCOLDSTAKEVERIFY opcode if applicable.
                .Register<StraxColdStakingRule>()
                .Register<SaveCoinviewRule>();
        }

        public static void RegisterMempoolRules(IConsensus consensus)
        {
            consensus.MempoolRules = new List<Type>()
            {
                typeof(CheckConflictsMempoolRule),
                typeof(StraxCoinViewMempoolRule),
                typeof(CreateMempoolEntryMempoolRule),
                typeof(CheckSigOpsMempoolRule),
                typeof(StraxTransactionFeeMempoolRule),
                typeof(CheckRateLimitMempoolRule),
                typeof(CheckAncestorsMempoolRule),
                typeof(CheckReplacementMempoolRule),
                typeof(CheckAllInputsMempoolRule),
                typeof(CheckTxOutDustRule)
            };
        }

        public static Block CreateGenesisBlock(ConsensusFactory consensusFactory, uint time, uint nonce, uint bits, int version, Money genesisReward, string genesisText)
        {
            string PgPSign = "-----BEGIN PGP PUBLIC KEY BLOCK-----\nVersion: Keybase OpenPGP v1.0.0\nComment: https://keybase.io/crypto\n\n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\n/cdGfLLFqWwyJQ==\n=vzIT\n-----END PGP PUBLIC KEY BLOCK-----\n";
            
            Transaction txNew = consensusFactory.CreateTransaction();
            txNew.Version = 1;
            txNew.AddInput(new TxIn()
            {
                ScriptSig = new Script(Op.GetPushOp(0), new Op()
                {
                    Code = (OpcodeType)0x1,
                    PushData = new[] { (byte)42 }
                }, Op.GetPushOp(Encoders.ASCII.DecodeData(PgPSign)))
            });
            txNew.AddOutput(new TxOut()
            {
                Value = genesisReward,
            });

            Block genesis = consensusFactory.CreateBlock();
            genesis.Header.BlockTime = Utils.UnixTimeToDateTime(time);
            genesis.Header.Bits = bits;
            genesis.Header.Nonce = nonce;
            genesis.Header.Version = version;
            genesis.Transactions.Add(txNew);
            genesis.Header.HashPrevBlock = uint256.Zero;
            genesis.UpdateMerkleRoot();

            return genesis;

        }

        public static readonly Dictionary<NetworkType, Func<Network>> MainChainNetworks = new Dictionary<NetworkType, Func<Network>>
        {
            { NetworkType.Mainnet, Networks.Amaza.Mainnet },
            { NetworkType.Testnet, Networks.Amaza.Testnet },
            { NetworkType.Regtest, Networks.Amaza.Regtest }
        };
    }
}

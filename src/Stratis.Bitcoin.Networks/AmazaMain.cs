using System;
using System.Collections.Generic;
using System.Net;
using NBitcoin;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.DataEncoders;
using NBitcoin.Protocol;
using Stratis.Bitcoin.Networks.Deployments;
using Stratis.Bitcoin.Networks.Policies;

namespace Stratis.Bitcoin.Networks
{
    public class AmazaMain : Network
    {
        public AmazaMain()
        {
            this.Magic = 0xD9B4BEF9;
            this.Name = "AmazaMain";
            this.NetworkType = NetworkType.Mainnet;
            this.DefaultPort = 17105;
            this.DefaultMaxOutboundConnections = 16;
            this.DefaultMaxInboundConnections = 100;
            this.DefaultRPCPort = 17104;
            this.DefaultAPIPort = 17103;
            this.DefaultSignalRPort = 17102;
            this.MaxTipAge = 2 * 60 * 60;
            this.MinTxFee = 10000;
            this.FallbackFee = 10000;
            this.MinRelayTxFee = 10000;
            this.MaxTimeOffsetSeconds = 4200;
            this.MaxTipAge = 86400;
            this.RootFolderName = AmazaNetwork.AmazaRootFolderName;
            this.DefaultConfigFilename = AmazaNetwork.AmazaDefaultConfigFilename;
            this.MaxTimeOffsetSeconds = 25 * 60;
            this.CoinTicker = "TIMES";
            this.DefaultBanTimeSeconds = 60 * 60 * 24; // 24 Hours
            
            this.Federations = new Federations();
            this.Federations.RegisterFederation(new Federation(new[]
            {
                new PubKey("020ddcbbd3f0c80d11cc9583ed9a936197f4ac0df40e564e34cd862b5f3e3b18d4"),
                new PubKey("03253d2c9fe2c79c655540dadff7c5d2ddd462b2714294b49757f9fe16112b8bde"),
                new PubKey("03f64315129a415dffb11e0c231cfb4a211456935a823f748948e0e2fdf4e2a88f"),
                new PubKey("0288845744df1b85836c19092599bc151ba4c0ea6e73224d47af0e47bbaa77a98f"),
                new PubKey("032837835af1f28e31c652576b184c49d512a751644755da4e6713976d137e7161")}));
            
            var consensusOptions = new PosConsensusOptions(
                maxBlockBaseSize: 1_000_000,
                maxStandardVersion: 2,
                maxStandardTxWeight: 100_000,
                maxBlockSigopsCost: 20_000,
                maxStandardTxSigopsCost: 20_000 / 5,
                witnessScaleFactor: 4
            );

            var consensusFactory = new PosConsensusFactory();
            
            this.GenesisTime = 1632296362;
            this.GenesisNonce = 1;
            this.GenesisVersion = 1;
            this.GenesisReward = Money.Zero;
            
            Block genesisBlock = AmazaNetwork.CreateGenesisBlock(consensusFactory, this.GenesisTime, this.GenesisNonce, this.GenesisBits, this.GenesisVersion, this.GenesisReward, "https://sagetowers.com"); // Genesis Text Hardcoded in AmazaNetwork

            this.Genesis = genesisBlock;

            var buriedDeployments = new BuriedDeploymentsArray
            {
                [BuriedDeployments.BIP34] = 0,
                [BuriedDeployments.BIP65] = 0,
                [BuriedDeployments.BIP66] = 0
            };

            var bip9Deployments = new StraxBIP9Deployments()
            {
                [StraxBIP9Deployments.CSV] = new BIP9DeploymentsParameters("CSV", 0, BIP9DeploymentsParameters.AlwaysActive, 999999999, BIP9DeploymentsParameters.DefaultMainnetThreshold),
                [StraxBIP9Deployments.Segwit] = new BIP9DeploymentsParameters("Segwit", 1, BIP9DeploymentsParameters.AlwaysActive, 999999999, BIP9DeploymentsParameters.DefaultMainnetThreshold),
                [StraxBIP9Deployments.ColdStaking] = new BIP9DeploymentsParameters("ColdStaking", 2, BIP9DeploymentsParameters.AlwaysActive, 999999999, BIP9DeploymentsParameters.DefaultMainnetThreshold)
            };
           
            this.Consensus = new NBitcoin.Consensus(
                consensusFactory: consensusFactory,
                consensusOptions: consensusOptions,
                coinType: 9001, // https://github.com/satoshilabs/slips/blob/master/slip-0044.md
                hashGenesisBlock: genesisBlock.GetHash(),
                subsidyHalvingInterval: 2500000,
                majorityEnforceBlockUpgrade: 750,
                majorityRejectBlockOutdated: 950,
                majorityWindow: 1000,
                buriedDeployments: buriedDeployments,
                bip9Deployments: bip9Deployments,
                bip34Hash: null,
                minerConfirmationWindow: 1008, // https://wiki.bitcoinsv.io/index.php/Difficulty we read adjust weekly
                maxReorgLength: 750,
                defaultAssumeValid: null, // TODO: Set this once some checkpoint candidates have elapsed
                maxMoney: 1500000000 * Money.COIN, // 1.5b , one billion genesis so 500,000,000 to farm so 66% of total funds are in genesis block
                coinbaseMaturity: 30,
                premineHeight: 2,
                premineReward: Money.Coins(1000000000),
                proofOfWorkReward: Money.Coins(30),
                powTargetTimespan: TimeSpan.FromSeconds( 4 * 60 * 60),
                targetSpacing: TimeSpan.FromSeconds(30),
                powAllowMinDifficultyBlocks: false,
                posNoRetargeting: false,
                powNoRetargeting: false,
                powLimit: new Target(new uint256("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")), // Adjusted difficulty 
                minimumChainWork:null,
                isProofOfStake: true,
                lastPowBlock:  10000000,
                proofOfStakeLimit: new BigInteger(uint256.Parse("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeLimitV2: new BigInteger(uint256.Parse("000000000000ffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeReward: Money.Coins(15) 
            );

            this.Consensus.PosEmptyCoinbase = false;

            this.Base58Prefixes = new byte[12][];
            this.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { 75 }; // X
            this.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { 140 }; // y
            this.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (75 + 128) };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_NO_EC] = new byte[] { 0x01, 0x42 };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_EC] = new byte[] { 0x01, 0x43 };
            this.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x88), (0xB2), (0x1E) };
            this.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x88), (0xAD), (0xE4) };
            this.Base58Prefixes[(int)Base58Type.PASSPHRASE_CODE] = new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 };
            this.Base58Prefixes[(int)Base58Type.CONFIRMATION_CODE] = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A };
            this.Base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { 0x2a };
            this.Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 23 };
            this.Base58Prefixes[(int)Base58Type.COLORED_ADDRESS] = new byte[] { 0x13 };

           this.Checkpoints = new Dictionary<int, CheckpointInfo>
            { 
            };
           
            this.Bech32Encoders = new Bech32Encoder[2];
            var encoder = new Bech32Encoder("amaza");
            this.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            this.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;
            
            this.DNSSeeds = new List<DNSSeedData>();
 
            this.SeedNodes = new List<NetworkAddress>
            {
                new NetworkAddress(IPAddress.Parse("207.148.15.3"), 17105),
                new NetworkAddress(IPAddress.Parse("45.76.230.173"), 17105),
            };

            this.StandardScriptsRegistry = new StraxStandardScriptsRegistry();
            
            AmazaNetwork.RegisterRules(this.Consensus);
            AmazaNetwork.RegisterMempoolRules(this.Consensus);
        }
    }
}